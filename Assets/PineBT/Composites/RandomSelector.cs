using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    /// <summary> 
    /// The RandomSelector runs its children in a random order 
    /// until a child returns success, then the RandomSelector returns success.
    /// If all children fail, then the RandomSelector returns failure.
    /// </summary>
    public class RandomSelector : Composite
    {
        /// <summary>Creates a RandomSelector with a default name.</summary>
        public RandomSelector() : this("RandomSelector")
        {}

        /// <summary>
        /// Creates a RandomSelector with a custom name and instantiates the children node list.
        /// </summary>
        public RandomSelector(string name) : base(name)
        {}

        /// <summary>
        /// Creates a RandomSelector with a default name and a provided set of children.
        /// </summary>
        public RandomSelector(params Node[] nodes) : base("RandomSelector", nodes)
        {}

        /// <summary>
        /// Creates a RandomSelector with a custom name and a provided set of children.
        /// </summary>
        public RandomSelector(string name, params Node[] nodes) : base(name, nodes)
        {}
        
        /// <summary>
        /// Called before the RandomSelector begins Executing its children.
        /// Resets the currently running child, current child index and shuffles the children.
        /// </summary>
        public override void Start()
        {
            runningChild = null;
            currentChildIndex = 0;
            children.Shuffle();
        }

        /// <summary>
        /// Called after the RandomSelector has Executed its children.
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