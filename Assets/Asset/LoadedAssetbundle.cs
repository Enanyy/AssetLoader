using UnityEngine;
using System.Collections.Generic;


public class LoadedAssetBundle
{
	AssetBundleManifest mManifest;

    public string mAssetBundleName;
    public AssetBundle mAssetbundle;

  
    public List<LoadedAssetBundle> mDependenceList = new List<LoadedAssetBundle> ();

	public LoadedAssetBundle(AssetBundleManifest varManifest,string varAssetBundleName,AssetBundle varAssetbundle)
	{
		mAssetBundleName = varAssetBundleName;
		mAssetbundle =varAssetbundle;
		mManifest = varManifest;
      
		AddDependence ();
	}

	



	private void AddDependence()
	{
		if (mManifest != null && mAssetbundle != null) {
		
			string[] tmpDependencesArray = mManifest.GetAllDependencies (mAssetBundleName);

			for (int i = 0,max = tmpDependencesArray.Length; i < max; ++i) {

				string tmpDependence = tmpDependencesArray [i].ToLower ();

				LoadedAssetBundle tmpLoadedAssetbundle = AssetManager.getMe ().GetLoadedAssetbundle (tmpDependence);

                if(tmpLoadedAssetbundle!=null && mDependenceList.Contains(tmpLoadedAssetbundle)==false)
                {
                    mDependenceList.Add(tmpLoadedAssetbundle);
                }
			}
		}
        Debug.Log(mAssetBundleName + " dependence:" + mDependenceList.Count);
        for (int i = 0, max = mDependenceList.Count; i <max; ++i)
        {
            Debug.Log("             " + mDependenceList[i].mAssetBundleName);
        }
	}
  



    public bool Dependence(string varAssetBundleName)
    {
        if(string.IsNullOrEmpty(varAssetBundleName))
        {
            return false;
        }

        for(int i = 0,max = mDependenceList.Count; i <max; ++i )
        {
            if(mDependenceList[i].mAssetBundleName == varAssetBundleName)
            {
                return true;
            }
        }
        return false;
    }

    public void UnLoad()
	{
		if (mAssetbundle != null) {
			
			mAssetbundle.Unload (true);
			mAssetbundle = null;
		}
        mDependenceList.Clear();
    }
}

