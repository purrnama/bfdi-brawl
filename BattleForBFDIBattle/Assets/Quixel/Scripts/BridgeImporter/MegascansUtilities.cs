#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quixel {
    public class MegascansUtilities : MonoBehaviour {
        #region Terrain_Normal_Map_Processing
        static int resX, resY, importResolution;

        /// <summary>
        /// used for packing an alpha channel into an existing RGB texture.
        /// Will fail if textures aren't the same resolution.
        /// </summary>
        /// <param name="cPixels"></param>
        /// <param name="aPixels"></param>
        /// <param name="invertAlpha"></param>
        /// <returns></returns>
        public static void ImportTerrainNormal (string rgbPath, string savePath, int _resX, int _resY, int _importResolution) {

            resX = _resX;
            resY = _resY;
            importResolution = _importResolution;
            ImportTerrainNormal(rgbPath, savePath);
        }

        /// <summary>
        /// used for packing an alpha channel into an existing RGB texture. Uses native Unity API calls, can't be multithreaded, and is considerably slower than our own method.
        /// Currently has to run if using Mac OSX as there is no support for the system.imaging library on that operating system.
        /// Will fail if textures aren't the same resolution.
        /// </summary>
        /// <param name="cPixels"></param>
        /// <param name="invertAlpha"></param>
        /// <returns></returns>
        static void ImportTerrainNormal(string rgbPath, string savePath) {
            if (rgbPath == null) {
                return;
            }
            UnityEngine.Color[] rgbCols = ImportJPG (rgbPath) != null ? ImportJPG (rgbPath).GetPixels () : null;
            UnityEngine.Color[] rgbaCols = new UnityEngine.Color[resX * resY];
            for (int i = 0; i < resX * resY; ++i) {
                rgbaCols[i] = rgbCols != null ? rgbCols[i] : new UnityEngine.Color (1.0f, 1.0f, 1.0f);
                rgbaCols[i].g = 1.0f - rgbaCols[i].g;
                rgbaCols[i].a = 1.0f;
            }
            Texture2D tex = new Texture2D (resX, resY, TextureFormat.RGBAFloat, false);
            tex.SetPixels (rgbaCols);
            File.WriteAllBytes (savePath, tex.EncodeToPNG ());
            AssetDatabase.ImportAsset (savePath);
            TextureImportSetup (savePath, true, false);
        }

        /// <summary>
        /// reads a texture file straight from hard drive absolute path, converts it to a Unity texture.
        /// </summary>
        /// <param name="absPath"></param>
        /// <returns></returns>
        static Texture2D ImportJPG (string absPath) {
            if (absPath == null) {
                return null;
            }
            if (!File.Exists (absPath)) {
                Debug.LogWarning ("Could not find " + absPath + "\nPlease make sure it is downloaded.");
                return null;
            }
            byte[] texData = File.ReadAllBytes (absPath);
            Texture2D tex = new Texture2D (2, 2, TextureFormat.RGBAFloat, true);
            tex.LoadImage (texData);
            resX = tex.width;
            resY = tex.height;
            return tex;
        }

        /// <summary>
        /// Sets the import settings for textures, normalmap, sRGB etc.
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="normalMap"></param>
        /// <param name="sRGB"></param>
        static void TextureImportSetup (string assetPath, bool normalMap, bool sRGB = true) {
            TextureImporter tImp = AssetImporter.GetAtPath (assetPath) as TextureImporter;
            if (tImp == null) {
                return;
            }
            tImp.maxTextureSize = importResolution;
            tImp.sRGBTexture = sRGB;
            tImp.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            AssetDatabase.ImportAsset (assetPath);
            AssetDatabase.LoadAssetAtPath<Texture2D> (assetPath);
        }
        #endregion

        #region Other_Utils
        /// <summary>
        /// Check whether the child folder you're trying to make already exists, if not, create it and return the directory.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static string ValidateFolderCreate (string parent, string child) {
            string tempPath = FixSlashes (Path.Combine (parent, child));
            if (!AssetDatabase.IsValidFolder (tempPath)) {
                string newPath = AssetDatabase.CreateFolder (parent, child);
                return AssetDatabase.GUIDToAssetPath (newPath);
            }
            return FixSlashes (tempPath);
        }

        /// <summary>
        /// fixes slashes so they work in Unity.
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string FixSlashes (string txt) {
            txt = txt.Replace ("\\", "/");
            txt = txt.Replace (@"\\", "/");
            return txt;
        }

        /// <summary>
        /// Remove spaces and remove special characters.
        /// </summary>
        /// <param name="orgPath"></param>
        /// <returns></returns>
        public static string FixPath (string orgPath) {
            string path = orgPath.Trim ();
            string[] pathFolders = path.Split ('/');

            for (int j = 0; j < pathFolders.Length - 1; j++) {
                pathFolders[j] = pathFolders[j].Trim ();
            }

            path = pathFolders[0];
            for (int i = 1; i < pathFolders.Length; i++) {
                path += "/" + pathFolders[i];
            }
            return path;
        }

        static bool isLegacyProject = false;
        static bool identifiedPipeline = false;

        public static bool isLegacy () {
            if (!identifiedPipeline) {
                string[] versionParts = Application.unityVersion.Split ('.');
                int majorVersion = int.Parse (versionParts[0]);
                int minorVersion = int.Parse (versionParts[1]);
                isLegacyProject = (majorVersion < 2018 || (majorVersion == 2018 && minorVersion < 3));
            }
            return isLegacyProject;

        }

        //attempt to auto-detect a settings file for Lightweight or HD pipelines
        public static Pipeline getCurrentPipeline () {
            Pipeline currentPipeline = Pipeline.HDRP;

            //attempt to auto-detect a settings file for Lightweight or HD pipelines
            if (AssetDatabase.IsValidFolder ("Assets/Settings")) {
                if (AssetDatabase.LoadAssetAtPath ("Assets/Settings/HDRenderPipelineAsset.asset", typeof (ScriptableObject))) {
                    currentPipeline = Pipeline.HDRP;
                } else if (AssetDatabase.LoadAssetAtPath ("Assets/Settings/Lightweight_RenderPipeline.asset", typeof (ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath ("Assets/Settings/LWRP-HighQuality.asset", typeof (ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath ("Assets/Settings/LWRP-LowQuality.asset", typeof (ScriptableObject)) ||
                    AssetDatabase.LoadAssetAtPath ("Assets/Settings/LWRP-MediumQuality.asset", typeof (ScriptableObject))) {
                    currentPipeline = Pipeline.LWRP;
                } else {
                    currentPipeline = Pipeline.Standard;
                }
            } else {
                if (AssetDatabase.FindAssets ("HDRenderPipelineAsset").Length > 0) {
                    currentPipeline = Pipeline.HDRP;
                } else if (AssetDatabase.FindAssets ("Lightweight_RenderPipeline").Length > 0 ||
                    AssetDatabase.FindAssets ("LWRP-HighQuality").Length > 0 ||
                    AssetDatabase.FindAssets ("LWRP-LowQuality").Length > 0 ||
                    AssetDatabase.FindAssets ("LWRP-MediumQualit").Length > 0) {
                    currentPipeline = Pipeline.LWRP;
                } else {
                    currentPipeline = Pipeline.Standard;
                }
            }

            return currentPipeline;
        }
        #endregion

        #region Selection Helpers
        /// <summary>
        /// Retrieves selected folders on Project view.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSelectedFolders (List<UnityEngine.Object> selections) {
            List<string> folders = new List<string> ();

            foreach (UnityEngine.Object obj in selections) //Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                string path = AssetDatabase.GetAssetPath (obj);
                if (!string.IsNullOrEmpty (path)) {
                    folders.Add (path);
                }
            }
            return folders;
        }

        /// <summary>
        /// Recursively gather all files under the given path including all its subfolders.
        /// </summary>
        public static List<string> GetFiles (string path, string fileType = null) {
            List<string> files = new List<string> ();
            Queue<string> queue = new Queue<string> ();
            queue.Enqueue (path);
            while (queue.Count > 0) {
                path = queue.Dequeue ();
                foreach (string subDir in Directory.GetDirectories (path)) {
                    queue.Enqueue (subDir);
                }
                foreach (string s in Directory.GetFiles (path)) {
                    if (fileType != null && s.Contains (fileType)) {
                        if (s.Contains (fileType)) {
                            files.Add (s);
                        }
                    } else {
                        files.Add (s);
                    }

                }
            }
            return files;
        }
        #endregion

        #region HDRP Features PreDefined Macro Setup
#if UNITY_2018_2 || UNITY_2018_3 || UNITY_2019
        [MenuItem ("Window/Quixel/Enable HDRP Features")]
        private static void EnableHDRP () {
            Debug.Log ("HDRP enabled.");
            AddDefineIfNecessary ("HDRP", EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        [MenuItem ("Window/Quixel/Disable HDRP Features")]
        private static void DisableHDRP () {
            Debug.Log ("HDRP disabled.");
            RemoveDefineIfNecessary ("HDRP", EditorUserBuildSettings.selectedBuildTargetGroup);
        }
#endif
        public static void AddDefineIfNecessary (string _define, BuildTargetGroup _buildTargetGroup) {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup (_buildTargetGroup);

            if (defines == null) { defines = _define; } else if (defines.Length == 0) { defines = _define; } else { if (defines.IndexOf (_define, 0) < 0) { defines += ";" + _define; } }

            PlayerSettings.SetScriptingDefineSymbolsForGroup (_buildTargetGroup, defines);
        }

        public static void RemoveDefineIfNecessary (string _define, BuildTargetGroup _buildTargetGroup) {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup (_buildTargetGroup);

            if (defines.StartsWith (_define + ";")) {
                // First of multiple defines.
                defines = defines.Remove (0, _define.Length + 1);
            } else if (defines.StartsWith (_define)) {
                // The only define.
                defines = defines.Remove (0, _define.Length);
            } else if (defines.EndsWith (";" + _define)) {
                // Last of multiple defines.
                defines = defines.Remove (defines.Length - _define.Length - 1, _define.Length + 1);
            } else {
                // Somewhere in the middle or not defined.
                var index = defines.IndexOf (_define, 0, System.StringComparison.Ordinal);
                if (index >= 0) { defines = defines.Remove (index, _define.Length + 1); }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup (_buildTargetGroup, defines);
        }
        #endregion

    }
}

#endif

public enum Pipeline {
    HDRP,
    LWRP,
    Standard
}