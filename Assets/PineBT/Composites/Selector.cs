using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT.Composites
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
        public Selector(string name) : this(name, new List<Node>())
        {}

        /// <summary>
        /// Creates a Selector with a custom name and a provided children node list.
        /// </summary>
        public Selector(string name, List<Node> nodes) : base(name, nodes)
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
        protected override void ChildRunning(Node child)
        {
            // Set the running child, and notify up
            runningChild = child;
            Running();
        }
    }
}