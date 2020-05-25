using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    /// <summary> 
    /// The RandomSequence runs its children in a random order 
    /// until a child returns failure, then the RandomSequence returns failure.
    /// If all children succeed, then the RandomSequence returns success.
    /// </summary>
    public class RandomSequence : Composite
    {
        /// <summary>Creates a RandomSequence with a default name.</summary>
        public RandomSequence() : this("RandomSequence")
        {}

        /// <summary>
        /// Creates a RandomSequence with a custom name and instantiates the children node list.
        /// </summary>
        public RandomSequence(string name) : this(name, new List<Node>())
        {}

        /// <summary>
        /// Creates a RandomSequence with a custom name and a provided children node list.
        /// </summary>
        public RandomSequence(string name, List<Node> nodes) : base(name, nodes)
        {}
        
        /// <summary>
        /// Called before the RandomSequence begins Executing its children.
        /// Resets the currently running child, current child index and shuffles the children.
        /// </summary>
        public override void Start()
        {
            runningChild = null;
            currentChildIndex = 0;
            children.Shuffle();
        }

        /// <summary>
        /// Called after the RandomSequence has Executed its children.
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
        /// If all children have been executed, the RandomSequence succeeds.
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