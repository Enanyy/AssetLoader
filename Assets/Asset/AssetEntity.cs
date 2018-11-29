using UnityEngine;
using System.Collections;

public class AssetEntity
{
    public AssetBundleEntity bundleEntity { get; private set; }

    public string assetName { get; private set; }

    public Object asset { get; private set; }

    public GameObject gameObject { get; private set; }

    public AssetEntity()
    {
        gameObject = new GameObject(GetType().Name);
    }

    public void LoadAsset(string varAssetBundleName, string varAssetName,System.Action<GameObject> varCallback=null)
    {
        assetName = varAssetName;
#if UNITY_EDITOR
        if (AssetBundleManager.GetSingleton().assetMode == 0)
        {
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(varAssetName);

            if (asset)
            {
                var go = Object.Instantiate(asset) as GameObject;
                go.transform.SetParent(gameObject.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                OnLoadAsset(go);

                if (varCallback != null)
                {
                    varCallback(go);
                }
            }
            else
            {
                if (varCallback != null)
                {
                    varCallback(null);
                }
            }


            return;
        }
#endif

        AssetBundleManager.GetSingleton().Load(varAssetBundleName, (varAssetBundleEntity) => {

            if(varAssetBundleEntity!=null)
            {
                bundleEntity = varAssetBundleEntity;
                bundleEntity.AddReference(this);
                asset = bundleEntity.LoadAsset(assetName);
                if (asset)
                {
                    var go = Object.Instantiate(asset) as GameObject;
                    go.transform.SetParent(gameObject.transform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

                    OnLoadAsset(go);

                    if (varCallback != null)
                    {
                        varCallback(go);
                    }
                }
                else
                {
                    if (varCallback != null)
                    {
                        varCallback(null);
                    }
                }

            }
            else
            {
                if (varCallback != null)
                {
                    varCallback(null);
                }
            }
        });
    }


    protected virtual void OnLoadAsset(GameObject go)
    {

    }
   

   
    ~AssetEntity()
    {
        //Destroy();
    }


    public virtual void Destroy()
    {
       if(bundleEntity!=null)
        {
            bundleEntity.RemoveReference(this);
        }
        Object.Destroy(gameObject);
    }

}

