using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    /// <summary>
    /// <para>A Service (Decorator) runs a function continuously at a set time interval.
    /// By default a Service will only run if it is on a branch that is also running, 
    /// which is why Services are commonly placed near the root of of the tree.
    /// </para>
    /// 
    /// <para>A Service can be setup to run continuously independent of whether its branch is running.</para>
    /// </summary>
    public class Service : Decorator
    {
        /// <summary>The function executed by the Service</summary>
        private System.Action serviceFunction;
        /// <summary>
        /// The time frequency the Service should be executed. Example: 0.150f = 150 milliseconds.
        /// </summary>
        private float interval = 0f;
        /// <summary>
        /// Whether the service should additionally execute on each <see cref="Start"/> call.
        /// Defaults to true, meaning the Service will perform another execution independently of the current time between intervals.
        /// </summary>
        private bool executeOnEachStart = true;
        /// <summary>
        /// Whether the service should continue to execute even if its branch isn't running.
        /// Defaults to false, meaning if the branch the Service is on isn't receiving Update calls, the Service will not execute.
        /// </summary>
        private bool executeContinuously = false;

        // Has the service done an initial execution on its first Start call
        private bool initialExecution = false;

        /// <summary><para>
        /// Service thats executes at every Update call.</para>
        /// <para><see cref="executeOnEachStart"/> Defaults to true, meaning the Service will perform another execution 
        /// independently of the current time between intervals.</para>
        /// <para><see cref="executeContinuously"/> Defaults to false, 
        /// meaning if the branch the Service is on isn't receiving Update calls, the Service will not execute.</para>
        /// </summary>
        /// <param name="service">The function to be executed.</param>
        public Service(System.Action service) : this("Service", 0, service)
        {}

        /// <summary><para>
        /// Named Service thats executes at the interval rate.</para>
        /// <para><see cref="executeOnEachStart"/> Defaults to true, meaning the Service will perform another execution 
        /// independently of the current time between intervals.</para>
        /// <para><see cref="executeContinuously"/> Defaults to false, 
        /// meaning if the branch the Service is on isn't receiving Update calls, the Service will not execute.</para>
        /// </summary>
        /// <param name="name">Name of the Service</param>
        /// <param name="interval">The time frequency the Service should be executed. Example: 0.150f = 150 milliseconds.</param>
        /// <param name="service">The function to be executed.</param>
        public Service(string name, float interval, System.Action service) : base(name)
        {}

        /// <summary>
        /// Named Service thats executes at the interval rate.
        /// </summary>
        /// <param name="name">Name of the Service</param>
        /// <param name="interval">The time frequency the Service should be executed. Example: 0.150f = 150 milliseconds.</param>
        /// <param name="service">The function to be executed.</param>
        /// <param name="executeOnEachStart">Whether the service should additionally execute on each <see cref="Start"/> call.
        /// Defaults to true, meaning the Service will perform another execution independently of the current time between intervals.</param>
        /// <param name="executeContinuously">Whether the service should continue to execute even if its branch isn't running.
        /// Defaults to false, meaning if the branch the Service is on isn't receiving Update calls, the Service will not execute.</param>
        public Service(
            string name, float interval, System.Action service, bool executeOnEachStart, bool executeContinuously) 
            : base(name)
        {
            this.interval = interval;
            this.serviceFunction = service;
            this.executeOnEachStart = executeOnEachStart;
            this.executeContinuously = executeContinuously;
        }

        /// <summary>
        /// Named Service thats executes at the interval rate.
        /// </summary>
        /// <param name="name">Name of the Service</param>
        /// <param name="interval">The time frequency the Service should be executed. Example: 0.150f = 150 milliseconds.</param>
        /// <param name="service">The function to be executed.</param>
        /// <param name="executeOnEachStart">Whether the service should additionally execute on each <see cref="Start"/> call.
        /// Defaults to true, meaning the Service will perform another execution independently of the current time between intervals.</param>
        /// <param name="executeContinuously">Whether the service should continue to execute even if its branch isn't running.
        /// Defaults to false, meaning if the branch the Service is on isn't receiving Update calls, the Service will not execute.</param>
        /// <param name="child">The Decorator's child Node.</param>
        public Service(
            string name, float interval, System.Action service, bool executeOnEachStart, bool executeContinuously, Node child) 
            : base(name, child)
        {
            this.interval = interval;
            this.serviceFunction = service;
            this.executeOnEachStart = executeOnEachStart;
            this.executeContinuously = executeContinuously;
        }

        /// <summary>
        /// Property exposing <see cref="executeOnEachStart"/>.
        /// Whether the service should additionally execute on each <see cref="Start"/> call.
        /// Defaults to true, meaning the Service will perform another execution independently of the current time between intervals.
        /// </summary>
        public bool ExecuteOnEachStart
        {
            get {return executeOnEachStart;}
            set {executeOnEachStart = value;}
        }

        /// <summary>
        /// Property exposing <see cref="executeContinuously"/>.
        /// Whether the service should continue to execute even if its branch isn't running.
        /// Defaults to false, meaning if the branch the Service is on isn't receiving Update calls, the Service will not execute.
        /// </summary>
        public bool ExecuteContinuously
        {
            get {return executeContinuously;}
            set {executeContinuously = value;}
        }

        /// <summary>
        /// Begins the Service's operation.
        /// If <see cref="executeOnEachStart"/> is true then the Service will execute its function.
        /// </summary>
        public override void Start()
        {
            if (!this.tree.TreeManager.HasTimer(serviceFunction))
                this.tree.TreeManager.RegisterTimer(interval, -1, serviceFunction);
            if (executeOnEachStart || !initialExecution)
            {
                serviceFunction.Invoke();
                initialExecution = true;
            } 
        }

        /// <summary>
        /// Ends the Service's operation.
        /// If <see cref="executeContinuously"/> is false then the Service will 
        /// unregister and not perform anymore executions of its function.
        /// </summary>
        public override void Finish()
        {
            if (!executeContinuously)
                this.tree.TreeManager.UnregisterTimer(serviceFunction);
        }
    }
}

