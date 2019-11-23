using UnityEngine;
public interface IAsset
{
    string assetName { get; }
    BundleAsset bundle { get; }
    Object asset { get; }
    /// <summary>
    /// 已经被销毁
    /// </summary>
    bool destroyed { get; }

    void Destroy(bool removeReference = true);

}
/// <summary>
/// 当非实例化资源（非GameObject，如Material，Texture，TextAsset等）使用完毕后,需要主动调用Destroy去释放资源
/// </summary>
/// <typeparam name="T">资源类型</typeparam>
public class Asset<T> : IAsset where T : Object
{
    public BundleAsset bundle { get; private set; }

    public string assetName { get; private set; }

    public Object asset { get; private set; }

    public T assetObject { get; private set; }

    public bool destroyed { get { return assetObject == null; } }

    public Asset(string assetName, BundleAsset bundle, Object asset, T assetObject)
    {
        this.assetName = assetName;
        this.bundle = bundle;
        this.asset = asset;
        this.assetObject = assetObject;

        if (bundle != null)
        {
            bundle.AddReference(this);
        }
    }
    /// <summary>
    /// 主动释放资源
    /// </summary>
    /// <param name="removeReference">是否移除引用</param>
    public virtual void Destroy(bool removeReference = true)
    {
        if (typeof(T) == typeof(GameObject))
        {
            Object.Destroy(assetObject as GameObject);
        }

        if (bundle != null && removeReference)
        {
            bundle.RemoveReference(this);
        }
        asset = null;
        assetObject = null;
    }

    public void ReturnAsset()
    {
        if (bundle != null)
        {
            bundle.ReturnAsset(this);
        }
        else
        {
            Destroy();
        }
    }
  
}

