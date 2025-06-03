
using UnityEngine;


namespace BehaviorTree
{
    public class Repeat : DecoratorNode
    {
        private int repeatCount;
        private int currentCount = 0;

        public Repeat(int repeatCount = -1) // -1 -> infinite
        {
            this.repeatCount = repeatCount;
        }

        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            if (repeatCount != -1 && currentCount >= repeatCount)
            {
                state = NodeState.Success;
                return state;
            }

            switch (child.Evaluate())
            {
                case NodeState.Success:
                    child.Reset();
                    currentCount++;

                    if (currentCount >= repeatCount)
                    {
                        state = NodeState.Success;
                        return state;
                    }
                    break;

                case NodeState.Failure:
                    child.Reset();
                    state = NodeState.Failure;
                    return state;

                case NodeState.Running:
                    state = NodeState.Running;
                    return state;
            }

            child.Reset();
            state = NodeState.Running;
            return state;
        }

        public override void Reset()
        {
            base.Reset();
            currentCount = 0;
        }
    }

    public class Inverter : DecoratorNode
    {
        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            switch (child.Evaluate())
            {
                case NodeState.Success:
                    child.Reset();
                    state = NodeState.Failure;
                    break;
                case NodeState.Failure:
                    child.Reset();
                    state = NodeState.Success;
                    break;
                case NodeState.Running:
                    state = NodeState.Running;
                    break;
            }

            return state;
        }
    }


    public class Retry : DecoratorNode
    {
        private int maxRetries;
        private int currentRetries = 0;

        public Retry(int maxRetries)
        {
            this.maxRetries = maxRetries;
        }

        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            switch (child.Evaluate())
            {
                case NodeState.Success:
                    currentRetries = 0;
                    state = NodeState.Success;
                    return state;

                case NodeState.Running:
                    state = NodeState.Running;
                    return state;

                case NodeState.Failure:
                    currentRetries++;

                    if (currentRetries < maxRetries)
                    {
                        child.Reset(); // 재시도를 위해 리셋
                        state = NodeState.Running;
                        return state;
                    }
                    else
                    {
                        currentRetries = 0;
                        state = NodeState.Failure;
                        return state;
                    }
            }

            state = NodeState.Failure;
            return state;
        }

        public override void Reset()
        {
            base.Reset();
            currentRetries = 0;
        }
    }

    public class ForceSuccess : DecoratorNode
    {
        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Success;
                return state;
            }

            NodeState childState = child.Evaluate();

            if (childState == NodeState.Running)
            {
                state = NodeState.Running;
            }
            else
            {
                state = NodeState.Success;
            }

            return state;
        }
    }


    public class ForceFailure : DecoratorNode
    {
        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            NodeState childState = child.Evaluate();

            if (childState == NodeState.Running)
            {
                state = NodeState.Running;
            }
            else
            {
                state = NodeState.Failure;
            }

            return state;
        }
    }


    public class Timeout : DecoratorNode
    {
        private float timeout;
        private float startTime;
        private bool isRunning = false;

        public Timeout(float timeout)
        {
            this.timeout = timeout;
        }

        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            if (!isRunning)
            {
                startTime = Time.time;
                isRunning = true;
            }

            // time over
            if (Time.time - startTime >= timeout)
            {
                child.Reset();
                isRunning = false;
                state = NodeState.Failure;
                return state;
            }

            NodeState childState = child.Evaluate();

            if (childState != NodeState.Running)
            {
                isRunning = false;
            }

            state = childState;
            return state;
        }

        public override void Reset()
        {
            base.Reset();
            isRunning = false;
        }
    }

    public class Delay : DecoratorNode
    {
        private float delay;
        private float startTime;
        private bool complete = false;

        public Delay(float delay)
        {
            this.delay = delay;
        }

        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            if (!complete)
            {
                if (startTime == 0)
                {
                    startTime = Time.time;
                }

                if (Time.time - startTime >= delay)
                {
                    complete = true;
                }
                else
                {
                    state = NodeState.Running;
                    return state;
                }
            }

            // 딜레이 끝
            NodeState childState = child.Evaluate();
            state = childState;
            return state;
        }

        public override void Reset()
        {
            base.Reset();
            startTime = 0;
            complete = false;
        }
    }

    public class UntilSuccess : DecoratorNode
    {
        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            NodeState childState = child.Evaluate();

            switch (childState)
            {
                case NodeState.Success:
                    state = NodeState.Success;
                    return state;

                case NodeState.Running:
                    state = NodeState.Running;
                    return state;

                case NodeState.Failure:
                    child.Reset();
                    state = NodeState.Running;
                    return state;
            }

            state = NodeState.Running;
            return state;
        }
    }

    public class UntilFailure : DecoratorNode
    {
        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Success;
                return state;
            }

            NodeState childState = child.Evaluate();

            switch (childState)
            {
                case NodeState.Failure:
                    state = NodeState.Success;
                    return state;

                case NodeState.Running:
                    state = NodeState.Running;
                    return state;

                case NodeState.Success:
                    child.Reset();
                    state = NodeState.Running;
                    return state;
            }

            state = NodeState.Running;
            return state;
        }
    }
}