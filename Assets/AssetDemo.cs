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

       


        
        AssetManager.GetSingleton().Load("assets/r/ui/ui_root.prefab", "assets/r/ui/ui_root.prefab", (varGo) => {

            if(varGo)
            {
                go0 = AssetManager.GetSingleton().Instantiate("assets/r/ui/ui_root.prefab", "assets/r/ui/ui_root.prefab",varGo) as GameObject;
            }
        });

       

        AssetManager.GetSingleton().Load("assets/r/ui/ui_root1.prefab", "assets/r/ui/ui_root1.prefab", (varGo) =>
        {

            if (varGo)
            {
                go1 = AssetManager.GetSingleton().Instantiate("assets/r/ui/ui_root1.prefab", "assets/r/ui/ui_root1.prefab",varGo) as GameObject;
            }
        });
       
    }

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
            Destroy(go2);

        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            AssetManager.GetSingleton().Load("assets/r/ui/ui_root.prefab", "assets/r/ui/ui_root.prefab", (varGo) =>
            {

                if (varGo)
                {
                    go0 = AssetManager.GetSingleton().Instantiate("assets/r/ui/ui_root.prefab", "assets/r/ui/ui_root.prefab", varGo) as GameObject;

                    go2 = Instantiate(go0) as GameObject;
                }
            });
        }
    }
	
	
}
