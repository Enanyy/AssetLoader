using UnityEngine;
using System.Collections;

public class AssetRecycle : MonoBehaviour
{
	public string mAssetBundleName;

	public void SetData(string varAssetBundleName)
	{
		mAssetBundleName = varAssetBundleName;
	}


	void OnDestroy()
	{
		
	}

	public void Recycle()
	{
		
	}
}

