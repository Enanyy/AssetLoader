using UnityEngine;
using System.Collections;

public class AssetReference : MonoBehaviour
{
    public string mAssetBundleName;
    public string mAssetName;
    public void SetData(string varAssetBundleName, string varAssetName)
    {
        mAssetBundleName = varAssetBundleName;
        mAssetName = varAssetName;
    }

    void Start()
    {
        LoadedAssetBundle tmpLoadedAssetBundle = AssetManager.GetSingleton().GetLoadedAssetBundle(mAssetBundleName);
        if (tmpLoadedAssetBundle != null)
        {
            tmpLoadedAssetBundle.AddReference(this);
        }
    }


    void OnDestroy()
    {
        AssetManager.GetSingleton().Destroy(this);
    }
}

