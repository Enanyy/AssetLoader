using UnityEngine;
using System.Collections;

public class AssetRecycle : MonoBehaviour
{
	public string mAssetBundleName;
    public string mAssetName;
	public void SetData(string varAssetBundleName, string varAssetName)
	{
		mAssetBundleName = varAssetBundleName;
        mAssetName = varAssetName;
	}


	void OnDestroy()
	{
		
	}

	public void Recycle()
	{
		
	}
}

