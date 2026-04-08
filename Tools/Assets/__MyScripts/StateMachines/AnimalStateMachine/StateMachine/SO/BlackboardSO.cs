using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "Blackboard", menuName = "StateMachine/Blackboard", order = 1)]
    public class BlackboardSO : ScriptableObject
    {
        [Header("Movement Settings")]
        [Tooltip("Whether random walk target positions should be within screen bounds")]
        public bool isRandomWalkWithinScreenBounds = false;

        [Header("Fly Settings")]
        [Tooltip("Initial position when starting to fly, used for returning to original height")]
        public Vector3 flyInitialPosition;

        [Header("Collision Settings")]
        [Tooltip("Current object that the animal is collided with")]
        private GameObject currentCollidedObject;

        public void SetCurrentCollidedObject(GameObject obj)
        {
            //LogManager.Log($"碰撞到:{obj.name} 并记录");
            currentCollidedObject = obj;
        }

        public GameObject GetCurrentCollidedObject()
        {
            return currentCollidedObject;
        }
    }
}