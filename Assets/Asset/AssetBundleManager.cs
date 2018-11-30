using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum LoadType
{
    Sync,
    Async,
    WWW,
}
public class AssetBundleManager:MonoBehaviour  {

    #region GetSingleton
    private static AssetBundleManager mInstance;
    public static AssetBundleManager GetSingleton()
    {
        if(mInstance == null)
        {
            GameObject go = new GameObject(typeof(AssetBundleManager).Name);
            mInstance = go.AddComponent<AssetBundleManager>();
            DontDestroyOnLoad(go);
        }
        return mInstance;
    }
    #endregion

    AssetBundle mManifestAssetBundle;
	AssetBundleManifest mManifest;
	Dictionary<string, AssetBundleEntity> mAssetBundleDic = new Dictionary<string, AssetBundleEntity>();

		
	Queue<AssetBundleLoadTask> mAssetBundleLoadTaskQueue = new Queue<AssetBundleLoadTask>();

    public int assetMode { get; private set; }


    private string mAssetBundlePath;

    private bool mInited = false;

    /// <summary>
    /// 没有初始化完毕就开始加载的资源
    /// </summary>
    private class AssetBundleWaitTask {

        public string assetBundleName;
        public Action<AssetBundleEntity> callback;

    }

    private Queue<AssetBundleWaitTask> mWaitTaskQueue = new Queue<AssetBundleWaitTask>();

    public  LoadType loadType = LoadType.Async;

    public void Init(string varAssetManifestName)
	{
        mInited = false;

        assetMode = PlayerPrefs.GetInt("assetmode");

        if (loadType == LoadType.Sync)
        {
            LoadManifest(varAssetManifestName);
        }
        else if(loadType == LoadType.Async)
        {
            StartCoroutine(LoadManifestAsync(varAssetManifestName));
        }
        else
        {
            StartCoroutine(LoadManifestWWW(varAssetManifestName));
        }
    }

    private void LoadManifest(string varAssetManifestName)
    {
        string tmpAssetManifestPath = GetAssetBundlePath() + varAssetManifestName;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            tmpAssetManifestPath = Uri.EscapeUriString(tmpAssetManifestPath);
        }

        var assetBundle = AssetBundle.LoadFromFile(tmpAssetManifestPath);

