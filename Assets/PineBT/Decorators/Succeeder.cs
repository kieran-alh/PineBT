namespace PineBT
{
    /// <summary>
    /// <para>Succeeder will always Succeed when the child node returns <see cref="State.FAILURE"/>.</para>
    /// <para>By default the successOnRunning flag is set to false and the Succeeder
    /// will return a <see cref="State.RUNNING"/> when the child returns a Running State.
    /// When successOnRunning is true, the Succeeder will return a Running State and the child will be cancelled.</para>
    /// </summary>
    public class Succeeder : Decorator
    {
        /// <summary>
        /// Whether the Succeeder will return a Success when the child returns a Running State.
        /// </summary>
        private bool successOnRunning = false;

        /// <summary>
        /// Succeeder, successOnRunning defautls to false.
        /// </summary>
        /// <param name="successOnRunning">
        /// Return Success when child returns Running and cancel child. Defaults to false.</param>
        public Succeeder(bool successOnRunning = false) : this("Succeeder", successOnRunning)
        {}
        
        /// <summary>
        /// Succeeder with provided child.
        /// successOnRunning defautls to false.
        /// </summary>
        /// <param name="child">Succeeder's child.</param>
        public Succeeder(Node child) : this("Succeeder", false, child)
        {}
        
        /// <summary>
        /// Succeeder with custom name.
        /// successOnRunning defaults to false.
        /// </summary>
        /// <param name="name">Name of Succeeder Node.</param>
        /// <param name="successOnRunning">
        /// Return Success when child returns Running and cancel child. Defaults to false.</param>
        public Succeeder(string name, bool successOnRunning = false) : base(name)
        {
            this.successOnRunning = successOnRunning;
        }
        
        /// <summary>
        /// Succeeder with custom name and provided child.
        /// </summary>
        /// <param name="name">Name of Succeeder Node.</param>
        /// <param name="child">Succeeder's child.</param>
        public Succeeder(string name, Node child) : this(name, false, child)
        {}
        
        /// <summary>
        /// Succeeder, provided successOnRunning and child.
        /// </summary>
        /// <param name="successOnRunning">Return Success when child returns Running and cancel child.</param>
        /// <param name="child">Succeeder's child.</param>
        public Succeeder(bool successOnRunning, Node child) : this("Succeeder", successOnRunning, child)
        {
            this.successOnRunning = successOnRunning;
        }
        
        /// <summary>
        /// Succeeder, custom name, provided successOnRunning and child.
        /// </summary>
        /// <param name="name">Name of Succeeder Node.</param>
        /// <param name="successOnRunning">Return Success when child returns Running and cancel child.</param>
        /// <param name="child">Succeeder's child.</param>
        public Succeeder(string name, bool successOnRunning, Node child) : base(name, child)
        {
            this.successOnRunning = successOnRunning;
        }

        /// <summary>
        /// Return Success when child returns Running and cancel child. Defaults to false.
        /// </summary>
        public bool SuccessOnRunning
        {
            get { return successOnRunning; }
            set { successOnRunning = value; }
        }

        /// <summary>
        /// Called when child returns failure. Succeeds when called. 
        /// </summary>
        /// <param name="child">Failed child.</param>
        protected override void ChildFailure(Node child)
        {
            Success();
        }

        /// <summary>
        /// Called when child returns Running.
        /// If successOnRunning is false (default) Running will be called.
        /// If successOnRunning is true the child will be cancelled and Success will be called.
        /// </summary>
        /// <param name="child">Running child.</param>
        protected override void ChildRunning(Node child)
        {
            if (successOnRunning)
            {
                child.Cancel();
                Success();
            }
            else
            {
                Running();
            }
        }
    }
}

