namespace PineBT
{
    /// <summary> 
    /// The Sequence runs its children until a child returns failure, 
    /// then the Sequence returns failure.
    /// If all children succeed, then the Sequence returns success.
    /// </summary>
    public class Sequence : Composite
    {
        /// <summary>Creates a Sequence with a default name.</summary>
        public Sequence() : this("Sequence")
        {}

        /// <summary>
        /// Creates a Sequence with a custom name and instantiates the children node list.
        /// </summary>
        /// <param name="name">Name of the Sequence.</param>
        public Sequence(string name) : base(name)
        {}

        /// <summary>
        /// Creates a Sequence with a default name and a provided set of children.
        /// </summary>
        /// <param name="nodes">Set of children nodes.</param>
        public Sequence(params Node[] nodes) : base("Sequence", nodes)
        {}

        /// <summary>
        /// Creates a Sequence with a custom name and a provided set of children.
        /// </summary>
        /// <param name="name">Name of the Sequence.</param>
        /// <param name="nodes">Set of children nodes.</param>
        public Sequence(string name, params Node[] nodes) : base(name, nodes)
        {}
        
        /// <summary>
        /// Called before the Sequence begins Executing its children.
        /// Resets the currently running child, and current child index.
        /// </summary>
        public override void Start()
        {
            runningChild = null;
            currentChildIndex = 0;
        }

        /// <summary>
        /// Called after the Sequence has Executed its children.
        /// Resets the currently running child.
        /// </summary>
        public override void Finish()
        {
            runningChild = null;
            currentChildIndex = 0;
        }

        /// <summary>
        /// Called when a child succeeds. 
        /// If there are more children to execute, then next child is executed.
        /// If all children have been executed, the Sequence succeeds.
        /// </summary>
        /// <param name="child">Successful child.</param>
        protected override void ChildSuccess(Node child)
        {
            runningChild = null;
            currentChildIndex++;
            if (currentChildIndex < children.Count)
            {
                Execute();
            }
            else
            {
                Success();
            }
        }

        /// <summary>
        /// Called when at least one child fails.
        /// </summary>
        /// <param name="child">Failed child.</param>
        protected override void ChildFailure(Node child)
        {
            runningChild = null;
            Fail();
        }

        /// <summary>
        /// Called when a child is still running and sets the child as the running child.
        /// </summary>
        /// <param name="child">Running child.</param>
        protected override void ChildRunning(Node child)
        {
            // Set the running child, and notify up
            runningChild = child;
            Running();
        }
    }
}