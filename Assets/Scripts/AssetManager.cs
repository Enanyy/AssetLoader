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

public enum AssetMode
{
    Editor,
    AssetBundle,
}

public class AssetManager : MonoBehaviour
{

    #region Instance
    private static AssetManager mInstance;
    public static AssetManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                GameObject go = new GameObject(typeof(AssetManager).Name);
                mInstance = go.AddComponent<AssetManager>();
                DontDestroyOnLoad(go);
            }
            return mInstance;
        }
    }
    #endregion

    private AssetBundle mManifestBundle;
    private AssetBundleManifest mManifest;
    private string mAssetBundlePath;
    private Dictionary<string, BundleObject> mAssetBundleDic = new Dictionary<string, BundleObject>();

    public AssetMode assetMode { get; private set; }
    public LoadType loadType { get; private set; }
    public bool initialized { get; private set; }
    public void Init(LoadType type, string manifest)
    {
        loadType = type;

        assetMode = (AssetMode)PlayerPrefs.GetInt("assetMode");

        if(loadType == LoadType.Sync)
        {
            string path = GetPath(manifest);

            var assetBundle = AssetBundle.LoadFromFile(path);

            if (assetBundle)
            {
                OnInitFinish(assetBundle);
            }
            else
            {
                Debug.LogError(manifest + ": Error!!");
            }
        }
        else if(loadType == LoadType.Async)
        {
            StartCoroutine(InitAsync(manifest));
        }
        else if(loadType == LoadType.WWW)
        {
            StartCoroutine(InitWWW(manifest));
        }
    }
    private IEnumerator InitAsync(string manifest)
    {
        string path = GetPath(manifest);

        var request = AssetBundle.LoadFromFileAsync(path);
        yield return request;

        if (request.isDone && request.assetBundle)
        {
            OnInitFinish(request.assetBundle);
        }
        else
        {
            Debug.LogError("Load assetbundle:" + manifest + " failed!!");
        }
    }
    private IEnumerator InitWWW(string manifest)
    {
        string path = GetPath(manifest);

        using (WWW www = new WWW(path))
        {
            yield return www;
            if (www.isDone && www.assetBundle)
            {
                OnInitFinish(www.assetBundle);
            }
            else
            {
                Debug.LogError("Load assetbundle:" + manifest + " failed!!");
            }              
        }
    }
    private void OnInitFinish(AssetBundle assetBundle)
    {
        mManifestBundle = assetBundle;

        mManifest = mManifestBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

        DontDestroyOnLoad(mManifest);

        initialized = true;
    }

    public void Load(string bundleName, Action<BundleObject> callback)
    {
        if (initialized == false)
        {
            return;
        }
        bundleName = bundleName.ToLower();

        BundleObject bundle = CreateBundle(bundleName);

        if (bundle.bundle == null)
        {
            if (loadType == LoadType.Sync)
            {
                bundle.Load(callback);
            }
            else if(loadType== LoadType.Async)
            {
                StartCoroutine(bundle.LoadAsync(callback));
            }
            else if(loadType == LoadType.WWW)
            {
                StartCoroutine(bundle.LoadWWW(callback));
            }
        }
        else
        {
            if (callback != null)
            {
                callback(bundle);
            }
        }
    }

    public BundleObject GetBundle(string bundleName)
    {
        BundleObject bundle = null;

        mAssetBundleDic.TryGetValue(bundleName, out bundle);

        return bundle;
    }

    public BundleObject CreateBundle(string bundleName)
    {
        
        BundleObject bundle = GetBundle(bundleName);

        if(bundle == null)
        {
            bundle = new BundleObject(bundleName);
            mAssetBundleDic.Add(bundleName, bundle);
        }
        return bundle;
    }

    public void RemoveBundle(BundleObject bundle)
    {
        if(bundle == null)
        {
            return;
        }
        string bundleName = bundle.bundleName;
        if(mAssetBundleDic.ContainsKey(bundleName))
        {
            mAssetBundleDic.Remove(bundleName);
        }
    }

    public string GetPath(string bundleName)
    {
        string fullpath = GetRoot() + bundleName;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            fullpath = Uri.EscapeUriString(fullpath);
        }
        return fullpath;
    }

    public string GetRoot()
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

    public string[] GetAllDependencies(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName) || mManifest == null)
        {
            return null;
        }

        return mManifest.GetAllDependencies(bundleName);
    }

    public bool OtherDependence(BundleObject entity ,string bundleName)
    {    
        var it = mAssetBundleDic.GetEnumerator();
        while (it.MoveNext())
        {
            if (it.Current.Value != entity 
                && it.Current.Value.Dependence(bundleName))
            {
                return true;
            }
        }
        it.Dispose();

        return false;
    }

    public void UnLoad(string bundleName)
    {
        BundleObject bundle = GetBundle(bundleName);
        if (bundle != null)
        {
            bundle.UnLoad();
        }
    }

    public void Destroy()
    {
        string[] bundleArray = new string[mAssetBundleDic.Count];
        mAssetBundleDic.Keys.CopyTo(bundleArray, 0);
        for (int i = 0, max = bundleArray.Length; i < max; ++i)
        {
            UnLoad(bundleArray[i]);
        }

        Array.Clear(bundleArray, 0, bundleArray.Length);
        bundleArray = null;

        mAssetBundleDic.Clear();

        if (mManifestBundle)
        {
            mManifestBundle.Unload(true);
        }
        mManifest = null;
    }
}
