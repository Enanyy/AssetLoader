using UnityEngine;
using System.Collections.Generic;


public class AssetBundleEntity
{
    public string assetBundleName { get; private set; }
    public AssetBundle assetBundle { get; private set; }

  
    //该资源依赖别的资源
    private List<AssetBundleEntity> mDependenceList = new List<AssetBundleEntity> ();
    //场景中实例化出来的,即引用
    private List<AssetEntity> mReferenceList = new List<AssetEntity>();
    //已经加载出来的asset
    private Dictionary<string, Object> mAssetDic = new Dictionary<string, Object>();

    public int dependenceCount { get { return mDependenceList.Count; } }
    public int referenceCount { get { return mReferenceList.Count; } }

    public AssetBundleEntity() { }

	public AssetBundleEntity(string varAssetBundleName,AssetBundle varAssetbundle)
	{
		assetBundleName = varAssetBundleName;
		assetBundle =varAssetbundle;
	  
		AddDependence ();
	}

	
    public void AddReference(AssetEntity varReference)
    {
        if (varReference == null) { return; }
        if(mReferenceList.Contains(varReference)==false)
        {
            mReferenceList.Add(varReference);
        }
    }

    public void RemoveReference(AssetEntity varReference)
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
        if (referenceCount == 0)
        {
           AssetBundleManager.GetSingleton().UnLoad(this);
        }
    }


	private void AddDependence()
	{
		if ( assetBundle != null) {
		
			string[] tmpDependencesArray = AssetBundleManager.GetSingleton().GetAllDependencies (assetBundleName);

            if (tmpDependencesArray != null)
            {
                for (int i = 0, max = tmpDependencesArray.Length; i < max; ++i)
                {
                    string tmpDependence = tmpDependencesArray[i].ToLower();

                    AssetBundleEntity tmpBundleEntity = AssetBundleManager.GetSingleton().GetAssetBundleEntity(tmpDependence);

                    if (tmpBundleEntity != null && mDependenceList.Contains(tmpBundleEntity) == false)
                    {
                        mDependenceList.Add(tmpBundleEntity);
                    }
                }
            }
		}
        string log = assetBundleName + " dependence:" + mDependenceList.Count;
       
        for (int i = 0, max = mDependenceList.Count; i <max; ++i)
        {
            log += "\n             " + mDependenceList[i].assetBundleName;
        }
        Debug.Log(log);
	}
  



    public bool Dependence(string varAssetBundleName)
    {
        if(string.IsNullOrEmpty(varAssetBundleName))
        {
            return false;
        }

        for(int i = 0,max = mDependenceList.Count; i <max; ++i )
        {
            if(mDependenceList[i].assetBundleName == varAssetBundleName)
            {
                return true;
            }
        }
        return false;
    }

    public Object LoadAsset(string varAssetName)
    {
        if (mAssetDic.ContainsKey(varAssetName) == false)
        {
            mAssetDic.Add(varAssetName, assetBundle.LoadAsset(varAssetName));
        }

        return mAssetDic[varAssetName];
    }

    public void UnLoad()
	{
        assetBundleName = null;
        if (assetBundle != null) {
			
			assetBundle.Unload (true);
			assetBundle = null;
		}

        //卸载依赖
        for (int i = 0, max = mDependenceList.Count; i < max; ++i)
        {
            if (AssetBundleManager.GetSingleton().OtherDependence(mDependenceList[i].assetBundleName) == false)
            {
                AssetBundleManager.GetSingleton().UnLoad(mDependenceList[i]);
            }
        }
        mDependenceList.Clear();
    }
}

