using UnityEngine;
using System.Collections;

public class AssetObject
{
    public BundleObject bundle { get; private set; }

    public string assetName { get; private set; }

    public Object asset { get; private set; }

    public GameObject gameObject { get; private set; }

    public AssetObject()
    {
        gameObject = new GameObject(GetType().Name);
    }

    public void LoadAsset(string bundleName, string assetName, System.Action<GameObject> callback = null)
    {
        this.assetName = assetName.ToLower();
#if UNITY_EDITOR
        if (AssetManager.Instance.assetMode == 0)
        {
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetName);

            if (asset)
            {
                var go = Object.Instantiate(asset) as GameObject;
                go.transform.SetParent(gameObject.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                OnLoadAsset(go);

                if (callback != null)
                {
                    callback(go);
                }
            }
            else
            {
                if (callback != null)
                {
                    callback(null);
                }
            }


            return;
        }
#endif

        AssetManager.Instance.Load(bundleName, (entity) =>
        {

            if (entity != null)
            {
                bundle = entity;
                bundle.AddReference(this);
                asset = bundle.LoadAsset(this.assetName);
                if (asset)
                {
                    var go = Object.Instantiate(asset) as GameObject;
                    go.transform.SetParent(gameObject.transform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

                    OnLoadAsset(go);

                    if (callback != null)
                    {
                        callback(go);
                    }
                }
                else
                {
                    if (callback != null)
                    {
                        callback(null);
                    }
                }

            }
            else
            {
                if (callback != null)
                {
                    callback(null);
                }
            }
        });
    }


    protected virtual void OnLoadAsset(GameObject go)
    {

    }



    ~AssetObject()
    {
        //Destroy();
    }


    public virtual void Destroy()
    {
        asset = null;
        if (bundle != null)
        {
            bundle.RemoveReference(this);
        }
        Object.Destroy(gameObject);
    }

}

