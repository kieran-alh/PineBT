namespace PineBT
{
    /// <summary> 
    /// The abstract base class for leaf tasks.
    /// The task class can be extended to perform more complex operations.
    /// </summary>
    public abstract class Task : Node
    {
        /// <summary>Creates a new task with a custom name.</summary>
        public Task(string name) : base(name)
        {}

        /// <summary>
        /// The task's main function that contains all of its update logic.
        /// The <c>Run</c> function is executed in the <c>Execute</c> function
        /// The operation must return a <see cref="State"/> such as 
        /// <see cref="State.SUCCESS"/> <see cref="State.FAILURE"/> <see cref="State.RUNNING"/> 
        /// </summary>
        public abstract State Run();

        /// <summary>
        /// Executes the <c>Task</c>'s <c>Run</c> function.
        /// The result is used to trigger either the 
        /// <see cref="Success"/> <see cref="Fail"/> or <see cref="Running"/> functions.
        /// </summary>
        public override void Execute()
        {
            State taskState = Run();
            switch(taskState)
            {
                case State.SUCCESS:
                    Success();
                    break;
                case State.FAILURE:
                    Fail();
                    break;
                case State.RUNNING:
                    Running();
                    break;
                default:
                    Fail();
                    break;
            }
        }
        
        /// <summary>Tasks have no children, this function is empty.</summary>
        protected override void ChildSuccess(Node child){}
        /// <summary>Tasks have no children, this function is empty.</summary>
        protected override void ChildFailure(Node child){}
        /// <summary>Tasks have no children, this function is empty.</summary>
        protected override void ChildRunning(Node child){}
    }
}

