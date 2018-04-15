using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        AssetManager.GetSingleton().Load("assets/r/ui/ui_root.prefab", "assets/r/ui/ui_root.prefab", (varGo) => {

            if(varGo)
            {
                go0 = Instantiate(varGo) as GameObject;
            }
        });

        AssetManager.GetSingleton().Load("assets/r/ui/ui_root.prefab", "assets/r/ui/ui_root.prefab", (varGo) =>
        {

            if (varGo)
            {
                go2 = Instantiate(varGo) as GameObject;
            }
        });

        AssetManager.GetSingleton().Load("assets/r/ui/ui_root1.prefab", "assets/r/ui/ui_root1.prefab", (varGo) =>
        {

            if (varGo)
            {
                go1 = Instantiate(varGo) as GameObject;
            }
        });

    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Keypad0))
        {
            Destroy(go0);
            Destroy(go2);
            AssetManager.GetSingleton().UnLoad("assets/r/ui/ui_root.prefab");
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            Destroy(go1);
            AssetManager.GetSingleton().UnLoad("assets/r/ui/ui_root1.prefab");
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            AssetManager.GetSingleton().Load("assets/r/ui/ui_root.prefab", "assets/r/ui/ui_root.prefab", (varGo) =>
            {

                if (varGo)
                {
                    go0 = Instantiate(varGo) as GameObject;
                }
            });

        }
    }
	
	
}
