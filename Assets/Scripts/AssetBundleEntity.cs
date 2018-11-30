using UnityEngine;
using System.Collections.Generic;


public class AssetBundleEntity
{
    public string assetBundleName { get; private set; }
    public AssetBundle assetBundle { get; private set; }

    public Dictionary<string, AssetBundleEntity> dependences { get; private set; }
    public string[] dependenceNames { get; private set; }

    //场景中实例化出来的,即引用
    public Dictionary<string, List<AssetEntity>> references { get; private set; }
    //已经加载出来的asset
    private Dictionary<string, Object> mAssetDic = new Dictionary<string, Object>();

    public AssetBundleEntity(string varAssetbundleName)
    {
        dependences = new Dictionary<string, AssetBundleEntity>();
        references = new Dictionary<string, List<AssetEntity>>();
        assetBundleName = varAssetbundleName;
        dependenceNames = AssetBundleManager.GetSingleton().GetAllDependencies(assetBundleName);
    }

    public void Load()
    {
        if (dependenceNames != null)
        {
            for (int i =0; i <dependenceNames.Length; ++i)
            {
                string dependenceName = dependenceNames[i].ToLower();

                if (dependences.ContainsKey(dependenceName) == false)
                {
                    AssetBundleEntity assetBundleEntity = AssetBundleManager.GetSingleton().CreateAssetBundleEntity(dependenceName);

                    dependences[dependenceName] = assetBundleEntity;

                    if (assetBundleEntity.assetBundle == null)
                    {
                        assetBundleEntity.Load();
                    }
                }
            }
        }
        string tmpAssetBundlePath = AssetBundleManager.GetSingleton().GetAssetBundlePath() + assetBundleName;

        assetBundle = AssetBundle.LoadFromFile(tmpAssetBundlePath);
    }
   

    public UnityEngine.Object LoadAsset(string varAssetName)
    {
        if(string.IsNullOrEmpty(varAssetName))
        {
            return null;
        }
        varAssetName = varAssetName.ToLower();
        if(mAssetDic.ContainsKey(varAssetName)==false)
        {
            mAssetDic[varAssetName] = assetBundle.LoadAsset<UnityEngine.Object>(varAssetName);
        }

        return mAssetDic[varAssetName];
    }

    public void AddReference(AssetEntity varReference)
    {
        if(varReference== null)
        {
            return;
        }
        string varAssetName = varReference.assetName.ToLower();
        if(references.ContainsKey(varAssetName)==false)
        {
            references.Add(varAssetName, new List<AssetEntity>());
        }
        if(references[varAssetName].Contains(varReference)==false)
        {
            references[varAssetName].Add(varReference);
        }

    }

    public void RemoveReference(AssetEntity varReference)
    {
        if (varReference == null)
        {
            return;
        }
        string varAssetName = varReference.assetName.ToLower();
        if (references.ContainsKey(varAssetName))
        {
            references[varAssetName].Remove(varReference);
        }
        int referenceCount = 0;
        var it = references.GetEnumerator();
        while(it.MoveNext())
        {
            referenceCount += it.Current.Value.Count;
        }
        if(referenceCount==0)
        {
            UnLoad();
        }
    }

    public bool Dependence(string varAssetBundleName)
    {
        if (string.IsNullOrEmpty(varAssetBundleName))
        {
            return false;
        }

        varAssetBundleName = varAssetBundleName.ToLower();
        if(dependenceNames!=null)
        {
            for (int i = 0; i < dependenceNames.Length; ++i)
            {
                string dependenceName = dependenceNames[i].ToLower();
                if(dependenceName == varAssetBundleName)
                {
                    return true;
                }
            }
        }

        var it = dependences.GetEnumerator();

        while(it.MoveNext())
        {
            if(it.Current.Value.Dependence(varAssetBundleName))
            {
                return true;
            }
        }
        
        return false;
    }

    public void UnLoad()
    {
        assetBundleName = null;
        if (assetBundle != null)
        {

            assetBundle.Unload(true);
            assetBundle = null;
        }
        
        //卸载依赖
        for (int i = 0, max = dependenceNames.Length; i < max; ++i)
        {
            string dependenceName = dependenceNames[i].ToLower();
            if (AssetBundleManager.GetSingleton().OtherDependence(this, dependenceName) == false)
            {
                dependences[dependenceName].UnLoad();
            }
        }
        dependences.Clear();
    }
}

