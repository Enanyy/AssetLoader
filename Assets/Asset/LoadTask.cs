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
    public System.Action<AssetBundle> mCallback;
    public LoadTaskState mState;
    public AssetBundle mAssetBundle;

    public LoadTask()
    {
       
    }

    public void Init(string varAssetBundleName, Action<AssetBundle> varCallback)
    {
        mAssetBundleName = varAssetBundleName;
        mCallback = varCallback;
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

    public void Clear()
    {
        mAssetBundleName = null;
        mAssetBundle = null;
        mCallback = null;
        mState = LoadTaskState.UnLoad;
    }

}
