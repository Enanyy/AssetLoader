using System;
using System.Collections;
using System.Collections.Generic;
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
        string tmpFullPath = AssetBundleManager.GetSingleton().GetAssetBundlePath() + assetBundleName;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            tmpFullPath = Uri.EscapeUriString(tmpFullPath);
        }

        state = LoadStatus.Loading;

        assetBundle = AssetBundle.LoadFromFile(tmpFullPath);
        if(assetBundle)
        {
            state = LoadStatus.Finish;
        }
        else
        {
            Debug.LogError("Load assetbundle " +assetBundleName + " error!!");
            state = LoadStatus.Error;
        }
    }

    public IEnumerator LoadAsync()
    {
        string tmpFullPath = AssetBundleManager.GetSingleton().GetAssetBundlePath() + assetBundleName;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            tmpFullPath = Uri.EscapeUriString(tmpFullPath);
        }

        state = LoadStatus.Loading;
        var request = AssetBundle.LoadFromFileAsync(tmpFullPath);
        yield return request;

        if(request.isDone && request.assetBundle)
        {
            assetBundle = request.assetBundle;
            state = LoadStatus.Finish;
        }
        else
        {
            state = LoadStatus.Error;
        }

    }

    public IEnumerator LoadWWW()
    {
        string tmpFullPath = AssetBundleManager.GetSingleton().GetAssetBundlePath() + assetBundleName;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            tmpFullPath = Uri.EscapeUriString(tmpFullPath);
        }

        state = LoadStatus.Loading;
        using (WWW www = new WWW(tmpFullPath))
        {
            yield return www;

            if (www.isDone && www.assetBundle)
            {
                assetBundle = www.assetBundle;
                state = LoadStatus.Finish;
            }
            else
            {
                Debug.LogError(assetBundleName + ":" + www.error);
                state = LoadStatus.Error;
            }
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
