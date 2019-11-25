# AssetLoader
一个使用简单，功能齐全，管理方便的Unity资源管理器。

## 主要特点有
- **统一接口** 无论是Resources资源还是Assetbundle资源都使用统一接口加载或卸载。场景的加载和卸载也使用统一接口。
  
- **异步加载** 所有的资源和场景加载方式都是异步加载，并提供回调和加载任务的控制，可以随时取消加载。
  
- **依赖和引用管理**  内部集成了AssetBundle的依赖管理和实例化资源的引用管理，自动检查并释放没有被使用的AssetBundle。
  
- **资源对象池**    内部集成了资源对象池，当资源使用完毕后可回收到对象池，避免频繁的Destroy和重新加载和实例化。
  
- **方便的打包管理工具** 打包工具提供灵活的合并打Bundle或者单独Bundle的设置，一键Build相应平台的Bundle。
  
- **资源列表** 生成全部选中的资源列表，方便热更对比。

- **两种模式** 提供Editor模式和AssetBundle两种加载模式，方便快速开发和调试。

  
  ## 设置
- **1.设置资源的名称和是否合并打Bundle**
  在资源的Inspector面板的Asset✔，并设置资源名称，表示该资源会被使用。只有✔的资源才能加载成功。
  
  勾选Bundle并设置Bundle名称，表示该资源会合并打包进相应的Bundle内，如果不勾选和设置，则会单独打Bundle，路径则为资源以Assets/开始的相对路径。
  注意：Resources目录下的资源如果没有设置Bundle名称时不会打出AssetBunlde的。
  
  可以设置指定目录可以勾选，在AssetTool.cs内添加指定目录：
  
    /// <summary>
    /// 指定目录可以选中(小写字符串)
    /// </summary>
    static List<string> s_AssetDirs = new List<string>
    {
        { "assets/resources/"},
        { "assets/r/" },
    };
  
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
        
- **3.菜单说明**       
    Tools/Asset/Mode/Editor： 切换为Editor开发模式，直接从AssetDatabase加载资源。
    Tools/Asset/Mode/AssetBundle： 切换为AssetBundle模式，需要打包AssetBundle。
    Tools/Asset/生成资源配置：自动生成指定目录（s_AssetDirs配置的）下的所有资源到assets.txt
    Tools/Asset/保存资源配置：保存当前勾选了的资源到assets.txt
    Tools/Asset/Build：打包当前平台的AssetBunlde
    
        
## 其他说明
  勾选了资源后会在Assets/目录下生成一个assets.txt的资源列表文件，保存了当前勾选了的资源和设置的Bundle名。
  打包AssetBundle后也会在生成的根目录生成一个assets.txt的资源列表文件，包括Resources和Bundle资源。
   
