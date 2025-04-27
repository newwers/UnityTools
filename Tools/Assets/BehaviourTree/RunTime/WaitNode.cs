using UnityEngine;

namespace Z.BehaviourTree
{
    public class WaitNode : ActionNode
    {
        public float waitTime = 1;
        float timer;
        protected override void OnStart()
        {
            timer = Time.time;
        }

        protected override void OnStop()
        {

        }

        protected override State OnUpdate()
        {
            if (timer + waitTime <= Time.time)
            {
                return State.Success;
            }
            else
            {
                return State.Running;
            }
        }
    }
}
