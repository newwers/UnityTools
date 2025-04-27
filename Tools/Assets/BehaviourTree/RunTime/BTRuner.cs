using UnityEngine;
using Z.BehaviourTree;

public class BTRuner : MonoBehaviour
{
    public BehaviorTree tree;


    // Start is called before the first frame update
    void Start()
    {
        tree = tree.Clone();

    }

    private void Update()
    {
        tree.Update();
    }

    void Test()
    {
        tree = ScriptableObject.CreateInstance<BehaviorTree>();
        var wait = ScriptableObject.CreateInstance<WaitNode>();
        var log = ScriptableObject.CreateInstance<LogNode>();
        log.message = "Hello World";
        var seq = ScriptableObject.CreateInstance<SequenceNode>();
        var repeat = ScriptableObject.CreateInstance<RepeatNode>();

        seq.AddChildrenNode(wait);
        seq.AddChildrenNode(log);
        repeat.SetChild(seq);
        tree.rootNode = repeat;
    }
}
