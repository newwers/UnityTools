using UnityEngine;
namespace Z.BehaviourTree
{
    /// <summary>
    /// 创建行为树文件脚本
    /// </summary>
    [CreateAssetMenu(fileName = "行为树文件", menuName = "行为树/创建行为树文件")]
    public class BehaviorTree : ScriptableObject
    {
        public Node rootNode;
        public Node.State state = Node.State.Running;

        public Node.State Update()
        {
            if (rootNode == null)
            {
                Debug.LogError("行为树没有根节点");
                return Node.State.Failure;
            }
            if (state == Node.State.Running)
            {
                state = rootNode.Update();
            }

            return state;
        }
    }
}