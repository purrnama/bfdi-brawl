#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

namespace Quixel
{
    /// <summary>
    /// Completely rewritten importer with lots of commenting... Should be quite a bit faster than the previous one too.
    /// For those looking to customize, please be aware that some of the methods in here do not use built in Unity API calls.
    /// This is because in future, i'd like to try to multi-thread some of the stuff that happens in here, if possible.
    /// Unity does not like multithreading, you cannot call any Unity API from outside of the main thread.
    /// This made my life very difficult. I hope you have better luck.
    /// Lee Devonald - Technical Artist @ Quixel.
    /// Ajwad Imran - Technical Artist @ Quixel.
    /// </summary>
    public class MegascansImporter : Editor
    {
        private bool plant = false;
        private string assetName;
        private string id;
        private int resX;
        private int resY;
        private int assetType;
        private string mapName;
        private string finalName;

        private string path;
        private int dispType;
        private int texPack;
        private int importResolution;
        private int lodFadeMode;
        private int shaderType;
        private string userPrefix;
        private string userSuffix;
        private bool setupCollision = false;
        private bool generateTerrainNormal = false;

        private string texPath;
        private string matPath;
        private string meshPath;

        private Material finalMat;
        private Material billboardMat;
        private Material hpMat;

        private bool highPoly = false;

        //private List<Thread> threads = new List<Thread>();

        /// <summary>
        /// Takes an imported JSON object, and breaks it into relevant components and data.
        /// Then calls relevant functions for actual import of asset.
        /// </summary>
        /// <param name="objectList"></param>
        public void ImportMegascansAssets(Newtonsoft.Json.Linq.JObject objectList)
        {
            var startTime = System.DateTime.Now;

            //get texture components from the current object.
            Newtonsoft.Json.Linq.JArray textureComps = (Newtonsoft.Json.Linq.JArray)objectList["components"];

            //get mesh components from the current object.
            Newtonsoft.Json.Linq.JArray meshComps = (Newtonsoft.Json.Linq.JArray)objectList["meshList"];

            //run a check to see if we're using Unity 5 or below, and then if we're trying to import a high poly mesh. if so, let the user know we are aborting the import.
            if (meshComps.Count > 0)
            {
                string ver = Application.unityVersion;
                if (ver[0] == '5' || ver[0] == '3')
                {
                    //get test path
                    string tp = (string)meshComps[0]["path"];
                    if (tp.ToLower().Contains("high"))
                    {
                        string msg = "Unity version 5 and below does not support the import of high poly meshes, aborting asset import.";
                        if (EditorUtility.DisplayDialog("WARNING!", msg, "ok"))
                        {
                            return;
                        }
                    }
                }
            }

            mapName = "";

            assetType = (int)objectList["meshVersion"];
            string type = (string)objectList["type"];
            if (type.ToLower().Contains("3dplant"))
            {
                plant = true;
            }

            try {
                GetPresets();
                path = ConstructPath(objectList);

                if(path == null) {
                    Debug.Log("Asset: " + (string)objectList["name"] + " already exist in the Project. Please delete/rename the existing folder and re-import this asset.");
                    AssetDatabase.Refresh();
                    return;
                }

                GetShaderType();
            } catch (Exception ex) {
                Debug.Log("Error setting import path.");
                Debug.Log(ex.ToString());
            }

            try {
                //process textures
                ProcessTextures(textureComps);
            } catch (Exception ex) {
                Debug.Log("Error importing textures.");
                Debug.Log(ex.ToString());
            }

            //process meshes
            if (meshComps == null && !type.Contains("surface"))
            {
                Debug.LogError("No meshes found. Please double check your export settings.");
                Debug.Log("Import failed.");
                EditorUtility.ClearProgressBar();
                return;
            }

            if (meshComps.Count > 0)
            {
                try {
                //process meshes
                ProcessMeshes(meshComps);
                } catch (Exception ex) {
                    Debug.Log("Error importing meshes.");
                    Debug.Log(ex.ToString());
                }
                
                try {
                    //process prefabs
                    if (assetType > 1)
                    {
                        CreatePrefabs(plant);
                    }
                    else
                    {
                        CreatePrefabs();
                    }
                } catch (Exception ex) {
                    Debug.Log("Error making prefabs.");
                    Debug.Log(ex.ToString());
                }
            }

            EditorUtility.ClearProgressBar();

            var endTime = System.DateTime.Now;
            var totalTime = endTime - startTime;
            Debug.Log("Asset Import Time: " + totalTime);
            AssetDatabase.Refresh();
        }

        #region Mesh Processing Methods

