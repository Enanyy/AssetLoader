using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public static class AssetTool
{
    private static AssetList list = new AssetList();
    /// <summary>
    /// 指定目录可以选中(小写字符串)
    /// </summary>
    static List<string> s_AssetDirs = new List<string>
    {
        { "assets/resources/"},
        { "assets/r/" },
    };

    private static string ASSETSFILE = Application.dataPath + "/" + AssetPath.ASSETSFILE;

    [InitializeOnLoadMethod]
    private static void InitEditor()
    {
        Editor.finishedDefaultHeaderGUI -= OnPostHeaderGUI;
        Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        EditorApplication.quitting -= SaveAssetList;
        EditorApplication.quitting += SaveAssetList;
  
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        if (EditorApplication.isPlayingOrWillChangePlaymode==false)
        {
            LoadAssetList();
        }
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            LoadAssetList();

            UpdateBuildSettingScene(false);

        }
        else if(state == PlayModeStateChange.ExitingEditMode)
        {
            SaveAssetList();

            UpdateBuildSettingScene(true);
        }
    }

    static void LoadAssetList()
    {
        if (File.Exists(ASSETSFILE))
        {
            string xml = File.ReadAllText(ASSETSFILE);
            list.FromXml(xml);
        }
        else
        {
            SaveAssetList();
        }
    }

    static void UpdateBuildSettingScene(bool add)
    {
       List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
       var it = list.assets.GetEnumerator();
       while (it.MoveNext())
       {
           string path = it.Current.Value.asset;
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
  
    

    static bool IsValid(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)
            || assetPath.EndsWith(".cs"))
        {
            return false;
        }
        if (s_AssetDirs.Count > 0)
        {
            var it = s_AssetDirs.GetEnumerator();
            while (it.MoveNext())
            {
                if (assetPath.StartsWith(it.Current))
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return true;
        }
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

        var asset = GetAssetFile(editor.target);

        GUILayout.BeginHorizontal();

        bool isSelectAsset = asset != null && list.Contains(asset.name);
        bool isSelectMixed = IsMixedSelect(editor.targets);

        if (isSelectMixed)
        {
            isSelectAsset = false;
        }
        

        bool isSelectGroup;
        if (isSelectMixed)
        {
            isSelectGroup = GUILayout.Toggle(isSelectAsset, "Asset", s_ToggleMixed, GUILayout.ExpandWidth(false));
        }
        else
        {
            isSelectGroup = GUILayout.Toggle(isSelectAsset, "Asset", GUILayout.ExpandWidth(false));
        }
        if (isSelectAsset && isSelectGroup == false)
        {
            RemoveFromList(editor.targets);
            asset = null;
        }
        else if (isSelectAsset == false && isSelectGroup)
        {
            AddToList(editor.targets);
        }

        if (asset != null && isSelectAsset && editor.targets.Length == 1)
        {

            asset.asset = assetPath;

            var assetName = EditorGUILayout.DelayedTextField(asset.name, GUILayout.ExpandWidth(true));
            if (assetName != asset.name)
            {
                list.Remove(asset);
                asset.name = assetName.ToLower();
                list.Add(asset);
                SaveAssetList();
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (asset != null && list.Contains(asset.name))
        {
            isSelectAsset = string.IsNullOrEmpty(asset.bundle) == false;
            isSelectMixed = IsMixedGroup(editor.targets);
            if (isSelectMixed)
            {
                isSelectGroup = GUILayout.Toggle(isSelectAsset, "Bundle", s_ToggleMixed, GUILayout.ExpandWidth(false));
            }
            else
            {
                isSelectGroup = GUILayout.Toggle(isSelectAsset, "Bundle", GUILayout.ExpandWidth(false));
            }
            if (isSelectGroup == false && isSelectAsset)
            {
                SetAssetsBundle(editor.targets, null);
                SaveAssetList();
            }
            else
            {
                if (isSelectGroup)
                {
                    if (isSelectAsset == false)
                    {
                        string dir = Path.GetDirectoryName(asset.asset);
                        asset.bundle = dir.Substring(dir.LastIndexOf("\\") + 1);
                    }

                    var group = EditorGUILayout.DelayedTextField(asset.bundle, GUILayout.ExpandWidth(true));
                    if ( isSelectAsset == false ||  group != asset.bundle)
                    {
                        //Debug.Log(check + "," + select + "," + asset.group + "," + group);
                        SetAssetsBundle(editor.targets, group);
                        SaveAssetList();
                    }
                }

            }
        }

        GUILayout.EndHorizontal();

    }

    static void AddToList(Object[] targets)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var asset = GetAssetFile(targets[i]);
            if (asset != null)
            {
                list.Add(asset);
            }
        }
    }

    static void RemoveFromList(Object[] targets)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var asset = GetAssetFile(targets[i]);
            if (asset != null)
            {
                list.Remove(asset);
            }
        }
    }

    static void SetAssetsBundle(Object[] targets, string bundle)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var asset = GetAssetFile(targets[i]);
            if (asset != null)
            {
                asset.bundle = bundle;
            }
        }
    }


    static bool IsMixedSelect(Object[] targets)
    {
        bool firstSelected = false;
        bool firstSet = false;
        
        for (int i = 0; i < targets.Length; ++i)
        {
            var asset = GetAssetFile(targets[i]);
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
        string firstGroup = null;
        bool firstSet = false;
        for(int i = 0; i < targets.Length; ++i)
        {
            var asset = GetAssetFile(targets[i]);
            if (asset != null)
            {
                if (firstSet == false)
                {
                    if (string.IsNullOrEmpty(asset.bundle) == false)
                    {
                        firstGroup = asset.bundle;
                    }
                    firstSet = true;
                }
                else
                {
                    if(!string.IsNullOrEmpty(firstGroup) || !string.IsNullOrEmpty(asset.bundle))
                    {
                        if (firstGroup != asset.bundle)
                        {
                            return true;
                        }
                    } 
                }
            }
        }
        return false;
    }

    static AssetFile GetAssetFile(Object target)
    {
        string assetPath = AssetDatabase.GetAssetPath(target).ToLower();

        if (IsValid(assetPath) == false)
        {
            return null;
        }
        string name = Path.GetFileName(assetPath);
        //Editor模式下是InstanceID
        string md5 = target.GetInstanceID().ToString();
        AssetFile asset = list.Get(name);
        if (asset == null)
        {
            asset = list.Get(md5);
        }
        if(asset!= null)
        {
            asset.asset = assetPath;
        }

        if (asset == null)
        {
            asset = new AssetFile();
            asset.name = name;
            asset.asset = assetPath;
            asset.md5 = md5;
            asset.size = 0;
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
       
        string[] files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; ++i)
        {
            string file = files[i].Replace("\\","/").ToLower();
            if (file.EndsWith(".meta")
            || file.EndsWith(".manifest"))
            {
                continue;
            }
          
            string name = Path.GetFileName(file);
            string assetPath = name;
           
            if (file.Contains("assets/"))
            {
                assetPath = file.Substring(file.IndexOf("assets/"));
            }
            Object target = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            string md5 = target.GetInstanceID().ToString();

            AssetFile asset = list.Get(name);
            if (asset == null) asset = list.Get(assetPath);
            if (asset == null) asset = list.Get(md5);
            if (asset == null)
            {
                asset = new AssetFile();
                asset.name = name;
                asset.asset = assetPath;
            }

            asset.size = 0;

            asset.md5 = md5;

            list.Add(asset);

        }
    }

    [MenuItem("Tools/Asset/保存资源配置")]
    static void SaveAssetList()
    {
        StreamWriter writerXml = new StreamWriter(ASSETSFILE);
        writerXml.Write(list.ToXml());
        writerXml.Close();
        writerXml.Dispose();
    }


    [MenuItem("Tools/Asset/Clear All AssetBundleName")]
    static void ClearAllAssetBundleName()
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
      
        var it = list.assets.GetEnumerator();
        int count = 0;
        while (it.MoveNext())
        {
            var file = it.Current.Value;
            if (AssetDatabase.LoadAssetAtPath<Object>(file.asset))
            {
                //Resources目录设置了bundle才打bundle
                if (file.asset.Contains("resources/") == false || string.IsNullOrEmpty(file.bundle) == false)
                {
                    if (file.asset.EndsWith(".unity"))
                    {
                        scenes.Add(file.asset);                       
                    }
                    else
                    {
                        AssetImporter importer = AssetImporter.GetAtPath(file.asset);
                        if (importer != null)
                        {
                            importer.assetBundleName = string.IsNullOrEmpty(file.bundle)
                                ? file.asset
                                : file.bundle; 
                        }
                    }
                }              
            }
            else
            {
                removes.Add(file);
            }

            EditorUtility.DisplayProgressBar("Set AssetbundleName", "Setting:"+ file.asset, count * 1f/ list.assets.Count);
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

        for(int i = 0; i< removes.Count;++i)
        {
            list.Remove(removes[i]);
        }
        SaveAssetList();

        SaveBuildList(outputPath);

        Debug.Log("Build Success! Output:" + outputPath);
    }

    static void SaveBuildList(string outputPath)
    {
        AssetList buildList = new AssetList();
        HashSet<string> bundeList = new HashSet<string>();
        buildList.manifest = EditorUserBuildSettings.activeBuildTarget.ToString();
        bundeList.Add(buildList.manifest);

        foreach(var file in list.assets.Values)
        {
         
            if (file.asset.Contains("resources/") == false || string.IsNullOrEmpty(file.bundle)==false)
            {
                if (buildList.Contains(file.name) == false && buildList.assets.ContainsValue(file) == false)
                {
                    if (string.IsNullOrEmpty(file.bundle))
                    {
                        string fullPath = string.Format("{0}{1}", outputPath, file.asset);
                        byte[] bytes = File.ReadAllBytes(fullPath);
                        file.size = bytes.Length;
                        file.md5 = MD5Hash.Get(bytes);
                    }
                    else
                    {
                        file.size = 0;
                        file.md5 = null;
                        bundeList.Add(file.bundle);   
                    }
                    buildList.assets.Add(file.name, file);
                }         
            }
            else
            {
                if (buildList.Contains(file.name) == false && buildList.assets.ContainsValue(file) == false)
                {
                    string fullPath = string.Format("{0}{1}",Application.dataPath.Replace("Assets",""),file.asset);
                    byte[] bytes = File.ReadAllBytes(fullPath);
                    file.size = bytes.Length;
                    file.md5 = MD5Hash.Get(bytes);
                    buildList.assets.Add(file.name, file);
                }
            }
        }
        foreach(var name in bundeList)
        {
            string fullPath = string.Format("{0}{1}", outputPath, name);
            byte[] bytes = File.ReadAllBytes(fullPath);
            AssetFile file = new AssetFile();
            file.name = name;
            file.bundle = name;
            file.size = bytes.Length;
            file.md5 = MD5Hash.Get(bytes);
            buildList.assets.Add(file.name, file);
        }
       
        string assetXmlFile = outputPath + "/" + AssetPath.ASSETSFILE;

        StreamWriter writerXml = new StreamWriter(assetXmlFile);
        writerXml.Write(buildList.ToXml());
        writerXml.Close();
        writerXml.Dispose();
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

