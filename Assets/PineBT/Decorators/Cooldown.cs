namespace PineBT
{
    /// <summary>
    /// <para>Cooldown decorator sets a time limit (cooldown time) between executions.
    /// During the Cooldown period, the Cooldown will not execute any Nodes below it on its branch, 
    /// but will immediately return a <see cref="State"/>.</para> 
    /// <para>The <see cref="State"/> returned is determined by the
    /// <see cref="returnPreviousChildState"/> or <see cref="coolDownReturnState"/> parameters.</para>
    /// <para>Once the cooldown period is over the Cooldown Node will allow executions to occur
    /// but will NOT automatically execute its child. Instead it will wait for an execution call like a normal <see cref="Node"/>.
    /// </para>
    /// <para></para>
    /// <remarks>DO NOT set <see cref="coolDownReturnState"/> to <see cref="State.FAILURE"/> 
    /// and <see cref="cancelCooldownOnFailure"/> to false. No cooldown period will occur with this combination and it doesn't make sense.</remarks>
    /// </summary>
    public class Cooldown : Decorator
    {
        /// <summary>
        /// The cooldown period or how long the Cooldown should wait between executions. 0.125 = 125MS
        /// </summary>
        private float coolDownTime;
        /// <summary>
        /// Starts the cooldown timer after the child has finished execution and returned a <see cref="State"/>. Defaults to false.
        /// </summary>
        private bool startCooldownAfterChild = false;
        /// <summary>
        /// Cancels the timer if the child's execution result is a <see cref="State.FAILURE"/>. Defaults to false.
        /// </summary>
        private bool cancelCooldownOnFailure = false;
        /// <summary>
        /// Returns the child's <see cref="State"/> when the Cooldown is executed during the cooldown period. Defaults to false.
        /// </summary>
        private bool returnPreviousChildState = false;
        /// <summary>
        /// The <see cref="State"/> to return when the Cooldown is executed during the cooldown period. Defaults to <see cref="State.SUCCESS"/>.
        /// </summary>
        private State coolDownReturnState = State.SUCCESS;
        /// <summary>
        /// The amount of random variation added to the cooldown time.
        /// </summary>
        private float timerRandomVariation;

        // Is is the cooldown period over and can the Child be executed.
        private bool canExecute = true;
        // PineTreeManager to Register/Unregister the Cooldown as a timer.
        private PineTreeManager treeManager;

        /// <summary>
        /// Cooldown that returns a provided <see cref="State"/> during cooldown period.
        /// <para>
        /// <see cref="startCooldownAfterChild"/> defaults to false.
        /// <see cref="cancelCooldownOnFailure"/> defaults to false.
        /// </para>
        /// </summary>
        /// <param name="name">Name of the Cooldown Node.</param>
        /// <param name="coolDownTime">Length of the cooldown period.</param>
        /// <param name="randomVariation">The amount of random variation added to the cooldown time.</param>
        /// <param name="coolDownReturnState">State to return during cooldown period.</param>
        public Cooldown(string name, float coolDownTime, float randomVariation, State coolDownReturnState) 
        : this(name, coolDownTime, randomVariation, false)
        {
            this.coolDownReturnState = coolDownReturnState;
        }

        /// <summary>
        /// Cooldown that either returns the child's previous <see cref="State"/> during cooldown period
        /// or <see cref="State.SUCCESS"/>.
        /// <para>
        /// <see cref="startCooldownAfterChild"/> defaults to false.
        /// <see cref="cancelCooldownOnFailure"/> defaults to false.
        /// </para>
        /// </summary>
        /// <param name="name">Name of the Cooldown Node.</param>
        /// <param name="coolDownTime">Length of the cooldown period.</param>
        /// <param name="randomVariation">The amount of random variation added to the cooldown time.</param>
        /// <param name="returnPreviousChildState">Whether to return the child's <see cref="State"/></param>
        public Cooldown(string name, float coolDownTime, float randomVariation, bool returnPreviousChildState) 
        : base(name)
        {
            this.coolDownTime = coolDownTime;
            this.returnPreviousChildState = returnPreviousChildState;
            this.startCooldownAfterChild = false;
            this.cancelCooldownOnFailure = false;
            this.timerRandomVariation = randomVariation;
            this.treeManager = PineTreeUnityContext.Instance().TreeManager;
        }

        /// <summary>
        /// Cooldown that returns a provided <see cref="State"/> during cooldown period.
        /// <para>
        /// <see cref="startCooldownAfterChild"/> defaults to false.
        /// <see cref="cancelCooldownOnFailure"/> defaults to false.
        /// </para>
        /// </summary>
        /// <param name="name">Name of the Cooldown Node.</param>
        /// <param name="coolDownTime">Length of the cooldown period.</param>
        /// <param name="randomVariation">The amount of random variation added to the cooldown time.</param>
        /// <param name="coolDownReturnState">State to return during cooldown period.</param>
        /// <param name="child">The child of the Cooldown decorator.</param>
        public Cooldown(string name, float coolDownTime, float randomVariation, State coolDownReturnState, Node child) 
        : this(name, coolDownTime, randomVariation, false, child)
        {
            this.coolDownReturnState = coolDownReturnState;
        }

        /// <summary>
        /// Cooldown that either returns the child's previous <see cref="State"/> during cooldown period
        /// or <see cref="State.SUCCESS"/>.
        /// <para>
        /// <see cref="startCooldownAfterChild"/> defaults to false.
        /// <see cref="cancelCooldownOnFailure"/> defaults to false.
        /// </para>
        /// </summary>
        /// <param name="name">Name of the Cooldown Node.</param>
        /// <param name="coolDownTime">Length of the cooldown period.</param>
        /// <param name="randomVariation">The amount of random variation added to the cooldown time.</param>
        /// <param name="returnPreviousChildState">Whether to return the child's <see cref="State"/></param>
        /// <param name="child">The child of the Cooldown decorator.</param>
        public Cooldown(string name, float coolDownTime, float randomVariation, bool returnPreviousChildState, Node child) 
        : base(name, child)
        {
            this.coolDownTime = coolDownTime;
            this.returnPreviousChildState = returnPreviousChildState;
            this.startCooldownAfterChild = false;
            this.cancelCooldownOnFailure = false;
            this.timerRandomVariation = randomVariation;
            this.treeManager = PineTreeUnityContext.Instance().TreeManager;
        }

        /// <summary>
        /// Cooldown that returns a provided <see cref="State"/> during cooldown period.
        /// </summary>
        /// <param name="name">Name of the Cooldown Node.</param>
        /// <param name="coolDownTime">Length of the cooldown period.</param>
        /// <param name="randomVariation">The amount of random variation added to the cooldown time.</param>
        /// <param name="coolDownReturnState">State to return during cooldown period.</param>
        /// <param name="startCooldownAfterChild">
        /// Start the cooldown timer after the child has executed and returned a <see cref="State"/>.</param>
        /// <param name="cancelCooldownOnFailure">Cancel the cooldown period if the child's execution is a <see cref="State.FAILURE"/>.</param>
        public Cooldown(string name, float coolDownTime, float randomVariation, State coolDownReturnState, bool startCooldownAfterChild, bool cancelCooldownOnFailure) 
        : this(name, coolDownTime, randomVariation, false, startCooldownAfterChild, cancelCooldownOnFailure)
        {
            this.coolDownReturnState = coolDownReturnState;
        }

        /// <summary>
        /// Cooldown that either returns the child's previous <see cref="State"/> during cooldown period
        /// or <see cref="State.SUCCESS"/>.
        /// </summary>
        /// <param name="name">Name of the Cooldown Node.</param>
        /// <param name="coolDownTime">Length of the cooldown period.</param>
        /// <param name="randomVariation">The amount of random variation added to the cooldown time.</param>
        /// <param name="returnPreviousChildState">Whether to return the child's <see cref="State"/></param>
        /// <param name="startCooldownAfterChild">
        /// Start the cooldown timer after the child has executed and returned a <see cref="State"/>.</param>
        /// <param name="cancelCooldownOnFailure">Cancel the cooldown period if the child's execution is a <see cref="State.FAILURE"/>.</param>
        public Cooldown(string name, float coolDownTime, float randomVariation, bool returnPreviousChildState, bool startCooldownAfterChild, bool cancelCooldownOnFailure) 
        : base(name)
        {
            this.coolDownTime = coolDownTime;
            this.returnPreviousChildState = returnPreviousChildState;
            this.startCooldownAfterChild = startCooldownAfterChild;
            this.cancelCooldownOnFailure = cancelCooldownOnFailure;
            this.timerRandomVariation = randomVariation;
            this.treeManager = PineTreeUnityContext.Instance().TreeManager;
        }

        /// <summary>
        /// Cooldown that returns a provided <see cref="State"/> during cooldown period.
        /// </summary>
        /// <param name="name">Name of the Cooldown Node.</param>
        /// <param name="coolDownTime">Length of the cooldown period.</param>
        /// <param name="randomVariation">The amount of random variation added to the cooldown time.</param>
        /// <param name="coolDownReturnState">State to return during cooldown period.</param>
        /// <param name="startCooldownAfterChild">
        /// Start the cooldown timer after the child has executed and returned a <see cref="State"/>.</param>
        /// <param name="cancelCooldownOnFailure">Cancel the cooldown period if the child's execution is a <see cref="State.FAILURE"/>.</param>
        /// <param name="child">The child of the Cooldown decorator.</param>
        public Cooldown(string name, float coolDownTime, float randomVariation, State coolDownReturnState, bool startCooldownAfterChild, bool cancelCooldownOnFailure, Node child) 
        : this(name, coolDownTime, randomVariation, false, startCooldownAfterChild, cancelCooldownOnFailure, child)
        {
            this.coolDownReturnState = coolDownReturnState;
        }

        /// <summary>
        /// Cooldown that either returns the child's previous <see cref="State"/> during cooldown period
        /// or <see cref="State.SUCCESS"/>.
        /// </summary>
        /// <param name="name">Name of the Cooldown Node.</param>
        /// <param name="coolDownTime">Length of the cooldown period.</param>
        /// <param name="randomVariation">The amount of random variation added to the cooldown time.</param>
        /// <param name="returnPreviousChildState">Whether to return the child's <see cref="State"/></param>
        /// <param name="startCooldownAfterChild">
        /// Start the cooldown timer after the child has executed and returned a <see cref="State"/>.</param>
        /// <param name="cancelCooldownOnFailure">Cancel the cooldown period if the child's execution is a <see cref="State.FAILURE"/>.</param>
        /// <param name="child">The child of the Cooldown decorator.</param>
        public Cooldown(string name, float coolDownTime, float randomVariation, bool returnPreviousChildState, bool startCooldownAfterChild, bool cancelCooldownOnFailure, Node child) 
        : base(name, child)
        {
            this.coolDownTime = coolDownTime;
            this.returnPreviousChildState = returnPreviousChildState;
            this.startCooldownAfterChild = startCooldownAfterChild;
            this.cancelCooldownOnFailure = cancelCooldownOnFailure;
            this.timerRandomVariation = randomVariation;
            this.treeManager = PineTreeUnityContext.Instance().TreeManager;
        }

        /// <summary>
        /// If the cooldown period is over, execute the child and if
        /// the <see cref="startCooldownAfterChild"/> is false, the cooldown period begins.
        /// If the cooldown period is still active return the cooldown result.
        /// </summary>
        public override void Execute()
        {
            if (canExecute)
            {
                // Cooldown is over
                if (!startCooldownAfterChild)
                {
                    // Reset canExecute to false for next cooldown period and start timer
                    canExecute = false;
                    treeManager.RegisterTimer(this.coolDownTime, timerRandomVariation, 1, OnCooldownTimeReached);
                }
                // Run Decorator base execute
                base.Execute();
            }
            else
            {
                // The cooldown is still in progress
                CooldownResult(child.State);
            }
        }

        /// <summary>
        /// Function called when the cooldown period is over.
        /// Sets <see cref="canExecute"/> to true to allow for child execution.
        /// </summary>
        private void OnCooldownTimeReached()
        {
            this.canExecute = true;
        }

        /// <summary>
        /// Returns a <see cref="State"/> during the cooldown period.
        /// If <see cref="returnPreviousChildState"/> is true, the Cooldown's child state is returned
        /// otherwise <see cref="coolDownReturnState"/> is returned.
        /// If <see cref="startCooldownAfterChild"/> is true, the cooldown period begins.
        /// </summary>
        /// <param name="childState">Cooldown child's state.</param>
        private void CooldownResult(State childState)
        {
            State state = coolDownReturnState;
            if (returnPreviousChildState)
                state = childState;
            switch(state)
            {
                case State.SUCCESS:
                    Success();
                    break;
                case State.FAILURE:
                    OnFailure();
                    break;
                case State.RUNNING:
                    Running();
                    break;
                default:
                    Success();
                    break;
            }
            // Start the cooldown period if the state isn't failure and cancelCooldownOnFailure is false
            // If <see cref="coolDownReturnState"/> is set to <see cref="State.FAILURE"/> and
            // cancelCooldownOnFailure is true then a Cooldown timer will never be Register
            if (startCooldownAfterChild && !(state == State.FAILURE && cancelCooldownOnFailure))
            {
                canExecute = false;
                treeManager.RegisterTimer(coolDownTime, timerRandomVariation, 1, OnCooldownTimeReached);
            }
        }

        /// <summary>
        /// Returns the Failure state.
        /// If <see cref="cancelCooldownOnFailure" is true, 
        /// set canExecute to true and Unregister the cooldown timer.
        /// </summary>
        private void OnFailure()
        {
            Fail();
            if (cancelCooldownOnFailure)
            {
                canExecute = true;
                treeManager.UnregisterTimer(OnCooldownTimeReached);
            }
        }
    }
}

