using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    public class Limiter : Decorator
    {
        private float limitTime;
        private bool startAfterChild = false;
        private bool resetOnFailure = false;
        private bool returnPreviousChildState = false;
        private State limitReturnState = State.SUCCESS;

        private bool canExecute = true;
        private PineTreeManager treeManager;
       
        public Limiter(string name, float limitTime, State limitReturnState) : this(name, limitTime, false)
        {
            this.limitReturnState = limitReturnState;
        }

        public Limiter(string name, float limitTime, bool returnPreviousChildState) : base(name)
        {
            this.limitTime = limitTime;
            this.returnPreviousChildState = returnPreviousChildState;
            this.startAfterChild = false;
            this.resetOnFailure = false;
            this.treeManager = PineTreeUnityContext.Instance().TreeManager;
        }

        public Limiter(string name, float limitTime, State limitReturnState, Node child) : this(name, limitTime, false, child)
        {
            this.limitReturnState = limitReturnState;
        }

        public Limiter(string name, float limitTime, bool returnPreviousChildState, Node child) : base(name, child)
        {
            this.limitTime = limitTime;
            this.returnPreviousChildState = returnPreviousChildState;
            this.startAfterChild = false;
            this.resetOnFailure = false;
            this.treeManager = PineTreeUnityContext.Instance().TreeManager;
        }

        public Limiter(string name, float limitTime, State limitReturnState, bool startAfterChild, bool resetOnFailure) 
        : this(name, limitTime, false, startAfterChild, resetOnFailure)
        {
            this.limitReturnState = limitReturnState;
        }

        public Limiter(string name, float limitTime, bool returnPreviousChildState, bool startAfterChild, bool resetOnFailure) : base(name)
        {
            this.limitTime = limitTime;
            this.returnPreviousChildState = returnPreviousChildState;
            this.startAfterChild = startAfterChild;
            this.resetOnFailure = resetOnFailure;
            this.treeManager = PineTreeUnityContext.Instance().TreeManager;
        }

        public Limiter(string name, float limitTime, State limitReturnState, bool startAfterChild, bool resetOnFailure, Node child) 
        : this(name, limitTime, false, startAfterChild, resetOnFailure, child)
        {
            this.limitReturnState = limitReturnState;
        }

        public Limiter(string name, float limitTime, bool returnPreviousChildState, bool startAfterChild, bool resetOnFailure, Node child) 
        : base(name, child)
        {
            this.limitTime = limitTime;
            this.returnPreviousChildState = returnPreviousChildState;
            this.startAfterChild = startAfterChild;
            this.resetOnFailure = resetOnFailure;
            this.treeManager = PineTreeUnityContext.Instance().TreeManager;
        }

        public override void Execute()
        {
            if (this.canExecute)
            {
                this.canExecute = false;
                if (!startAfterChild)
                    treeManager.RegisterTimer(this.limitTime, 1, OnLimitTimeReached);
                base.Execute();
            }
            else
            {
                // The limit is still in progress
                LimitResult(child.State);
            }
        }

        private void OnLimitTimeReached()
        {
            this.canExecute = true;
        }

        private void LimitResult(State childState)
        {
            State state = limitReturnState;
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
            if (startAfterChild)
                treeManager.RegisterTimer(limitTime, 1, OnLimitTimeReached);
        }

        private void OnFailure()
        {
            Fail();
            if (resetOnFailure)
                treeManager.UnregisterTimer(OnLimitTimeReached);
        }
    }
}

