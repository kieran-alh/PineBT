namespace PineBT
{
    /// <summary>
    /// A Decorator node that inverts the result of its child.
    /// </summary>
    public class Inverter : Decorator
    {
        /// <summary>Inverter with a default name.</summary>
        public Inverter() : base("Inverter")
        {}

        /// <summary>Inverter with a custom name.</summary>
        public Inverter(string name) : base(name)
        {}

        /// <summary>Inverter with a default name, and a provided child.</summary>
        public Inverter(Node child) : base("Inverter", child)
        {}

        /// <summary>Inverter with a custom name, and a provided child.</summary>
        public Inverter(string name, Node child) : base(name, child)
        {}

        /// <summary>
        /// Called by the child when the child succeeds, and triggers a failure.
        /// </summary>
        protected override void ChildSuccess(Node child)
        {
            Fail();
        }

        /// <summary>
        /// Called by the child when the child fails, and triggers a success.
        /// </summary>
        protected override void ChildFailure(Node child)
        {
            Success();
        }
    }
}

