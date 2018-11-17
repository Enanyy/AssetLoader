using UnityEngine;
using System.Collections;

public class AssetEntity 
{
    public AssetBundleEntity bundleEntity { get; private set; }
  
    public string assetName { get; private set; }

    public Object assetObject { get; private set; }

    public GameObject gameObject { get; private set; }

    public AssetEntity(AssetBundleEntity assetBundle,Object assetObject, string assetName)
    {
        this.assetName = assetName;
        this.bundleEntity = assetBundle;
        this.assetObject = assetObject;

        if(this.bundleEntity!=null)
        {
            this.bundleEntity.AddReference(this);
        }
    }
    ~AssetEntity()
    {
        Destroy();
    }
 

    public virtual  void Destroy()
    {
        AssetManager.GetSingleton().Destroy(this);
       
    }

}

