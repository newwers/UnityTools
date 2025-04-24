using UnityEditor;
using UnityEngine;

namespace Project001.Editor
{
    public class MenuToolsEditor
    {

        [MenuItem("Tools/Editor/启动游戏 _F5")]
        static void PlayGame()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
            else
            {
                //保存当前场景
                var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                if (scene != null && scene.isDirty && !string.IsNullOrWhiteSpace(scene.name) && !string.IsNullOrWhiteSpace(scene.path))
                {
                    if (UnityEditor.EditorUtility.DisplayDialog("提示", "当前场景未保存,是否保存?", "保存", "不保存"))
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                    }
                }
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene.path);
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("Tools/Editor/暂停游戏 _F6")]
        static void PauseGame()
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }

        [MenuItem("Tools/Editor/逐帧游戏 _F7")]
        static void NextStep()
        {
            EditorApplication.Step();
        }
        //------------------------------------------------------

        [MenuItem("Tools/Editor/刷新资源")]
        static void RefreshAssets()
        {
            //EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
        [MenuItem("Assets/复制文件路径")]
        public static void CopyPath()
        {
            if (Selection.objects == null || Selection.objects.Length == 0)
            {
                return;
            }
            GUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(Selection.objects[0]);
        }
    }
}