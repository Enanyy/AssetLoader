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
    public Dictionary<string, List<AssetObject>> references { get; private set; }
    //已经加载出来的asset
    private Dictionary<string, Object> mAssetDic = new Dictionary<string, Object>();

    public BundleObject(string varAssetbundleName)
    {
        dependences = new Dictionary<string, BundleObject>();
        references = new Dictionary<string, List<AssetObject>>();
        bundleName = varAssetbundleName;
        dependenceNames = AssetManager.Instance.GetAllDependencies(bundleName);
    }

    public void Load(System.Action<BundleObject> callback = null)
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
                        bundle.Load();
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
       
        if (callback != null)
        {
            callback(this);
        }
    }
    public IEnumerator LoadAsync(System.Action<BundleObject> callback = null)
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
        if (callback != null)
        {
            callback(this);
        }
    }

    public IEnumerator LoadWWW(System.Action<BundleObject> callback = null)
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
            if (callback != null)
            {
                callback(this);
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

    public void AddReference(AssetObject reference)
    {
        if(reference== null)
        {
            return;
        }
        string name = reference.assetName;
        if(references.ContainsKey(name)==false)
        {
            references.Add(name, new List<AssetObject>());
        }
        if(references[name].Contains(reference)==false)
        {
            references[name].Add(reference);
        }
    }

    public void RemoveReference(AssetObject reference)
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

