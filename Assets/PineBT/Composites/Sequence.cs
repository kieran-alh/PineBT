using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT.Composites
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
        public Sequence(string name) : this(name, new List<Node>())
        {}

        /// <summary>
        /// Creates a Sequence with a custom name and a provided children node list.
        /// </summary>
        public Sequence(string name, List<Node> nodes) : base(name, nodes)
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
        protected override void ChildFailure(Node child)
        {
            runningChild = null;
            Fail();
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