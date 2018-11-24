using UnityEngine;
using System.Collections;

public class AssetEntity
{
    public AssetBundleEntity bundleEntity { get; private set; }

    public string assetName { get; private set; }

    public Object asset { get; private set; }

    public GameObject gameObject { get; private set; }

    public AssetEntity(AssetBundleEntity varBundleEntity, string varAssetName)
    {
        assetName = varAssetName;
        bundleEntity = varBundleEntity;

        if (bundleEntity != null)
        {
            asset = bundleEntity.LoadAsset(assetName);
            bundleEntity.AddReference(this);
            gameObject = Object.Instantiate(asset) as GameObject;
        }
        else
        {
            Debug.LogError("assetBundle is null!!");
        }

    }

    public AssetEntity(Object varAsset, string varAssetName)
    {
        asset = varAsset;
        assetName = varAssetName;
        gameObject = Object.Instantiate(asset) as GameObject;
    }

   
    ~AssetEntity()
    {
        Destroy();
    }


    public virtual void Destroy()
    {
        AssetBundleManager.GetSingleton().Destroy(this);
        Object.Destroy(gameObject);
    }

}

