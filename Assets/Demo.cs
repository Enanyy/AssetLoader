using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Demo : MonoBehaviour
{
    public Scene mScene;
    private void Start()
    {
        var sceneTask = AssetLoader.LoadScene("testscene.unity", LoadSceneMode.Additive, (scene, mode) =>
        {
            mScene = scene;

        });
        sceneTask.isCancel = true; //取消加载

       
        var cubeTask = AssetLoader.LoadAsset<GameObject>("cube.prefab", (asset) =>
        {

            if (asset != null)
            {
                Debug.Log("Load Success:cube.prefab!");
                asset.Destroy(); //销毁
            }
            else
            {
                Debug.Log("Load Failed:cube.prefab!");

            }

        });

        var cubeTas1= AssetLoader.LoadAsset<GameObject>("cube.prefab", (asset) =>
        {

            if (asset != null)
            {
                Debug.Log("Load Success:cube.prefab!");
            }
            else
            {
                Debug.Log("Load Failed:cube.prefab!");
            }

        });

        //cubeTas1.isCancel = true;

        var cubeResourceTask = AssetLoader.LoadAsset<GameObject>("CubeResource.prefab", (asset) =>
        {

            if (asset != null)
            {
                Debug.Log("Load Success:CubeResource.prefab!");
            }
            else
            {
                Debug.Log("Load Failed:CubeResource.prefab!");

            }

        });
        cubeResourceTask.isCancel = true;

    }

}
