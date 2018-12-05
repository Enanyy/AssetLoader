using UnityEngine;
using System.Collections;

public class AssetEntity
{
    public AssetBundleEntity assetBundleEntity { get; private set; }

    public string assetName { get; private set; }

    public Object asset { get; private set; }

    public GameObject gameObject { get; private set; }

    public bool isPool { get; set; }

    public AssetEntity()
    {
        gameObject = new GameObject(GetType().Name);
    }

    public void LoadAsset(string varAssetBundleName, string varAssetName, System.Action<GameObject> varCallback = null)
    {
        assetName = varAssetName.ToLower();
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

        AssetBundleManager.GetSingleton().Load(varAssetBundleName, (varAssetBundleEntity) =>
        {
            if (varAssetBundleEntity != null)
            {
                assetBundleEntity = varAssetBundleEntity;
                assetBundleEntity.AddReference(this);
                asset = assetBundleEntity.LoadAsset(assetName);
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
        
    }
    public virtual void OnCreate()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnRecycle()
    {
        gameObject.SetActive(false);
    }

    public void Recycle()
    {
        AssetEntityPool.GetSingleton().RecycleAssetEntity(this);
    }

    public void Destroy()
    {
        asset = null;
        if (assetBundleEntity != null)
        {
            assetBundleEntity.RemoveReference(this);
        }
        Object.Destroy(gameObject);
    }

}

