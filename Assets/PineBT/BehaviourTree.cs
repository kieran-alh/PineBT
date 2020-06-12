using UnityEngine;

namespace PineBT
{
    /// <summary> 
    /// The BehaviourTree class acts as an overall anchor for every <see cref="Node"/> in the tree.
    /// The BehaviourTree contains the Root node and receives updates from the <see cref="PineTreeManager"/>.
    /// Each BehaviourTree contains a Blackboard and if a Blackboard isn't provided in construction then
    /// a Blackboard is created.
    /// </summary>
    public class BehaviourTree : Node
    {
        /// <summary>The root node of the tree.</summary>
        protected Node root;

        /// <summary>The Blackboard for the tree.</summary>
        protected Blackboard blackboard;
        
        private PineTreeManager treeManager = PineTreeUnityContext.Instance().TreeManager;

        /// <summary>Constructs a Tree with a basic name, and no root.</summary>
        public BehaviourTree() : this("Tree", null)
        {}

        /// <summary>Constructs a Tree with a custom name, and no root.</summary>
        /// <param name="name">Name of the BehaviourTree.</param>
        public BehaviourTree(string name) : this(name, null)
        {}

        /// <summary>Constructs a Tree with a basic name, and a provided root.</summary>
        /// <param name="root">Tree's Root Node.</param>
        public BehaviourTree(Node root) : this("Tree", root)
        {}

        /// <summary>
        /// Constructs a Tree with a custom name, and a provided root.
        /// Creates a Blackboard.
        /// </summary>
        /// <param name="name">Name of the BehaviourTree.</param>
        /// <param name="root">Tree's Root Node.</param>
        public BehaviourTree(string name, Node root) : this(name, new Blackboard(), root)
        {}

        /// <summary>Constructs a Tree with a provided name, blackboard, and root node.</summary>
        /// <param name="name">Name of the BehaviourTree.</param>
        /// <param name="blackboard">Provided Blackboard for the BehaviourTree.</param>
        /// <param name="root">Tree's Root Node.</param>
        public BehaviourTree(string name, Blackboard blackboard, Node root) : base(name)
        {
            this.root = root;
            this.blackboard = blackboard;
            this.tree = this;
            if (root != null)
                root.SetParent(this);
        } 

        public PineTreeManager TreeManager
        {
            get {return treeManager;}
        }

        public Blackboard Blackboard
        {
            get {return blackboard;}
        }

        /// <summary>Set the Tree's root node.</summary>
        /// <param name="root">Root Node for the BehaviourTree.</param>
        public void SetRoot(Node root)
        {
            if (this.root == null)
            {
                this.root = root;
                root.SetParent(this);
            }
        #if UNITY_EDITOR
            else
            {
                Debug.LogError($"Tree [{name}] can only have one root.");
            }
        #endif
        }

        /// <summary>
        /// Registers the BehaviourTree to begin receiving updates.
        /// </summary>
        public void Enable()
        {
            treeManager.RegisterTree(this);
        }

        /// <summary>
        /// Unregisters the BehaviourTree from receiving updates.
        /// </summary>
        public void Disable()
        {
            if (treeManager != null)
                treeManager.UnregisterTree(this);
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
        /// <param name="child">Successful child.</param>
        protected override void ChildSuccess(Node child)
        {
            Success();
        }

        /// <summary>
        /// Called by a child node indicating the nodes in the tree have executed their task's 
        /// and returned a failure.
        /// </summary>
        /// <param name="child">Failed child.</param>
        protected override void ChildFailure(Node child)
        {
            Fail();
        }

        /// <summary>
        /// A node in the tree is still running and needs to run again next Update cycle.
        /// </summary>
        /// <param name="child">Running child.</param>
        protected override void ChildRunning(Node child)
        {
            Running();
        }
    }
}
