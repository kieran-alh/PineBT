namespace PineBT
{
    /// <summary> 
    /// A basic task that can be passed functions to be executed.
    /// </summary>
    public class Action : Task
    {
        /// <summary> 
        /// A task function that has no return value and always will result in a success.
        /// </summary>
        private System.Action action = null;

        /// <summary> 
        /// A task function that explicitly returns a <see cref="State"/>.
        /// </summary>
        private System.Func<State> function = null;

        /// <summary> 
        /// Creates a new Action with a default name, and a provided function with no return value.
        /// The function provided will always result in a <see cref="State.SUCCESS"/>.
        /// </summary>
        /// <param name="task">Task Action with no return value.</param>
        public Action(System.Action task) : this("Action", task)
        {}

        /// <summary> 
        /// Creates a new Action with a default name, 
        /// and a provided function that returns a <see cref="State"/>.
        /// </summary>
        /// <param name="task">Task Function with State return value.</param>
        public Action(System.Func<State> task) : this("Action", task)
        {}

        /// <summary> 
        /// Creates a new Action with a custom name, and a provided function with no return value.
        /// The function provided will always result in a <see cref="State.SUCCESS"/>.
        /// </summary>
        /// <param name="name">Name of the Action.</param>
        /// <param name="task">Task Action with no return value.</param>
        public Action(string name, System.Action task) : base(name)
        {
            this.action = task;
        }

        /// <summary> 
        /// Creates a new Action with a custom name, 
        /// and a provided function that returns a <see cref="State"/>.
        /// </summary>
        /// <param name="name">Name of the Action.</param>
        /// <param name="task">Task Function with State return value.</param>
        public Action(string name, System.Func<State> task) : base(name)
        {
            this.function = task;
        }

        public override void Start()
        {}

        public override void Finish()
        {}

        /// <summary>
        /// Invokes either the Action and returns <see cref="State.SUCCESS"/>,
        /// or the Func and returns its <see cref="State"/> result.
        /// </summary>
        public override State Run()
        {
            if (action != null)
            {
                this.action.Invoke();
                return State.SUCCESS;
            }
            else if(function != null)
            {
                return this.function.Invoke();
            }
            return State.FAILURE;
        }
    }
}

