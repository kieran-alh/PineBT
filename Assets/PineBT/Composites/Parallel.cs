using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    public class Parallel : Composite
    {
        private Policy policy;
        private Executor executor;
        private bool hasRunningChildren;
        
        private int childrenSucceeded = 0;
        private int childrenFailed = 0;
        private int childrenRunning = 0;
        private bool? lastChildState;
        private State? finalState;

        public Parallel(string name) : this(name, Policy.SEQUENCE, Executor.ENTIRE)
        {}

        public Parallel(string name, params Node[] nodes) : this(name, Policy.SEQUENCE, Executor.ENTIRE, nodes)
        {}

        public Parallel(string name, Policy policy) : this(name, policy, Executor.ENTIRE)
        {}

        public Parallel(string name, Executor executor) : this(name, Policy.SEQUENCE, executor)
        {}

        public Parallel(string name, Policy policy, Executor executor) : base(name)
        {
            this.policy = policy;
            this.executor = executor;
        }

        public Parallel(string name, Policy policy, Executor executor, params Node[] nodes) : base(name, nodes)
        {
            this.policy = policy;
            this.executor = executor;
        }

        public override void Start()
        {
            currentChildIndex = 0;
            childrenSucceeded = 0;
            childrenFailed = 0;
            childrenRunning = 0;
        }

        public override void Execute()
        {
            switch (executor)
            {
                case Executor.ENTIRE:
                    EntireExecute();
                    break;
                case Executor.REMAINING:
                    RemainingExecute();
                    break;
            }
        }

        public override void Finish()
        {
            currentChildIndex = 0;
            childrenSucceeded = 0;
            childrenFailed = 0;
            childrenRunning = 0;
        }

        private void EntireExecute()
        {
            hasRunningChildren = false;
            lastChildState = null;
            for (currentChildIndex = 0; currentChildIndex < children.Count; currentChildIndex++)
            {
                Node child = children[currentChildIndex];
                ChildExecute(child);

                if (lastChildState != null)
                {
                    CancelRunningChildren(hasRunningChildren ? 0 : currentChildIndex + 1);
                    if ((bool)lastChildState)
                        Success();
                    else
                        Fail();
                    return;
                }
            }

            if (finalState != null)
            {
                switch (finalState)
                {
                    case State.SUCCESS:
                        Success();
                        break;
                    case State.FAILURE:
                        Fail();
                        break;
                    default:
                        Running();
                        break;
                }
                return;
            }
            Running();
        }

        private void RemainingExecute()
        {
            hasRunningChildren = false;
            lastChildState = null;
            for (currentChildIndex = 0; currentChildIndex < children.Count; currentChildIndex++)
            {
                Node child = children[currentChildIndex];
                State childState = child.State;
                switch (childState)
                {
                    case State.RUNNING:
                        child.Execute();
                        break;
                    case State.SUCCESS:
                    case State.FAILURE:
                        break;
                    default:
                        ChildExecute(child);
                        break;
                }

                if (lastChildState != null)
                {
                    CancelRunningChildren(hasRunningChildren ? 0 : currentChildIndex + 1);
                    ResetChildren();
                    if ((bool)lastChildState)
                        Success();
                    else
                        Fail();
                    return;
                }
            }
            
            if (finalState != null)
            {
                switch (finalState)
                {
                    case State.SUCCESS:
                        Success();
                        break;
                    case State.FAILURE:
                        Fail();
                        break;
                    default:
                        Running();
                        break;
                }
                return;
            }
            Running();
        }

        private void ChildExecute(Node child)
        {
            if (child.State == State.RUNNING)
            {
                child.Execute();
            }
            else
            {
                child.SetParent(this);
                child.Start();
                child.Execute();
            }
        }

        protected override void ChildSuccess(Node child)
        {
            childrenSucceeded++;
            switch (policy)
            {
                case Policy.SEQUENCE:
                    lastChildState = !hasRunningChildren && children.Last().State == State.SUCCESS
                        ? (bool?) true
                        : null;
                    break;
                case Policy.SEQUENCE_CONTINUE:
                    // If finalState hasn't been set to Failure yet, set Success
                    finalState = (finalState != State.FAILURE) ? State.SUCCESS : State.FAILURE;
                    break;
                case Policy.SELECTOR:
                    lastChildState = true;
                    break;
                case Policy.SELECTOR_CONTINUE:
                    finalState = !hasRunningChildren ? State.SUCCESS : State.RUNNING;
                    break;
            }
        }

        protected override void ChildFailure(Node child)
        {
            childrenFailed++;
            switch (policy)
            {
                case Policy.SEQUENCE:
                    lastChildState = false;
                    break;
                case Policy.SEQUENCE_CONTINUE:
                    finalState = !hasRunningChildren ? State.FAILURE : State.RUNNING;
                    break;
                case Policy.SELECTOR:
                    lastChildState = !hasRunningChildren && currentChildIndex == children.Count - 1 ? (bool?)false : null;
                    break;
                case Policy.SELECTOR_CONTINUE:
                    finalState = (finalState != State.SUCCESS) ? State.FAILURE : State.SUCCESS;
                    break;
            }
        }

        protected override void ChildRunning(Node child)
        {
            childrenRunning++;
            hasRunningChildren = true;
        }

        public enum Policy
        {
            /// <summary>
            /// If one child fails the parallel node fails and all remaining children will be cancelled.
            /// If all children succeed the parallel node succeeds.
            /// SEQUENCE is the default Policy.
            /// </summary>
            SEQUENCE,
            /// <summary>
            /// If one child succeeds the parallel node succeeds and all remaining children will be cancelled.
            /// If all children fail the parallel node fails.
            /// </summary>
            SELECTOR,
            /// <summary>
            /// If one child fails the parallel node fails but all remaining children will continue to execute.
            /// If all children succeed the parallel node succeeds.
            /// </summary>
            SEQUENCE_CONTINUE,
            /// <summary>
            /// If one child succeeds the parallel node succeeds and all remaining children will continue to execute.
            /// If all children fail the parallel node fails.
            /// </summary>
            SELECTOR_CONTINUE
        }

        public enum Executor
        {
            /// <summary>
            /// All children will either start or continue execution. ENTIRE is the default Executor.
            /// </summary>
            ENTIRE,
            /// <summary>
            /// Only children in a <see cref="State.RUNNING"/> will be executed.
            /// Once the Parallel as either succeeded or failed, the children will be re-executed.
            /// </summary>
            REMAINING
        }
    }
}
