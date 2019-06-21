using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

public class AssetBundleEditor : Editor
{
    static string mOutputPath = Application.dataPath + "/StreamingAssets/";
    static string mAssetPath =  Application.dataPath + "/R/";
    [MenuItem("BuildAssetBundle/BuildAssets")]
    static void BuildAssets()
    {
        if (Directory.Exists(mOutputPath))
        {
            Directory.Delete(mOutputPath, true);
        }

        if (Directory.Exists(mOutputPath) == false)
        {
            Directory.CreateDirectory(mOutputPath);
        }
        //打包资源
        BuildPipeline.BuildAssetBundles(mOutputPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

    }
    [MenuItem("BuildAssetBundle/BuildScene")]
    static void BuildScene()
    {      
        //打包场景
        string[] tmpScenes =
        {
            "Assets/R/Scenes/test.unity",
        };
        if (Directory.Exists(mOutputPath) == false)
        {
            Directory.CreateDirectory(mOutputPath);
        }
        EditorUtility.DisplayProgressBar("BuildScene", "BuildSceneAssetBundle", 0f);
        for (int i = 0; i < tmpScenes.Length; i++)
        {
            string tmpSceneAssetbundlePath = mOutputPath + tmpScenes[i].ToLower().Replace(".unity",".level");
            string tmpSceneAssetbundleDir = Path.GetDirectoryName(tmpSceneAssetbundlePath);
            if (Directory.Exists(tmpSceneAssetbundleDir) == false)
            {
                Directory.CreateDirectory(tmpSceneAssetbundleDir);
            }

            string[] tmpLevels = { tmpScenes[i] };
            BuildPipeline.BuildPlayer(tmpLevels, tmpSceneAssetbundlePath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.BuildAdditionalStreamedScenes);

            EditorUtility.DisplayProgressBar("BuildScene", "BuildSceneAssetBundle", i * 1.0f / tmpScenes.Length);
        }

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/BuildSelectAssetBundleName")]
    static void BuildSelectAssetBundleName()
    {
        EditorUtility.DisplayProgressBar("Clear AssetBundleName", "AssetBundleName", 0f);

        SetAssetBundleName("");

        EditorUtility.ClearProgressBar();


        UnityEngine.Object[] arr = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);

        for (int i = 0; i < arr.Length; ++i)
        {
            string tmpStringFilePath = AssetDatabase.GetAssetPath(arr[i]);

            Debug.Log(tmpStringFilePath);

            AssetImporter tmpAssetImport = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(arr[i]));

            if (tmpAssetImport == null) return;

            tmpAssetImport.assetBundleName = tmpAssetImport.assetPath;
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("BuildAssetBundle/SetAssetBundleName")]
    static void SetAssetBundleName()
    {
        EditorUtility.DisplayProgressBar("SetAssetBundleName", "SetAssetBundleName", 0f);

        SetAssetBundleName(null);

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("BuildAssetBundle/ClearAssetBundleName")]
    static void ClearAssetBundleName()
    {

        string[] tmpFileArray = Directory.GetFiles(Application.dataPath +"/", "*.*", SearchOption.AllDirectories);

        for (int i = 0; i < tmpFileArray.Length; i++)
        {
            string tmpStringFilePath = tmpFileArray[i];

            if (tmpStringFilePath.EndsWith(".cs")
                || tmpStringFilePath.EndsWith(".meta")
                )
            {
                continue;
            }

            tmpStringFilePath = tmpStringFilePath.Replace("\\", "/").Replace(Application.dataPath + "/", "");
            tmpStringFilePath = "Assets/" + tmpStringFilePath;
            Debug.Log(tmpStringFilePath);
            AssetImporter tmpAssetImport = AssetImporter.GetAtPath(tmpStringFilePath);

            if (tmpAssetImport == null) continue;
            tmpAssetImport.assetBundleName = "";

            EditorUtility.DisplayProgressBar("Clear AssetBundleName", "Clear:" + tmpStringFilePath, i / (float)tmpFileArray.Length);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    static void SetAssetBundleName(string assetbundleName)
    {
        //找到目录里所有资源 修改AssetbundleName
        string[] tmpFileArray = Directory.GetFiles(mAssetPath, "*.*", SearchOption.AllDirectories);

        for (int i = 0; i < tmpFileArray.Length; i++)
        {
            string tmpStringFilePath = tmpFileArray[i];

            if (tmpStringFilePath.EndsWith(".cs")
                || tmpStringFilePath.EndsWith(".meta")
                || tmpStringFilePath.EndsWith(".unity")
                || tmpStringFilePath.EndsWith(".js"))
            {
                continue;
            }

            tmpStringFilePath = tmpStringFilePath.Replace("\\", "/").Replace(Application.dataPath + "/", "");
            tmpStringFilePath = "Assets/" + tmpStringFilePath;
            Debug.Log(tmpStringFilePath);
            AssetImporter tmpAssetImport = AssetImporter.GetAtPath(tmpStringFilePath);

            if (tmpAssetImport == null) continue;

            //if (tmpStringFilePath.EndsWith(".prefab")
            //    || tmpStringFilePath.EndsWith(".anim"))
            //{
                UnityEngine.Object go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(tmpAssetImport.assetPath);
                if(go == null)
                {
                    tmpAssetImport.assetBundleName = "";
                    continue;
                }
               
                tmpAssetImport.assetBundleName = string.IsNullOrEmpty(assetbundleName) ? tmpStringFilePath: assetbundleName;
              
            //}
            //else
            //{
            //    tmpAssetImport.assetBundleName = "";
            //}
            EditorUtility.DisplayProgressBar("Set AssetBundleName", "Setting:" + tmpStringFilePath, i / (float)tmpFileArray.Length);
        }
        EditorUtility.ClearProgressBar();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

  

    [MenuItem("BuildAssetBundle/SetEditorMode")]
    static void SetEditorMode()
    {
        PlayerPrefs.SetInt("assetMode", (int)AssetMode.Editor);
    }
    [MenuItem("BuildAssetBundle/SetAssetBundleMode")]
    static void SetAssetBundleMode()
    {
        PlayerPrefs.SetInt("assetMode", (int)AssetMode.AssetBundle);
    }
}
