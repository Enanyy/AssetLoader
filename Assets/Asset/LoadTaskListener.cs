using UnityEngine;
using System.Collections.Generic;
using System;

public class LoadTaskListener : MonoBehaviour
{
	ILoadTaskReceiver mLoadTaskReceiver;
	
	List<LoadTask> mLoadTaskList = new List<LoadTask> ();

	bool mLoadFinished = false;

	int mFinishCount = 0;

	public void AddLoadTask(ILoadTaskReceiver varLoadTaskReceiver, LoadTask varLoadTask)
	{
		mLoadTaskReceiver = varLoadTaskReceiver;
		if (varLoadTask == null) {
			return;
		}
		mLoadTaskList.Add (varLoadTask);

		mLoadFinished = false;
	}

    void Update()
    {
        if (mLoadFinished)
        {
            return;
        }

        int tmpCount = 0;
        for (int i = mLoadTaskList.Count - 1; i >= 0; --i)
        {
            if (mLoadTaskList[i] != null)
            {
                if (mLoadTaskList[i].mState == LoadTask.LoadTaskState.Cancel
                    || mLoadTaskList[i].mState == LoadTask.LoadTaskState.Loaded)
                {

                    tmpCount++;
                }
            }
            else
            {
                mLoadTaskList.RemoveAt(i);
            }
        }

        if (tmpCount == mLoadTaskList.Count)
        {

            mLoadFinished = true;
            Debug.Log(gameObject.name + " Loaded:" + tmpCount);
            if (mLoadTaskReceiver != null)
            {
                mLoadTaskReceiver.OnLoadFinish();
            }

            Invoke("Destroy", 5);
        }
        if (tmpCount > mFinishCount)
        {

            mFinishCount = tmpCount;
            if (mLoadTaskReceiver != null)
            {
                mLoadTaskReceiver.OnLoadProcess(tmpCount, mLoadTaskList.Count);
            }
        }

    }

	void Destroy()
	{
		GameObject.DestroyObject (this);
	}

	void OnDestroy()
	{
		for (int i = 0; i < mLoadTaskList.Count; ++i) {
			if (mLoadTaskList [i] != null) {
				mLoadTaskList [i].mState = LoadTask.LoadTaskState.Cancel;
			}
		}
	}

	public static LoadTaskListener Get(GameObject varGo,ILoadTaskReceiver varLoadTaskReceiver, LoadTask varLoadTask)
	{
		
		LoadTaskListener tmpLoadTaskPlugin = null;

		if (varGo) {
			tmpLoadTaskPlugin = varGo.GetComponent<LoadTaskListener> ();
			if (tmpLoadTaskPlugin == null) {
				tmpLoadTaskPlugin = varGo.AddComponent<LoadTaskListener> ();
			}
		} 

		if (varLoadTask != null) {

			if (tmpLoadTaskPlugin != null) {
				tmpLoadTaskPlugin.AddLoadTask (varLoadTaskReceiver,varLoadTask);
			}
		}
		return tmpLoadTaskPlugin;
	}

    public static LoadTaskListener Get(GameObject varGo, ILoadTaskReceiver varLoadTaskReceiver, string varAssetBundleName,string varAssetName, Action<UnityEngine.Object> varCallback)
    {
        LoadTask tmpLoadTask = AssetManager.GetSingleton().Load(varAssetBundleName, varAssetName, varCallback);

        return Get(varGo, varLoadTaskReceiver, tmpLoadTask);
    }
}

