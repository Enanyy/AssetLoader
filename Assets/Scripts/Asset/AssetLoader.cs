using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AssetLoader : MonoBehaviour
{
    #region 静态公有方法
    /// <summary>
    /// 60s自动检测已经销毁的资源，并释放对应的Bundle
    /// </summary>
    public static float unloadInterval = 60;

    /// <summary>
    /// key可以是文件名(带后缀)或者文件路径(Assets/...)。
    /// 注意，当非实例化资源（非GameObject，如Material，Texture，TextAsset等）使用完毕后,需要主动调用Destroy去释放资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static AssetLoadTask<T> LoadAsset<T>(string key, Action<Asset<T>> callback) where T : UnityEngine.Object
    {
        AssetLoadTask<T> task = new AssetLoadTask<T>(key.ToLower(), callback);

        LoadAsset(task);

        return task;
    }
    /// <summary>
    /// 自定义LoadTask。当非实例化资源（非GameObject，如Material，Texture，TextAsset等）使用完毕后,需要主动调用Destroy去释放资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    public static void LoadAsset<T>(IAssetLoadTask<T> task) where T : UnityEngine.Object
    {
        if (task == null)
        {
            return;
        }

        Instance.StartCoroutine(Instance.LoadAssetAsync(task));
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="key"></param>
    /// <param name="mode"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static SceneLoadTask LoadScene(string key, LoadSceneMode mode, Action<Scene, LoadSceneMode> callback)
    {
        SceneLoadTask task = new SceneLoadTask(key.ToLower(), mode, callback);
        LoadScene(task);
        return task;
    }
    /// <summary>
    /// 自定义LoadTask
    /// </summary>
    /// <param name="task"></param>
    public static void LoadScene(ISceneLoadTask task)
    {
        if (task == null)
        {
            return;
        }
        Instance.StartCoroutine(Instance.LoadSceneAsync(task));
    }
  

    /// <summary>
    /// 卸载一个Additive场景
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="callback"></param>
    public static void UnLoadScene(Scene scene, Action callback)
    {
        Instance.StartCoroutine(Instance.UnloadSceneAsync(scene, callback));
    }

    public static T GetBundle<T>(string bundleName) where T:Bundle
    {
        Bundle bundle;
        Instance.mAssetBundleDic.TryGetValue(bundleName, out bundle);

        return bundle as T;
    }

    public static T GetOrCreateBundle<T>(string bundleName) where T:Bundle,new ()
    {
        T bundle = GetBundle<T>(bundleName);
       
        if(bundle == null)
        {
            bundle = new T();
            bundle.bundleName = bundleName;
            Instance.mAssetBundleDic.Add(bundleName, bundle);
        }
        return bundle;
    }

    public static void RemoveBundle(Bundle bundle)
    {
        if (bundle == null || string.IsNullOrEmpty(bundle.bundleName))
        {
            return;
        }

        Instance.mAssetBundleDic.Remove(bundle.bundleName);
    }

   
    /// <summary>
    /// 获取直接依赖
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public static string[] GetDirectDependencies(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName) || Instance.mAssetManifest == null || Instance.mAssetManifest.assetObject == null)
        {
            return null;
        }

        return Instance.mAssetManifest.assetObject.GetDirectDependencies(bundleName);
    }

    public static void Unload(string bundleName)
    {
        Bundle bundle;
        if(Instance.mAssetBundleDic.TryGetValue(bundleName, out bundle))
        {
            bundle.Unload();
        }
    }

    public static void Destroy()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            mInstance = null;
        }
    }
    #endregion

    #region 私有方法
    #region Instance
    private static AssetLoader mInstance;
    private static AssetLoader Instance
    {
        get
        {
            if (mInstance == null)
            {
                GameObject go = new GameObject(typeof(AssetLoader).Name);
                mInstance = go.AddComponent<AssetLoader>();
                DontDestroyOnLoad(go);
            }
            return mInstance;
        }
    }
    #endregion
    private Asset<AssetBundleManifest> mAssetManifest;

    private Dictionary<string, Bundle> mAssetBundleDic = new Dictionary<string, Bundle>();

    private LoadStatus mStatus = LoadStatus.None;

    private List<string> mBundleNameList = new List<string>();
    private float mLastUnloadTime;
    private IEnumerator BeginInitialize()
    {
        if (mStatus == LoadStatus.Done)
        {
            yield break;
        }
        else if (mStatus == LoadStatus.Loading)
        {
            yield return new WaitUntil(() => mStatus == LoadStatus.Done);
        }
        else
        {
            mStatus = LoadStatus.Loading;
            //初始化资源列表
            yield return AssetPath.Initialize();

            if (AssetPath.mode == AssetMode.AssetBundle)
            {
                BundleAsset bundle = GetOrCreateBundle<BundleAsset>(AssetPath.list.manifest);

                AssetLoadTask<AssetBundleManifest> task = new AssetLoadTask<AssetBundleManifest>(AssetPath.list.manifest, FinishInitialize);

                task.assetName = "AssetBundleManifest";

                yield return bundle.LoadAssetAsync(task);

            }
            else
            {
                FinishInitialize(null);
            }
        }
    }
    private void FinishInitialize(Asset<AssetBundleManifest> asset)
    {
        mAssetManifest = asset;

        if (mAssetManifest != null && mAssetManifest.assetObject != null)
        {
            DontDestroyOnLoad(mAssetManifest.assetObject);
        }

        mStatus = LoadStatus.Done;

        Debug.Log("Initialize AssetManager Finish!");

    }

    private IEnumerator LoadAssetAsync<T>(IAssetLoadTask<T> task) where T : UnityEngine.Object
    {
        if (mStatus != LoadStatus.Done)
        {
            yield return BeginInitialize();
        }


        BundleAsset bundle = GetOrCreateBundle<BundleAsset>(task.bundleName);

        yield return bundle.LoadAssetAsync(task);
    }

    private IEnumerator LoadSceneAsync(ISceneLoadTask task)
    {
        if (mStatus != LoadStatus.Done)
        {
            yield return BeginInitialize();
        }

        BundleScene bundle = GetOrCreateBundle<BundleScene>(task.assetName);
    
        yield return bundle.LoadSceneAsync(task);
    }

    private IEnumerator UnloadSceneAsync(Scene scene, Action callback)
    {
        var request = SceneManager.UnloadSceneAsync(scene);
        yield return request;
        if (callback != null)
        {
            callback();
        }
    }

    private void Update()
    {
        if (Time.time - mLastUnloadTime > unloadInterval)
        {
            mLastUnloadTime = Time.time;

            mBundleNameList.AddRange(mAssetBundleDic.Keys);

            for (int i = 0; i < mBundleNameList.Count; i++)
            {
                Bundle bundle;
                if (mAssetBundleDic.TryGetValue(mBundleNameList[i], out bundle))
                {
                    var bundleAsset = bundle as BundleAsset;
                    if (bundleAsset != null)
                    {
                        bundleAsset.RemoveReference();
                    }
                }
            }
            mBundleNameList.Clear();
        }
    }



    
    private void OnDestroy()
    {
        mBundleNameList.Clear();
        mBundleNameList.AddRange(mAssetBundleDic.Keys);
        for(int i = 0; i < mBundleNameList.Count; ++i)
        {
            var bundleName = mBundleNameList[i];
            if(mAssetBundleDic.TryGetValue(bundleName,out Bundle bundle))
            {
                bundle.Unload(true);
            }
        }
        mBundleNameList.Clear();
        mAssetBundleDic.Clear();
        mAssetManifest = null;
        mStatus = LoadStatus.None;
    }
    #endregion
}
