using UnityEngine;
using System.Collections.Generic;


public class LoadedAssetBundle:IPool<LoadedAssetBundle>
{
	

    public string mAssetBundleName;
    public AssetBundle mAssetBundle;

  
    public List<LoadedAssetBundle> mDependenceList = new List<LoadedAssetBundle> ();
    //场景中实例化出来的
    public List<AssetReference> mReferenceList = new List<AssetReference>();

    public LoadedAssetBundle() { }

	public void Init(string varAssetBundleName,AssetBundle varAssetbundle)
	{
		mAssetBundleName = varAssetBundleName;
		mAssetBundle =varAssetbundle;
	
      
		AddDependence ();
	}

	



	private void AddDependence()
	{
		if ( mAssetBundle != null) {
		
			string[] tmpDependencesArray = AssetManager.GetSingleton().GetAllDependencies (mAssetBundleName);

            if (tmpDependencesArray != null)
            {
                for (int i = 0, max = tmpDependencesArray.Length; i < max; ++i)
                {

                    string tmpDependence = tmpDependencesArray[i].ToLower();

                    LoadedAssetBundle tmpLoadedAssetBundle = AssetManager.GetSingleton().GetLoadedAssetBundle(tmpDependence);

                    if (tmpLoadedAssetBundle != null && mDependenceList.Contains(tmpLoadedAssetBundle) == false)
                    {
                        mDependenceList.Add(tmpLoadedAssetBundle);
                    }
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
        mAssetBundleName = null;
        if (mAssetBundle != null) {
			
			mAssetBundle.Unload (true);
			mAssetBundle = null;
		}
        mDependenceList.Clear();
    }
}

