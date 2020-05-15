namespace PineBT
{
    /// <summary> 
    /// The abstract base class of all behaviour tree nodes.
    /// Each node contains information about its state, parent node, and behaviour tree. 
    /// </summary>
    public abstract class Node 
    {        
        /// <summary>The name of the node.</summary>
        protected string name;
        /// <summary>The behaviour tree the node belongs to.</summary>
        protected BehaviourTree tree;
        /// <summary>The node's parent node.</summary>
        protected Node parent;
        /// <summary>The current state of the node.</summary>
        protected State state;
        /// <summary>The previous state of the node.</summary>
        protected State previousState;
        /// <summary>The update cycle the node processes a update on.</summary>
        protected UpdateCycle updateCycle;

        public Node(string name)
        {
            this.name = name;
        }

        public State State
        {
            get {return state;}
        }

        public Node Parent
        {
            get {return parent;}
        }

        public string Name
        {
            get {return name;}
        }

        public void SetTree(BehaviourTree tree)
        {
            this.tree = tree;
        }

        public void SetParent(Node node)
        {
            parent = node;
            tree = parent.tree;
        }

        /// <summary>Called before the <c>Node</c> begins execution.</summary>
        public virtual void Start()
        {}

        /// <summary>Contains all the operations a <c>Node</c> will execute on a update.</summary>
        public abstract void Execute();
        
        /// <summary>
        /// Called after the <c>Node</c> finishes execution
        /// by either the <c>Success</c>, <c>Fail</c>, or <c>Cancel</c> functions.
        /// </summary>
        public virtual void Finish()
        {}

        /// <summary>
        /// Called in when the <c>Node</c> has been successful.
        /// The <c>Node</c> will set it's status to SUCCESS and notify its parent 
        /// using <c>ChildSuccess</c>.
        /// </summary>
        public void Success()
        {
            previousState = state;
            state = State.SUCCESS;
            Finish();
            if (parent != null)
                parent.ChildSuccess(this);
        }

        /// <summary>
        /// Called in when the <c>Node</c> has failed.
        /// The <c>Node</c> will set it's status to FAILURE, call <c>Finish</c>
        /// and notify its parent using <c>ChildFailure</c>.
        /// </summary>
        public void Fail()
        {
            previousState = state;
            state = State.FAILURE;
            Finish();
            if (parent != null)
                parent.ChildFailure(this);
        }

        /// <summary>
        /// Called in when the <c>Node</c> is still Running.
        /// The <c>Node</c> will set it's status to RUNNING and notify its parent 
        /// using <c>ChildRunning</c>. The <c>Node</c> won't call <c>Finish</c>.
        /// </summary>
        public void Running()
        {
            previousState = state;
            state = State.RUNNING;
            if (parent != null)
                parent.ChildRunning(this);
        }

        public virtual void Cancel()
        {
            previousState = state;
            state = State.CANCELLED;
            Finish();
        }

        /// <summary>Resets the <c>Node</c> to a fresh state.</summary>
        protected void Reset()
        {
            previousState = State.FRESH;
            state = State.FRESH;
        }

        /// <summary>The path from <c>Node</c> to the Tree.</summary>
        public virtual string GetPath()
        {
            if (parent != null)
            {
                return $"{parent.GetPath()}/{name}";
            }
            else
                return name;
        }

        /// <summary>
        /// Called by a child to notify the <c>Node</c> that it's execution has succeeded.
        /// </summary>
        protected abstract void ChildSuccess(Node child);

        /// <summary>
        /// Called by a child to notify the <c>Node</c> that it's execution has failed.
        /// </summary>
        protected abstract void ChildFailure(Node child);

        /// <summary>
        /// Called by a child <c>Node</c> when it's execution is still running, 
        /// and needs to be ran again in the next update.
        /// </summary>
        protected abstract void ChildRunning(Node child);
    }
}

