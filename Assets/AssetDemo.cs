using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AssetDemo : MonoBehaviour {

    AssetEntity go0;
    AssetEntity go1;
    AssetEntity go2;
    List<GameObject> mList = new List<GameObject>();

    void Awake()
    {
       
        AssetBundleManager.GetSingleton().Init("StreamingAssets");
    }
	// Use this for initialization
	void Start () {


        string plane = "assets/r/res/Plane.prefab";


        go0 = new AssetEntity();
        go0.LoadAsset(plane, plane);

        string cube = "assets/r/res/Cube.prefab";
        go1 = new AssetEntity();
        go1.LoadAsset(cube, cube);


    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Keypad0))
        {
          
            go0.Destroy();
            go0 = null;
           
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
          
           
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            if(mList.Count > 0)
            {
                Destroy(mList[0]);
                mList.RemoveAt(0);
            }

        }
       
    
    }
    private void OnGUI()
    {
        if (GUI.Button(new Rect(20, 20, 200, 40), "Change test scene"))
        {
            AssetBundleManager.GetSingleton().Load("assets/r/scenes/test.level", (bundleEntity) =>
            {

                if (bundleEntity != null)
                {
                    SceneManager.LoadScene("test");
                }
            });
        }

        GUI.Label(new Rect(20, 60, 1000, 40), Application.streamingAssetsPath);
        GUI.Label(new Rect(20, 80, 1000, 40), Application.dataPath);
        GUI.Label(new Rect(20, 100, 1000, 40), Application.persistentDataPath);
    }

    private void OnApplicationQiut()
    {
        AssetBundleManager.GetSingleton().Destroy();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }
}
