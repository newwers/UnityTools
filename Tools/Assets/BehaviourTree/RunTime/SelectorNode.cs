using Z.BehaviourTree;

public class SelectorNode : CompositeNode
{
    int currentIndex = 0;

    protected override void OnStart()
    {
        currentIndex = 0;
    }

    protected override void OnStop() { }

    protected override State OnUpdate()
    {
        var children = childNodes[currentIndex];
        switch (children.Update())
        {
            case State.Running:
                return State.Running;
            case State.Success:
                return State.Success;
            case State.Failure:
                currentIndex++;
                break;
        }
        return currentIndex == childNodes.Count ? State.Failure : State.Running;
    }
}