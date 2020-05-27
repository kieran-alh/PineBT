using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    /// <summary> 
    /// The BehaviourTree class acts as an overall anchor for every <see cref="Node"/> in the tree.
    /// The BehaviourTree contains the Root node and receives updates from the <see cref="PineTreeManager"/>.
    /// </summary>
    public class BehaviourTree : Node
    {
        /// <summary>The root node of the tree.</summary>
        protected Node root;
        
        private PineTreeManager treeManager = PineTreeUnityContext.GetInstance().TreeManager;

        /// <summary>Constructs a <c>Tree</c> with a basic name, and no root.</summary>
        public BehaviourTree() : this("Tree", null)
        {}

        /// <summary>Constructs a <c>Tree</c> with a custom name, and no root.</summary>
        public BehaviourTree(string name) : this(name, null)
        {}

        /// <summary>Constructs a <c>Tree</c> with a basic name, and a provided root.</summary>
        public BehaviourTree(Node root) : this("Tree", root)
        {}

        /// <summary>Constructs a <c>Tree</c> with a custom name, and a provided root.</summary>
        public BehaviourTree(string name, Node root) : base(name)
        {
            this.root = root;
            this.tree = this;
            if (root != null)
                root.SetParent(this);
        }

        public PineTreeManager TreeManager
        {
            get {return treeManager;}
        }

        /// <summary>Set the Tree's root node.</summary>
        public void SetRoot(Node root)
        {
            if (this.root == null)
            {
                this.root = root;
                root.SetParent(this);
            }
            else
            {
                #if UNITY_EDITOR
                    Debug.LogError($"Tree [{name}] can only have one root.");
                #endif
            }
        }

        /// <summary>
        /// Registers the BehaviourTree to begin receiving updates.
        /// </summary>
        public void Enable()
        {
            PineTreeUnityContext.GetInstance().TreeManager.RegisterTree(this);
        }

        /// <summary>
        /// Unregisters the BehaviourTree from receiving updates.
        /// </summary>
        public void Disable()
        {
            //TODO: Is clean necessary?
            PineTreeUnityContext.GetInstance().TreeManager.UnregisterTree(this);
        }
        
        /// <summary>Called before the <c>Tree</c> begins execution.</summary>
        public override void Start()
        {
            root.Start();
        }

        /// <summary>Set the Tree's root node.</summary>
        public override void Finish()
        {
            root.Finish();
        }

        /// <summary>Contains the operations a <c>Tree</c> will execute on an update.</summary>
        public override void Execute()
        {
            root.Execute();
        }

        /// <summary>
        /// Called to process a new Update on the tree. 
        /// Update is called by the TreeManager after receiving the Update callback from Unity.
        /// </summary>
        public void Update()
        {
            if (root.State == State.RUNNING)
            {
                Execute();
            }
            else
            {
                Start();
                Execute();
            }
        }

        /// <summary>
        /// Called to process a new physics FixedUpdate on the tree.
        /// FixedUpdate is called by the TreeManager after receiving the 
        /// FixedUpdate callback from Unity.
        /// </summary>
        public void FixedUpdate()
        {

        }
        
        /// <summary>
        /// Called by a child node indicating the nodes in the tree have executed their task's 
        /// and returned a success.
        /// </summary>
        /// <param name="child">The child that Succeeded.</param>
        protected override void ChildSuccess(Node child)
        {
            Success();
        }

        /// <summary>
        /// Called by a child node indicating the nodes in the tree have executed their task's 
        /// and returned a failure.
        /// </summary>
        /// <param name="child">The child that Failed.</param>
        protected override void ChildFailure(Node child)
        {
            Fail();
        }

        /// <summary>
        /// A node in the tree is still running and needs to run again next Update cycle.
        /// </summary>
        /// <param name="child">The child that is Running.</param>
        protected override void ChildRunning(Node child)
        {
            Running();
        }
    }
}