        /// <summary>
        /// Import meshes, start from highest LOD and import the chain.
        /// </summary>
        /// <param name="meshComps"></param>
        void ProcessMeshes(Newtonsoft.Json.Linq.JArray meshComps)
        {
            meshPath = ValidateFolderCreate(path, "Models");
            string meshName = meshPath + "/" + finalName;
            //first we need to determine which LOD is the highest that's being imported.
            string lod = (string)meshComps[0]["name"];
            int ld = lod.Contains("LOD1") ? 1 : 0;
            for (int i = 2; i < 6; ++i)
            {
                ld = lod.Contains("LOD" + i.ToString()) ? i : ld;
            }

            //detect if we're trying to import a high poly mesh...
            string msg = "You are about to import a high poly mesh. \nThese meshes are usually millions of polygons and can cause instability to your project. \nWould you like to fall back to the next LOD instead?";
            if (lod.ToLower().Contains("high"))
            {
                if (!EditorUtility.DisplayDialog("WARNING!", msg, "Yes", "No"))
                {
#if UNITY_EDITOR_WIN
                    hpMat = new Material(finalMat.shader);
                    hpMat.CopyPropertiesFromMaterial(finalMat);
                    hpMat.SetTexture("_NormalMap", null);
                    hpMat.SetTexture("_BumpMap", null);
                    hpMat.DisableKeyword("_NORMALMAP_TANGENT_SPACE");
                    hpMat.DisableKeyword("_NORMALMAP");
                    hpMat.name = FixSpaces(new string[] { hpMat.name, "HighPoly" });
                    string hpMatDir = FixSpaces(new string[] { matPath, "HighPoly.mat" });
                    AssetDatabase.CreateAsset(hpMat, hpMatDir);
#endif
                    highPoly = true;
                }
            }

            //loop through each mesh component in the json file
            string absPath = "";
            for (int i = 0; i < meshComps.Count; ++i)
            {
                EditorUtility.DisplayProgressBar("Processing Asset: " + assetName, "Importing LODs...", (float)i / (meshComps.Count - 1));
                //get the path of the highest LOD to be imported.
                absPath = (string)meshComps[i]["path"];

                string newPath = meshName;
                //Need to do some special stuff if high poly mesh is being imported, like create a high poly material...
                if (lod.ToLower().Contains("high"))
                {
                    if (highPoly)
                    {
                        string highPath = FixSpaces(new string[] { newPath, i.ToString(), "HighPoly" });
                        if (lod.ToLower().Contains(".fbx"))
                        {
                            highPath += ".fbx";
                        }
                        else
                        {
                            highPath += ".obj";
                        }
                        if (File.Exists(absPath))
                        {
                            File.Copy(absPath, highPath, true);
                            AssetDatabase.ImportAsset(highPath);
                        }
                    }
                    absPath = absPath.Replace("High", "LOD0");
                    ld = 0;
                }

                newPath = meshName + "_" + i.ToString() + "_LOD" + ld.ToString();
                if (lod.ToLower().Contains(".fbx"))
                {
                    newPath += ".fbx";
                }
                else
                {
                    newPath += ".obj";
                }

                for (int j = ld; j < 6; ++j)
                {
                    //Then loop through the lower LODs, and import them too.
                    string nld = "_LOD" + j.ToString();
                    if (File.Exists(absPath))
                    {
                        File.Copy(absPath, newPath, true);
                        AssetDatabase.ImportAsset(newPath);
                    }
                    absPath = absPath.Replace(nld, "_LOD" + (j + 1).ToString());
                    newPath = newPath.Replace(nld, "_LOD" + (j + 1).ToString());
                }
            }
        }

        /// <summary>
        /// Creates prefabs from the newer assets on bridge, has an option for billboard materials on plants.
        /// </summary>
        /// <param name="hasBillboard"></param>
        /// <returns></returns>
        void CreatePrefabs(bool hasBillboard = false)
        {
            string prefabPath = ValidateFolderCreate(path, "Prefabs");
            string prefabName = finalName;

            string[] p = new string[] { meshPath };
            string[] s = AssetDatabase.FindAssets("t:Model", p);
            List<string> meshes = new List<string>();
            for (int i = 0; i < s.Length; ++i)
            {
                meshes.Add(AssetDatabase.GUIDToAssetPath(s[i]));
            }

            List<List<string>> variants = new List<List<string>>();
            for (int i = 0; i < meshes.Count; ++i)
            {
                EditorUtility.DisplayProgressBar(assetName, "Loading Mesh Variants...", (float)i / (meshes.Count - 1));
                List<string> v = new List<string>();
                for (int j = 0; j < meshes.Count; ++j)
                {
                    string meshA = CleanUp(meshes[j], true);
                    string meshB = CleanUp(meshes[i], true);
                    if ((meshA.Length == meshB.Length) && meshA.Contains(meshB))
                    {
                        v.Add(meshes[j]);
                    }
                }
                bool exists = false;
                for (int j = 0; j < variants.Count; ++j)
                {
                    if (variants[j][0].ToLower().Contains(v[0].ToLower()))
                    {
                        exists = true;
                    }
                }
                if (!exists)
                {
                    variants.Add(v);
                }
            }

            for (int i = 0; i < variants.Count; ++i)
            {
                EditorUtility.DisplayProgressBar("Processing Asset: " + assetName, "Creating LODs...", (float)i / (variants.Count - 1));
                GameObject g = new GameObject();
                g.AddComponent<LODGroup>();
                g.name = prefabName;

                for (int j = 0; j < variants[i].Count; ++j)
                {
                    UnityEngine.Object m = AssetDatabase.LoadAssetAtPath(variants[i][j], typeof(UnityEngine.Object));
                    GameObject mo = UnityEngine.Object.Instantiate(m) as GameObject;
                    mo.transform.parent = g.transform;
                    mo.name = mo.name.Replace("(Clone)", "");
                }

                LOD[] lods = new LOD[g.transform.childCount];
                float lodHeight = 1.0f;
                for (int j = 0; j < g.transform.childCount; ++j)
                {
                    Renderer[] r = new Renderer[1];
                    r[0] = g.transform.GetChild(j).GetComponent<Renderer>();
                    if (!r[0])
                    {
                        r[0] = g.transform.GetChild(j).GetChild(0).GetComponent<Renderer>();
                    }
                    r[0].material = finalMat;
                    if (j == g.transform.childCount - 1 && hasBillboard)
                    {
                        r[0].material = billboardMat;
                    }
                    if (j < 1)
                    {
                        lodHeight *= 0.25f;
                    }
                    else
                    {
                        lodHeight *= 0.5f;
                    }
                    lods[j] = new LOD(lodHeight, r);
                }
                g.GetComponent<LODGroup>().SetLODs(lods);
                g.GetComponent<LODGroup>().RecalculateBounds();
                g.GetComponent<LODGroup>().animateCrossFading = true;
                g.GetComponent<LODGroup>().fadeMode = (LODFadeMode)lodFadeMode; // Casting lod fade mode to enum.

                string finalName = prefabPath + "/" + prefabName + i.ToString() + ".prefab";
                finalName = finalName.Replace("(Clone)", "");
                UnityEngine.Object pf = AssetDatabase.LoadAssetAtPath(finalName, typeof(UnityEngine.Object));
                if (!pf)
                {
                    PrefabUtility.CreatePrefab(finalName, g);
                }
                else
                {
                    PrefabUtility.ReplacePrefab(g, pf, ReplacePrefabOptions.ReplaceNameBased);
                }
                DestroyImmediate(g);
            }
        }

