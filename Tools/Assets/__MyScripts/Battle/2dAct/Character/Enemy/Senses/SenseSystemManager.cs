using System.Collections.Generic;
using UnityEngine;

namespace Senses
{
    [DisallowMultipleComponent]
    public class SenseSystemManager : MonoBehaviour
    {
        [Header("感知系统配置")]
        [Tooltip("是否启用视觉系统")]
        public bool enableVision = true;
        [Tooltip("是否启用听力系统")]
        public bool enableHearing = true;
        [Tooltip("检测频率 (秒)")]
        public float detectionInterval = 0.1f;

        [Header("视觉系统")]
        public VisionSense visionSense;

        [Header("听力系统")]
        public HearingSense hearingSense;

        private float detectionTimer = 0f;
        private List<SenseEvent> currentSenseEvents = new List<SenseEvent>();

        public delegate void SenseEventHandler(SenseEvent senseEvent);
        public event SenseEventHandler OnSenseEvent;

        public bool IsVisionEnabled => enableVision && visionSense != null;
        public bool IsHearingEnabled => enableHearing && hearingSense != null;

        private void Awake()
        {
            InitializeSenses();
        }

        private void Start()
        {
            if (visionSense != null)
            {
                visionSense.OnSenseDetected += HandleSenseEvent;
            }

            if (hearingSense != null)
            {
                hearingSense.OnSenseDetected += HandleSenseEvent;
            }
        }

        private void Update()
        {
            detectionTimer += Time.deltaTime;
            if (detectionTimer >= detectionInterval)
            {
                PerformDetection();
                detectionTimer = 0f;
            }
        }

        private void InitializeSenses()
        {
            if (visionSense == null)
            {
                visionSense = GetComponent<VisionSense>();
                if (visionSense == null && enableVision)
                {
                    visionSense = gameObject.AddComponent<VisionSense>();
                }
            }

            if (hearingSense == null && enableHearing)
            {
                hearingSense = GetComponent<HearingSense>();
                if (hearingSense == null)
                {
                    hearingSense = gameObject.AddComponent<HearingSense>();
                }
            }

            visionSense?.Initialize(this);
            hearingSense?.Initialize(this);
        }

        private void PerformDetection()
        {
            currentSenseEvents.Clear();

            if (enableVision && visionSense != null)
            {
                visionSense.UpdateDetection();
            }

            if (enableHearing && hearingSense != null)
            {
                hearingSense.UpdateDetection();
            }
        }

        private void HandleSenseEvent(SenseEvent senseEvent)
        {
            currentSenseEvents.Add(senseEvent);
            OnSenseEvent?.Invoke(senseEvent);
        }

        public void SetVisionEnabled(bool enabled)
        {
            enableVision = enabled;
        }

        public void SetHearingEnabled(bool enabled)
        {
            enableHearing = enabled;
        }

        public List<SenseEvent> GetCurrentSenseEvents()
        {
            return new List<SenseEvent>(currentSenseEvents);
        }

        public SenseEvent GetMostRecentSenseEvent()
        {
            if (currentSenseEvents.Count == 0)
                return null;

            return currentSenseEvents[currentSenseEvents.Count - 1];
        }

        public SenseEvent GetMostRecentSenseEventByType(SenseType senseType)
        {
            for (int i = currentSenseEvents.Count - 1; i >= 0; i--)
            {
                if (currentSenseEvents[i].senseType == senseType)
                {
                    return currentSenseEvents[i];
                }
            }
            return null;
        }

        public void ClearSenseEvents()
        {
            currentSenseEvents.Clear();
        }

        private void OnDestroy()
        {
            if (visionSense != null)
            {
                visionSense.OnSenseDetected -= HandleSenseEvent;
            }

            if (hearingSense != null)
            {
                hearingSense.OnSenseDetected -= HandleSenseEvent;
            }
        }
    }
}
