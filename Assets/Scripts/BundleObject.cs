using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BundleObject
{
    public string bundleName { get; private set; }
    public AssetBundle bundle { get; private set; }

    public Dictionary<string, BundleObject> dependences { get; private set; }
    public string[] dependenceNames { get; private set; }

    //场景中实例化出来的,即引用
    public Dictionary<string, List<IAssetObject>> references { get; private set; }
    //已经加载出来的asset
    private Dictionary<string, Object> mAssetDic = new Dictionary<string, Object>();

    public BundleObject(string bundleName,string[] dependenceNames)
    {
        dependences = new Dictionary<string, BundleObject>();
        references = new Dictionary<string, List<IAssetObject>>();
        this.bundleName = bundleName;
        this.dependenceNames = dependenceNames; 
    }

    public void LoadSync(LoadTask<BundleObject> task = null)
    {
        if (dependenceNames != null)
        {
            for (int i =0; i <dependenceNames.Length; ++i)
            {
                string dependenceName = dependenceNames[i];

                if (dependences.ContainsKey(dependenceName) == false)
                {
                    BundleObject bundle = AssetManager.Instance.CreateBundle(dependenceName);

                    dependences[dependenceName] = bundle;

                    if (bundle.bundle == null)
                    {
                        bundle.LoadSync();
                    }
                }
            }
        }
        string path = AssetManager.Instance.GetPath(bundleName);

        bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            Debug.LogError("Load assetbundle:" + bundleName + " failed!!");
        }
       
        if (task!= null && task.callback != null)
        {
            task.callback(this);
        }
    }
    public IEnumerator LoadAsync(LoadTask<BundleObject> task = null)
    {
        if (dependenceNames != null)
        {
            for (int i = 0; i < dependenceNames.Length; ++i)
            {
                string dependenceName = dependenceNames[i];

                if (dependences.ContainsKey(dependenceName) == false)
                {
                    BundleObject bundle = AssetManager.Instance.CreateBundle(dependenceName);

                    dependences[dependenceName] = bundle;

                    if (bundle.bundle == null)
                    {
                        var coroutine =  AssetManager.Instance.StartCoroutine(bundle.LoadAsync());
                        yield return coroutine;
                    }
                }
            }
        }

        string path = AssetManager.Instance.GetPath(bundleName);

        var request = AssetBundle.LoadFromFileAsync(path);
        yield return request;

        if (request.isDone && request.assetBundle)
        {
            bundle = request.assetBundle;
        }
        else
        {
            Debug.LogError("Load assetbundle:" + bundleName + " failed from:" + bundleName + "!!");
        }
        if (task!= null && task.callback != null)
        {
            task.callback(this);
        }
    }

    public IEnumerator LoadWWW(LoadTask<BundleObject> task = null)
    {
        if (dependenceNames != null)
        {
            for (int i = 0; i < dependenceNames.Length; ++i)
            {
                string dependenceName = dependenceNames[i];

                if (dependences.ContainsKey(dependenceName) == false)
                {
                    BundleObject bundle = AssetManager.Instance.CreateBundle(dependenceName);

                    dependences[dependenceName] = bundle;

                    if (bundle.bundle == null)
                    {
                        var coroutine = AssetManager.Instance.StartCoroutine(bundle.LoadWWW());
                        yield return coroutine;
                    }
                }
            }
        }

        string path = AssetManager.Instance.GetPath(bundleName);

        using (WWW www = new WWW(path))
        {
            yield return www;
            if(www.isDone && www.assetBundle)
            {
                bundle = www.assetBundle;
            }
            else
            {
                Debug.LogError("Load assetbundle:" + bundleName + " failed from:" + bundleName + "!!");
            }
            if (task!= null && task.callback != null)
            {
                task.callback(this);
            }
        }
    }

    public Object LoadAsset(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (bundle && mAssetDic.ContainsKey(name) == false)
        {
            mAssetDic[name] = bundle.LoadAsset<Object>(name);
        }
        if (mAssetDic.ContainsKey(name))
        {
            return mAssetDic[name];
        }
        return null;
    }

    public AssetObject<T> LoadAsset<T>(string assetName) where T : UnityEngine.Object
    {
        AssetObject<T> assetObject = null;

        var asset = LoadAsset(assetName);
        if (asset)
        {
            if (typeof(T) == typeof(GameObject))
            {
                var go = UnityEngine.Object.Instantiate(asset) as GameObject;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                assetObject = new AssetObject<T>(assetName, this, asset, go as T);
            }
            else
            {
                assetObject = new AssetObject<T>(assetName, this, asset, asset as T);
            }
        }

        return assetObject;
    }

    public void AddReference(IAssetObject reference)
    {
        if(reference== null)
        {
            return;
        }
        string name = reference.assetName;
        if(references.ContainsKey(name)==false)
        {
            references.Add(name, new List<IAssetObject>());
        }
        if(references[name].Contains(reference)==false)
        {
            references[name].Add(reference);
        }
    }

    public void RemoveReference(IAssetObject reference)
    {
        if (reference == null)
        {
            return;
        }
        string name = reference.assetName;
        if (references.ContainsKey(name))
        {
            references[name].Remove(reference);
        }
        int referenceCount = 0;
        var it = references.GetEnumerator();
        while(it.MoveNext())
        {
            referenceCount += it.Current.Value.Count;
        }
        it.Dispose();
        if(referenceCount==0)
        {
            UnLoad();
        }
    }

    public bool Dependence(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            return false;
        }

        if(dependenceNames!=null)
        {
            for (int i = 0; i < dependenceNames.Length; ++i)
            {
                string dependenceName = dependenceNames[i];
                if(dependenceName == bundleName)
                {
                    return true;
                }
            }
        }

        var it = dependences.GetEnumerator();

        while(it.MoveNext())
        {
            if(it.Current.Value.Dependence(bundleName))
            {
                return true;
            }
        }
        it.Dispose();

        return false;
    }

    public void UnLoad()
    {
        if (AssetManager.Instance.OtherDependence(this, bundleName) == false)
        {
            AssetManager.Instance.RemoveBundle(this);

            if (bundle != null)
            {
                bundle.Unload(true);
                bundle = null;
            }

            var it = dependences.GetEnumerator();
            while(it.MoveNext())
            {
                it.Current.Value.UnLoad();
            }
            it.Dispose();
            dependences.Clear();
        }
    }
}

