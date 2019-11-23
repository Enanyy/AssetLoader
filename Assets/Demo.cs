using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Demo : MonoBehaviour
{
    public Scene mScene;
    private void Start()
    {
        var task = AssetLoader.LoadScene("testscene.unity", LoadSceneMode.Additive, (scene, mode) =>
        {
            mScene = scene;

        });
        //task.Cancel();

       
        AssetLoader.LoadAsset<GameObject>("cube.prefab", (asset) =>
        {

            if (asset != null)
            {
                Debug.Log("Load Success:cube.prefab!");
                asset.Destroy();
            }
            else
            {
                Debug.Log("Load Failed:cube.prefab!");

            }

        });



        AssetLoader.LoadAsset<GameObject>("CubeResource.prefab", (asset) =>
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
        //task.Cancel();

    }

}
