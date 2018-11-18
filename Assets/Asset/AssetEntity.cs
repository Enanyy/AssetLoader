using UnityEngine;
using System.Collections;

public class AssetEntity 
{
    public AssetBundleEntity bundleEntity { get; private set; }
  
    public string assetName { get; private set; }

    public Object asset { get; private set; }

    public GameObject gameObject { get; private set; }

    public AssetEntity(AssetBundleEntity varBundleEntity,Object varAsset, string varAssetName)
    {
        assetName = varAssetName;
        bundleEntity = varBundleEntity;
        asset = varAsset;

        if(bundleEntity!=null)
        {
            bundleEntity.AddReference(this);
        }
        gameObject = Object.Instantiate(asset) as GameObject;
    }
    ~AssetEntity()
    {
        //Destroy();
    }
 

    public virtual  void Destroy()
    {
        AssetBundleManager.GetSingleton().Destroy(this);
       
    }

}

