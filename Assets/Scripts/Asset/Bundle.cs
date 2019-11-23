using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class Bundle
{
    public string bundleName { get; set; }
    public AssetBundle bundle { get; private set; }

    public LoadStatus status { get; private set; }
    public Bundle()
    {
        status = LoadStatus.None;
    }

    public virtual IEnumerator LoadAsync()
    {
#if UNITY_EDITOR
        if (AssetPath.mode == AssetMode.Editor)
        {
            yield break;
        }
#endif
        if (status == LoadStatus.Done)
        {
            yield break;
        }
        else if (status == LoadStatus.Loading)
        {
            yield return new WaitUntil(() => status == LoadStatus.Done);
        }
        else
        {
            string path = AssetPath.GetFullPath(bundleName);
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);

            status = LoadStatus.Loading;

            yield return request;

            status = LoadStatus.Done;

            if (bundle == null)
            {
                if (request.isDone && request.assetBundle)
                {
                    bundle = request.assetBundle;
                }
                else
                {
                    Debug.Log("Can't Load AssetBundle:" + bundleName + "  from :" + path + "!!");
                }
            }
        }
    }

    public virtual bool Unload(bool force = false)
    {
        if (force == false)
        {
            Debug.Log("卸载Bundle:" + bundleName);
        }
        AssetLoader.RemoveBundle(this);

        if (bundle != null)
        {
            bundle.Unload(true);
            bundle = null;
        }
        status = LoadStatus.None;

        return true;
    }

}

public class BundleAsset:Bundle
{
    /// <summary>
    /// 依赖哪些资源？
    /// </summary>
    public Dictionary<string, BundleAsset> dependences { get; private set; }
    /// <summary>
    /// 被哪些父级资源依赖
    /// </summary>
    public Dictionary<string, BundleAsset> dependencesby { get; private set; }
    /// <summary>
    /// 场景中实例化出来的,即引用
    /// </summary>
    public Dictionary<string, List<IAsset>> references { get; private set; }
    //已经加载出来的asset
    private Dictionary<string, Object> mAssetDic = new Dictionary<string, Object>();

    //缓存的场景资源
    private Dictionary<string,List<IAsset>> mCacheAssetDic = new Dictionary<string, List<IAsset>>();

    /// <summary>
    /// Resource资源名
    /// </summary>
    private HashSet<string> mResourceAssets = new HashSet<string>();

    private int mAssetCount = 0;

    /// <summary>
    /// 该Bundle所有资源数
    /// </summary>
    public int assetCount
    {
        get
        {
            if (mAssetCount == 0)
            {
                if (bundle != null)
                {
                    mAssetCount = bundle.GetAllAssetNames().Length;
                }
            }
            return mAssetCount;
        }
    }

    private HashSet<ILoadTask> mLoadTasks = new HashSet<ILoadTask>();

    public BundleAsset()
    {
        dependences = new Dictionary<string, BundleAsset>();
        dependencesby = new Dictionary<string, BundleAsset>();
        references = new Dictionary<string, List<IAsset>>();
    }
    public override IEnumerator LoadAsync()
    {
#if UNITY_EDITOR
        if (AssetPath.mode == AssetMode.Editor)
        {
            yield break;
        }
#endif
        string[] dependenceNames = AssetLoader.GetDirectDependencies(bundleName);
        if (dependenceNames != null)
        {
            for (int i = 0; i < dependenceNames.Length; ++i)
            {
                string dependenceName = dependenceNames[i];

                BundleAsset bundleObject;
                if (dependences.TryGetValue(dependenceName, out bundleObject) == false)
                {
                    bundleObject = AssetLoader.GetOrCreateBundle<BundleAsset>(dependenceName);
                    bundleObject.AddDependenceBy(this);

                    dependences.Add(dependenceName, bundleObject);
                }
            }
        }

        var it = dependences.GetEnumerator();
        while (it.MoveNext())
        {
            BundleAsset bundleObject = it.Current.Value;
            if (bundleObject.status == LoadStatus.None)
            {
                yield return bundleObject.LoadAsync();
            }
        }

        yield return base.LoadAsync();
    }

