using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour {

    private void Awake()
    {
        AssetBundleManager.GetSingleton().Init("StreamingAssets");
    }
    List<AssetEntity> entities = new List<AssetEntity>();
    // Use this for initialization
    void Start () {

        string cube = "assets/r/res/cube.prefab";

        AssetEntity entity = new AssetEntity();
        entity.LoadAsset(cube, cube);
        entities.Add(entity);
    }

    
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.D))
        {
            if(entities.Count >0)
            {
                entities[0].Destroy();
                entities.RemoveAt(0);
            }
        }
	}
}
