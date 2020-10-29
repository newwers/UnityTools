using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// 扩展unity编辑器功能
/// </summary>
public class MyUnityExtendScript : MonoBehaviour {



    [MenuItem("Tools/获取场景中选中游戏物体底下所有的Text组件,并选中他们 &1", false, 990)]
    public static void GetSelectGameObjectAllTextComponent()
    {

        GameObject[] selectGameObjects = Selection.gameObjects;
        //获取选中的第一个物体底下的所有text组件,包含隐藏的组件
        Text[] selectTexts = selectGameObjects[0].GetComponentsInChildren<Text>(true);

        GameObject[] TextsGameObjects = new GameObject[selectTexts.Length];



        for (int i = 0; i < selectTexts.Length; i++)
        {
            TextsGameObjects[i] = selectTexts[i].gameObject;
            Debug.Log(selectTexts[i].name);
        }

        Selection.objects = TextsGameObjects;
    }



    [MenuItem("Tools/查找当前脚本挂有那些预置体")]
    static void GetReference()
    {
        string target = "";
        if (Selection.activeObject != null)
            target = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(target))
            return;
        string[] files = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
        string[] scene = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);

        List<Object> filelst = new List<Object>();
        for (int i = 0; i < files.Length; i++)
        {
            string[] source = AssetDatabase.GetDependencies(new string[] { files[i].Replace(Application.dataPath, "Assets") });
            for (int j = 0; j < source.Length; j++)
            {
                if (source[j] == target)
                    filelst.Add(AssetDatabase.LoadMainAssetAtPath(files[i].Replace(Application.dataPath, "Assets")));
            }
        }
        for (int i = 0; i < scene.Length; i++)
        {
            string[] source = AssetDatabase.GetDependencies(new string[] { scene[i].Replace(Application.dataPath, "Assets") });
            for (int j = 0; j < source.Length; j++)
            {
                if (source[j] == target)
                    filelst.Add(AssetDatabase.LoadMainAssetAtPath(scene[i].Replace(Application.dataPath, "Assets")));
            }
        }
        Selection.objects = filelst.ToArray();
    }


    [MenuItem("Tools/设置选中的text组件字体,大小等属性", false, 995)]
    public static void SetSelectText()
    {
        GameObject[] selectGameObjects = Selection.gameObjects;

        Text[] selectTexts = new Text[selectGameObjects.Length];

        for (int i = 0; i < selectGameObjects.Length; i++)
        {
            if (selectGameObjects[i].GetComponent<Text>() != null)
            {
                //这边如果多选中了没有text组件的游戏对象,这边数组的赋值索引就会有问题,所以需要先执行上面选中函数的,再执行这句
                selectTexts[i] = selectGameObjects[i].GetComponent<Text>();
            }
        }

        //设置字体
        //Font font = Resources.Load<Font>("Font/PingFang Medium");

        foreach (var item in selectTexts)
        {
            //PingFang Medium (UnityEngine.Font)
            //Arial (UnityEngine.Font)
            //item.font = font;

            //设置字体大小
            item.fontSize = 32;

            //设置对齐方式
            item.alignment = TextAnchor.MiddleCenter;

            //设置不接受射线点击
            item.raycastTarget = false;

            //设置text大小
            item.rectTransform.sizeDelta = new Vector2(200, 80);
        }
    }


    [MenuItem("Tools/Image替换成RawImage", false, 994)]
    public static void ImageToRawImage()
    {
        GameObject[] selectGameObjects = Selection.gameObjects;
        Undo.RegisterCompleteObjectUndo(selectGameObjects, "images");
        for (int i = 0; i < selectGameObjects.Length; i++)
        {

            Image image = selectGameObjects[i].GetComponent<Image>();
            if (image != null)
            {
                //记录要删除前的组件
                Undo.RegisterCompleteObjectUndo(image, "image" + i);
                Texture texture = null;
                if (image.sprite != null)
                {
                    texture = image.sprite.texture;
                }

                bool isRatcast = image.raycastTarget;
                Color imageColor = image.color;
                DestroyImmediate(image);
                RawImage rawImage = selectGameObjects[i].AddComponent<RawImage>();
                //记录创建出来的组件
                Undo.RegisterCreatedObjectUndo(rawImage, "raw image" + i);
                rawImage.texture = texture;
                rawImage.raycastTarget = isRatcast;
                rawImage.color = imageColor;
            }
        }

    }

    [MenuItem("Tools/修改选中物体名字")]
    public static void ModifyName()
    {
        var selectList = Selection.gameObjects;

        if (selectList.Length <=0)
        {
            Debug.LogError("没有选择物体!!");
            return;
        }
        foreach (var item in selectList)
        {
            Debug.Log(item.name);
        }
        Undo.RecordObjects(selectList, "selectList");
        for (int i = 1; i <= selectList.Length; i++)
        {
            string name = selectList[i - 1].name;
            name = name.Split(' ')[0];
            selectList[i - 1].name = name + i.ToString();
        }

    }

    [MenuItem("Tools/查找某个字符串在哪些文件当中")]
    static void GetReference()
    {
        //确定查找的文件类型
        string[] assets = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories);
        string[] prefabs = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);

        //设置搜索的字符串内容
        string atFunctionID = "1524476877";

        List<Object> filelst = new List<Object>();

        for (int i = 0; i < assets.Length; i++)
        {
            EditorUtility.DisplayProgressBar("搜索asset文件中", assets[i], (float)i / assets.Length);

            string prefab = File.ReadAllText(assets[i]);
            bool isContains = prefab.Contains(atFunctionID);
            if (isContains)
            {
                filelst.Add(AssetDatabase.LoadMainAssetAtPath(assets[i].Replace(Application.dataPath, "Assets")));
                Debug.LogError("匹配到文件名:" + assets[i]);
            }
        }
        EditorUtility.ClearProgressBar();

        for (int i = 0; i < prefabs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("搜索prefab文件中", prefabs[i], (float)i / prefabs.Length);

            string prefab = File.ReadAllText(prefabs[i]);
            bool isContains = prefab.Contains(atFunctionID);
            if (isContains)
            {
                filelst.Add(AssetDatabase.LoadMainAssetAtPath(prefabs[i].Replace(Application.dataPath, "Assets")));
                Debug.LogError("匹配到文件名:" + prefabs[i]);
            }
        }
        EditorUtility.ClearProgressBar();


        Selection.objects = filelst.ToArray();
    }
}
