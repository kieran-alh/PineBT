using UnityEngine;

namespace PineBT
{
    /// <summary>
    /// The abstract base class of all nodes that have 1 child.
    /// </summary>
    public abstract class Decorator : Node
    {
        /// <summary>The Decorator's child.</summary>
        protected Node child;

        /// <summary>Creates a Decorator with a custom name.</summary>
        /// <param name="name">Name of Decorator Node.</param>
        public Decorator(string name) : base(name)
        {}

        /// <summary>Creates a Decorator with a custom name, and provided child.</summary>
        /// <param name="name">Name of Decorator Node.</param>
        /// <param name="child">Decorator's child.</param>
        public Decorator(string name, Node child) : base(name)
        {
            this.child = child;
            child.SetParent(this);
        }

        /// <summary>
        /// Adds a child to the Decorator and sets the child's parent to the Decorator.
        /// </summary>
        /// <param name="child">Decorator's child to be added.</param>
        public void AddChild(Node child)
        {
            if (this.child == null)
            {
                this.child = child;
                this.child.SetParent(this);
            }
        #if UNITY_EDITOR
            else
            {
                Debug.LogError("Decorator nodes can have only one child node.");
            }
        #endif
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
        /// <param name="child">Successful child.</param>
        protected override void ChildSuccess(Node child)
        {
            Success();
        }

        /// <summary>
        /// Called by the child when the child fails.
        /// </summary>
        /// <param name="child">Failed child.</param>
        protected override void ChildFailure(Node child)
        {
            Fail();
        }

        /// <summary>
        /// Called by the child when the child is still running.
        /// </summary>
        /// <param name="child">Running child.</param>
        protected override void ChildRunning(Node child)
        {
            Running();
        }
    }
}
