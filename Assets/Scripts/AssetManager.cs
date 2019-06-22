using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum LoadMode
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


    private List<KeyValuePair<string, Action<BundleObject>>> mLoadTask = new List<KeyValuePair<string, Action<BundleObject>>>();

    private AssetBundle mManifestBundle;
    private AssetBundleManifest mManifest;
    private string mAssetBundlePath;
    private Dictionary<string, BundleObject> mAssetBundleDic = new Dictionary<string, BundleObject>();

    public AssetMode assetMode { get; private set; }
    public LoadMode loadMode { get; private set; }
    public bool initialized { get; private set; }
    public void Init(LoadMode mode, string manifest)
    {
        loadMode = mode;

        assetMode = (AssetMode)PlayerPrefs.GetInt("assetMode");

        if(loadMode == LoadMode.Sync)
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
        else if(loadMode == LoadMode.Async)
        {
            StartCoroutine(InitAsync(manifest));
        }
        else if(loadMode == LoadMode.WWW)
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

        for(int i = 0; i < mLoadTask.Count; ++i)
        {
            Load(mLoadTask[i].Key, mLoadTask[i].Value);
        }
        mLoadTask.Clear();
    }

    public void LoadAsset<T>(string bundleName, string assetName, System.Action<AssetObject<T>> callback = null) where T : UnityEngine.Object
    {
        assetName = assetName.ToLower(); ;

#if UNITY_EDITOR
        if (assetMode == AssetMode.Editor)
        {
            AssetObject<T> assetObject = null;

            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetName);

            if (asset)
            {
                if (typeof(T) == typeof(GameObject))
                {
                    if (callback != null)
                    {
                        var go = UnityEngine.Object.Instantiate(asset) as GameObject;

                        go.transform.localPosition = Vector3.zero;
                        go.transform.localRotation = Quaternion.identity;
                        go.transform.localScale = Vector3.one;

                        assetObject = new AssetObject<T>(assetName, null, asset, go as T);

                        callback(assetObject);
                    }
                }
                else
                {
                    if (callback != null)
                    {
                        assetObject = new AssetObject<T>(assetName, null, asset, null);
                        callback(assetObject);
                    }
                }
            }
            else
            {
                if (callback != null)
                {
                    callback(assetObject);
                }
            }
            return;
        }
#endif

        Load(bundleName, (bundle) =>
        {
            if (bundle != null)
            {
                AssetObject<T> assetObject = bundle.LoadAsset<T>(assetName);

                if (callback != null)
                {
                    callback(assetObject);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback(null);
                }
            }
        });
    }


    public void Load(string bundleName, Action<BundleObject> callback)
    {
        if (initialized == false)
        {
            mLoadTask.Add(new KeyValuePair<string, Action<BundleObject>>(bundleName, callback));

            return;
        }
        bundleName = bundleName.ToLower();

        BundleObject bundle = CreateBundle(bundleName);

        if (bundle.bundle == null)
        {
            if (loadMode == LoadMode.Sync)
            {
                bundle.LoadSync(callback);
            }
            else if(loadMode== LoadMode.Async)
            {
                StartCoroutine(bundle.LoadAsync(callback));
            }
            else if(loadMode == LoadMode.WWW)
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
            bundle = new BundleObject(bundleName, GetAllDependencies(bundleName));
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
