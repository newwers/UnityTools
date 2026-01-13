using UnityEditor;
using UnityEngine;

namespace Senses.Editor
{
    public class SenseSystemEditor : EditorWindow
    {
        [MenuItem("Tools/Sense System/Add Sense System to Selected")]
        public static void AddSenseSystemToSelected()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogError("No GameObject selected. Please select a GameObject to add the sense system to.");
                return;
            }

            GameObject selectedObject = Selection.activeGameObject;
            SenseSystemManager senseManager = selectedObject.GetComponent<SenseSystemManager>();

            if (senseManager == null)
            {
                senseManager = selectedObject.AddComponent<SenseSystemManager>();
                Debug.Log($"Added SenseSystemManager to {selectedObject.name}");
            }
            else
            {
                Debug.Log($"SenseSystemManager already exists on {selectedObject.name}");
            }

            // 确保添加了视觉和听力系统
            VisionSense visionSense = selectedObject.GetComponent<VisionSense>();
            if (visionSense == null)
            {
                visionSense = selectedObject.AddComponent<VisionSense>();
                senseManager.visionSense = visionSense;
                Debug.Log($"Added VisionSense to {selectedObject.name}");
            }

            HearingSense hearingSense = selectedObject.GetComponent<HearingSense>();
            if (hearingSense == null)
            {
                hearingSense = selectedObject.AddComponent<HearingSense>();
                senseManager.hearingSense = hearingSense;
                Debug.Log($"Added HearingSense to {selectedObject.name}");
            }

            // 选择新添加的组件以便用户可以在Inspector中查看
            Selection.activeGameObject = selectedObject;
        }

        [MenuItem("Tools/Sense System/Open Sense System Documentation")]
        public static void OpenSenseSystemDocumentation()
        {
            Debug.Log("Sense System Documentation:\n" +
                      "1. SenseSystemManager: Main manager for all sense systems\n" +
                      "2. VisionSense: Handles line-of-sight detection with configurable parameters\n" +
                      "3. HearingSense: Handles sound detection with distance and obstruction effects\n" +
                      "4. SoundSource: Component to attach to objects that produce sound\n\n" +
                      "Usage:\n" +
                      "- Add SenseSystemManager to any GameObject\n" +
                      "- Configure vision and hearing parameters in the Inspector\n" +
                      "- Attach SoundSource components to objects that should produce sound\n" +
                      "- Listen to OnSenseEvent for detection events\n" +
                      "- Use GetCurrentSenseEvents() to query current detections\n");
        }
    }
}
