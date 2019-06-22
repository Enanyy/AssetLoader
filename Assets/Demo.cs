using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    List<IAssetObject> entities = new List<IAssetObject>();
   
   
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
            AssetManager.Instance.LoadAsset<GameObject>(cube, cube, (assetObject) => {

                entities.Add(assetObject);

            });
           
        }

        if (GUI.Button(new Rect(10, 140, 100, 40), "Plane"))
        {
            if (AssetManager.Instance.initialized == false)
            {
                AssetManager.Instance.Init(LoadMode.Async, "StreamingAssets");
            }
            string plane = "assets/r/res/plane.prefab";

            AssetManager.Instance.LoadAsset<GameObject>(plane, plane, (assetObject) => {

                entities.Add(assetObject);

            });
           
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
