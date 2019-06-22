using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    List<AssetObject> entities = new List<AssetObject>();
    // Use this for initialization
    void Start () {

    
    }

    
	
	// Update is called once per frame
	void OnGUI () {
		if(GUI.Button(new Rect(10,20,100,40),"Delete"))
        {
            if(entities.Count >0)
            {
                entities[0].Destroy();
                entities.RemoveAt(0);
            }
        }

        if (GUI.Button(new Rect(10, 80, 100, 40), "Cube"))
        {
            if (AssetManager.Instance.initialized==false)
            {
                AssetManager.Instance.Init(LoadMode.Async, "StreamingAssets");
            }
            string cube = "assets/r/res/cube.prefab";

            AssetObject entity = new AssetObject();
            entity.LoadAsset<GameObject>(cube, cube);
            entities.Add(entity);
        }

        if (GUI.Button(new Rect(10, 140, 100, 40), "Plane"))
        {
            if (AssetManager.Instance.initialized == false)
            {
                AssetManager.Instance.Init(LoadMode.Async, "StreamingAssets");
            }
            string plane = "assets/r/res/plane.prefab";

            AssetObject entity = new AssetObject();
            entity.LoadAsset<GameObject>(plane, plane);
            entities.Add(entity);
        }

        if (GUI.Button(new Rect(10, 200, 100, 40), "Scene"))
        {
            if (AssetManager.Instance.initialized == false)
            {
                AssetManager.Instance.Init(LoadMode.Async, "StreamingAssets");
            }
            string level = "assets/r/scenes/test.level";

            AssetManager.Instance.Load(level, (bundle) => {
                if(bundle!=null && bundle.bundle)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("test");
                }
            });
        }

        if (GUI.Button(new Rect(10, 260, 100, 40), "Destroy"))
        {
            AssetManager.Instance.Destroy();
        }
    }
}
