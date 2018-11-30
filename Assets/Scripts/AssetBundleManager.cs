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
    public void Init(string varAssetManifestName)
    {
        assetMode = PlayerPrefs.GetInt("assetmode");

        string tmpAssetManifestPath = GetAssetBundlePath() + varAssetManifestName;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            tmpAssetManifestPath = Uri.EscapeUriString(tmpAssetManifestPath);
        }

        var assetBundle = AssetBundle.LoadFromFile(tmpAssetManifestPath);

        if (assetBundle)
        {
            mManifestAssetBundle = assetBundle;

            mManifest = mManifestAssetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

            DontDestroyOnLoad(mManifest);
        }
        else
        {
            Debug.LogError(varAssetManifestName + ": Error!!");
        }
    }
    public void Load(string varAssetBundleName, Action<AssetBundleEntity> varCallback)
    {
        varAssetBundleName = varAssetBundleName.ToLower();

        AssetBundleEntity assetBundleEntity = CreateAssetBundleEntity(varAssetBundleName);

        if (assetBundleEntity.assetBundle == null)
        {
            assetBundleEntity.Load();
        }
        if(varCallback!=null)
        {
            varCallback(assetBundleEntity);
        }
    }

    public AssetBundleEntity GetAssetBundleEntity(string varAssetBundleName)
    {
        varAssetBundleName = varAssetBundleName.ToLower();

        AssetBundleEntity assetBundleEntity = null;

        mAssetBundleDic.TryGetValue(varAssetBundleName, out assetBundleEntity);

        return assetBundleEntity;
    }

    public AssetBundleEntity CreateAssetBundleEntity(string varAssetBundleName)
    {
        varAssetBundleName = varAssetBundleName.ToLower();

        AssetBundleEntity assetBundleEntity = GetAssetBundleEntity(varAssetBundleName);

        if(assetBundleEntity == null)
        {
            assetBundleEntity = new AssetBundleEntity(varAssetBundleName);
            mAssetBundleDic.Add(varAssetBundleName, assetBundleEntity);
        }
        return assetBundleEntity;
    }

    public string GetAssetBundlePath()
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
        varAssetBundleName = varAssetBundleName.ToLower();
        var it = mAssetBundleDic.GetEnumerator();
        while (it.MoveNext())
        {
            if (it.Current.Value != varAssetBundleEntity && it.Current.Value.Dependence(varAssetBundleName))
            {
                return true;
            }
        }
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
            AssetBundleEntity tmpBundleEntity = GetAssetBundleEntity(tmpAssetBundleArray[i]);
            if(tmpBundleEntity!=null)
            {
                tmpBundleEntity.UnLoad();
            }
        }

        Array.Clear(tmpAssetBundleArray, 0, tmpAssetBundleArray.Length);
        tmpAssetBundleArray = null;

        mAssetBundleDic.Clear();

        if (mManifestAssetBundle)
        {
            mManifestAssetBundle.Unload(true);
        }
    }
}
