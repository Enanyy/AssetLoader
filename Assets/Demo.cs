﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour {

    private void Awake()
    {
        AssetManager.Instance.Init(LoadType.Async,"StreamingAssets");
    }
    List<AssetObject> entities = new List<AssetObject>();
    // Use this for initialization
    void Start () {

        Debug.Log(transform.forward);
        Debug.Log(transform.rotation * Vector3.forward);

        Debug.Log(transform.right);
        Debug.Log(transform.rotation * Vector3.right);
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

            AssetObject entity = new AssetObject();
            entity.LoadAsset(cube, cube);
            entities.Add(entity);
        }

        if (GUI.Button(new Rect(10, 140, 100, 40), "Plane"))
        {
            string plane = "assets/r/res/plane.prefab";

            AssetObject entity = new AssetObject();
            entity.LoadAsset(plane, plane);
            entities.Add(entity);
        }

        if (GUI.Button(new Rect(10, 200, 100, 40), "Scene"))
        {
            string level = "assets/r/scenes/test.level";

            AssetManager.Instance.Load(level, (varAssetBundleEntity) => {
                if(varAssetBundleEntity!=null && varAssetBundleEntity.bundle)
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