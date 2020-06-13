namespace PineBT
{
    /// <summary>
    /// <para>Parallel node executes all of its children in the same update.
    /// The children are executed in sequence and in the order they are added to the Parallel node.
    /// The Parallel node has 2 options to control execution and state, <see cref="Policy"/> and <see cref="Executor"/></para>
    /// <para></para>
    /// The method of execution is set by the <see cref="Executor"/>.
    /// <list type="bullet">
    /// <item><description>
    /// <see cref="Executor.ENTIRE"/>: All children will either start or continue execution on each update. ENTIRE is the default Executor.
    /// </description></item>
    /// <item><description>
    /// <see cref="Executor.REMAINING"/>: Children will execute until they succeed or fail and will nto be re-executed
    /// the Parallel node has succeeded or failed and the children have been reset.
    /// Children in a <see cref="State.RUNNING"/> state will be executed each update until they either succeed or fail.
    /// </description></item>
    /// </list>
    /// <para></para>
    /// 
    /// The execution state is set by the <see cref="Policy"/>.
    /// <list type="bullet">
    /// <item><description>
    /// <see cref="Policy.SEQUENCE"/>: If one child fails the parallel node fails and all remaining children will be cancelled.
    /// If all children succeed the parallel node succeeds. SEQUENCE is the default Policy.
    /// </description></item>
    /// <item><description>
    /// <see cref="Policy.SELECTOR"/>: If one child succeeds the parallel node succeeds and all remaining children will be cancelled.
    /// If all children fail the parallel node fails.
    /// </description></item>
    /// <item><description>
    /// <see cref="Policy.SEQUENCE_CONTINUE"/>: If one child fails the parallel node will fail but will not return a fail state until
    /// all remaining children have completed their execution. If all children succeed the parallel node succeeds.
    /// </description></item>
    /// <item><description>
    /// <see cref="Policy.SELECTOR_CONTINUE"/>: If one child succeeds the parallel node succeeds but will not return a success state until
    /// all remaining children have completed their execution. If all children fail the parallel node fails.
    /// </description></item>
    /// </list>
    /// </summary>
    public class Parallel : Composite
    {
        /// <summary>The Parallel node's State policy.</summary>
        private Policy policy;
        /// <summary>The Parallel node's Execution policy.</summary>
        private Executor executor;
        /// <summary>Whether there are running children.</summary>
        private bool hasRunningChildren;
        
        /// <summary>Number of children that have succeeded.</summary>
        private int childrenSucceeded = 0;
        /// <summary>Number of children that have failed.</summary>
        private int childrenFailed = 0;
        /// <summary>Number of children that are currently running.</summary>
        private int childrenRunning = 0;
        /// <summary>The State of the last child that has executed.</summary>
        private State lastChildState;
        /// <summary>The final State the Parallel node will return. Used in the 2 CONTINUE policies.</summary>
        private State finalState;

        /// <summary>
        /// Parallel node with a custom name.
        /// Default Sequence Policy and default Entire Executor.
        /// </summary>
        /// <param name="name">Name of the Parallel Node.</param>
        public Parallel(string name) : this(name, Policy.SEQUENCE, Executor.ENTIRE)
        {}

        /// <summary>
        /// Parallel node with a custom name and provided set of children nodes.
        /// Default Sequence Policy and default Entire Executor.
        /// </summary>
        /// <param name="name">Name of the Parallel Node.</param>
        /// <param name="nodes">Set of children nodes.</param>
        public Parallel(string name, params Node[] nodes) : this(name, Policy.SEQUENCE, Executor.ENTIRE, nodes)
        {}

        /// <summary>
        /// Parallel node with a custom name and provided Policy.
        /// Default Entire Executor.
        /// </summary>
        /// <param name="name">Name of the Parallel Node.</param>
        /// <param name="policy">Parallel State Policy.</param>
        public Parallel(string name, Policy policy) : this(name, policy, Executor.ENTIRE)
        {}

        /// <summary>
        /// Parallel node with a custom name and provided Executor.
        /// Default Sequence Policy.
        /// </summary>
        /// <param name="name">Name of the Parallel Node.</param>
        /// <param name="executor">Parallel Executor method.</param>
        public Parallel(string name, Executor executor) : this(name, Policy.SEQUENCE, executor)
        {}

        /// <summary>
        /// Parallel node with a custom name and provided Policy and Executor.
        /// </summary>
        /// <param name="name">Name of the Parallel Node.</param>
        /// <param name="policy">Parallel State Policy.</param>
        /// <param name="executor">Parallel Executor method.</param>
        public Parallel(string name, Policy policy, Executor executor) : base(name)
        {
            this.policy = policy;
            this.executor = executor;
        }

        /// <summary>
        /// Parallel node with a custom name, provided Policy and Executor, and provided set of children nodes.
        /// </summary>
        /// <param name="name">Name of the Parallel Node.</param>
        /// <param name="policy">Parallel State Policy.</param>
        /// <param name="executor">Parallel Executor method.</param>
        /// <param name="nodes">Set of children nodes.</param>
        public Parallel(string name, Policy policy, Executor executor, params Node[] nodes) : base(name, nodes)
        {
            this.policy = policy;
            this.executor = executor;
        }

        /// <summary>
        /// Called before Parallel begins Execution.
        /// Resets the currentChildIndex and number of succeeded, failed, and running children.
        /// </summary>
        public override void Start()
        {
            currentChildIndex = 0;
            childrenSucceeded = 0;
            childrenFailed = 0;
            childrenRunning = 0;
        }

        /// <summary>
        /// Parallel's execution function, called on each update.
        /// Calls on of the Executor functions specified by the Parallel's <see cref="Executor"/> policy.
        /// </summary>
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

        /// <summary>
        /// Called after Parallel finishes Execution.
        /// Resets the currentChildIndex and number of succeeded, failed, and running children.
        /// </summary>
        public override void Finish()
        {
            currentChildIndex = 0;
            childrenSucceeded = 0;
            childrenFailed = 0;
            childrenRunning = 0;
        }

        /// <summary>
        /// The Execution function for the <see cref="Executor.ENTIRE"/> policy.
        /// </summary>
        private void EntireExecute()
        {
            hasRunningChildren = false;
            lastChildState = State.NONE;
            for (currentChildIndex = 0; currentChildIndex < children.Count; currentChildIndex++)
            {
                Node child = children[currentChildIndex];
                ChildExecute(child);

                if (lastChildState == State.SUCCESS || lastChildState == State.FAILURE)
                {
                    // A non CONTINUE policy is in place, cancel all running children.
                    CancelRunningChildren(hasRunningChildren ? 0 : currentChildIndex + 1);
                    if (lastChildState == State.SUCCESS)
                        Success();
                    else
                        Fail();
                    return;
                }
            }

            if (finalState != State.NONE)
            {
                // The Policy is a CONTINUE policy, return a final state.
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

        /// <summary>
        /// The Execution function for the <see cref="Executor.REMAINING"/> policy.
        /// </summary>
        private void RemainingExecute()
        {
            hasRunningChildren = false;
            lastChildState = State.NONE;
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

                if (lastChildState == State.SUCCESS || lastChildState == State.FAILURE)
                {
                    // A non CONTINUE policy is in place, cancel all running children.
                    CancelRunningChildren(hasRunningChildren ? 0 : currentChildIndex + 1);
                    // Reset children so they can be executed again
                    ResetChildren();
                    if (lastChildState == State.SUCCESS)
                        Success();
                    else
                        Fail();
                    return;
                }
            }
            
            if (finalState != State.NONE)
            {
                // The Policy is a CONTINUE policy, return a final state.
                switch (finalState)
                {
                    case State.SUCCESS:
                        ResetChildren();
                        Success();
                        break;
                    case State.FAILURE:
                        ResetChildren();
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

        /// <summary>
        /// Standard child execution sequence.
        /// </summary>
        /// <param name="child">Child to be executed.</param>
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

        /// <summary>
        /// Called when a child has returned a <see cref="State.SUCCESS"/>.
        /// The <see cref="Policy"/> determines Parallel's state returned.
        /// </summary>
        /// <param name="child">The child that was successful.</param>
        protected override void ChildSuccess(Node child)
        {
            childrenSucceeded++;
            switch (policy)
            {
                case Policy.SEQUENCE:
                    switch (executor)
                    {
                        case Executor.ENTIRE:
                            lastChildState = !hasRunningChildren && currentChildIndex == children.Count - 1
                                ? State.SUCCESS
                                : State.NONE;
                            break;
                        case Executor.REMAINING:
                            // If there are no running children and the last child has succeeded
                            // set status to success
                            lastChildState = !hasRunningChildren && children.Last().State == State.SUCCESS
                                ? State.SUCCESS
                                : State.NONE;
                            break;
                    }
                    break;
                case Policy.SEQUENCE_CONTINUE:
                    // If there are running children, State is Running
                    // otherwise if a failure has occured keep the failure
                    if (hasRunningChildren)
                        finalState = State.RUNNING;
                    else
                        finalState = (finalState != State.FAILURE) ? State.SUCCESS : State.FAILURE;
                    break;
                case Policy.SELECTOR:
                    lastChildState = State.SUCCESS;
                    break;
                case Policy.SELECTOR_CONTINUE:
                    finalState = !hasRunningChildren ? State.SUCCESS : State.RUNNING;
                    break;
            }
        }

        /// <summary>
        /// Called when a child has returned a <see cref="State.FAILURE"/>.
        /// The <see cref="Policy"/> determines Parallel's state returned.
        /// </summary>
        /// <param name="child">The child that failed.</param>
        protected override void ChildFailure(Node child)
        {
            childrenFailed++;
            switch (policy)
            {
                case Policy.SEQUENCE:
                    lastChildState = State.FAILURE;
                    break;
                case Policy.SEQUENCE_CONTINUE:
                    finalState = !hasRunningChildren ? State.FAILURE : State.RUNNING;
                    break;
                case Policy.SELECTOR:
                    // if all children have been executed and are not running, set State Failure 
                    lastChildState = !hasRunningChildren && currentChildIndex == children.Count - 1 
                        ? State.FAILURE 
                        : State.NONE;
                    break;
                case Policy.SELECTOR_CONTINUE:
                    // If there are running children, State is Running
                    // otherwise if a success has occured keep the success
                    if (hasRunningChildren)
                        finalState = State.RUNNING;
                    else
                        finalState = (finalState != State.SUCCESS) ? State.FAILURE : State.SUCCESS;
                    break;
            }
        }

        /// <summary>
        /// Called when a child is still in a <see cref="State.RUNNING"/>.
        /// </summary>
        /// <param name="child">The child still running.</param>
        protected override void ChildRunning(Node child)
        {
            childrenRunning++;
            hasRunningChildren = true;
        }
        
        /// <summary>
        /// The Policy determines the Parallel's state when children have executed and returned their State.
        /// The Policy also factors how the Parallel will execute its children.
        /// </summary>
        public enum Policy
        {
            /// <summary>
            /// If one child fails the parallel node fails and all remaining children will be cancelled.
            /// If all children succeed the parallel node succeeds. SEQUENCE is the default Policy.
            /// </summary>
            SEQUENCE,
            /// <summary>
            /// If one child succeeds the parallel node succeeds and all remaining children will be cancelled.
            /// If all children fail the parallel node fails.
            /// </summary>
            SELECTOR,
            /// <summary>
            /// If one child fails the parallel node will fail but will not return a fail state until
            /// all remaining children have completed their execution. If all children succeed the parallel node succeeds.
            /// </summary>
            SEQUENCE_CONTINUE,
            /// <summary>
            /// If one child succeeds the parallel node succeeds but will not return a success state until
            /// all remaining children have completed their execution. If all children fail the parallel node fails.
            /// </summary>
            SELECTOR_CONTINUE
        }

        /// <summary>
        /// The Executor factors in how the Parallel will execute its children.
        /// </summary>
        public enum Executor
        {
            /// <summary>
            /// All children will either start or continue execution on each update. ENTIRE is the default Executor.
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
