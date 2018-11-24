using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public enum LoadStatus
{
    Wait,
    Loading,
    Finish,
    Cancel,
    Error,
}


public class AssetBundleLoadTask
{

    public string assetBundleName { get; private set; }
    public LoadStatus state { get; set; }
    public AssetBundle assetBundle { get; private set; }
    private AssetBundleCreateRequest mRequest;


    /// <summary>
    /// 加载AssetBundle完成需要Load的资源
    /// </summary>
    private List<Action<AssetBundleEntity>> mAssetLoadTaskList = new List<Action<AssetBundleEntity>>();

    public AssetBundleLoadTask()
    {
       
    }

    public AssetBundleLoadTask(string varAssetBundleName)
    {
        assetBundleName = varAssetBundleName;
      
        state = LoadStatus.Wait;
        assetBundle = null;
    }
   

    public void LoadSync()
    {
        state = LoadStatus.Loading;
        string tmpFullPath = AssetBundleManager.GetAssetBundlePath() + assetBundleName;
        if (File.Exists(tmpFullPath))
        {
            assetBundle = AssetBundle.LoadFromFile(tmpFullPath);

            state = LoadStatus.Finish;
        }
        else
        {
            Debug.Log("Can not find file:" + tmpFullPath);
            state = LoadStatus.Error;
        }
    }

    public void LoadAsync()
    {
        string tmpFullPath = AssetBundleManager.GetAssetBundlePath() + assetBundleName;
        if (File.Exists(tmpFullPath))
        {
            mRequest = AssetBundle.LoadFromFileAsync(tmpFullPath);
            state = LoadStatus.Loading;         
        }
        else
        {
            state = LoadStatus.Error;
        }
    }

    public void CheckLoadAsync()
    {
        if(mRequest!=null)
        {
            if(mRequest.isDone)
            {
                assetBundle = mRequest.assetBundle;
                state = LoadStatus.Finish;
            }
        }
        else
        {
            state = LoadStatus.Error;
        }
    }


    public void AddAssetLoadTask(string varAssetName, Action<AssetBundleEntity> varCallback)
    {
        if (varCallback != null)
        {
            mAssetLoadTaskList.Add(varCallback);
        }
    }
    public void LoadFinish(AssetBundleEntity varBundleEntity)
    {
        if(assetBundle==null)
        {
            assetBundle = varBundleEntity.assetBundle;
        }
        for (int i = 0; i < mAssetLoadTaskList.Count; ++i)
        {
            var tmpAssetLoadTask = mAssetLoadTaskList[i];
            if (tmpAssetLoadTask != null)
            {
                tmpAssetLoadTask(varBundleEntity);
            } 
        }
        mAssetLoadTaskList.Clear();
    }

  

}
