using UnityEngine;
using System.Collections;

public class AssetEntity 
{
    public AssetBundleEntity bundleEntity { get; private set; }
  
    public string assetName { get; private set; }

    public Object asset { get; private set; }

    public GameObject gameObject { get; private set; }

    public AssetEntity(AssetBundleEntity assetBundle,Object assetObject, string assetName)
    {
        this.assetName = assetName;
        bundleEntity = assetBundle;
        asset = assetObject;

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
        AssetManager.GetSingleton().Destroy(this);
       
    }

}

