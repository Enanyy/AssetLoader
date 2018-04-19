using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AssetDemo : MonoBehaviour {

    GameObject go0;
    GameObject go1;
    GameObject go2;
    void Awake()
    {
        AssetManager.GetSingleton();
    }
	// Use this for initialization
	void Start () {


        string plane = "assets/r/Plane.prefab";



        AssetManager.GetSingleton().Load(plane, plane, (varGo) => {

            if(varGo)
            {
                go0 = AssetManager.GetSingleton().Instantiate(plane, plane, varGo) as GameObject;
            }
        });


        string cube = "assets/r/Cube.prefab";
        AssetManager.GetSingleton().Load(cube, cube, (varGo) =>
        {

            if (varGo)
            {
                go1 = AssetManager.GetSingleton().Instantiate(cube, cube, varGo) as GameObject;
            }
        });
       
    }

    List<GameObject> mList = new List<GameObject>();
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Keypad0))
        {
            Destroy(go0);

           
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            Destroy(go1);
           
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

            AssetManager.GetSingleton().Load(plane, plane, (varGo) =>
            {

                if (varGo)
                {
                    GameObject go = AssetManager.GetSingleton().Instantiate(plane, plane, varGo) as GameObject;

                    mList.Add(go);
                }
            });
        }
    }
	
	
}
