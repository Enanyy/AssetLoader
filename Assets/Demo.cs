using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour {

    private void Awake()
    {
        AssetBundleManager.GetSingleton().Init(LoadType.Async,"StreamingAssets");
    }
    List<AssetEntity> entities = new List<AssetEntity>();
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
            string cube = "assets/r/res/cube.prefab";

            AssetEntity entity = new AssetEntity();
            entity.LoadAsset(cube, cube);
            entities.Add(entity);
        }

        if (GUI.Button(new Rect(10, 140, 100, 40), "Plane"))
        {
            string plane = "assets/r/res/plane.prefab";

            AssetEntity entity = new AssetEntity();
            entity.LoadAsset(plane, plane);
            entities.Add(entity);
        }

        if (GUI.Button(new Rect(10, 200, 100, 40), "Scene"))
        {
            string level = "assets/r/scenes/test.level";

            AssetBundleManager.GetSingleton().Load(level, (varAssetBundleEntity) => {
                if(varAssetBundleEntity!=null && varAssetBundleEntity.assetBundle)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("test");
                }
            });
        }
    }
}
