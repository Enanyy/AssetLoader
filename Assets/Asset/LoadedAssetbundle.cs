using UnityEngine;
using System.Collections.Generic;


public class LoadedAssetBundle:IPool<LoadedAssetBundle>
{
	

    public string mAssetBundleName;
    public AssetBundle mAssetBundle;

  
    //该资源依赖别的资源
    private List<LoadedAssetBundle> mDependenceList = new List<LoadedAssetBundle> ();
    //场景中实例化出来的,即引用
    private List<AssetReference> mReferenceList = new List<AssetReference>();

    public int dependenceCount { get { return mDependenceList.Count; } }
    public int referenceCount { get { return mReferenceList.Count; } }

    public LoadedAssetBundle() { }

	public void Init(string varAssetBundleName,AssetBundle varAssetbundle)
	{
		mAssetBundleName = varAssetBundleName;
		mAssetBundle =varAssetbundle;
	
      
		AddDependence ();
	}

	
    public void AddReference(AssetReference varReference)
    {
        if (varReference == null) { return; }
        if(mReferenceList.Contains(varReference)==false)
        {
            mReferenceList.Add(varReference);
        }
    }

    public void RemoveReference(AssetReference varReference)
    {
        if(varReference == null)
        {
            return;
        }
        for (int i = mReferenceList.Count - 1; i >= 0; --i)
        {
            if (mReferenceList[i] == null || mReferenceList[i] == varReference)
            {
                mReferenceList.RemoveAt(i);
            }
        }
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

        //卸载依赖
        for (int i = 0, max = mDependenceList.Count; i < max; ++i)
        {
            if (AssetManager.GetSingleton().OtherDependence(mDependenceList[i].mAssetBundleName) == false)
            {
                AssetManager.GetSingleton().UnLoad(mDependenceList[i].mAssetBundleName);
            }
        }
        mDependenceList.Clear();
    }
}