        /// <summary>
        /// Generates prefabs from imported meshes.
        /// </summary>
        void CreatePrefabs()
        {
            string prefabPath = ValidateFolderCreate(path, "Prefabs");
            string prefabName = finalName;

            List<GameObject> gos = new List<GameObject>();
            List<List<Transform>> transforms = new List<List<Transform>>();
            string[] p = new string[] { meshPath };
            string[] s = AssetDatabase.FindAssets("t:Model", p);
            for (int i = 0; i < s.Length; ++i)
            {
                EditorUtility.DisplayProgressBar("Processing Asset: " + assetName, "Processing Meshes...", (float)i / (s.Length - 1));
                string sap = AssetDatabase.GUIDToAssetPath(s[i]);
                UnityEngine.Object o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sap);
                if (o.name.ToLower().Contains("highpoly") && !highPoly)
                {
                    continue;
                }
                GameObject go = UnityEngine.Object.Instantiate(o) as GameObject;
                List<Transform> t = new List<Transform>();
                if (go.transform.childCount > 0)
                {
                    for (int j = 0; j < go.transform.childCount; ++j)
                    {
                        t.Add(go.transform.GetChild(j));
                    }
                }
                else
                {
                    t.Add(go.transform);
                }
                transforms.Add(t);
                gos.Add(go);
            }

            if ((transforms.Count < 1) || (transforms[0].Count < 1))
            {
                return;
            }

            for (int i = 0; i < transforms[0].Count; ++i)
            {
                EditorUtility.DisplayProgressBar("Processing Asset: " + assetName, "Creating LODs...", (float)i / (transforms.Count - 1));
                GameObject go = new GameObject();
                go.AddComponent<LODGroup>();
                go.name = prefabName;

                for (int j = 0; j < transforms.Count; ++j)
                {
                    if (transforms[j].Count > 0)
                    {
                        transforms[j][i].parent = go.transform;
                        transforms[j][i].transform.localPosition = Vector3.zero;
                    }
                }

                LOD[] lods = new LOD[go.transform.childCount];
                float lodHeight = 1.0f;
                for (int j = 0; j < go.transform.childCount; ++j)
                {
                    Renderer[] r = new Renderer[1];
                    r[0] = go.transform.GetChild(j).GetComponent<Renderer>();
                    r[0].material = finalMat;
                    if (highPoly && j == 0)
                    {
                        r[0].material = hpMat;
#if UNITY_EDITOR_OSX
                        r[0].material = finalMat;
#endif
                    }
                    if (go.transform.childCount > 2)
                    {
                        if (j < (go.transform.childCount / 2))
                        {
                            lodHeight *= 0.75f;
                        }
                        if (j >= (go.transform.childCount / 2))
                        {
                            lodHeight *= 0.5f;
                        }
                    }
                    else
                    {
                        if (j < 1)
                        {
                            lodHeight *= 0.75f;
                        }
                        else
                        {
                            lodHeight *= 0.15f;
                        }
                    }
                    lods[j] = new LOD(lodHeight, r);

                    //this should only add Collision to the first 2 meshes.
                    if (j < 2 && setupCollision)
                    {
                        go.transform.GetChild(j).gameObject.AddComponent<MeshCollider>().sharedMesh = go.transform.GetChild(go.transform.childCount - 1).GetComponent<MeshFilter>().sharedMesh;
                    }
                }
                go.GetComponent<LODGroup>().SetLODs(lods);
                go.GetComponent<LODGroup>().RecalculateBounds();
                go.GetComponent<LODGroup>().fadeMode = (LODFadeMode)lodFadeMode; //Casting lod fade mode to enum.
                go.GetComponent<LODGroup>().animateCrossFading = true;
                go.isStatic = true;

                string finalName = prefabPath + "/" + prefabName + "_" + i.ToString() + ".prefab";
                finalName = finalName.Replace("(Clone)", "");
                UnityEngine.Object pf = AssetDatabase.LoadAssetAtPath(finalName, typeof(UnityEngine.Object));
                if (!pf)
                {
                    PrefabUtility.CreatePrefab(finalName, go);
                }
                else
                {
                    PrefabUtility.ReplacePrefab(go, pf, ReplacePrefabOptions.ReplaceNameBased);
                }
                DestroyImmediate(go);
            }
            for (int i = 0; i < gos.Count; ++i)
            {
                DestroyImmediate(gos[i]);
            }
        }

