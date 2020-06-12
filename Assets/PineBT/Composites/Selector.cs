namespace PineBT
{
    /// <summary> 
    /// The Selector runs its children until a child returns success, 
    /// then the Selector returns success.
    /// If all children fail, then the Selector returns failure.
    /// </summary>
    public class Selector : Composite
    {
        /// <summary>Creates a Selector with a default name.</summary>
        public Selector() : this("Selector")
        {}

        /// <summary>
        /// Creates a Selector with a custom name and instantiates the children node list.
        /// </summary>
        /// <param name="name">Name of the Selector.</param>
        public Selector(string name) : base(name)
        {}

        /// <summary>
        /// Creates a Selector with a default name and a provided set of children.
        /// </summary>
        /// <param name="nodes">Set of children nodes.</param>
        public Selector(params Node[] nodes) : base("Selector", nodes)
        {}

        /// <summary>
        /// Creates a Selector with a custom name and a provided set of children.
        /// </summary>
        /// <param name="name">Name of the Selector.</param>
        /// <param name="nodes">Set of children nodes.</param>
        public Selector(string name, params Node[] nodes) : base(name, nodes)
        {}
        
        /// <summary>
        /// Called before the Selector begins Executing its children.
        /// Resets the currently running child, and current child index.
        /// </summary>
        public override void Start()
        {
            runningChild = null;
            currentChildIndex = 0;
        }

        /// <summary>
        /// Called after the Selector has Executed its children.
        /// Resets the currently running child.
        /// </summary>
        public override void Finish()
        {
            runningChild = null;
            currentChildIndex = 0;
        }

        /// <summary>
        /// Called when at least one child succeeds.
        /// </summary>
        /// <param name="child">Successful child.</param>
        protected override void ChildSuccess(Node child)
        {
            runningChild = null;
            Success();
        }

        /// <summary>
        /// Called when a child fails. 
        /// If there are more children to execute, then next child is executed.
        /// If all children have been executed, the Selector fails.
        /// </summary>
        /// <param name="child">Failed child.</param>
        protected override void ChildFailure(Node child)
        {
            currentChildIndex++;
            if (currentChildIndex < children.Count)
            {
                // Run next child
                Execute();
            }
            else
            {
                // The last child has ran, and failed
                Fail();
            }
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