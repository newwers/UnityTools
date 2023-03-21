using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project001.Editor
{
    public class MenuToolsEditor
    {

        [MenuItem("Tools/Editor/������Ϸ _F5")]
        static void PlayGame()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
            else
            {
                //���浱ǰ����
                var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                if (scene != null && scene.isDirty && !string.IsNullOrWhiteSpace(scene.name) && !string.IsNullOrWhiteSpace(scene.path))
                {
                    if (UnityEditor.EditorUtility.DisplayDialog("��ʾ", "��ǰ����δ����,�Ƿ񱣴�?", "����", "������"))
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                    }
                }
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("Tools/Editor/��ͣ��Ϸ _F6")]
        static void PauseGame()
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }

        [MenuItem("Tools/Editor/��֡��Ϸ _F7")]
        static void NextStep()
        {
            EditorApplication.Step();
        }
        //------------------------------------------------------

        [MenuItem("Tools/Editor/ˢ����Դ")]
        static void RefreshAssets()
        {
            //EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
    }
}