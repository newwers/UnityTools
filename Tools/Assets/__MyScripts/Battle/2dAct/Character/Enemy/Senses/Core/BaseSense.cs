using UnityEngine;

namespace Senses
{
    public enum SenseType
    {
        [InspectorName("视觉")]
        Vision,
        [InspectorName("听觉")]
        Hearing,
    }

    [System.Serializable]
    public class SenseEvent
    {
        public SenseType senseType;
        public GameObject detectedObject;
        public Vector3 sourcePosition;
        public float intensity;
        public float timestamp;
        public string eventTag;

        public SenseEvent(SenseType type, GameObject obj, Vector3 position, float strength = 1.0f, string tag = "")
        {
            senseType = type;
            detectedObject = obj;
            sourcePosition = position;
            intensity = strength;
            timestamp = Time.time;
            eventTag = tag;
        }
    }

    public abstract class BaseSense : MonoBehaviour
    {
        [Tooltip("是否启用此感知模块")]
        public bool isEnabled = true;
        [Tooltip("检测优先级")]
        public int priority = 0;

        protected SenseSystemManager senseManager;

        public delegate void SenseDetectedEventHandler(SenseEvent senseEvent);
        public event SenseDetectedEventHandler OnSenseDetected;

        public virtual void Initialize(SenseSystemManager manager)
        {
            senseManager = manager;
        }

        public abstract void UpdateDetection();
        public abstract void DrawGizmos();

        protected void TriggerSenseEvent(SenseEvent senseEvent)
        {
            OnSenseDetected?.Invoke(senseEvent);
        }

        protected virtual bool IsValidTarget(GameObject target)
        {
            if (target == null || target == gameObject || !target.activeInHierarchy)
                return false;
            return true;
        }
    }
}
