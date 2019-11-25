# AssetLoader
一个使用简单，功能齐全，管理方便的Unity资源管理器。

## 主要特点有
- **统一接口** 无论是Resources资源还是Assetbundle资源都使用统一接口加载或卸载。场景的加载和卸载也使用统一接口。
  
- **异步加载** 所有的资源和场景加载方式都是异步加载，并提供回调和加载任务的控制，可以随时取消加载。
  
- **依赖和引用管理**  内部集成了AssetBundle的依赖管理和实例化资源的引用管理，自动检查并释放没有被使用的AssetBundle。
  
- **资源对象池**    内部集成了资源对象池，当资源使用完毕后可回收到对象池，避免频繁的Destroy和重新加载和实例化。
  
- **方便的打包管理工具** 打包工具提供灵活的合并打Bundle或者单独Bundle的设置，一键Build相应平台的Bundle。
  
- **资源列表** 生成全部选中的资源列表，方便热更对比。
  
  
  ## 使用
- **1.加载和卸载资源**
        var task = AssetLoader.LoadAsset<GameObject>("cube.prefab", (asset) =>
        {
            if (asset != null)
            {
                Debug.Log("Load Success:cube.prefab!");
                
                asset.Destroy(); //卸载
                //asset.ReturnAsset();//回收
            }
            else
            {
                Debug.Log("Load Failed:cube.prefab!");

            }
        });
               
- **2.加载和卸载场景**
        
        var task = AssetLoader.LoadScene("testscene.unity", LoadSceneMode.Additive, (scene, mode) =>
        {
            AssetLoader.UnloadScene(scene, null); //卸载场景

        });
        

   
