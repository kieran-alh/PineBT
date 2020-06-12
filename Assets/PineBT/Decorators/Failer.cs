namespace PineBT
{
    /// <summary>
    /// <para>Failer will alwasys Fail when the child node returns <see cref="State.SUCCESS"/>.</para>
    /// <para>By default the failOnRunning flag is set to flase and the Failer
    /// will return a <see cref="State.RUNNING"/> when the child returns a Running State.
    /// When failOnRunning is true, the Failer will return a Running State and the child will be cancelled.</para>
    /// </summary>
    public class Failer : Decorator
    {
        /// <summary>
        /// Whether the Failer will return a Failure when the child returns a Running State.
        /// </summary>
        private bool failOnRunning = false;

        /// <summary>
        /// Failer, failOnRunning defautls to false.
        /// </summary>
        /// <param name="failOnRunning">
        /// Return Fail when child returns Running and cancel child. Defaults to false.</param>
        public Failer(bool failOnRunning = false) : base("Failer")
        {
            this.failOnRunning = failOnRunning;
        }
        
        /// <summary>
        /// Failer with provided child.
        /// failOnRunning defautls to false.
        /// </summary>
        /// <param name="child">Failer's child.</param>
        public Failer(Node child) : this("Failer", false, child)
        {}
        
        /// <summary>
        /// Failer with custom name.
        /// failOnRunning defaults to false.
        /// </summary>
        /// <param name="name">Name of Failer Node.</param>
        /// <param name="failOnRunning">
        /// Return Fail when child returns Running and Cancel child. Defaults to false.</param>
        public Failer(string name, bool failOnRunning = false) : base(name)
        {
            this.failOnRunning = failOnRunning;
        }

        /// <summary>
        /// Failer with custom name, and provided child.
        /// failOnRunning defaults to false.
        /// </summary>
        /// <param name="name">Name of Failer Node.</param>
        /// <param name="child">Failer's child.</param>
        public Failer(string name, Node child) : this(name, false, child)
        {}
        
        /// <summary>
        /// Failer, provided failOnRunning and child.
        /// </summary>
        /// <param name="failOnRunning">Return Fail when child returns Running and Cancel child.</param>
        /// <param name="child">Failer's child.</param>
        public Failer(bool failOnRunning, Node child) : base("Failer", child)
        {
            this.failOnRunning = failOnRunning;
        }

        /// <summary>
        /// Failer, custom name, provided failOnRunning and child.
        /// </summary>
        /// <param name="name">Name of Failer Node.</param>
        /// <param name="failOnRunning">Return Fail when child returns Running and Cancel child.</param>
        /// <param name="child">Failer's child.</param>
        public Failer(string name, bool failOnRunning, Node child) : base(name, child)
        {
            this.failOnRunning = failOnRunning;
        }

        /// <summary>
        /// Return Fail when child returns Running and Cancel child.
        /// </summary>
        public bool FailOnRunning
        {
            get { return failOnRunning; }
            set { failOnRunning = value; }
        }

        /// <summary>
        /// Called when child returns success. Fail will be called.
        /// </summary>
        /// <param name="child">Successful child.</param>
        protected override void ChildSuccess(Node child)
        {
            Fail();
        }

        /// <summary>
        /// Called when child returns Running.
        /// If failOnRunning is false (default) Running will be called.
        /// If failOnRunning is true the child will be cancelled and Fail will be called.
        /// </summary>
        /// <param name="child">Running child.</param>
        protected override void ChildRunning(Node child)
        {
            if (failOnRunning)
            {
                child.Cancel();
                Fail();
            } 
            else
                Running();
        }
    }
}