        if (assetBundle)
        {
            LoadAssetManifestFinish(assetBundle);
        }
        else
        {
            Debug.LogError(varAssetManifestName + ": Error!!");
        }
    }
    private IEnumerator LoadManifestAsync(string varAssetManifestName)
    {
        string tmpAssetManifestPath = GetAssetBundlePath() + varAssetManifestName;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            tmpAssetManifestPath = Uri.EscapeUriString(tmpAssetManifestPath);
        }

        var request = AssetBundle.LoadFromFileAsync(tmpAssetManifestPath);
        yield return request;

        if(request.isDone && request.assetBundle)
        {
            LoadAssetManifestFinish(request.assetBundle);
        }
        else

        { Debug.LogError(varAssetManifestName + ": Error!!" ); }

    }

    private IEnumerator LoadManifestWWW(string varAssetManifestName)
    {
        string tmpAssetManifestPath = GetAssetBundlePath() + varAssetManifestName;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            tmpAssetManifestPath = Uri.EscapeUriString(tmpAssetManifestPath);
        }


        using (WWW www = new WWW(tmpAssetManifestPath))
        {
            yield return www;

            if (www.isDone && www.assetBundle)
            {
                LoadAssetManifestFinish(www.assetBundle);
            }
            else
            {
                Debug.LogError(varAssetManifestName + ":" + www.error);
               
            }
        }
    }
    private void LoadAssetManifestFinish(AssetBundle assetBundle)
    {
        mManifestAssetBundle = assetBundle;

        mManifest = mManifestAssetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

        DontDestroyOnLoad(mManifest);

        mInited = true;

        while (mWaitTaskQueue.Count > 0)
        {
            var waitTask = mWaitTaskQueue.Dequeue();
            Load(waitTask.assetBundleName, waitTask.callback);
        }
    }


    void Update()
	{
        if(mInited == false)
        {
            return;
        }

		if (mAssetBundleLoadTaskQueue.Count > 0) {

			AssetBundleLoadTask tmpLoadTask = mAssetBundleLoadTaskQueue.Peek ();

			if (tmpLoadTask == null) 
			{
                mAssetBundleLoadTaskQueue.Dequeue ();

				return;
			} 
			else
			{
				string tmpAssetBundleName = tmpLoadTask.assetBundleName.ToLower ();

				if (mAssetBundleDic.ContainsKey (tmpAssetBundleName))
				{
					Debug.Log("Loaded:"+tmpLoadTask.assetBundleName);

					tmpLoadTask.state = LoadStatus.Finish;
                   
                    tmpLoadTask.LoadFinish(mAssetBundleDic[tmpAssetBundleName]);

                    mAssetBundleLoadTaskQueue.Dequeue ();


                    return;
				}

				if (tmpLoadTask.state == LoadStatus.Cancel||
                    tmpLoadTask.state == LoadStatus.Error)
                {
                    mAssetBundleLoadTaskQueue.Dequeue ();
					return;
				}
				else if (tmpLoadTask.state == LoadStatus.Wait) 
				{
                    if (loadType == LoadType.Sync)
                    {
                        tmpLoadTask.LoadSync();
                    }
                    else if(loadType == LoadType.Async)
                    {
                        StartCoroutine(tmpLoadTask.LoadAsync());
                    }
                    else
                    {
                        StartCoroutine(tmpLoadTask.LoadWWW());
                    }
					return;
				} 
				else if (tmpLoadTask.state ==LoadStatus.Loading) 
				{                 
					return;
				} 
				else if (tmpLoadTask.state == LoadStatus.Finish)
				{
					
                    OnAssetBundleLoadTaskFinish(tmpLoadTask);

                    mAssetBundleLoadTaskQueue.Dequeue ();

				}
			}
		}
	}

    public AssetBundleLoadTask GetLoadTask(string varAssetBundleName)
    {
        var it = mAssetBundleLoadTaskQueue.GetEnumerator();
      
        while(it.MoveNext())
        {
            if(it.Current.assetBundleName == varAssetBundleName)
            {

                return it.Current;
            }
        }
        return null;
    }

    public string[] GetAllDependencies(string varAssetBundleName)
    {
        if(string.IsNullOrEmpty(varAssetBundleName) || mManifest == null)
        {
            return null;
        }

        return mManifest.GetAllDependencies(varAssetBundleName);

    }


    private void OnAssetBundleLoadTaskFinish(AssetBundleLoadTask varLoadTask)
    {
        if (varLoadTask == null) { return; }

        string tmpAssetBundleName = varLoadTask.assetBundleName.ToLower();

        if (mAssetBundleDic.ContainsKey(tmpAssetBundleName) == false)
        {
            AssetBundleEntity tmpBundleEntity = new AssetBundleEntity(tmpAssetBundleName, varLoadTask.assetBundle);

            mAssetBundleDic.Add(tmpAssetBundleName, tmpBundleEntity);
        }
      
        varLoadTask.LoadFinish(mAssetBundleDic[tmpAssetBundleName]);

    }

    public AssetBundleLoadTask Load(string varAssetBundleName,System.Action<AssetBundleEntity> varCallback)
    {
        //没有初始化完成，加入等待队列
        if (mInited == false)
        {
            AssetBundleWaitTask waitTask = new AssetBundleWaitTask
            {
                assetBundleName = varAssetBundleName,
                callback = varCallback
            };

            mWaitTaskQueue.Enqueue(waitTask);

            return null;
        }

        string tmpAssetBundleName = varAssetBundleName.ToLower();

        AssetBundleLoadTask tmpLoadTask = null;
 
        if (mAssetBundleDic.ContainsKey(varAssetBundleName))
        {
            if (varCallback != null)
            {
                AssetBundleEntity tmpBundleEntity = mAssetBundleDic[tmpAssetBundleName];

                varCallback(tmpBundleEntity);
            }

            tmpLoadTask = new AssetBundleLoadTask(tmpAssetBundleName);

            return tmpLoadTask;
        }

        tmpLoadTask = GetLoadTask(tmpAssetBundleName);

        if (tmpLoadTask != null)
        {
            tmpLoadTask.AddAssetLoadTask(varAssetBundleName, varCallback);
            return tmpLoadTask;
        }

        string[] tmpDependences = GetAllDependencies(varAssetBundleName);
        if (tmpDependences != null)
        {
            for (int i = 0; i < tmpDependences.Length; ++i)
            {
                string tmpDependentAssetBundleName = tmpDependences[i].ToLower();

                if (!mAssetBundleDic.ContainsKey(tmpDependentAssetBundleName) && GetLoadTask(tmpDependentAssetBundleName) == null)
                {
                    AssetBundleLoadTask tmpDependenceLoadTask = new AssetBundleLoadTask(tmpDependentAssetBundleName);

                    mAssetBundleLoadTaskQueue.Enqueue(tmpDependenceLoadTask);
                }
            }
        }

        tmpLoadTask = new AssetBundleLoadTask(tmpAssetBundleName);

        tmpLoadTask.AddAssetLoadTask(varAssetBundleName, varCallback);

        mAssetBundleLoadTaskQueue.Enqueue(tmpLoadTask);

        return tmpLoadTask;
    }


	public AssetBundleEntity GetAssetBundleEntity(string varAssetbundleName)
	{
		AssetBundleEntity tmpBundleEntity = null;

		mAssetBundleDic.TryGetValue (varAssetbundleName.ToLower(), out  tmpBundleEntity);

		return tmpBundleEntity;
	}

    public void UnLoad(string varAssetBundleName)
    {
        AssetBundleEntity assetBundleEntity = GetAssetBundleEntity(varAssetBundleName);
        if(assetBundleEntity!=null)
        {
            UnLoad(assetBundleEntity);
        }
    }

    public void UnLoad(AssetBundleEntity varBundleEntity)
	{
        if(varBundleEntity == null)
        {
            return;
        }
        Debug.Log("UnLoad:"+ varBundleEntity.assetBundleName);

        string tmpAssetBundleName = varBundleEntity.assetBundleName.ToLower();
	
        //先移除
		mAssetBundleDic.Remove (tmpAssetBundleName);

        //卸载本身以及单独的依赖
        varBundleEntity.UnLoad(); 
    }

    public bool OtherDependence(string varAssetBundleName)
    {
        var it = mAssetBundleDic.GetEnumerator();
        while (it.MoveNext())
        {
            if (it.Current.Value.Dependence(varAssetBundleName))
            {
                return true;
            }
        }
        return false;
    }


	public string GetAssetBundlePath()
	{
        if (string.IsNullOrEmpty(mAssetBundlePath))
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                mAssetBundlePath = Application.streamingAssetsPath+"/" ;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                mAssetBundlePath = Application.streamingAssetsPath + "/";
            }
            else
            {
                mAssetBundlePath = Application.dataPath + "/StreamingAssets/";
            }
        }
        return mAssetBundlePath;

    }


    public void Destroy()
	{
        string[] tmpAssetBundleArray = new string[mAssetBundleDic.Count];
        mAssetBundleDic.Keys.CopyTo(tmpAssetBundleArray, 0);
        for (int i = 0,  max = tmpAssetBundleArray.Length; i <max; ++i)
        {
            AssetBundleEntity tmpBundleEntity = GetAssetBundleEntity(tmpAssetBundleArray[i]);
            UnLoad(tmpBundleEntity);
        }

        Array.Clear(tmpAssetBundleArray, 0, tmpAssetBundleArray.Length);
        tmpAssetBundleArray = null;

        mAssetBundleDic.Clear ();

		if (mManifestAssetBundle) {
			mManifestAssetBundle.Unload (true);
		}
	}
}
