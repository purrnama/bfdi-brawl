#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace Quixel {
    public class MegascansImporterWindow : EditorWindow {
        static private int texPack;
        static private int texPackUpdate;
        static private string[] texPacking = new string[] {
            "Metallic",
            "Specular",
        };
        static private int dispType;
        static private int dispTypeUpdate;
        static private string[] dispTypes = new string[] {
            "None",
            "Vertex",
            "Pixel",
        };
        static private int shaderType;
        static private int shaderTypeUpdate;
        static private string[] shaderTypes = new string[] {
            "HDRenderPipeline",
            "Lightweight",
            "Legacy",
            "Auto-Detect",
        };

        static private int importResolution;
        static private int importResolutionUpdate;
        static private string[] importResolutions = new string[] {
            "512",
            "1024",
            "2048",
            "4096",
            "8192",
        };

        static private int lodFadeMode;
        static private int lodFadeModeUpdate;
        static private string[] lodFadeModeSettings = new string[] {
            "None",
            "Cross Fade",
            "Speed Tree"
        };

        static private string path;
        static private string prefix;
        static private string suffix;
        static private string pathUpdate;
        static private string prefixUpdate;
        static private string suffixUpdate;

        static private Texture2D MSLogo;
        static private GUIStyle MSLogoStyle;
        static private Texture2D MSBackground;
        static private GUIStyle MSField;
        static private GUIStyle MSPopup;
        static private GUIStyle MSText;
        static private GUIStyle MSCheckBox;
        static private GUIStyle MSHelpStyle;
        static private GUIStyle MSNormalTextStyle;
        static private GUIStyle MSWarningTextStyle;
        static private GUIStyle MSHeadingTextStyle;
        static private GUIStyle MSTabsStyle;
        static private GUIStyle MSStrechedWidthStyle;
        static private bool connection;
        static private bool connectionUpdate;
        static private bool setupCollision;
        static private bool setupCollisionUpdate;
        static private bool terrainNormal;
        static private bool terrainNormalUpdate;

        static private bool SuperHD;

        static private Vector2 size;
        static private Vector2 logoSize;
        static private Vector2 textSize;
        static private Vector2 textHeadingSize;
        static private Vector2 fieldSize;
        static private Vector2 tabSize;
        static private Rect mainSize;
        static private Rect collisionLoc;
        static private Rect terrainNormalLoc;
        static private Rect connectionLoc;

        //Decal Properties
        static private string decalBlend = "100";
        static private string decalSize = "1";

        //Decal Properties
        static private string decalBlendUpdate = "100";
        static private string decalSizeUpdate = "1";

        private int tab = 0;
        //Terrain tools properties
        static private string terrainMaterialName = "Terrain Material";
        static private string terrainMaterialPath = "Quixel/";
        static private string tiling = "10";

        static private string terrainMaterialNameUpdate = "Terrain Material";
        static private string terrainMaterialPathUpdate = "Quixel/";
        static private string tilingUpdate = "10";

        [MenuItem ("Window/Quixel/Megascans Importer", false, 10)]
        public static void Init () {
            MegascansImporterWindow window = (MegascansImporterWindow) EditorWindow.GetWindow (typeof (MegascansImporterWindow));
            GUIContent header = new GUIContent ();
            header.text = " Bridge LiveLink";
            header.image = (Texture) MSLogo;
            header.tooltip = "Megascans Bridge Livelink.";
            window.titleContent = header;
            window.maxSize = size * 4f;
            window.minSize = size;
            window.Show ();
        }

        void OnGUI () {

            GUI.DrawTexture (mainSize, MSBackground, ScaleMode.StretchToFill);

            GUILayout.BeginHorizontal ();

            if (GUILayout.Button (MSLogo, MSLogoStyle, GUILayout.Height (logoSize.y), GUILayout.Width (logoSize.x)))
                Application.OpenURL ("https://quixel.com/megascans/library/latest");

            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();

            tab = GUILayout.Toolbar (tab, new string[] { "Importer", "Utilities" }, MSTabsStyle, GUILayout.Height (tabSize.y), GUILayout.Width (tabSize.x));

            GUILayout.EndHorizontal ();

            GUILayout.BeginHorizontal ();
            Handles.color = Color.white;
            Handles.DrawLine (new Vector3 (0, 105), new Vector3 (310, 105));
            GUILayout.EndHorizontal ();

            if (tab == 0) {

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Workflow", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                texPack = EditorGUILayout.Popup (texPack, texPacking, MSPopup, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Displacement", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                dispType = EditorGUILayout.Popup (dispType, dispTypes, MSPopup, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Shader Type", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                shaderType = EditorGUILayout.Popup (shaderType, shaderTypes, MSPopup, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Import Resolution", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                importResolution = EditorGUILayout.Popup (importResolution, importResolutions, MSPopup, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("LOD Fade Mode", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                lodFadeMode = EditorGUILayout.Popup (lodFadeMode, lodFadeModeSettings, MSPopup, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Import Path", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                path = EditorGUILayout.TextField (path, MSField, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Asset Prefix", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                prefix = EditorGUILayout.TextField (prefix, MSField, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Asset Suffix", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                suffix = EditorGUILayout.TextField (suffix, MSField, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                setupCollision = EditorGUI.Toggle (collisionLoc, setupCollision, MSCheckBox);
                GUILayout.Box ("Setup Collision", MSNormalTextStyle, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                terrainNormal = EditorGUI.Toggle (terrainNormalLoc, terrainNormal, MSCheckBox);
                GUILayout.Box ("Generate Terrain Normals", MSNormalTextStyle, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                connection = EditorGUI.Toggle (connectionLoc, connection, MSCheckBox);
                GUILayout.Box ("Enable Live Link", MSNormalTextStyle, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                if (GUILayout.Button ("Help...", MSHelpStyle))
                    Application.OpenURL ("https://docs.google.com/document/d/17FmQzTxo63NIvGkRcfVfLtp73GSfBBvZmlq3v6C-9nY/edit?usp=sharing");

                GUILayout.EndHorizontal ();
            } else {

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Terrain Tools (Beta)", MSHeadingTextStyle, GUILayout.Height (textHeadingSize.y), GUILayout.Width (textHeadingSize.x));

                GUILayout.EndHorizontal ();

                if (MegascansUtilities.isLegacy ()) {

                    GUILayout.BeginHorizontal ();

                    GUILayout.Box ("Material Name", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                    terrainMaterialName = EditorGUILayout.TextField (terrainMaterialName, MSField, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                    GUILayout.EndHorizontal ();

                    GUILayout.BeginHorizontal ();

                    GUILayout.Box ("Material Path", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                    terrainMaterialPath = EditorGUILayout.TextField (terrainMaterialPath, MSField, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                    GUILayout.EndHorizontal ();
                }

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Texture Tiling", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                tiling = EditorGUILayout.TextField (tiling, MSField, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                if (GUILayout.Button ("Setup Paint Layers", MSStrechedWidthStyle, GUILayout.Height (textSize.y), GUILayout.Width (size.x)))
                    MegascansTerrainTools.SetupTerrain ();

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Warning: This feature works properly with the \nmetallic workflow only.", MSWarningTextStyle, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));

                GUILayout.EndHorizontal ();

                if (MegascansUtilities.isLegacy ()) {
                    GUILayout.BeginHorizontal ();

                    GUILayout.Box ("Warning: This feature requires HD Render Pipeline.", MSWarningTextStyle, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));

                    GUILayout.EndHorizontal ();
                }

                GUILayout.BeginHorizontal ();

                Handles.color = Color.white;
                Handles.DrawLine (new Vector3 (0, 105), new Vector3 (310, 105));

                GUILayout.EndHorizontal ();

#if HDRP && (UNITY_2018_2 || UNITY_2018_3 || UNITY_2019)

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Decal Setup (Beta)", MSHeadingTextStyle, GUILayout.Height (textHeadingSize.y), GUILayout.Width (textHeadingSize.x));

                GUILayout.EndHorizontal ();

                GUILayout.BeginHorizontal ();

                GUILayout.Box ("Global Opacity (%)", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                decalBlend = EditorGUILayout.TextField (decalBlend, MSField, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                GUILayout.EndHorizontal ();

                if (!MegascansUtilities.isLegacy ()) {
                    GUILayout.BeginHorizontal ();

                    GUILayout.Box ("Scale", MSText, GUILayout.Height (textSize.y), GUILayout.Width (textSize.x));
                    decalSize = EditorGUILayout.TextField (decalSize, MSField, GUILayout.Height (fieldSize.y), GUILayout.Width (fieldSize.x));

                    GUILayout.EndHorizontal ();
                }

                GUILayout.BeginHorizontal ();

                if (GUILayout.Button ("Create Decal Projector", MSStrechedWidthStyle, GUILayout.Height (textSize.y), GUILayout.Width (size.x)))
                    MegascansDecalTools.SetupDecalProjector ();

                GUILayout.EndHorizontal ();
#endif
            }

            if (!MSLogo) {
                InitStyle ();
                Repaint ();
            }
        }

        void OnEnable () {
            SuperHD = false;
            if (Display.main.systemHeight > 2100) {
                SuperHD = true;
            }
            size = SuperHD ? new Vector2 (567, 970) : new Vector2 (308, 560);
            mainSize = SuperHD ? new Rect (0, 0, 567, 970) : new Rect (0, 0, 308, 550);
            textSize = SuperHD ? new Vector2 (200, 54) : new Vector2 (100, 30);
            textHeadingSize = SuperHD ? new Vector2 (555, 64) : new Vector2 (308, 40);
            fieldSize = SuperHD ? new Vector2 (290, 54) : new Vector2 (152, 30);
            tabSize = SuperHD ? new Vector2 (490, 74) : new Vector2 (300, 40);
            collisionLoc = SuperHD ? new Rect (25, 690, 32, 32) : new Rect (13, 420, 17, 17);
            terrainNormalLoc = SuperHD ? new Rect (25, 770, 32, 32) : new Rect (13, 464, 17, 17);
            connectionLoc = SuperHD ? new Rect (25, 860, 32, 32) : new Rect (13, 508, 17, 17);
            logoSize = SuperHD ? new Vector2 (64, 64) : new Vector2 (34, 34);
            InitStyle ();
            GetDefaults ();
            Repaint ();
        }

        //If the values dont exist in editor prefs they are replaced with the default values.
        static void GetDefaults () {
            path = EditorPrefs.GetString ("QuixelDefaultPath", "Quixel/Megascans/");
            prefix = EditorPrefs.GetString ("QuixelDefaultPrefix", "");
            suffix = EditorPrefs.GetString ("QuixelDefaultSuffix", "");
            dispType = EditorPrefs.GetInt ("QuixelDefaultDisplacement", 0);
            texPack = EditorPrefs.GetInt ("QuixelDefaultTexPacking", 0);
            shaderType = EditorPrefs.GetInt ("QuixelDefaultShader", 3);
            importResolution = EditorPrefs.GetInt ("QuixelDefaultImportResolution", 4);
            lodFadeMode = EditorPrefs.GetInt ("QuixelDefaultLodFadeMode", 1);
            connection = EditorPrefs.GetBool ("QuixelDefaultConnection", false);
            setupCollision = EditorPrefs.GetBool ("QuixelDefaultSetupCollision", true);
            terrainNormal = EditorPrefs.GetBool ("QuixelDefaultTerrainNormal", false);

            decalBlend = EditorPrefs.GetString ("QuixelDefaultDecalBlend", "100");
            decalSize = EditorPrefs.GetString ("QuixelDefaultDecalSize", "1");

            terrainMaterialName = EditorPrefs.GetString ("QuixelDefaultMaterialName", "Terrain Material");
            terrainMaterialPath = EditorPrefs.GetString ("QuixelDefaultMaterialPath", "Quixel/");
            tiling = EditorPrefs.GetString ("QuixelDefaultTiling", "10");

            pathUpdate = path;
            prefixUpdate = prefix;
            suffixUpdate = suffix;
            dispTypeUpdate = dispType;
            texPackUpdate = texPack;
            shaderTypeUpdate = shaderType;
            connectionUpdate = connection;
            setupCollisionUpdate = setupCollision;
            terrainNormalUpdate = terrainNormal;
            importResolutionUpdate = importResolution;
            lodFadeModeUpdate = lodFadeMode;

            //Decal Properties
            decalBlendUpdate = decalBlend;
            decalSizeUpdate = decalSize;

            //Terrain tool properties
            terrainMaterialNameUpdate = terrainMaterialName;
            terrainMaterialPathUpdate = terrainMaterialPath;
            tilingUpdate = tiling;
        }

        void SaveDefaults () {

            if (connection != connectionUpdate) {
                if (connection)
                    MegascansBridgeLink.StartServer ();
                else
                    MegascansBridgeLink.EndServer ();
            }

            EditorPrefs.SetString ("QuixelDefaultPath", path);
            EditorPrefs.SetString ("QuixelDefaultPrefix", prefix);
            EditorPrefs.SetString ("QuixelDefaultSuffix", suffix);
            EditorPrefs.SetInt ("QuixelDefaultDisplacement", dispType);
            EditorPrefs.SetInt ("QuixelDefaultTexPacking", texPack);
            EditorPrefs.SetInt ("QuixelDefaultShader", shaderType);
            EditorPrefs.SetBool ("QuixelDefaultConnection", connection);
            EditorPrefs.SetBool ("QuixelDefaultSetupCollision", setupCollision);
            EditorPrefs.SetBool ("QuixelDefaultTerrainNormal", terrainNormal);
            EditorPrefs.SetInt ("QuixelDefaultImportResolution", importResolution);
            EditorPrefs.SetInt ("QuixelDefaultLodFadeMode", lodFadeMode);

            pathUpdate = path;
            prefixUpdate = prefix;
            suffixUpdate = suffix;
            dispTypeUpdate = dispType;
            texPackUpdate = texPack;
            shaderTypeUpdate = shaderType;
            importResolutionUpdate = importResolution;
            connectionUpdate = connection;
            setupCollisionUpdate = setupCollision;
            terrainNormalUpdate = terrainNormal;
            lodFadeModeUpdate = lodFadeMode;

            //Decal Properties

            EditorPrefs.SetString ("QuixelDefaultDecalBlend", decalBlend);
            EditorPrefs.SetString ("QuixelDefaultDecalSize", decalSize);

            decalBlendUpdate = decalBlend;
            decalSizeUpdate = decalSize;

            //Terrain tool properties

            EditorPrefs.SetString ("QuixelDefaultMaterialName", terrainMaterialName);
            EditorPrefs.SetString ("QuixelDefaultMaterialPath", terrainMaterialPath);
            EditorPrefs.SetString ("QuixelDefaultTiling", tiling);

            terrainMaterialNameUpdate = terrainMaterialName;
            terrainMaterialPathUpdate = terrainMaterialPath;
            tilingUpdate = tiling;

        }

        void ConstructPopUp () {
            MSPopup = new GUIStyle ();
            MSPopup.normal.textColor = new Color (1.0f, 1.0f, 1.0f);
            MSPopup.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/Popup_Background.png");
            MSPopup.font = AssetDatabase.LoadAssetAtPath<Font> ("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSPopup.fontSize = SuperHD ? 24 : 13;
            MSPopup.padding = SuperHD ? new RectOffset (20, 0, 10, 0) : new RectOffset (10, 5, 7, 4);
            MSPopup.margin = SuperHD ? new RectOffset (0, 20, 13, 7) : new RectOffset (0, 10, 6, 5);
        }

        void ConstructText () {
            MSText = new GUIStyle ();
            MSText.normal.textColor = new Color (0.4f, 0.4f, 0.4f);
            MSText.font = AssetDatabase.LoadAssetAtPath<Font> ("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSText.fontSize = SuperHD ? 24 : 13;
            MSText.padding = SuperHD ? new RectOffset (5, 0, 10, 0) : new RectOffset (5, 5, 7, 4);
            MSText.margin = SuperHD ? new RectOffset (20, 0, 13, 7) : new RectOffset (10, 20, 6, 5);
        }

        void ConstructBackground () {
            MSBackground = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/Background.png");
        }

        void ConstructLogo () {
            MSLogo = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/M.png");
            MSLogoStyle = new GUIStyle ();
            MSLogoStyle.margin = SuperHD ? new RectOffset (25, 0, 27, 33) : new RectOffset (15, 0, 15, 15);
        }

        void ConstructField () {
            MSField = new GUIStyle ();
            MSField.normal.textColor = new Color (1.0f, 1.0f, 1.0f);
            MSField.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/Field_Background.png");
            MSField.font = AssetDatabase.LoadAssetAtPath<Font> ("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSField.clipping = TextClipping.Clip;
            MSField.fontSize = SuperHD ? 24 : 13;
            MSField.padding = SuperHD ? new RectOffset (20, 0, 10, 0) : new RectOffset (10, 5, 7, 4);
            MSField.margin = SuperHD ? new RectOffset (0, 20, 13, 7) : new RectOffset (0, 10, 6, 5);
        }

        void ConstructCheckBox () {
            MSCheckBox = new GUIStyle ();
            MSCheckBox.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/CheckBoxOff.png");
            MSCheckBox.hover.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/CheckBoxHover.png");
            MSCheckBox.onNormal.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/CheckBoxOn.png");
        }

        void ConstructHelp () {
            MSHelpStyle = new GUIStyle ();
            MSHelpStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/Help.png");
            MSHelpStyle.font = AssetDatabase.LoadAssetAtPath<Font> ("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSHelpStyle.margin = SuperHD ? new RectOffset (152, 20, 35, 15) : new RectOffset (102, 0, 16, 5);
            MSHelpStyle.padding = SuperHD ? new RectOffset (20, 20, 10, 10) : new RectOffset (10, 10, 5, 5);
            MSHelpStyle.fontSize = SuperHD ? 24 : 12;
            MSHelpStyle.normal.textColor = new Color (0.16796875f, 0.59375f, 0.9375f);
        }

        void ConstructNormalText () {
            MSNormalTextStyle = new GUIStyle ();
            MSNormalTextStyle.normal.textColor = new Color (1.0f, 1.0f, 1.0f);
            MSNormalTextStyle.font = AssetDatabase.LoadAssetAtPath<Font> ("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSNormalTextStyle.fontSize = SuperHD ? 24 : 13;
            MSNormalTextStyle.padding = SuperHD ? new RectOffset (5, 0, 15, 15) : new RectOffset (5, 5, 7, 4);
            MSNormalTextStyle.margin = SuperHD ? new RectOffset (72, 0, 27, 10) : new RectOffset (37, 20, 13, 5);
        }

        void ConstructWarningText () {
            MSWarningTextStyle = new GUIStyle ();
            MSWarningTextStyle.normal.textColor = new Color (1.0f, 1.0f, 0.0f);
            MSWarningTextStyle.font = AssetDatabase.LoadAssetAtPath<Font> ("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSWarningTextStyle.fontSize = SuperHD ? 24 : 13;
            MSWarningTextStyle.padding = SuperHD ? new RectOffset (5, 0, 15, 15) : new RectOffset (5, 5, 7, 4);
            MSWarningTextStyle.margin = SuperHD ? new RectOffset (10, 0, 27, 10) : new RectOffset (10, 0, 13, 5);
        }

        void ConstructHeadingText () {
            MSHeadingTextStyle = new GUIStyle ();
            MSHeadingTextStyle.normal.textColor = new Color (1.0f, 1.0f, 1.0f);
            MSHeadingTextStyle.font = AssetDatabase.LoadAssetAtPath<Font> ("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSHeadingTextStyle.fontSize = SuperHD ? 30 : 16;
            MSHeadingTextStyle.alignment = TextAnchor.MiddleCenter;
        }

        void ContrauctTabs () {
            MSTabsStyle = new GUIStyle ();
            MSTabsStyle.font = AssetDatabase.LoadAssetAtPath<Font> ("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSTabsStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/Text_Background.png");
            MSTabsStyle.hover.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/Field_Background.png");
            MSTabsStyle.hover.textColor = new Color (1.0f, 1.0f, 1.0f);
            MSTabsStyle.active.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/Field_Background.png");
            MSTabsStyle.active.textColor = new Color (0.5f, 0.5f, 0.5f);
            MSTabsStyle.fontSize = SuperHD ? 26 : 15;
            MSTabsStyle.normal.textColor = new Color (1.0f, 1.0f, 1.0f);
            MSTabsStyle.padding = SuperHD ? new RectOffset (10, 10, 15, 15) : new RectOffset (4, 4, 7, 7);
            MSTabsStyle.margin = SuperHD ? new RectOffset (10, 10, 10, 10) : new RectOffset (5, 5, 5, 10);
            MSTabsStyle.alignment = TextAnchor.MiddleCenter;
        }

        void ContrauctStrechedWidth () {
            MSStrechedWidthStyle = new GUIStyle ();
            MSStrechedWidthStyle.font = AssetDatabase.LoadAssetAtPath<Font> ("Assets/Quixel/Scripts/Fonts/SourceSansPro-Regular.ttf");
            MSStrechedWidthStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/Text_Background.png");
            MSStrechedWidthStyle.hover.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/Field_Background.png");
            MSStrechedWidthStyle.hover.textColor = new Color (1.0f, 1.0f, 1.0f);
            MSStrechedWidthStyle.active.background = AssetDatabase.LoadAssetAtPath<Texture2D> ("Assets/Quixel/Scripts/Images/Field_Background.png");
            MSStrechedWidthStyle.active.textColor = new Color (0.5f, 0.5f, 0.5f);
            MSStrechedWidthStyle.fontSize = SuperHD ? 26 : 15;
            MSStrechedWidthStyle.normal.textColor = new Color (1.0f, 1.0f, 1.0f);
            MSStrechedWidthStyle.margin = SuperHD ? new RectOffset (0, 0, 10, 10) : new RectOffset (0, 0, 10, 10);
            MSStrechedWidthStyle.alignment = TextAnchor.MiddleCenter;
        }

        void InitStyle () {
            ConstructBackground ();
            ConstructLogo ();
            ConstructPopUp ();
            ConstructText ();
            ConstructField ();
            ConstructCheckBox ();
            ConstructHelp ();
            ConstructNormalText ();
            ConstructWarningText ();
            ConstructHeadingText ();
            ContrauctTabs ();
            ContrauctStrechedWidth ();
        }

        public static string GetPath () {
            return path;
        }

        public static int GetDispType () {
            return dispType;
        }

        private void Update () {
            if (
                (dispType != dispTypeUpdate) ||
                (shaderType != shaderTypeUpdate) ||
                (texPack != texPackUpdate) ||
                (path != pathUpdate) ||
                (prefix != prefixUpdate) ||
                (connection != connectionUpdate) ||
                (suffix != suffixUpdate) ||
                (importResolution != importResolutionUpdate) ||
                (lodFadeMode != lodFadeModeUpdate) ||
                (terrainNormal != terrainNormalUpdate) ||
                (setupCollision != setupCollisionUpdate) ||
                (decalBlendUpdate != decalBlend) ||
                (decalSizeUpdate != decalSize) ||
                (terrainMaterialNameUpdate != terrainMaterialName) ||
                (terrainMaterialPathUpdate != terrainMaterialPath) ||
                (tilingUpdate != tiling)
            ) {
                SaveDefaults ();
            }
        }
    }
}

#endif