        /// <summary>
        /// Removes file extensions from meshes and textures.
        /// Optionally removes LODs as well.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="nukeLODs"></param>
        /// <returns></returns>
        string CleanUp(string path, bool nukeLODs = false)
        {
            //Nuke the "LOD" from mesh strings.
            if (nukeLODs)
            {
                path = path.Replace("_LOD0", "");
                path = path.Replace("_LOD1", "");
                path = path.Replace("_LOD2", "");
                path = path.Replace("_LOD3", "");
                path = path.Replace("_LOD4", "");
                path = path.Replace("_LOD5", "");
                path = path.Replace("_LOD6", "");
            }

            path = path.Replace(".fbx", "");
            path = path.Replace(".FBX", "");
            path = path.Replace(".obj", "");
            path = path.Replace(".OBJ", "");

            path = path.Replace(".jpg", "");
            path = path.Replace(".exr", "");

            //Nuke the package key from strings.
            //path = path.Replace(key, "");
            return path;
        }

        #endregion

        #region Texture Processing Methods

        /// <summary>
        /// Process textures from Megascans asset import.
        /// </summary>
        /// <param name="textureComponents"></param>
        /// <returns></returns>
        void ProcessTextures(Newtonsoft.Json.Linq.JArray textureComponents)
        {
            //create a subdirectory for textures.
            texPath = ValidateFolderCreate(path, "Textures");
            texPath += "/" + finalName.Replace(" ","_");

            matPath = ValidateFolderCreate(path, "Materials");
            matPath += "/" + finalName.Replace(" ","_");

            //Attempt to store all the paths we might need to get our textures.
            //It's quicker to do this, than create an array and loop through it continually using a lot of if-statements later on.
            string albedo = null;
            string opacity = null;
            string normals = null;
            string metallic = null;
            string specular = null;
            string AO = null;
            string gloss = null;
            string displacement = null;
            string roughness = null;
            string translucency = null;

            //Search the JSON array for each texture type, leave it null if it doesn't exist. This is important as we use the null check later.
            for (int i = 0; i < textureComponents.Count; ++i)
            {
                albedo = (string)textureComponents[i]["type"] == "albedo" ? (string)textureComponents[i]["path"] : albedo;
                albedo = (albedo == null && (string)textureComponents[i]["type"] == "diffuse") ? (string)textureComponents[i]["path"] : albedo;
                opacity = (string)textureComponents[i]["type"] == "opacity" ? (string)textureComponents[i]["path"] : opacity;
                normals = (string)textureComponents[i]["type"] == "normal" ? (string)textureComponents[i]["path"] : normals;
                metallic = (string)textureComponents[i]["type"] == "metalness" ? (string)textureComponents[i]["path"] : metallic;
                specular = (string)textureComponents[i]["type"] == "specular" ? (string)textureComponents[i]["path"] : specular;
                AO = (string)textureComponents[i]["type"] == "ao" ? (string)textureComponents[i]["path"] : AO;
                gloss = (string)textureComponents[i]["type"] == "gloss" ? (string)textureComponents[i]["path"] : gloss;
                displacement = (string)textureComponents[i]["type"] == "displacement" ? (string)textureComponents[i]["path"] : displacement;
                roughness = (string)textureComponents[i]["type"] == "roughness" ? (string)textureComponents[i]["path"] : roughness;
                translucency = (string)textureComponents[i]["type"] == "translucency" ? (string)textureComponents[i]["path"] : translucency;
            }

            //make sure we never try to import the high poly normalmap...
            if (normals != null)
            {
                for (int i = 0; i < 6; ++i)
                {
                    string ld = "_LOD" + i.ToString();
                    string n = normals.Replace("Bump", ld);
                    if (File.Exists(n))
                    {
                        normals = n;
                        break;
                    }
                }
            }

            finalMat = ReadWriteAllTextures(albedo, opacity, normals, metallic, specular, AO, gloss, displacement, roughness, translucency);
            if (assetType > 1 && plant)
            {
#if UNITY_EDITOR_WIN
                string[] pathParts = albedo.Split('\\');
#endif
#if UNITY_EDITOR_OSX
                string[] pathParts = albedo.Split('/');
#endif
                string[] nameParts = pathParts[pathParts.Length - 1].Split('_');
                albedo = albedo == null ? null : albedo.Replace("Atlas", "Billboard").Replace(nameParts[0], "Billboard");
                opacity = opacity == null ? null : opacity.Replace("Atlas", "Billboard").Replace(nameParts[0], "Billboard");
                normals = normals == null ? null : normals.Replace("Atlas", "Billboard").Replace(nameParts[0], "Billboard");
                metallic = metallic == null ? null : metallic.Replace("Atlas", "Billboard").Replace(nameParts[0], "Billboard");
                specular = specular == null ? null : specular.Replace("Atlas", "Billboard").Replace(nameParts[0], "Billboard");
                AO = AO == null ? null : AO.Replace("Atlas", "Billboard").Replace(nameParts[0], "Billboard");
                gloss = gloss == null ? null : gloss.Replace("Atlas", "Billboard").Replace(nameParts[0], "Billboard");
                displacement = displacement == null ? null : displacement.Replace("Atlas", "Billboard").Replace(nameParts[0], "Billboard");
                roughness = roughness == null ? null : roughness.Replace("Atlas", "Billboard").Replace(nameParts[0], "Billboard");
                translucency = translucency == null ? null : translucency.Replace("Atlas", "Billboard").Replace(nameParts[0], "Billboard");
                texPath = FixSpaces(new string[] { texPath, "Billboard" });
                matPath = FixSpaces(new string[] { matPath, "Billboard" });
                billboardMat = ReadWriteAllTextures(albedo, opacity, normals, metallic, specular, AO, gloss, displacement, roughness, translucency);
            }
        }

