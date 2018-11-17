using UnityEngine;
using System.Collections;

public class AssetEntity 
{
    public AssetBundleEntity assetBundle { get; private set; }
  
    public string assetName { get; private set; }

    public Object assetObject { get; private set; }

    public GameObject gameObject { get; private set; }

    public AssetEntity(AssetBundleEntity assetBundle,Object assetObject, string assetName)
    {
        this.assetName = assetName;
        this.assetBundle = assetBundle;
        this.assetObject = assetObject;

        if(this.assetBundle!=null)
        {
            this.assetBundle.AddReference(this);
        }
    }

 

    public virtual  void Destroy()
    {
        AssetManager.GetSingleton().Destroy(this);
       
    }

}

