using UnityEngine;

public class AssetObject
{
    public BundleObject bundle { get; private set; }

    public string assetName { get; private set; }

    public Object asset { get; private set; }

    public GameObject gameObject { get; private set; }

    public AssetObject()
    {
       
    }

    public void LoadAsset<T>(string bundleName, string assetName, System.Action<T> callback = null) where T:UnityEngine.Object
    {
        this.assetName = assetName.ToLower();
#if UNITY_EDITOR
        if (AssetManager.Instance.assetMode == 0)
        {
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetName);

            if (asset)
            {
                if (typeof(T) == typeof(GameObject))
                {
                    gameObject = Object.Instantiate(asset) as GameObject;

                    gameObject.transform.localPosition = Vector3.zero;
                    gameObject.transform.localRotation = Quaternion.identity;
                    gameObject.transform.localScale = Vector3.one;
                    if (callback != null)
                    {
                        callback(gameObject as T);
                    }
                }
                else
                {
                    if (callback != null)
                    {
                        callback(asset as T);
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


            return;
        }
#endif

        AssetManager.Instance.Load(bundleName, (bundle) =>
        {
            if (bundle != null)
            {
                this.bundle = bundle;
                this.bundle.AddReference(this);
                asset = this.bundle.LoadAsset(this.assetName);
                if (asset)
                {
                    if (typeof(T) == typeof(GameObject))
                    {
                        gameObject = Object.Instantiate(asset) as GameObject;
                        gameObject.transform.localPosition = Vector3.zero;
                        gameObject.transform.localRotation = Quaternion.identity;
                        gameObject.transform.localScale = Vector3.one;
                        if (callback != null)
                        {
                            callback(gameObject as T);
                        }
                    }
                    else
                    {
                        if(callback!= null)
                        {
                            callback(asset as T);
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

