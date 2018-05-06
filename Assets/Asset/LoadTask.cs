using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class IPool<T> where T : class,new()
{
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
        return t;
    }
    public static void Recycle(T t)
    {
        if (t != null && mCacheList.Contains(t) == false)
        {
            
            mCacheList.Add(t);
        }
    }

    public virtual void Clear()
    {

    }
}

public class LoadAssetTask:IPool<LoadAssetTask>
{
  
    public string mAssetName;
    public Action<UnityEngine.Object> mCallback;

    public LoadAssetTask() { }
    public void Init(string varAssetName, Action<UnityEngine.Object> varCallback)
    {
        mAssetName = varAssetName;
        mCallback = varCallback;
    }
    public override void Clear()
    {
        mAssetName = null;
        mCallback = null;
    }
}

public class LoadTask:IPool<LoadTask>
{
    public enum LoadTaskState
    {
        UnLoad,
        Loading,
        Loaded,
        Cancel,
    }

    public string mAssetBundleName;
    
    public LoadTaskState mState;
    public AssetBundle mAssetBundle;

    /// <summary>
    /// 加载AssetBundle完成需要Load的资源
    /// </summary>
    private List<LoadAssetTask> mLoadAssetTaskList = new List<LoadAssetTask>();

    public LoadTask()
    {
       
    }

    public void Init(string varAssetBundleName)
    {
        mAssetBundleName = varAssetBundleName;
      
        mState = LoadTaskState.UnLoad;
        mAssetBundle = null;
    }
   

    public void Load()
    {
        mState = LoadTaskState.Loading;
        string tmpFullPath = AssetManager.GetAssetBundlePath() + mAssetBundleName;
        if (File.Exists(tmpFullPath))
        {

            mAssetBundle = AssetBundle.LoadFromFile(tmpFullPath);

            mState = LoadTaskState.Loaded;

        }
        else
        {
            Debug.Log("Can not find file:" + tmpFullPath);
            mState = LoadTaskState.Loaded;
        }
    }

    public IEnumerator LoadAsync()
    {
        string tmpFullPath = AssetManager.GetAssetBundlePath() + mAssetBundleName;
        if (File.Exists(tmpFullPath))
        {
            AssetBundleCreateRequest tmpRequest = AssetBundle.LoadFromFileAsync(tmpFullPath);
            mState = LoadTaskState.Loading;
            yield return tmpRequest;

            if (tmpRequest.isDone)
            {

                mAssetBundle = tmpRequest.assetBundle;
            }

            mState = LoadTaskState.Loaded;
        }
        else
        {
            mState = LoadTaskState.Loaded;
        }
    }

    public void AddLoadAssetTask(string varAssetName, Action<UnityEngine.Object> varCallback)
    {
        LoadAssetTask tmpLoadAssetTask = LoadAssetTask.Create();
        tmpLoadAssetTask.Init(varAssetName, varCallback);
        mLoadAssetTaskList.Add(tmpLoadAssetTask);
    }
    public void OnLoadFinish()
    {
        for (int i = 0; i < mLoadAssetTaskList.Count; ++i)
        {
            LoadAssetTask tmpLoadAssetTask = mLoadAssetTaskList[i];
            if (tmpLoadAssetTask.mCallback != null)
            {
                UnityEngine.Object tmpObject = mAssetBundle.LoadAsset(tmpLoadAssetTask.mAssetName);
                tmpLoadAssetTask.mCallback(tmpObject);
            }
            tmpLoadAssetTask.Clear();
            LoadAssetTask.Recycle(tmpLoadAssetTask);
        }
        mLoadAssetTaskList.Clear();
    }

    public override void Clear()
    {
        mAssetBundleName = null;
        mAssetBundle = null;
        mState = LoadTaskState.UnLoad;
    }

}
