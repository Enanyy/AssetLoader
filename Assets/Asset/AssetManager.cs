using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class AssetManager : MonoBehaviour {

    #region GetSingleton
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
    #endregion

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
                    tmpLoadTask.mAssetBundle = mAssetBundleDic[tmpAssetBundleName].mAssetBundle;
                    tmpLoadTask.OnLoadFinish();

                    LoadTask.Recycle(mLoadTaskQueue.Dequeue ());


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
					//AssetBundle tmpAssetBundle = tmpLoadTask.mAssetBundle;

					//if (tmpLoadTask.mCallback != null) 
					//{
					//	tmpLoadTask.mCallback (tmpAssetBundle);
					//}

                    OnLoadTaskFinish(tmpLoadTask);

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


    private void OnLoadTaskFinish(LoadTask varLoadTask)
    {
        if (varLoadTask == null) { return; }


        string tmpAssetBundleName = varLoadTask.mAssetBundleName.ToLower();


        if (mAssetBundleDic.ContainsKey(tmpAssetBundleName) == false)
        {
            AssetBundle tmpAssetBundle = varLoadTask.mAssetBundle;

            LoadedAssetBundle tmpLoadedAssetBundle = LoadedAssetBundle.Create();
            tmpLoadedAssetBundle.Init(tmpAssetBundleName, tmpAssetBundle);


            mAssetBundleDic.Add(tmpAssetBundleName, tmpLoadedAssetBundle);
        }
        else
        {
            varLoadTask.mAssetBundle = mAssetBundleDic[tmpAssetBundleName].mAssetBundle;
            
        }

        varLoadTask.OnLoadFinish();

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
#if UNITY_EDITOR
            UnityEngine.Object tmpObj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(varAssetName);

            if (varCallback != null)
            {
                varCallback(tmpObj);
            }
            tmpLoadTask = new LoadTask();
            tmpLoadTask.Init(tmpAssetBundleName);
            tmpLoadTask.mState = LoadTask.LoadTaskState.Loaded;

            return tmpLoadTask;
#endif
        }

		if (mAssetBundleDic.ContainsKey(varAssetBundleName)) {

            LoadedAssetBundle tmpLoadedAssetBundle =  mAssetBundleDic[tmpAssetBundleName];

            UnityEngine.Object tmpObject = null;
            if (tmpLoadedAssetBundle != null && tmpLoadedAssetBundle.mAssetBundle != null)
            {

                 tmpObject = tmpLoadedAssetBundle.mAssetBundle.LoadAsset(varAssetName);

            }
            if(varCallback!=null)
            {
                varCallback(tmpObject);
            }

            tmpLoadTask =  LoadTask.Create();
           
            LoadTask.Recycle(tmpLoadTask);

            return tmpLoadTask;
		}

        tmpLoadTask = GetLoadTask(tmpAssetBundleName);

        if (tmpLoadTask!=null)
        {
          
            tmpLoadTask.AddLoadAssetTask(varAssetName, varCallback);
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
                    tmpDependenceLoadTask.Init(tmpDependentAssetBundleName);
                   
                    mLoadTaskQueue.Enqueue(tmpDependenceLoadTask);
                }
            }
        }
			
		tmpLoadTask = LoadTask.Create ();
        tmpLoadTask.Init(tmpAssetBundleName);
        tmpLoadTask.AddLoadAssetTask(varAssetName, varCallback);

        mLoadTaskQueue.Enqueue (tmpLoadTask);

		return tmpLoadTask;
	}

    public  GameObject Instantiate(string varAssetBundleName,string varAssetName, UnityEngine.Object varObject)
    {
        if(varObject == null)
        {
            Debug.LogError("Trying Instantiate object is NULL!");
            return null;
        }
        GameObject go = Instantiate(varObject) as GameObject;
        AssetReference dependence = go.AddComponent<AssetReference>();
        dependence.SetData(varAssetBundleName, varAssetName);

       

        return go;
    }

    public void Destroy(AssetReference varReference)
    {
        if (varReference == null)
        {
            return;
        }
      
        LoadedAssetBundle tmpLoadedAssetBundle = GetLoadedAssetBundle(varReference.mAssetBundleName);
        if (tmpLoadedAssetBundle != null)
        {
            tmpLoadedAssetBundle.RemoveReference(varReference);

            if(tmpLoadedAssetBundle.referenceCount == 0)
            {
                UnLoad(varReference.mAssetBundleName);
            }
        }
    }

	public LoadedAssetBundle GetLoadedAssetBundle(string varAssetbundleName)
	{
		LoadedAssetBundle tmpLoadedAssetbundle = null;

		mAssetBundleDic.TryGetValue (varAssetbundleName.ToLower(), out  tmpLoadedAssetbundle);

		return tmpLoadedAssetbundle;
	}

	public void LoadScene(string varAssetBundleName,System.Action varCallback)
	{
        Load(varAssetBundleName, "", (varGo) => {

            if(varCallback!=null)
            {
                varCallback();
            }
        });
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
        if(string.IsNullOrEmpty(varAssetBundleName))
        {
            return;
        }
        Debug.Log("UnLoad:"+ varAssetBundleName);


        string tmpAssetBundleName = varAssetBundleName.ToLower();
		LoadedAssetBundle tmpLoadedAssetBundle = GetLoadedAssetBundle (tmpAssetBundleName);
		if (tmpLoadedAssetBundle == null) {
            return;
		}
        //先移除
		mAssetBundleDic.Remove (tmpAssetBundleName);

       
        //卸载本身以及单独的依赖
        tmpLoadedAssetBundle.UnLoad();

        LoadedAssetBundle.Recycle(tmpLoadedAssetBundle);
    
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


	public static string GetAssetBundlePath()
	{
		return Application.dataPath +"/../StreamingAssets/";
	}
   

	void OnDestroy()
	{
        string[] tmpAssetBundleArray = new string[mAssetBundleDic.Count];
        mAssetBundleDic.Keys.CopyTo(tmpAssetBundleArray, 0);
        for (int i = 0,  max = tmpAssetBundleArray.Length; i <max; ++i)
        {
            UnLoad(tmpAssetBundleArray[i]);
        }

        Array.Clear(tmpAssetBundleArray, 0, tmpAssetBundleArray.Length);
        tmpAssetBundleArray = null;


        mAssetBundleDic.Clear ();

		if (mManifestAssetBundle) {
			mManifestAssetBundle.Unload (true);
		}
	}
}
