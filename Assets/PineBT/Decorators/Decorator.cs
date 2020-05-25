using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT.Decorators
{
    /// <summary>
    /// The abstract base class of all nodes that have 1 child.
    /// </summary>
    public abstract class Decorator : Node
    {
        /// <summary>The Decorator's child.</summary>
        protected Node child;

        /// <summary>Creates a Decorator with a custom name.</summary>
        public Decorator(string name) : base(name)
        {}

        /// <summary>Creates a Decorator with a custom name, and provided child.</summary>
        public Decorator(string name, Node child) : base(name)
        {
            this.child = child;
            child.SetParent(this);
        }

        /// <summary>
        /// Adds a child to the Decorator and sets the child's parent to the Decorator.
        /// </summary>
        public void AddChild(Node child)
        {
            if (this.child == null)
            {
                this.child = child;
                this.child.SetParent(this);
            }
            else
            {
                #if UNITY_EDITOR
                    Debug.LogError("Decorator nodes can have only one child node.");
                #endif
            }
        }

        /// <summary>
        /// Either continues running the child, 
        /// or starts and executes the child.
        /// </summary>
        public override void Execute()
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
        /// Called by the child when the child succeeds.
        /// </summary>
        protected override void ChildSuccess(Node child)
        {
            Success();
        }

        /// <summary>
        /// Called by the child when the child fails.
        /// </summary>
        protected override void ChildFailure(Node child)
        {
            Fail();
        }

        /// <summary>
        /// Called by the child when the child is still running.
        /// </summary>
        protected override void ChildRunning(Node child)
        {
            Running();
        }
    }
}
