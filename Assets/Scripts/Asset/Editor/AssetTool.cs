using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class AssetTool
{
    private static AssetList list = new AssetList();

    [InitializeOnLoadMethod]
    private static void InitEditor()
    {
        Editor.finishedDefaultHeaderGUI -= OnPostHeaderGUI;
        Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        EditorApplication.quitting -= SaveAssetList;
        EditorApplication.quitting += SaveAssetList;
        EditorApplication.projectChanged -= AssetChange;
        EditorApplication.projectChanged += AssetChange;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        if (EditorApplication.isPlayingOrWillChangePlaymode==false)
        {
            InitAssetList();
        }
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            InitAssetList();

            UpdateBuildSettingScene(false);

        }
        else if(state == PlayModeStateChange.ExitingEditMode)
        {
            SaveAssetList();

            UpdateBuildSettingScene(true);
        }
    }

    static void InitAssetList()
    {
        Debug.Log("Init AssetList");

        string path = string.Format("{0}/{1}", Application.dataPath, AssetPath.ASSETSFILE);
        string xml = File.ReadAllText(path);
        list.FromXml(xml);
    }

    static void UpdateBuildSettingScene(bool add)
    {
       List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
       var it = list.assets.GetEnumerator();
       while (it.MoveNext())
       {
           string path = it.Current.Value.path;
           if (path.EndsWith(".unity"))
           {
               var scene = GetScene(scenes, path);
               if (scene == null && add)
               {
                   scenes.Add(new EditorBuildSettingsScene(path, true));
               }
               else if(scene!= null && !add)
               {
                   scenes.Remove(scene);
                }
           }
       }
       EditorBuildSettings.scenes = scenes.ToArray();
    }

   
    static EditorBuildSettingsScene GetScene(List<EditorBuildSettingsScene> scenes, string path)
    {
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path.ToLower() == path)
            {
                return scenes[i];
            }
        }

        return null;
    }

    static GUIStyle s_ToggleMixed;
  
    
    static  List<string> s_AssetDirs = new List<string>
    {
        { "assets/resources/"},
        { "assets/r/" },
    };

    static bool IsValid(string assetPath)
    {
        bool isValid = false;

        if (string.IsNullOrEmpty(assetPath)
            || assetPath.EndsWith(".cs")
        )
        {
            return isValid;
        }
        var it = s_AssetDirs.GetEnumerator();
        while (it.MoveNext())
        {
            if (assetPath.StartsWith(it.Current))
            {
                isValid = true; break;
            }
        }

        return isValid;
    }
    static void OnPostHeaderGUI(Editor editor)
    {
        if (editor.target == null)
        {
            return;
        }
        string assetPath = AssetDatabase.GetAssetPath(editor.target).ToLower();

        if (IsValid(assetPath) == false)
        {
            return;
        }

        if (s_ToggleMixed == null)
        {
            s_ToggleMixed = new GUIStyle("ToggleMixed");
        }

        var asset = GetAsset(editor.target);

        GUILayout.BeginHorizontal();

        bool select = asset != null && list.Contains(asset.name);
        bool mixed = IsMixedSelect(editor.targets);

        if (mixed)
        {
            select = false;
        }
        

        bool check;
        if (mixed)
        {
            check = GUILayout.Toggle(select, "Asset", s_ToggleMixed, GUILayout.ExpandWidth(false));
        }
        else
        {
            check = GUILayout.Toggle(select, "Asset", GUILayout.ExpandWidth(false));
        }
        if (select && check == false)
        {
            RemoveAssets(editor.targets);
            asset = null;
        }
        else if (select == false && check)
        {
            AddAssets(editor.targets);
        }

        if (asset != null && select && editor.targets.Length == 1)
        {

            asset.path = assetPath;

            var assetName = EditorGUILayout.DelayedTextField(asset.name, GUILayout.ExpandWidth(true));
            if (assetName != asset.name)
            {
                list.Remove(asset);
                asset.name = assetName;
                list.Add(asset);
                SaveAssetList();
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (asset != null && list.Contains(asset.name))
        {
            select = string.IsNullOrEmpty(asset.group) == false;
            mixed = IsMixedGroup(editor.targets);
            if (mixed)
            {
                check = GUILayout.Toggle(select, "Group", s_ToggleMixed, GUILayout.ExpandWidth(false));
            }
            else
            {
                check = GUILayout.Toggle(select, "Group", GUILayout.ExpandWidth(false));
            }
            if (check == false && select)
            {
                SetAssetsGroup(editor.targets, "");
                SaveAssetList();
            }
            else
            {
                if (check)
                {
                    if (select == false)
                    {
                        string dir = Path.GetDirectoryName(asset.path);
                        asset.group = dir.Substring(dir.LastIndexOf("\\") + 1);
                    }

                    var group = EditorGUILayout.DelayedTextField(asset.group, GUILayout.ExpandWidth(true));
                    if ( select == false ||  group != asset.group)
                    {
                        //Debug.Log(check + "," + select + "," + asset.group + "," + group);
                        SetAssetsGroup(editor.targets, group);
                        SaveAssetList();
                    }
                }

            }
        }

        GUILayout.EndHorizontal();

    }

    static void AddAssets(Object[] targets)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var asset = GetAsset(targets[i]);
            if (asset != null)
            {
                list.Add(asset);
            }
        }
    }

    static void RemoveAssets(Object[] targets)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var asset = GetAsset(targets[i]);
            if (asset != null)
            {
                list.Remove(asset);
            }
        }
    }

    static void SetAssetsGroup(Object[] targets, string group)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var asset = GetAsset(targets[i]);
            if (asset != null)
            {
                asset.group = group;
            }
        }
    }

 



    static bool IsMixedSelect(Object[] targets)
    {
        bool firstSelected = false;
        bool firstSet = false;
        
        for (int i = 0; i < targets.Length; ++i)
        {
            var asset = GetAsset(targets[i]);
            if (asset != null)
            {
                bool select = list.Contains(asset.name);
                if (firstSet == false)
                {
                    firstSelected = select;
                    firstSet = true;
                }
                else
                {
                    if (firstSelected != select)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    static bool IsMixedGroup(Object[] targets)
    {
        string firstGroup = "";
        bool firstSet = false;
        for(int i = 0; i < targets.Length; ++i)
        {
            var asset = GetAsset(targets[i]);
            if (asset != null)
            {
                if (firstSet == false)
                {
                    if (string.IsNullOrEmpty(asset.group) == false)
                    {
                        firstGroup = asset.group;
                    }
                    firstSet = true;
                }
                else
                {
                    if (firstGroup != asset.group)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    static AssetFile GetAsset(Object target)
    {
        string assetPath = AssetDatabase.GetAssetPath(target).ToLower();

        if (IsValid(assetPath) == false)
        {
            return null;
        }

        string fullpath = Application.dataPath.Replace("Assets", "/") + assetPath;
        byte[] bytes = File.Exists(fullpath) ? File.ReadAllBytes(fullpath) : null;

        string name = Path.GetFileName(assetPath);

        string md5 = bytes != null ? MD5Hash.Get(bytes) : "";

        AssetFile asset = list.Get(name);
        if (asset == null)
        {
            asset = list.Get(md5);
        }
        if(asset!= null)
        {
            asset.path = assetPath;
        }

        if (asset == null && bytes != null)
        {
            asset = new AssetFile();
            asset.name = name;
            asset.path = assetPath;
            asset.md5 = md5;
            asset.size = bytes.Length;
        }


        return asset;
    }

    [MenuItem("Tools/Asset/生成资源配置")]
    static void GenerateAssetList()
    {
        list.Clear();
        var it = s_AssetDirs.GetEnumerator();
        while (it.MoveNext())
        {
            UpdateAssetList(list, string.Format("{0}/{1}",Application.dataPath.Replace("Assets",""),it.Current));
        }
        
        SaveAssetList();
    }

    static void UpdateAssetList(AssetList list, string dir)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(dir);
        if (dirInfo.Exists == false)
        {
            return;
        }
        FileInfo[] files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
      

        for (int i = 0; i < files.Length; ++i)
        {
            if (files[i].Name.EndsWith(".meta")
            || files[i].Name.EndsWith(".manifest"))
            {
                continue;
            }
            string fullPath = files[i].DirectoryName + "/" + files[i].Name;
            fullPath = fullPath.Replace("\\", "/").ToLower();

            string name = files[i].Name;
            string path = name;
            string md5 = MD5Hash.Get(File.ReadAllBytes(fullPath));

            if (fullPath.Contains("assets/"))
            {
                path = fullPath.Substring(fullPath.IndexOf("assets/")).ToLower();
            }

            AssetFile asset = list.Get(name);
            if (asset == null) asset = list.Get(path);
            if (asset == null) asset = list.Get(md5);
            if (asset == null)
            {
                asset = new AssetFile();
                asset.name = name;
                asset.path = path;
            }

            asset.size = files[i].Length;

            asset.md5 = md5;

            list.Add(asset);

        }
    }

    [MenuItem("Tools/Asset/保存资源配置")]
    static void SaveAssetList()
    {
        string assetXmlFile = Application.dataPath + "/"+ AssetPath.ASSETSFILE;

        StreamWriter writerXml = new StreamWriter(assetXmlFile);
        writerXml.Write(list.ToXml());
        writerXml.Close();
        writerXml.Dispose();
        Debug.Log("保存成功:" + assetXmlFile);
    }

    static void AssetChange()
    {
        
    }

    [MenuItem("Tools/Asset/Clear All AssetBundleName")]
    static void ClearAssetBundleName()
    {
        ClearAssetBundleName(Application.dataPath);
    }

    static void ClearAssetBundleName(string dic)
    {
        string[] files = Directory.GetFiles(dic, "*.*", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; i++)
        {
            string path = files[i];

            if (path.EndsWith(".cs")
                || path.EndsWith(".meta")
            )
            {
                continue;
            }

            path = path.Replace("\\", "/").Replace(Application.dataPath + "/", "");
            path = "Assets/" + path;

            AssetImporter importer = AssetImporter.GetAtPath(path);

            if (importer == null) continue;
            importer.assetBundleName = "";

            EditorUtility.DisplayProgressBar("Clear AssetBundleName", "Clear:" + path, i / (float)files.Length);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Asset/Build(只会Build勾选的资源)")]
    static void Build()
    {
        foreach(var dir in s_AssetDirs)
        {
            ClearAssetBundleName(Application.dataPath.Replace("Assets","")+ dir);
        }
        
        List<string> scenes = new List<string>();
        List<AssetFile> removes = new List<AssetFile>();
        AssetList buildList = new AssetList();
        var it = list.assets.GetEnumerator();
        int count = 0;
        while (it.MoveNext())
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(it.Current.Value.path))
            {
                //Resources目录不打Bundle
                if (it.Current.Value.path.Contains("resources/") == false)
                {
                    if (it.Current.Value.path.EndsWith(".unity"))
                    {
                        scenes.Add(it.Current.Value.path);
                        if (buildList.assets.ContainsValue(it.Current.Value) == false)
                        {
                            buildList.assets.Add(it.Current.Value.name, it.Current.Value);
                        }
                    }
                    else
                    {
                        AssetImporter importer = AssetImporter.GetAtPath(it.Current.Value.path);
                        if (importer != null)
                        {
                            importer.assetBundleName = string.IsNullOrEmpty(it.Current.Value.group)
                                ? it.Current.Value.path
                                : it.Current.Value.group;

                            if (buildList.assets.ContainsValue(it.Current.Value) == false)
                            {
                                buildList.assets.Add(it.Current.Value.name, it.Current.Value);
                            }
                        }
                    }
                }
                else
                {
                    if (buildList.assets.ContainsValue(it.Current.Value) == false)
                    {
                        buildList.assets.Add(it.Current.Value.name, it.Current.Value);
                    }
                }
            }
            else
            {
                removes.Add(it.Current.Value);
            }

            EditorUtility.DisplayProgressBar("Set AssetbundleName", "Setting:"+ it.Current.Value.path, count * 1f/ list.assets.Count);
            count++;
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        SetAssetBundleMode();

        string outputPath = AssetPath.streamingAssetsPath;

        if (Directory.Exists(outputPath))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(outputPath);
            if (dirInfo.Exists)
            {
                FileInfo[] files = dirInfo.GetFiles();
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Exists)
                    {
                        files[i].Delete();
                    }
                }
            }

            Directory.Delete(outputPath, true);
        }

        if (Directory.Exists(outputPath) == false)
        {
            Directory.CreateDirectory(outputPath);
        }
       
        //打包场景
        for (int i = 0; i < scenes.Count; i++)
        {
            string path = outputPath + scenes[i].ToLower();
            string dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            string[] scene = { scenes[i] };
            BuildPipeline.BuildPlayer(scene, path, EditorUserBuildSettings.activeBuildTarget, BuildOptions.BuildAdditionalStreamedScenes);

            EditorUtility.DisplayProgressBar("BuildScene", "Building Scene:"+ scenes[i], i * 1.0f / scenes.Count);
        }
        //打包资源
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

        EditorUtility.ClearProgressBar();

        UpdateAssetList(buildList, outputPath);
     
        buildList.manifest = EditorUserBuildSettings.activeBuildTarget.ToString();

        string assetXmlFile = outputPath + "/" + AssetPath.ASSETSFILE;

        StreamWriter writerXml = new StreamWriter(assetXmlFile);
        writerXml.Write(buildList.ToXml());
        writerXml.Close();
        writerXml.Dispose();

        for(int i = 0; i< removes.Count;++i)
        {
            list.Remove(removes[i]);
        }
        SaveAssetList();

        Debug.Log("Build Success!");
    }

  

    [MenuItem("Tools/Asset/Mode/Editor")]
    static void SetEditorMode()
    {
        PlayerPrefs.SetInt("assetMode", (int)AssetMode.Editor);
    }
    [MenuItem("Tools/Asset/Mode/AssetBundle")]
    static void SetAssetBundleMode()
    {
        PlayerPrefs.SetInt("assetMode", (int)AssetMode.AssetBundle);
    }

    [MenuItem("Tools/Asset/Delete Missing Scripts")]
    static void CleanupMissingScript()
    {
        EditorUtility.DisplayProgressBar("Clear Missing Scripts", "Clear Missing Scripts", 0f);
       
        GameObject[] pAllObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));

        for (int i = 0; i < pAllObjects.Length; ++i)
        {
            var item = pAllObjects[i];
            SerializedObject so = new SerializedObject(item);
            var soProperties = so.FindProperty("m_Component");
            var components = item.GetComponents<Component>();

            for(int j = components.Length - 1; j >=0; --j)
            {
                if (components[j] == null)
                {
                    soProperties.DeleteArrayElementAtIndex(j);
                }
            }
            so.ApplyModifiedProperties();

            EditorUtility.DisplayProgressBar("Clear Missing Scripts", item.name, i * 1f / pAllObjects.Length);

        }
        EditorUtility.ClearProgressBar();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

}

