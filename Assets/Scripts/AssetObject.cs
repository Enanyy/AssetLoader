using UnityEngine;
public interface IAssetObject
{
    string assetName { get; }
    BundleObject bundle { get; }
    Object asset { get; }
    void Destroy();
}
public class AssetObject<T>: IAssetObject
{
    public BundleObject bundle { get; private set; }

    public string assetName { get; private set; }

    public Object asset { get; private set; }

    public T assetObject { get; private set; }

    public AssetObject(string assetName, BundleObject bundle, Object asset, T assetObject)
    {
        this.assetName = assetName;
        this.bundle = bundle;
        this.asset = asset;
        this.assetObject = assetObject;

        if(bundle!= null)
        {
            bundle.AddReference(this);
        }
    }
    public virtual void Destroy()
    {
        asset = null;
        if (bundle != null)
        {
            bundle.RemoveReference(this);
        }
        if (typeof(T) == typeof(GameObject))
        {
            Object.Destroy(assetObject as GameObject);
        }
    }

}

