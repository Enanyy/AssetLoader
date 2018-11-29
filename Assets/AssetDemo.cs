using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AssetDemo : MonoBehaviour {

    AssetEntity go0;
    AssetEntity go1;
    AssetEntity go2;
    void Awake()
    {
        AssetBundleManager.GetSingleton().Init("StreamingAssets");
    }
	// Use this for initialization
	void Start () {


        string plane = "assets/r/Plane.prefab";


        go0 = new AssetEntity();
        go0.LoadAsset(plane, plane);

       

        
        string cube = "assets/r/Cube.prefab";
        go1 = new AssetEntity();
        go1.LoadAsset(cube, cube);


    }

    List<GameObject> mList = new List<GameObject>();
    void Update()
    {
       

        if(Input.GetKeyDown(KeyCode.Keypad0))
        {
            //Destroy(go0);
            go0.Destroy();
            go0 = null;
           
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
           // Destroy(go1);
           
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            if(mList.Count > 0)
            {
                Destroy(mList[0]);
                mList.RemoveAt(0);
            }

        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            string plane = "assets/r/Plane.prefab";
            /*
            AssetManager.GetSingleton().Load(plane, plane, (varGo) =>
            {

                if (varGo)
                {
                    GameObject go = AssetManager.GetSingleton().Instantiate(plane, plane, varGo) as GameObject;

                    mList.Add(go);
                }
            });
            */
        }
    
    }

    private void OnApplicationQiut()
    {
        AssetBundleManager.GetSingleton().Destroy();
    }

}
