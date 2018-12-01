using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum LoadType
{
    Sync,
    Async,
    WWW,
}

public class AssetBundleManager : MonoBehaviour
{

    #region GetSingleton
    private static AssetBundleManager mInstance;
    public static AssetBundleManager GetSingleton()
    {
        if (mInstance == null)
        {
            GameObject go = new GameObject(typeof(AssetBundleManager).Name);
            mInstance = go.AddComponent<AssetBundleManager>();
            DontDestroyOnLoad(go);
        }
        return mInstance;
    }
    #endregion

    private AssetBundle mManifestAssetBundle;
    private AssetBundleManifest mManifest;
    private string mAssetBundlePath;
    private Dictionary<string, AssetBundleEntity> mAssetBundleDic = new Dictionary<string, AssetBundleEntity>();

    public int assetMode { get; private set; }
    public LoadType loadType { get; private set; }
    public bool initialized { get; private set; }
    public void Init(LoadType type, string varAssetBundleManifestName)
    {
        loadType = type;

        assetMode = PlayerPrefs.GetInt("assetmode");

        if(loadType == LoadType.Sync)
        {
            string tmpAssetManifestPath = GetAssetBundlePath(varAssetBundleManifestName);

            var assetBundle = AssetBundle.LoadFromFile(tmpAssetManifestPath);

            if (assetBundle)
            {
                OnInitFinish(assetBundle);
            }
            else
            {
                Debug.LogError(varAssetBundleManifestName + ": Error!!");
            }
        }
        else if(loadType == LoadType.Async)
        {
            StartCoroutine(InitAsync(varAssetBundleManifestName));
        }
        else if(loadType == LoadType.WWW)
        {
            StartCoroutine(InitWWW(varAssetBundleManifestName));
        }
    }
    private IEnumerator InitAsync(string varAssetBundleManifestName)
    {
        string tmpAssetBundlePath = GetAssetBundlePath(varAssetBundleManifestName);

        var request = AssetBundle.LoadFromFileAsync(tmpAssetBundlePath);
        yield return request;

        if (request.isDone && request.assetBundle)
        {
            OnInitFinish(request.assetBundle);
        }
        else
        {
            Debug.LogError("Load assetbundle:" + varAssetBundleManifestName + " failed!!");
        }
    }
    private IEnumerator InitWWW(string varAssetBundleManifestName)
    {
        string tmpAssetBundlePath = GetAssetBundlePath(varAssetBundleManifestName);

        using (WWW www = new WWW(tmpAssetBundlePath))
        {
            yield return www;
            if (www.isDone && www.assetBundle)
            {
                OnInitFinish(www.assetBundle);
            }
            else
            {
                Debug.LogError("Load assetbundle:" + varAssetBundleManifestName + " failed!!");
            }              
        }
    }
    private void OnInitFinish(AssetBundle assetBundle)
    {
        mManifestAssetBundle = assetBundle;

        mManifest = mManifestAssetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

        DontDestroyOnLoad(mManifest);

        initialized = true;
    }

    public void Load(string varAssetBundleName, Action<AssetBundleEntity> varCallback)
    {
        if (initialized == false)
        {
            return;
        }
        varAssetBundleName = varAssetBundleName.ToLower();

        AssetBundleEntity assetBundleEntity = CreateAssetBundleEntity(varAssetBundleName);

        if (assetBundleEntity.assetBundle == null)
        {
            if (loadType == LoadType.Sync)
            {
                assetBundleEntity.Load();
                if (varCallback != null)
                {
                    varCallback(assetBundleEntity);
                }
            }
            else if(loadType== LoadType.Async)
            {
                StartCoroutine(assetBundleEntity.LoadAsync(varCallback));
            }
            else if(loadType == LoadType.WWW)
            {
                StartCoroutine(assetBundleEntity.LoadWWW(varCallback));
            }
        }
        else
        {
            if (varCallback != null)
            {
                varCallback(assetBundleEntity);
            }
        }
    }

    public AssetBundleEntity GetAssetBundleEntity(string varAssetBundleName)
    {
        AssetBundleEntity assetBundleEntity = null;

        mAssetBundleDic.TryGetValue(varAssetBundleName, out assetBundleEntity);

        return assetBundleEntity;
    }

    public AssetBundleEntity CreateAssetBundleEntity(string varAssetBundleName)
    {
        
        AssetBundleEntity assetBundleEntity = GetAssetBundleEntity(varAssetBundleName);

        if(assetBundleEntity == null)
        {
            assetBundleEntity = new AssetBundleEntity(varAssetBundleName);
            mAssetBundleDic.Add(varAssetBundleName, assetBundleEntity);
        }
        return assetBundleEntity;
    }

    public void RemoveAssetBundleEntity(AssetBundleEntity varAssetBundleEntity)
    {
        if(varAssetBundleEntity == null)
        {
            return;
        }
        string assetBundleName = varAssetBundleEntity.assetBundleName;
        if(mAssetBundleDic.ContainsKey(assetBundleName))
        {
            mAssetBundleDic.Remove(assetBundleName);
        }
    }

    public string GetAssetBundlePath(string varAssetBundleName)
    {
        string fullpath = GetAssetBundleRoot() + varAssetBundleName;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            fullpath = Uri.EscapeUriString(fullpath);
        }
        return fullpath;
    }

    public string GetAssetBundleRoot()
    {
        if (string.IsNullOrEmpty(mAssetBundlePath))
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                mAssetBundlePath = Application.streamingAssetsPath + "/";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                mAssetBundlePath = Application.streamingAssetsPath + "/";
            }
            else
            {
                mAssetBundlePath = Application.dataPath + "/StreamingAssets/";
            }
        }
        return mAssetBundlePath;

    }

    public string[] GetAllDependencies(string varAssetBundleName)
    {
        if (string.IsNullOrEmpty(varAssetBundleName) || mManifest == null)
        {
            return null;
        }

        return mManifest.GetAllDependencies(varAssetBundleName);
    }

    public bool OtherDependence(AssetBundleEntity varAssetBundleEntity ,string varAssetBundleName)
    {    
        var it = mAssetBundleDic.GetEnumerator();
        while (it.MoveNext())
        {
            if (it.Current.Value != varAssetBundleEntity 
                && it.Current.Value.Dependence(varAssetBundleName))
            {
                return true;
            }
        }
        it.Dispose();

        return false;
    }

    public void UnLoad(string varAssetBundleName)
    {
        AssetBundleEntity tmpAssetBundleEntity = GetAssetBundleEntity(varAssetBundleName);
        if (tmpAssetBundleEntity != null)
        {
            tmpAssetBundleEntity.UnLoad();
        }
    }

    public void Destroy()
    {
        string[] tmpAssetBundleArray = new string[mAssetBundleDic.Count];
        mAssetBundleDic.Keys.CopyTo(tmpAssetBundleArray, 0);
        for (int i = 0, max = tmpAssetBundleArray.Length; i < max; ++i)
        {
            UnLoad(tmpAssetBundleArray[i]);
        }

        Array.Clear(tmpAssetBundleArray, 0, tmpAssetBundleArray.Length);
        tmpAssetBundleArray = null;

        mAssetBundleDic.Clear();

        if (mManifestAssetBundle)
        {
            mManifestAssetBundle.Unload(true);
        }
        mManifest = null;
    }
}
