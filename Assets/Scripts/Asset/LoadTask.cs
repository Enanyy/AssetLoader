using System;
using UnityEngine.SceneManagement;
using System.IO;
public enum LoadStatus
{
    None,
    Loading,
    Done,
}
public interface ILoadTask
{
    string bundleName { get; }
    string assetName { get; }
    bool isCancel { get; }
}
public abstract class LoadTask : ILoadTask
{
    public string key { get; private set; }

    public string bundleName
    {
        get
        {
            if (path != null)
            {
                return string.IsNullOrEmpty(path.group) ? path.path : path.group;
            }
            return key;
        }
    }


    private string mAssetName;
    public string assetName
    {
        get
        {
            if (string.IsNullOrEmpty(mAssetName) == false)
            {
                return mAssetName;
            }
            else
            {
                if (path != null)
                {
                    return path.path;
                }
                return key;
            }
        }
        set { mAssetName = value; }
    }
    private AssetFile mFile;
    public AssetFile path
    {
        get
        {
            if (mFile == null)
            {
                mFile = AssetPath.Get(key);
                //Debug.Log(mPath.path);
            }
            return mFile;
        }
    }
    public LoadTask(string key)
    {
        this.key = key;

    }
    public bool isCancel { get; set; }
    public void Cancel()
    {
        isCancel = true;
    }
}

public interface ISceneLoadTask:ILoadTask
{
    LoadSceneMode mode { get; }
    string sceneName { get; }
    void OnCompleted(Scene scene, LoadSceneMode mode);
}

public class SceneLoadTask : LoadTask, ISceneLoadTask
{
    public LoadSceneMode mode { get; private set; }
    public Action<Scene, LoadSceneMode> callback { get; private set; }

    private string mSceneName;

    public virtual string sceneName
    {
        get
        {
            if(string.IsNullOrEmpty(mSceneName))
            {
                mSceneName = Path.GetFileNameWithoutExtension(assetName);
            }
            return mSceneName;
        }
    }
    public SceneLoadTask(string key, LoadSceneMode mode, Action<Scene, LoadSceneMode> callback) : base(key)
    {
        this.mode = mode;
        this.callback = callback;
    }
    public virtual void OnCompleted(Scene scene, LoadSceneMode mode)
    {
        if (callback != null)
        {
            callback(scene, mode);
        }
    }
}

public interface IAssetLoadTask<T>: ILoadTask where T: UnityEngine.Object
{
    void OnCompleted(Asset<T> t);
}

public class AssetLoadTask<T> : LoadTask, IAssetLoadTask<T> where T : UnityEngine.Object
{
    public Action<Asset<T>> callback { get; private set; }
    public AssetLoadTask(string key, Action<Asset<T>> callback) : base(key)
    {
        this.callback = callback;
    }
    public virtual void OnCompleted(Asset<T> t)
    {
        if (callback != null)
        {
            callback(t);
        }
    }
}