    public IEnumerator LoadAssetAsync<T>(IAssetLoadTask<T> task) where T : UnityEngine.Object
    {
        if (task == null || task.isCancel)
        {
            yield break;
        }

        Asset<T> assetT = null;
        Object asset = null;
      
        string assetName = task.assetName;
        
        if (mCacheAssetDic.TryGetValue(assetName,out List<IAsset> list))
        {
            for (int i = list.Count -1; i>=0; --i)
            {
                if (list[i] == null || list[i].destroyed)
                {
                    list.RemoveAt(i);continue;
                }
                var t = list[i] as Asset<T>;
                if(t != null)
                {
                    list.RemoveAt(i);
                    assetT = t;
                    break;
                }
            }       
        }
        

        if (assetT != null)
        {
            task.OnCompleted(assetT);
        }
        else
        {

#if UNITY_EDITOR
            if (AssetPath.mode == AssetMode.Editor)
            {
                mAssetDic.TryGetValue(assetName, out asset);

                if (asset == null)
                {
                    asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetName);
                    if (asset)
                    {
                        mAssetDic.Add(assetName, asset);
                    }
                }
            }
            else
#endif
            {

                mLoadTasks.Add(task);

                //不是Resources资源

                if (bundle == null)
                {
                    if (status == LoadStatus.None && task.isCancel == false)
                    {
                        yield return LoadAsync();
                    }
                    else
                    {
                        yield return new WaitUntil(() => status == LoadStatus.Done || task.isCancel);
                    }
                }

                if (mAssetDic.ContainsKey(assetName) == false)
                {
                    if (bundle != null && task.isCancel == false)
                    {
                        var request = bundle.LoadAssetAsync(assetName);

                        yield return request;
                        if (request.asset != null && mAssetDic.ContainsKey(assetName) == false)
                        {
                            asset = request.asset;
                            mAssetDic.Add(assetName, asset);
                        }
                    }
                }
            }

            if (mAssetDic.ContainsKey(assetName) == false && task.isCancel == false)
            {
                ///尝试从Resources加载

                string path = AssetPath.GetResourcePath(assetName);

                ResourceRequest request = Resources.LoadAsync(path);

                yield return request;

                if (request.asset != null && mAssetDic.ContainsKey(assetName) == false)
                {
                    asset = request.asset;

                    mAssetDic.Add(assetName, asset);
                    mResourceAssets.Add(assetName);
                }
            }


            if (asset == null)
            {
                mAssetDic.TryGetValue(assetName, out asset);
            }

            if (asset && task.isCancel == false)
            {
                if (typeof(T) == typeof(GameObject))
                {
                    var go = Object.Instantiate(asset) as GameObject;

                    assetT = new Asset<T>(assetName, this, asset, go as T);
                }
                else
                {
                    assetT = new Asset<T>(assetName, this, asset, asset as T);
                }
            }

            mLoadTasks.Remove(task);

            if (task.isCancel == false)
            {
                task.OnCompleted(assetT);
            }
            else
            {
                RemoveReference();
            }
        }
    }

    public void AddReference(IAsset reference)
    {
        if(reference== null)
        {
            return;
        }
        string name = reference.assetName;

        if(references.TryGetValue(name, out List<IAsset> list) ==false)
        {
            list = new List<IAsset>();
            references.Add(name, list);
        }
        if(list.Contains(reference)==false)
        {
            list.Add(reference);
        }
    }

    public void RemoveReference(IAsset reference)
    {
        if (reference == null)
        {
            return;
        }
        string name = reference.assetName;
        List<IAsset> list;
        if (references.TryGetValue(name, out list))
        {
            list.Remove(reference);
        }

        if (mCacheAssetDic.TryGetValue(name, out list))
        {
            list.Remove(reference);
        }

        RemoveReference();
    }
    /// <summary>
    /// 移除已经destroy的资源引用
    /// </summary>
    public void RemoveReference()
    {
        int referenceCount = 0;
        var it = references.GetEnumerator();
        while (it.MoveNext())
        {
            var list = it.Current.Value;
            for (int i = 0; i < list.Count;)
            {
                //移除已经被Destroy的引用
                if (list[i] == null || list[i].destroyed)
                {
                    list.RemoveAt(i); continue;
                }
                referenceCount++;
                ++i;
            }
        }
        //没有引用了
        if (referenceCount == 0)
        {
            int loadtaskCount = 0;
            var loadtask = mLoadTasks.GetEnumerator();
            while (loadtask.MoveNext())
            {
                if (loadtask.Current.isCancel == false)
                {
                    loadtaskCount++;
                }
            }
            //也没有正在加载的请求了
            if (loadtaskCount == 0)
            {
                //尝试卸载
                Unload();
            }
        }
    }
    

    private void AddDependenceBy(BundleAsset parent)
    {
        if(parent == null)
        {
            return;
        }
        if(dependencesby.ContainsKey(parent.bundleName)==false)
        {
            dependencesby.Add(parent.bundleName, parent);
        }
    }

    private void RemoveDependenceBy(BundleAsset parent)
    {
        if (parent == null)
        {
            return;
        }

        dependencesby.Remove(parent.bundleName);

        if (dependencesby.Count == 0)
        {
            //检查是否可以被卸载
            RemoveReference();
        }

    }

    /// <summary>
    /// 该Bundle是否被bundleName的资源依赖
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public bool DependenceBy(string bundleName)
    {
        if(string.IsNullOrEmpty(bundleName))
        {
            return false;
        }

        return dependencesby.ContainsKey(bundleName);
    }
    /// <summary>
    /// 该Bundle是否依赖了bundleName的资源
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public bool Dependence(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            return false;
        }

        var it = dependences.GetEnumerator();

        while(it.MoveNext())
        {
            if (it.Current.Key == bundleName)
            {
                return true;
            }
            else
            {
                if (it.Current.Value.Dependence(bundleName))
                {
                    return true;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// 回收一个资源
    /// </summary>
    /// <param name="assetObject"></param>
    public void ReturnAsset(IAsset assetObject)
    {
        if (assetObject == null)
        {
            return;
        }

        List<IAsset> list;
        if (mCacheAssetDic.TryGetValue(assetObject.assetName, out list) == false)
        {
            list = new List<IAsset>();
            mCacheAssetDic.Add(assetObject.assetName, list);
        }

        if (list.Contains(assetObject) == false)
        {
            list.Add(assetObject);
        }
    }

    public override bool Unload(bool force =false)
    {
        //没有被别的资源依赖，可以卸载
        if (dependencesby.Count <= 0 || force)
        {
            var referenceIter = references.GetEnumerator();
            while (referenceIter.MoveNext())
            {
                var list = referenceIter.Current.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Destroy(false);
                }
                list.Clear();
            }
            references.Clear();

            var dependenceIter = dependences.GetEnumerator();
            while (dependenceIter.MoveNext())
            {
                var dependence = dependenceIter.Current.Value;
                dependence.RemoveDependenceBy(this);
            }
            dependences.Clear();

            var assetIter = mAssetDic.GetEnumerator();
            while (assetIter.MoveNext())
            {
                if (mResourceAssets.Contains(assetIter.Current.Key))
                {
                    Resources.UnloadAsset(assetIter.Current.Value);
                }
            }

            base.Unload(force);

            mAssetDic.Clear();
            mCacheAssetDic.Clear();
            mResourceAssets.Clear();

            return true;
        }
        return false;
    }
}

public class BundleScene : Bundle
{
    public Scene scene { get;  private set; }
    public ISceneLoadTask task { get; private set; }

    public BundleScene()
    {
    }

    public IEnumerator LoadSceneAsync(ISceneLoadTask task)
    {
        if(task== null)
        {
            yield break;
        }
        this.task = task;

        //加载Bundle
        yield return LoadAsync();

        SceneManager.sceneLoaded += OnSceneLoaded;
        //加载场景
        yield return SceneManager.LoadSceneAsync(task.sceneName, task.mode);

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == task.sceneName)
        {
            this.scene = scene;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            if (task.isCancel == false)
            {
                task.OnCompleted(scene, mode);
            }
            else
            {
                AssetLoader.UnLoadScene(scene, null);
            }
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if(this.scene == scene)
        {
            Unload(true);
        }
    }

    public override bool Unload(bool force = false)
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("Unload scene bundle:" + bundleName);

        return base.Unload(force);
    }

}

