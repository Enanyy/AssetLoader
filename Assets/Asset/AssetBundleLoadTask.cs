using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class IPool<T> where T : IPool<T>,new()
{
    public bool isPool { get; private set; }
    private static List<T> mCacheList;
    public static T Create()
    {
        if (mCacheList == null)
        {
            mCacheList = new List<T>();
        }

        T t;
        if (mCacheList.Count > 0)
        {
            t = mCacheList[0];
            mCacheList.RemoveAt(0);
        }
        else
        {
            t = new T();
        }
        t.isPool = false;
        return t;
    }
    public static void Recycle(T t)
    {
        if (t != null && mCacheList.Contains(t) == false)
        {
            t.isPool = true;
            mCacheList.Add(t);
        }
    }

    public virtual void Clear()
    {

    }
}

public enum LoadStatus
{
    UnLoad,
    Loading,
    Loaded,
    Cancel,
}
public class AssetLoadTask
{
  
    public string assetName { get; private set; }
    public Action<AssetEntity> callback { get; private set; }

    public AssetLoadTask() { }
    public AssetLoadTask(string varAssetName, Action<AssetEntity> varCallback)
    {
        assetName = varAssetName;
        callback = varCallback;
    }
    public void Clear()
    {
        assetName = null;
        callback = null;
    }
}

public class AssetBundleLoadTask
{
    

    public string assetBundleName { get; private set; }
    public LoadStatus state { get; set; }
    public AssetBundle assetBundle { get; private set; }

    /// <summary>
    /// 加载AssetBundle完成需要Load的资源
    /// </summary>
    private List<AssetLoadTask> mAssetLoadTaskList = new List<AssetLoadTask>();

    public AssetBundleLoadTask()
    {
       
    }

    public AssetBundleLoadTask(string varAssetBundleName)
    {
        assetBundleName = varAssetBundleName;
      
        state = LoadStatus.UnLoad;
        assetBundle = null;
    }
   

    public void Load()
    {
        state = LoadStatus.Loading;
        string tmpFullPath = AssetManager.GetAssetBundlePath() + assetBundleName;
        if (File.Exists(tmpFullPath))
        {

            assetBundle = AssetBundle.LoadFromFile(tmpFullPath);

            state = LoadStatus.Loaded;

        }
        else
        {
            Debug.Log("Can not find file:" + tmpFullPath);
            state = LoadStatus.Loaded;
        }
    }

    public IEnumerator LoadAsync()
    {
        string tmpFullPath = AssetManager.GetAssetBundlePath() + assetBundleName;
        if (File.Exists(tmpFullPath))
        {
            AssetBundleCreateRequest tmpRequest = AssetBundle.LoadFromFileAsync(tmpFullPath);
            state = LoadStatus.Loading;
            yield return tmpRequest;

            if (tmpRequest.isDone)
            {

                assetBundle = tmpRequest.assetBundle;
            }

            state = LoadStatus.Loaded;
        }
        else
        {
            state = LoadStatus.Loaded;
        }
    }

    public void AddAssetLoadTask(string varAssetName, Action<AssetEntity> varCallback)
    {
        AssetLoadTask tmpLoadAssetTask = new AssetLoadTask(varAssetName, varCallback);

        mAssetLoadTaskList.Add(tmpLoadAssetTask);

    }
    public void LoadFinish(AssetBundleEntity bundleEntity)
    {
        if(assetBundle==null)
        {
            assetBundle = bundleEntity.assetBundle;
        }
        for (int i = 0; i < mAssetLoadTaskList.Count; ++i)
        {
            AssetLoadTask tmpAssetLoadTask = mAssetLoadTaskList[i];
            if (tmpAssetLoadTask.callback != null)
            {
                UnityEngine.Object tmpObject  = bundleEntity.LoadAsset(tmpAssetLoadTask.assetName);
                AssetEntity asset = new AssetEntity(bundleEntity, tmpObject, tmpAssetLoadTask.assetName);

                tmpAssetLoadTask.callback(asset);
            }
            tmpAssetLoadTask.Clear();
           
        }
        mAssetLoadTaskList.Clear();
    }

  

}
