using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetEntityPool  {

    private AssetEntityPool() { }

    private Dictionary<string, Queue<AssetEntity>> mAssetDic = new Dictionary<string, Queue<AssetEntity>>();

    private static AssetEntityPool mInstance;
    public static AssetEntityPool GetSingleton()
    {
        if(mInstance == null)
        {
            mInstance = new AssetEntityPool();
        }
        return mInstance;
    }

    public AssetEntity CreateAssetEntity(string varAssetBundleName,string varAssetName)
    {
        string name = string.Format("{0}+{1}", varAssetBundleName.ToLower(), varAssetName.ToLower());
        AssetEntity entity = null;
        if(mAssetDic.ContainsKey(name))
        {
            if(mAssetDic[name].Count >0)
            {
                entity = mAssetDic[name].Dequeue();
            }
        }

        if(entity == null)
        {
            entity = new AssetEntity();
            entity.LoadAsset(varAssetBundleName, varAssetName);
        }
        entity.isPool = false;
        entity.OnCreate();
        return entity;
    }

    public void RecycleAssetEntity(AssetEntity varAssetEntity)
    {
        if(varAssetEntity==null||varAssetEntity.assetBundleEntity == null)
        {
            return;
        }

        string name = string.Format("{0}+{1}", varAssetEntity.assetBundleEntity.assetBundleName.ToLower(), varAssetEntity.assetName.ToLower());

        if(mAssetDic.ContainsKey(name)==false)
        {
            mAssetDic.Add(name, new Queue<AssetEntity>());
        }
        if(mAssetDic[name].Contains(varAssetEntity)==false)
        {
            mAssetDic[name].Enqueue(varAssetEntity);
        }

        varAssetEntity.isPool = true;

        varAssetEntity.OnRecycle();
    }

    public void Clear(string varAssetBundleName, string varAssetName)
    {
        string name = string.Format("{0}+{1}", varAssetBundleName.ToLower(), varAssetName.ToLower());

        if(mAssetDic.ContainsKey(name))
        {
            var queue = mAssetDic[name];
            while(queue.Count>0)
            {
                var assetEntity = queue.Dequeue();
                assetEntity.Destroy();
            }
            mAssetDic.Remove(name);
        }

    }

    public void Clear()
    {
        var it = mAssetDic.GetEnumerator();
        while(it.MoveNext())
        {
            var queue = it.Current.Value;
            while (queue.Count > 0)
            {
                var assetEntity = queue.Dequeue();
                assetEntity.Destroy();
            }
        }
        mAssetDic.Clear();
    }
}
