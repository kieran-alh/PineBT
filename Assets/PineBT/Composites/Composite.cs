﻿using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    /// <summary> 
    /// The abstract base class of all nodes that have 1 or more children.
    /// The base Composite node keeps track of a single running child at a time.
    /// </summary>
    public abstract class Composite : Node
    {
        /// <summary>The index of the child that is currently running.</summary>
        protected int currentChildIndex;
        /// <summary>The child that is currently running.</summary>
        protected Node runningChild;
        /// <summary>The composite's list of children.</summary>
        protected List<Node> children;

        /// <summary>
        /// Creates a new composite with a custom name, and a initializes a list of children.
        /// </summary>
        public Composite(string name) : base(name)
        {
            children = new List<Node>();
            runningChild = null;
            currentChildIndex = 0;
        }

        /// <summary>
        /// Creates a new composite with a custom name, and a provided set of children.
        /// </summary>
        public Composite(string name, params Node[] nodes) : base(name)
        {
            children = new List<Node>(nodes);
            runningChild = null;
            currentChildIndex = 0;
        }

        /// <summary>
        /// Adds a child to the Composite and sets the child's parent to the Composite.
        /// </summary>
        public void AddChild(Node child)
        {
            children.Add(child);
            child.SetParent(this);
        }

        /// <summary>
        /// Either continues running the currently running child, 
        /// or starts and executes the next child.
        /// </summary>
        public override void Execute()
        {
            if (runningChild != null)
            {
                runningChild.Execute();
            }
            else
            {
                if (currentChildIndex < children.Count)
                {
                    children[currentChildIndex].SetParent(this);
                    children[currentChildIndex].Start();
                    children[currentChildIndex].Execute();
                }
            #if UNITY_EDITOR
                else
                {
                    Debug.LogError("Out of children to Execute");
                }
            #endif
            }
        }

        public override void Reset()
        {
            base.Reset();
            ResetChildren();
        }

        protected void CancelRunningChildren(int index)
        {
            for (int i = index; i < children.Count; i++)
            {
                if (children[i].State == State.RUNNING)
                    children[i].Cancel();
            }
        }

        protected void ResetChildren()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Reset();
            }
        }
    }
}