        /// <summary>
        /// Creates materials needed for the asset.
        /// </summary>
        /// <returns></returns>
        Material CreateMaterial()
        {
            string rp = matPath + ".mat";
            Material mat = (Material)AssetDatabase.LoadAssetAtPath(rp, typeof(Material));
            if (!mat)
            {
                mat = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, rp);
                AssetDatabase.Refresh();
                if (shaderType < 1)
                {
                    mat.shader = Shader.Find("HDRenderPipeline/Lit");
#if UNITY_2018_3
                    mat.shader = Shader.Find("HDRP/Lit");
#endif
                    mat.SetInt("_DisplacementMode", dispType);
                }
                if (shaderType > 0)
                {
                    if (MegascansUtilities.isLegacy())
                    {
                        mat.shader = Shader.Find("LightweightPipeline/Standard (Physically Based)");
                    } else
                    {
                        mat.shader = Shader.Find("Lightweight Render Pipeline/Lit");
                    }
                }
                if (shaderType > 1)
                {
                    mat.shader = Shader.Find("Standard");
                    if (texPack > 0)
                    {
                        mat.shader = Shader.Find("Standard (Specular setup)");
                    }
                }
            }
            return mat;
        }

        /// <summary>
        /// Previous version of the importer would loop through a list of texture paths, and use a bunch of if-statements and do things accordingly.
        /// This version just takes in every texture path and if it's not null, does the thing. Less looping, better overall performance.
        /// </summary>
        /// <param name="albedo"></param>
        /// <param name="opacity"></param>
        /// <param name="normals"></param>
        /// <param name="metallic"></param>
        /// <param name="specular"></param>
        /// <param name="AO"></param>
        /// <param name="gloss"></param>
        /// <param name="displacement"></param>
        /// <param name="roughness"></param>
        /// <param name="translucency"></param>
        Material ReadWriteAllTextures(string albedo, string opacity, string normals, string metallic, string specular, string AO, string gloss, string displacement, string roughness, string translucency)
        {
            Material mat = CreateMaterial();
            //create a new work thread for each texture to be processed.
            //Pack the opacity into the alpha channel of albedo if it exists.
            string p = FixSpaces(new string[] { texPath, "Albedo.png" });
            mapName = opacity != null ? "Albedo + Alpha" : "Albedo";
            Texture2D tex;
            tex = PackTextures(albedo, opacity, p);
            mat.SetTexture("_MainTex", tex);
            mat.SetTexture("_BaseColorMap", tex);
            if (opacity != null)
            {
                if (shaderType > 0)
                {
                    mat.SetFloat("_AlphaClip", 1);
                    mat.SetFloat("_Mode", 1);
                    mat.SetFloat("_Cull", 1);
                    mat.EnableKeyword("_ALPHATEST_ON");
                }
                else
                {
                    mat.SetInt("_AlphaCutoffEnable", 1);
                    mat.SetFloat("_AlphaCutoff", 0.333f);
                    mat.SetInt("_DoubleSidedEnable", 1);

                    mat.SetOverrideTag("RenderType", "TransparentCutout");
                    mat.SetInt("_ZTestGBuffer", (int)UnityEngine.Rendering.CompareFunction.Equal);
                    mat.SetInt("_CullMode", (int)UnityEngine.Rendering.CullMode.Off);
                    mat.SetInt("_CullModeForward", (int)UnityEngine.Rendering.CullMode.Back);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.renderQueue = 2450;
                    mat.SetInt("_ZTestGBuffer", (int)UnityEngine.Rendering.CompareFunction.Equal);

                    mat.EnableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_DOUBLESIDED_ON");
                    mat.DisableKeyword("_BLENDMODE_ALPHA");
                    mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                }
            }

            //test to see if gloss is absent but roughness is present...
            bool useRoughness = (gloss == null && roughness != null);
            if (texPack < 1 || shaderType < 1)
            {
                mapName = "Masks";
                p = FixSpaces(new string[] { texPath, "Masks.png" });
                mat.SetFloat("_Metallic", 1.0f);
                tex = PackTextures(metallic, AO, displacement, useRoughness ? roughness : gloss, p, useRoughness);
                mat.SetTexture("_MaskMap", tex);
                mat.EnableKeyword("_MASKMAP");
                mat.SetFloat("_MaterialID", 1);
                mat.SetTexture("_MetallicGlossMap", tex); 
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                mat.EnableKeyword("_METALLICGLOSSMAP");
            }

            //do we need to process a specular map?
            if (texPack > 0 && specular != null)
            {
                mapName = "Specular + Gloss";
                p = FixSpaces(new string[] { texPath, "Specular.png" });
                tex = PackTextures(specular, useRoughness ? roughness : gloss, p, useRoughness);
                mat.SetTexture("_SpecGlossMap", tex);
                mat.SetColor("_SpecColor", new UnityEngine.Color(1.0f, 1.0f, 1.0f));
                mat.SetColor("_SpecularColor", new UnityEngine.Color(1.0f, 1.0f, 1.0f));
                mat.SetFloat("_WorkflowMode", 0);
                mat.SetFloat("_MaterialID", 4);
                mat.SetTexture("_SpecularColorMap", tex);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                mat.EnableKeyword("_SPECGLOSSMAP");
                mat.EnableKeyword("_SPECULAR_SETUP");
                mat.EnableKeyword("_SPECULARCOLORMAP");
                mat.EnableKeyword("_MATERIAL_FEATURE_SPECULAR_COLOR");
            }

            //handle any textures which can just be converted in place.
            if (normals != null)
            {
                p = FixSpaces(new string[] { texPath, "Normals.png" });
                CreateTexture(normals, p);
                tex = TextureImportSetup(p, true, false);
                mat.SetTexture("_BumpMap", tex);
                mat.SetTexture("_NormalMap", tex);
                mat.EnableKeyword("_NORMALMAP_TANGENT_SPACE");
                mat.EnableKeyword("_NORMALMAP");

                p = FixSpaces(new string[] { texPath, "Normals_Terrain.png" });

                if(generateTerrainNormal)
                {
                    MegascansUtilities.ImportTerrainNormal(normals, p, resX, resY, importResolution);
                }
            }

            if (displacement != null && dispType > 0)
            {
                p = FixSpaces(new string[] { texPath, "Displacement.png" });
                CreateTexture(displacement, p);
                tex = TextureImportSetup(p, false, false);
                mat.SetTexture("_HeightMap", tex);
                mat.EnableKeyword("_DISPLACEMENT_LOCK_TILING_SCALE");
                if (dispType == 1)
                {
                    mat.EnableKeyword("_VERTEX_DISPLACEMENT");
                    mat.EnableKeyword("_VERTEX_DISPLACEMENT_LOCK_OBJECT_SCALE");
                }
                if (dispType == 2)
                {
                    mat.EnableKeyword("_PIXEL_DISPLACEMENT");
                    mat.EnableKeyword("_PIXEL_DISPLACEMENT_LOCK_OBJECT_SCALE");
                }
            }

            //occlusion may or may not need to be packed, depending on the shader used.
            if (shaderType > 1 && AO != null)
            {
                p = FixSpaces(new string[] { texPath, "Occlusion.png" });
                CreateTexture(AO, p);
                tex = TextureImportSetup(p, false, false);
                mat.SetTexture("_OcclusionMap", tex);
                mat.EnableKeyword("_OCCLUSIONMAP");
            }

            if (translucency != null)
            {
                mapName = "Translucency";
                p = FixSpaces(new string[] { texPath, "Translucency.png" });
                tex = PackTextures(translucency, translucency, translucency, null, p);
                mat.SetInt("_MaterialID", 0);
                mat.SetInt("_DiffusionProfile", 1);
                mat.SetFloat("_EnableSubsurfaceScattering", 1);
                mat.SetTexture("_SubsurfaceMaskMap", tex);
                mat.SetTexture("_ThicknessMap", tex);
                if (plant)
                {
                    mat.SetInt("_DiffusionProfile", 2);
                    mat.SetFloat("_CoatMask", 0.0f);
                    mat.SetInt("_EnableWind", 1);
                    mat.EnableKeyword("_VERTEX_WIND");
                }
                mat.EnableKeyword("_SUBSURFACE_MASK_MAP");
                mat.EnableKeyword("_THICKNESSMAP");
                mat.EnableKeyword("_MATERIAL_FEATURE_TRANSMISSION");
            }
            EditorUtility.ClearProgressBar();
            return mat;
        }

        /// <summary>
        /// Sets the import settings for textures, normalmap, sRGB etc.
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="normalMap"></param>
        /// <param name="sRGB"></param>
        Texture2D TextureImportSetup(string assetPath, bool normalMap, bool sRGB = true)
        {
            TextureImporter tImp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImp == null)
            {
                return null;
            }
            tImp.maxTextureSize = importResolution;
            tImp.sRGBTexture = sRGB;
            tImp.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            AssetDatabase.ImportAsset(assetPath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        /// <summary>
        /// literally just write the file to disk.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="assetPath"></param>
        void CreateTexture(string tex, string assetPath)
        {
            if ((tex == null) || (File.Exists(tex) == false))
            {
                return;
            }
            Texture2D t = ImportJPG(tex);
            File.WriteAllBytes(assetPath, t.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath);
        }

        /// <summary>
        /// used for packing an alpha channel into an existing RGB texture. Uses native Unity API calls, can't be multithreaded, and is considerably slower than our own method.
        /// Currently has to run if using Mac OSX as there is no support for the system.imaging library on that operating system.
        /// Will fail if textures aren't the same resolution.
        /// </summary>
        /// <param name="cPixels"></param>
        /// <param name="aPixels"></param>
        /// <param name="invertAlpha"></param>
        /// <returns></returns>
        Texture2D PackTextures(string rgbPath, string aPath, string savePath, bool invertAlpha = false)
        {
            if ((rgbPath == null) && (aPath == null))
            {
                return null;
            }
            UnityEngine.Color[] rgbCols = ImportJPG(rgbPath) != null ? ImportJPG(rgbPath).GetPixels() : null;
            UnityEngine.Color[] aCols = ImportJPG(aPath) != null ? ImportJPG(aPath).GetPixels() : null;
            UnityEngine.Color[] rgbaCols = new UnityEngine.Color[resX * resY];
            for (int i = 0; i < resX * resY; ++i)
            {
                if (IterationCounter(i, (resX * resY) - 1, 100))
                {
                    EditorUtility.DisplayProgressBar("Processing Asset: " + assetName, "Creating " + mapName + " RGB + A texture", (float)i / ((resX * resY) - 1));
                }
                rgbaCols[i] = rgbCols != null ? rgbCols[i] : new UnityEngine.Color(1.0f, 1.0f, 1.0f);
                rgbaCols[i].a = aCols != null ? ((aCols[i].r + aCols[i].g + aCols[i].b) / 3.0f) : 1.0f;
                rgbaCols[i].a = invertAlpha ? 1.0f - rgbaCols[i].a : rgbaCols[i].a;
            }
            Texture2D tex = new Texture2D(resX, resY, TextureFormat.RGBAFloat, false);
            tex.SetPixels(rgbaCols);
            File.WriteAllBytes(savePath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(savePath);
            DestroyImmediate(tex);
            return TextureImportSetup(savePath, false);
        }

        /// <summary>
        /// used for packing multiple textures into an RGBA mask texture. Uses native Unity API calls, can't be multithreaded, and is considerably slower than our own method.
        /// Currently has to run if using Mac OSX as there is no support for the system.imaging library on that operating system.
        /// Will fail if textures aren't the same resolution.
        /// </summary>
        /// <param name="rPixels"></param>
        /// <param name="gPixels"></param>
        /// <param name="bPixels"></param>
        /// <param name="aPixels"></param>
        /// <param name="invertAlpha"></param>
        /// <returns></returns>
        Texture2D PackTextures(string rPath, string gPath, string bPath, string aPath, string savePath, bool invertAlpha = false)
        {
            if ((rPath == null) && (gPath == null) && (bPath == null) && (aPath == null))
            {
                return null;
            }
            UnityEngine.Color[] rCols = ImportJPG(rPath) != null ? ImportJPG(rPath).GetPixels() : null;
            UnityEngine.Color[] gCols = ImportJPG(gPath) != null ? ImportJPG(gPath).GetPixels() : null;
            UnityEngine.Color[] bCols = ImportJPG(bPath) != null ? ImportJPG(bPath).GetPixels() : null;
            UnityEngine.Color[] aCols = ImportJPG(aPath) != null ? ImportJPG(aPath).GetPixels() : null;
            UnityEngine.Color[] rgbaCols = new UnityEngine.Color[resX * resY];
            for (int i = 0; i < resX * resY; ++i)
            {
                if (IterationCounter(i, (resX * resY) - 1, 100))
                {
                    EditorUtility.DisplayProgressBar("Processing Asset: " + assetName, "Creating " + mapName + " RGBA texture", (float)i / ((resX * resY) - 1));
                }
                rgbaCols[i].r = rCols != null ? (rCols[i].r + rCols[i].g + rCols[i].b) / 3.0f : 0.0f;
                rgbaCols[i].g = gCols != null ? (gCols[i].r + gCols[i].g + gCols[i].b) / 3.0f : 1.0f;
                rgbaCols[i].b = bCols != null ? (bCols[i].r + bCols[i].g + bCols[i].b) / 3.0f : 0.0f;
                rgbaCols[i].a = aCols != null ? (aCols[i].r + aCols[i].g + aCols[i].b) / 3.0f : 1.0f;
                rgbaCols[i].a = invertAlpha ? 1.0f - rgbaCols[i].a : rgbaCols[i].a;
            }
            Texture2D tex = new Texture2D(resX, resY, TextureFormat.RGBAFloat, false);
            tex.SetPixels(rgbaCols);
            File.WriteAllBytes(savePath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(savePath);
            DestroyImmediate(tex);
            return TextureImportSetup(savePath, false, false);
        }

        /// <summary>
        /// reads a texture file straight from hard drive absolute path, converts it to a Unity texture.
        /// </summary>
        /// <param name="absPath"></param>
        /// <returns></returns>
        Texture2D ImportJPG(string absPath)
        {
            if (absPath == null)
            {
                return null;
            }
            if (!File.Exists(absPath))
            {
                Debug.LogWarning("Could not find " + absPath + "\nPlease make sure it is downloaded.");
                return null;
            }
            byte[] texData = File.ReadAllBytes(absPath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBAFloat, true);
            tex.LoadImage(texData);
            resX = tex.width; resY = tex.height;
            return tex;
        }

        /// <summary>
        /// Gets importer settings stored in app registry.
        /// Without this, the asset would have incorrect pathing, and would not be able to create the correct materials etc.
        /// </summary>
        void GetPresets()
        {
            path = MegascansUtilities.FixPath(EditorPrefs.GetString("QuixelDefaultPath", "Quixel/Megascans/"));
            dispType = EditorPrefs.GetInt("QuixelDefaultDisplacement");
            texPack = EditorPrefs.GetInt("QuixelDefaultTexPacking");
            shaderType = EditorPrefs.GetInt("QuixelDefaultShader");
            userPrefix = "QXL_" + EditorPrefs.GetString("QuixelDefaultPrefix").Replace(" ", "_");
            userSuffix = EditorPrefs.GetString("QuixelDefaultSuffix").Replace(" ", "_");
            importResolution = Convert.ToInt32(Math.Pow(2, 9 + EditorPrefs.GetInt("QuixelDefaultImportResolution", 4)));
            lodFadeMode = EditorPrefs.GetInt("QuixelDefaultLodFadeMode", 1);
            setupCollision = EditorPrefs.GetBool("QuixelDefaultSetupCollision", true);
            generateTerrainNormal = EditorPrefs.GetBool("QuixelDefaultTerrainNormal", false);
        }


        /// <summary>
        /// a small function which checks the current iteration of a for-loop, and if that iteration is a repetition of your iterCheck, it returns true.
        /// for example, if your iterCheck is 10, true is returned very 10 iterations.
        /// </summary>
        /// <param name="currentIter"></param>
        /// <param name="maxIter"></param>
        /// <param name="iterCheck"></param>
        /// <returns></returns>
        bool IterationCounter(int currentIter, int maxIter, int iterCheck)
        {
            if (currentIter % (maxIter / iterCheck) == 0 && currentIter != 0)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Formatting Utilities

        /// <summary>
        /// Check whether the child folder you're trying to make already exists, if not, create it and return the directory.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        string ValidateFolderCreate(string parent, string child)
        {
            string tempPath = FixSlashes(Path.Combine(parent, child));
            if (!AssetDatabase.IsValidFolder(tempPath))
            {
                string newPath = AssetDatabase.CreateFolder(parent, child);
                return AssetDatabase.GUIDToAssetPath(newPath);
            }
            return FixSlashes(tempPath);
        }

        /// <summary>
        /// Determine which asset type we're creating. Surfaces, 3D_Assets, 3D_Scatter_Assets, 3D_Plants.
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        string GetAssetType(string jsonPath)
        {
            string t = "Surfaces";
            if (jsonPath.ToLower().Contains("3d"))
            {
                t = "3D_Assets";
            }
            if (jsonPath.ToLower().Contains("debris") ||
                jsonPath.ToLower().Contains("dbrs") ||
                jsonPath.ToLower().Contains("scatter") ||
                jsonPath.ToLower().Contains("sctr"))
            {
                t = "3D_Scatter_Assets";
            }
            if (jsonPath.ToLower().Contains("3dplant"))
            {
                t = "3D_Plants";
            }
            if (jsonPath.ToLower().Contains("decals") || jsonPath.ToLower().Contains("decal"))
            {
                t = "Decals";
            }
            else if (jsonPath.ToLower().Contains("atlas"))
            {
                t = "Atlases";
            }
            return t;
        }

        /// <summary>
        /// fixes slashes so they work in Unity.
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        string FixSlashes(string txt)
        {
            txt = txt.Replace("\\", "/");
            txt = txt.Replace(@"\\", "/");
            return txt;
        }

        /// <summary>
        /// Replace any spaces with underscores. if more than one input, place underscore between them.
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        string FixSpaces(string[] txt)
        {
            if (txt == null || txt.Length == 0)
            {
                return "";
            }

            string newTxt = "";
            
            int maxIterations = txt.Length;

            if (txt[maxIterations-1] == "") {
                maxIterations--;
            }
                
            for (int i = 0; i < maxIterations; ++i)
            {
                if (i > 0)
                {
                    newTxt += "_";
                }
                newTxt += txt[i];
            }

            return newTxt.Replace("__", "_");
        }

        /// <summary>
        /// Returns the final directory for our asset, creating subfolders where necessary in the 'Assets' directory.
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        string ConstructPath(Newtonsoft.Json.Linq.JObject objectList)
        {
            ///
            /// Unity doesn't allow you to create objects in directories which don't exist.
            /// So in this function, we create any and all necessary subdirectories that are required.
            /// We return the final subdirectory, which is used later in the asset creation too.
            ///

            //first, create the user specified path from the importer settings.
            string[] pathParts = FixSlashes(path).Split('/');
            string defPath = "Assets";
            if (pathParts.Length > 0)
            {
                for (int i = 0; i < pathParts.Length; ++i)
                {
                    defPath = ValidateFolderCreate(defPath, pathParts[i]);
                }
            }

            //then create check to see if the asset type subfolder exists, create it if it doesn't.
            defPath = ValidateFolderCreate(defPath, GetAssetType((string)objectList["path"]));

            //then create a unique subfolder for the asset.
            assetName = (string)objectList["name"];
            id = (string)objectList["id"];
            finalName = FixSpaces(new string[] {userPrefix, assetName.Replace(" ", "_"), id, userSuffix });
            
            string tempPath = FixSlashes(Path.Combine(defPath, finalName));
            if (AssetDatabase.IsValidFolder(tempPath))
            {
                return null;
            }

            defPath = ValidateFolderCreate(defPath, finalName);

            return defPath;
        }

        /// <summary>
        /// This function attempts to auto-detect which template the project is using. Defaults to Legacy/Standard if all else fails.
        /// </summary>
        void GetShaderType()
        {
            shaderType = EditorPrefs.GetInt("QuixelDefaultShader");
            if (shaderType == 3)
            {
                //attempt to auto-detect a settings file for Lightweight or HD pipelines
                switch (MegascansUtilities.getCurrentPipeline())
                {
                    case Pipeline.HDRP:
                        shaderType = 0;
                        break;
                    case Pipeline.LWRP:
                        shaderType = 1;
                        break;
                    case Pipeline.Standard:
                        shaderType = 2;
                        break;
                    default:
                        shaderType = 2;
                        break;
                }
            }
        }
        #endregion
    }
}
#endif
