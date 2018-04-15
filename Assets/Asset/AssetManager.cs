using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class AssetManager : MonoBehaviour {

    private static AssetManager mInstance;
    public static AssetManager GetSingleton()
    {
        if(mInstance == null)
        {
            GameObject go = new GameObject("AssetManager");
            DontDestroyOnLoad(go);
            mInstance = go.AddComponent<AssetManager>();
        }
        return mInstance;
    }
    AssetBundle mManifestAssetBundle;
	AssetBundleManifest mManifest;
	Dictionary<string, LoadedAssetBundle> mAssetBundleDic = new Dictionary<string, LoadedAssetBundle>();

		
	Queue<LoadTask> mLoadTaskQueue = new Queue<LoadTask>();

    int mAssetMode = 0;

    void Awake()
	{
		string tmpAssetManifest = GetAssetBundlePath () + "StreamingAssets";
		if (File.Exists (tmpAssetManifest)) {

			mManifestAssetBundle = AssetBundle.LoadFromFile (tmpAssetManifest);
		
			if (mManifestAssetBundle) {
			
				mManifest = mManifestAssetBundle.LoadAsset ("AssetBundleManifest") as AssetBundleManifest;

				DontDestroyOnLoad (mManifest);
			
			}
		}

        mAssetMode = PlayerPrefs.GetInt("assetmode");
    }

	void Start()
	{

	}

	void Update()
	{
		if (mLoadTaskQueue.Count > 0) {

			LoadTask tmpLoadTask = mLoadTaskQueue.Peek ();

			if (tmpLoadTask == null) 
			{
                LoadTask.Recycle(mLoadTaskQueue.Dequeue ());

				return;
			} 
			else
			{
				string tmpAssetBundleName = tmpLoadTask.mAssetBundleName.ToLower ();

				if (mAssetBundleDic.ContainsKey (tmpAssetBundleName))
				{
					Debug.Log("Loaded:"+tmpLoadTask.mAssetBundleName);

					tmpLoadTask.mState = LoadTask.LoadTaskState.Loaded;

                    LoadTask.Recycle(mLoadTaskQueue.Dequeue ());


                    tmpLoadTask.mCallback(mAssetBundleDic[tmpAssetBundleName].mAssetBundle);

                    return;
				}

				if (tmpLoadTask.mState == LoadTask.LoadTaskState.Cancel) {

                    LoadTask.Recycle(mLoadTaskQueue.Dequeue ());
					return;
				}
				else if (tmpLoadTask.mState == LoadTask.LoadTaskState.UnLoad) 
				{
					#if true

					tmpLoadTask.Load();

					#else

					StartCoroutine (tmpLoadTask.LoadAsync ());

					#endif

					//Debug.Log("Start Load:"+tmpLoadTask.mAssetBundleName);

					return;
				} 
				else if (tmpLoadTask.mState == LoadTask.LoadTaskState.Loading) 
				{
					return;
				} 
				else if (tmpLoadTask.mState == LoadTask.LoadTaskState.Loaded)
				{
					AssetBundle tmpAssetBundle = tmpLoadTask.mAssetBundle;

					if (tmpLoadTask.mCallback != null) 
					{
						tmpLoadTask.mCallback (tmpAssetBundle);
					}

					LoadTask.Recycle( mLoadTaskQueue.Dequeue ());

					//Debug.Log ("LoadTask Count:" + mLoadingAssetQueue.Count);
				}
			}
		}
	}

    public LoadTask GetLoadTask(string varAssetBundleName)
    {
        var it = mLoadTaskQueue.GetEnumerator();
      
        while(it.MoveNext())
        {
            if(it.Current.mAssetBundleName == varAssetBundleName)
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

    /// <summary>
    /// AssetBundle name is "Assets/..."
    /// </summary>
    /// <param name="varAssetBundleName">Variable asset bundle name.</param>
    /// <param name="varCallback">Variable callback.</param>
    public LoadTask Load(string varAssetBundleName, string varAssetName, System.Action<UnityEngine.Object> varCallback)
	{
		string tmpAssetBundleName = varAssetBundleName.ToLower ();

		LoadTask tmpLoadTask = null;
        if (mAssetMode == 0)
        {

            //UnityEngine.Object tmpObj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(varAssetName);

            //if (varCallback != null)
            //{
            //    varCallback(tmpObj);
            //}
            //tmpLoadTask = new LoadTask(varAssetBundleName);
            //tmpLoadTask.mState = LoadTask.LoadTaskState.Loaded;

            //return tmpLoadTask;

        }

		if (LoadAsset(tmpAssetBundleName, varAssetName, varCallback)) {

			tmpLoadTask =  LoadTask.Create();
           
            LoadTask.Recycle(tmpLoadTask);

            return tmpLoadTask;
		}

        tmpLoadTask = GetLoadTask(tmpAssetBundleName);

        Action<AssetBundle> tmpLoadAssetBindleFinishAction = delegate (AssetBundle varAssetBundle)
        {
            if (varAssetBundle)
            {
                if (mAssetBundleDic.ContainsKey(tmpAssetBundleName) == false)
                {

                    LoadedAssetBundle tmpLoadedAssetBundle = LoadedAssetBundle.Create();
                    tmpLoadedAssetBundle.Init(tmpAssetBundleName, varAssetBundle);


                    mAssetBundleDic.Add(tmpAssetBundleName, tmpLoadedAssetBundle);
                }

                LoadAsset(tmpAssetBundleName, varAssetName, varCallback);
            }
            else
            {
                if (varCallback != null)
                {

                    varCallback(null);
                }
            }
        };

        if (tmpLoadTask!=null)
        {
            tmpLoadTask.mCallback += tmpLoadAssetBindleFinishAction;

            return tmpLoadTask;
        }


		string[] tmpDependences = GetAllDependencies (varAssetBundleName);
        if (tmpDependences != null)
        {
            for (int i = 0; i < tmpDependences.Length; ++i)
            {
                string tmpDependentAssetBundleName = tmpDependences[i].ToLower();

                if (!mAssetBundleDic.ContainsKey(tmpDependentAssetBundleName) && GetLoadTask(tmpDependentAssetBundleName) == null)
                {
                    LoadTask tmpDependenceLoadTask = LoadTask.Create();
                    tmpDependenceLoadTask.Init(tmpDependentAssetBundleName, (varAssetBundle) =>
                    {

                        LoadedAssetBundle tmpLoadedAssetBundle = LoadedAssetBundle.Create();
                        tmpLoadedAssetBundle.Init(tmpDependentAssetBundleName, varAssetBundle);


                        mAssetBundleDic.Add(tmpDependentAssetBundleName, tmpLoadedAssetBundle);

                    });
                   
                    mLoadTaskQueue.Enqueue(tmpDependenceLoadTask);
                }
            }
        }
			
		tmpLoadTask = LoadTask.Create ();
        tmpLoadTask.Init(tmpAssetBundleName, tmpLoadAssetBindleFinishAction);
       
        mLoadTaskQueue.Enqueue (tmpLoadTask);

		return tmpLoadTask;
	}


	private bool LoadAsset(string varAssetBundleName, string varAssetName, System.Action<UnityEngine.Object> varCallback)
	{
		UnityEngine.Object tmpObject = null;
		
		string tmpAssetBundleName = varAssetBundleName.ToLower ();

		LoadedAssetBundle tmpLoadedAssetBundle = null;

		mAssetBundleDic.TryGetValue (tmpAssetBundleName, out  tmpLoadedAssetBundle);

		if (tmpLoadedAssetBundle != null && tmpLoadedAssetBundle.mAssetBundle!=null) {
		
			tmpObject = tmpLoadedAssetBundle.mAssetBundle.LoadAsset(varAssetName);

		}

        if (varCallback != null)
        {

            varCallback(tmpObject);
        }

        return tmpObject!=null;
	}

    public static GameObject Instantiate(string varAssetBundleName, UnityEngine.Object varObject)
    {
        GameObject go = Instantiate(varObject) as GameObject;
        return go;
    }

	public LoadedAssetBundle GetLoadedAssetBundle(string varAssetbundleName)
	{
		LoadedAssetBundle tmpLoadedAssetbundle = null;

		mAssetBundleDic.TryGetValue (varAssetbundleName.ToLower(), out  tmpLoadedAssetbundle);

		return tmpLoadedAssetbundle;
	}

	public void LoadScene(string varAssetBundleName,System.Action varCallback)
	{
		StartCoroutine (LoadSceneAsync (varAssetBundleName, varCallback));
	}

	IEnumerator LoadSceneAsync(string varAssetBundleName,System.Action varCallback)
	{
		string tmpAssetBundleName = varAssetBundleName.ToLower ();

		if (mAssetBundleDic.ContainsKey (tmpAssetBundleName)) {

			if (varCallback != null) {
				varCallback ();
			}

			yield break;
		
		}
		string tmpScenePath = GetAssetBundlePath () + varAssetBundleName;
		using(WWW www = new WWW ("file://" + tmpScenePath))
		{
			yield return www;

			if (www.isDone) {

				//if (www.assetBundle != null) {
					
				//}
				//mAssetBundleDic.Add (tmpAssetBundleName, new LoadedAssetbundle (varAssetBundleName, AssetType.Scene, www.assetBundle));

				if (varCallback != null) {
					varCallback ();
				}
			}
		}
	}

 


    public void UnLoad(string varAssetBundleName)
	{
        Debug.Log("UnLoad:"+ varAssetBundleName);


        string tmpAssetBundleName = varAssetBundleName.ToLower();
		LoadedAssetBundle tmpLoadedAssetBundle = GetLoadedAssetBundle (tmpAssetBundleName);
		if (tmpLoadedAssetBundle == null) {
            return;
		}
        //先移除
		mAssetBundleDic.Remove (tmpAssetBundleName);

        //找出需要移除的依赖
        List<LoadedAssetBundle> tmpUnLoadAssetBundleList = new List<LoadedAssetBundle>();

        for (int i = 0,max = tmpLoadedAssetBundle.mDependenceList.Count; i <max; ++i)
        {
            if(OtherDependence(tmpLoadedAssetBundle.mDependenceList[i].mAssetBundleName) == false)
            {
                tmpUnLoadAssetBundleList.Add(tmpLoadedAssetBundle.mDependenceList[i]);
            }
        }

        tmpLoadedAssetBundle.UnLoad();
        LoadedAssetBundle.Recycle(tmpLoadedAssetBundle);

        for (int i = 0,max = tmpUnLoadAssetBundleList.Count; i <max;++i )
        {
            UnLoad(tmpUnLoadAssetBundleList[i].mAssetBundleName);
        }

        tmpUnLoadAssetBundleList.Clear();
    }

    private bool OtherDependence(string varAssetBundleName)
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


	public static string GetAssetBundlePath()
	{
		return Application.dataPath +"/../StreamingAssets/";
	}


	void OnDestroy()
	{
		Dictionary<string,LoadedAssetBundle>.Enumerator it = mAssetBundleDic.GetEnumerator ();
		while (it.MoveNext ()) {
			if (it.Current.Value != null) {
				it.Current.Value.UnLoad();
			}
		}

		mAssetBundleDic.Clear ();

		if (mManifestAssetBundle) {
			mManifestAssetBundle.Unload (true);
		}
	}
}
