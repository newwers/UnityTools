#if UNITY_EDITOR  //在unity编辑模式下运行，这个编辑模式指的是未打包成程序之前，在unity中都为编辑模式，包含点击了Play后也是编辑模式


using UnityEngine;
using UnityEditor;//编辑器用到的命名空间
using System.IO;

public class MyScriptTools  {

    public static GameObject cubePrefab;

    //static string[] folderName = new string[] { @"\_MyScenes", @"\_MyMaterials", @"\_MyTextures", @"\_MyModels", @"\_MyPrefabs", @"\_MyAudio", @"\_MyScripts", @"\_MyAnimators" };

	[MenuItem("MyTools/CreatFolder")]//菜单选项方法必须为静态
    static  void CreateFolder()
    {
        //Debug.Log( Application.dataPath);//这个是在当前项目路径下的asset下(E:/Unity Projects/Vuforia AR01/Assets)

        string[] folderName = new string[] { "/__MyScenes", "/_MyMaterials", "/_MyTextures", "/_MyModels", "/_MyPrefabs", "/_MyAudio","/__MyScripts", "/_MyAnimators", "/_MyShader", "/Resources", "/Editor" , "/Gizmos" , "/Plugins", "/StreamingAssets" };

        for (int i = 0; i < folderName.Length; i++)
        {
            string path = Application.dataPath+folderName[i];//_MyScenes,_MyMaterials,_MyTextures,_MyModels,_MyPrefabs,_MyAudio,_MyScripts,_MyAnimators

            

            if (Directory.Exists(path))//如果不存在，则创建文件夹,用的是Directory而不是File，一个是对文件夹操作一个是对文件
            {
                Debug.Log("已存在" + folderName[i] + "文件夹");
            }
            else
            {
                Directory.CreateDirectory(path);//创建文件夹，
                Debug.Log("创建" + folderName[i] + "成功！");
            }
        }

        if (File.Exists(Application.dataPath + "/MyScriptTools.cs"))//如果当前目录下存在这个文件
        {
            //将自身的脚本移动到编辑文件目录下，因为打包编译的时候不允许有命名空间为UnityEditor;的，放在Editor文件下不会编译
            File.Move(Application.dataPath + "/MyScriptTools.cs", Application.dataPath + "/Editor/MyScriptTools.cs");
        }

        
        //移动完后，在原来的位置还是有文件
        //File.Delete(Application.dataPath + "/MyScriptTools.cs");

        AssetDatabase.Refresh();//刷新Project项目，能看得到新建的文件夹
        
    }

    /// <summary>
    /// 创建cube
    /// </summary>
    ///[MenuItem("MyTools/CreateCubes")]
    static void CreateCubes()//不可以有参数
    {
        Vector3 v3 = Vector3.zero;
       

        
        //生成25个方块，5*5
        for (int x = 0; x < 5; x++)
        {
            //(Mathf.Sqrt(25) - 1) / 2;
            for (int y = 0; y < 5; y++)
            {

                v3 = new Vector3(x-2, 0, y-2);
                //生成cube
                cubePrefab=Resources.Load("Cube") as GameObject;

                GameObject cube = GameObject.Instantiate(cubePrefab, v3, Quaternion.identity);
                //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = v3;
                //cube.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 1);//改变颜色
                Undo.RegisterCreatedObjectUndo(cube, "cubes" );//记录创建的cube，用RegisterCreatedObjectUndo。参数变量用RecordObject
            }

            
        }
        

    }
    /// <summary>
    /// 测试菜单方法
    /// </summary>
    [MenuItem("MyTools/Hi")]
    static void TestMenu()
    {
        Debug.Log("Hi");
    }

}

#endif
