using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    public class PineTreeManager
    {
        private List<BehaviourTree> trees = new List<BehaviourTree>();
        private HashSet<BehaviourTree> treesToAdd = new HashSet<BehaviourTree>();
        private HashSet<BehaviourTree> treesToRemove = new HashSet<BehaviourTree>();

        private Dictionary<System.Action, Timer> timers = new Dictionary<System.Action, Timer>();
        private Dictionary<System.Action, Timer> timersToAdd = new Dictionary<System.Action, Timer>();
        private HashSet<System.Action> timersToRemove = new HashSet<System.Action>();

        // Stores timers that are not in use
        private Queue<Timer> timerPool = new Queue<Timer>();

        private bool isUpdating;
        private bool isFixedUpdating;
        private double updateElapsedTime;
        private double fixedUpdateElapsedTime;

        public PineTreeManager()
        {
            isUpdating = false;
            isFixedUpdating = false;

            updateElapsedTime = 0f;
            fixedUpdateElapsedTime = 0f;
        }

        /// <summary>Called by Unity's Update function.</summary>
        public void Update(float deltaTime)
        {
            updateElapsedTime += deltaTime;
            isUpdating = true;

            // Update all current trees
            for (int i = 0; i < trees.Count; i++)
            {
                if (!treesToRemove.Contains(trees[i]))
                {
                    trees[i].Update();
                }
            }

            foreach(System.Action action in timers.Keys)
            {
                if (timersToRemove.Contains(action))
                    continue;
                
                Timer timer = timers[action];
                if (timer.IsThresholdMet(updateElapsedTime))
                {
                    action.Invoke();
                    timer.Schedule(updateElapsedTime);

                    timer.repeatCount--;

                    if (timer.repeatCount == 0)
                        UnregisterTimer(action);
                }
            }

            // Add & Remove Trees and Timers
            
            // Add all trees waiting to be added
            trees.AddRange(treesToAdd);

            // Remove all trees waiting to be removed
            foreach(BehaviourTree tree in treesToRemove)
            {
                trees.Remove(tree);
            }

            foreach(System.Action action in timersToAdd.Keys)
            {
                timers.Add(action, timersToAdd[action]);
            }

            foreach(System.Action action in timersToRemove)
            {
                ReturnTimer(timers[action]);
                timers.Remove(action);
            }

            treesToAdd.Clear();
            treesToRemove.Clear();
            timersToAdd.Clear();
            timersToRemove.Clear();

            isUpdating = false;
        }

        /// <summary>Called by Unity's FixedUpdate function.</summary>
        public void FixedUpdate(float fixedDeltaTime)
        {
            fixedUpdateElapsedTime += fixedDeltaTime;
            isFixedUpdating = true;


            isFixedUpdating = false;
        }

        /// <summary>Registers tree to receive Unity Updates.</summary>
        public void RegisterTree(BehaviourTree tree)
        {
            if (!isUpdating && !isFixedUpdating)
            {
                
                if (trees.IndexOf(tree) == -1)
                    trees.Add(tree);
                else
                {
                    #if UNITY_EDITOR
                        Debug.LogError("Tree already registered.");
                    #endif
                }
            }
            else
            {
                // If treesToAdd does not contain the tree, add it
                if (!treesToAdd.Contains(tree))
                    treesToAdd.Add(tree);
                // If treesToRemove contains the tree, remove it
                if (treesToRemove.Contains(tree))
                    treesToRemove.Remove(tree);
            }
        }

        /// <summary>Removes tree from receiving any Unity Updates.</summary>
        public void UnregisterTree(BehaviourTree tree)
        {
            if (!isUpdating && !isFixedUpdating)
            {
                
                if (!trees.Remove(tree))
                {
                    #if UNITY_EDITOR
                        Debug.LogError("Tree already unregistered.");
                    #endif
                }
            }
            else
            {
                // If treesToAdd contains the tree, remove it
                if (treesToAdd.Contains(tree))
                    treesToAdd.Remove(tree);
                // If treesToRemove contains the tree, add it
                if (!treesToRemove.Contains(tree))
                    treesToRemove.Add(tree);
            }
        }

        /// <summary>
        /// Registers an action to run on a timer at the specified interval.
        /// </summary>
        /// <param name="interval">The time interval for the timer to run at.</param>
        /// <param name="repeat">How many times the timer should repeat its execution. -1 = Infinite.</param>
        /// <param name="action">The action to execute on the timer's interval.</param>
        public void RegisterTimer(float interval, int repeat, System.Action action)
        {
            Timer timer = null;
            if (!isUpdating && !isFixedUpdating)
            {
                // If the action doesn't have a timer, get a timer.
                if (!timers.ContainsKey(action))
                    timers[action] = GetTimer();

                timer = timers[action];
            }
            else
            {
                if (!timersToAdd.ContainsKey(action))
                    timersToAdd[action] = GetTimer();
                
                timer = timersToAdd[action];

                if (timersToRemove.Contains(action))
                    timersToRemove.Remove(action);
            }
            
            timer.repeatCount = repeat;
            timer.interval = interval;
            // TODO: Only schedules timer for Update, add FixedUpdate
            timer.Schedule(updateElapsedTime);
        }

        /// <summary>
        /// Checks if there is a registered timer for an action.
        /// </summary>
        /// <param name="action">The action to check for a timer.</param>
        /// <returns></returns>
        public bool HasTimer(System.Action action)
        {
            if (timersToRemove.Contains(action))
                return false;
            else if (timersToAdd.ContainsKey(action))
                return true;
            else
                return timers.ContainsKey(action);
        }

        /// <summary>
        /// Removes the action's timer from executing.
        /// </summary>
        /// <param name="action">The action to be unregistered.</param>
        public void UnregisterTimer(System.Action action)
        {
            Timer timer = null;
            if (!isUpdating && !isFixedUpdating)
            {
                if (timers.ContainsKey(action))
                {
                    timer = timers[action];
                    ReturnTimer(timer);
                    timers.Remove(action);
                }
            }
            else
            {
                // If the timer to be removed is also waiting to be added
                // Remove it from timersToAdd and return the timer
                if (timersToAdd.ContainsKey(action))
                {
                    timer = timersToAdd[action];
                    ReturnTimer(timer);
                    timersToAdd.Remove(action);
                }
                
                if (timers.ContainsKey(action))
                    timersToRemove.Add(action);
            }
        }

        /// <summary>
        /// Either gets an instantiated timer from the <see cref="timerPool"/>
        /// or creates a new timer if there are none in the pool.
        /// </summary>
        /// <returns>The timer to be used.</returns>
        private Timer GetTimer()
        {
            Timer timer = null;
            if (timerPool.Count == 0)
            {
                timer = new Timer();
                timer.inUse = true;
            }
            else
            {
                timer = timerPool.Dequeue();
            }
            return timer;
        }

        /// <summary>
        /// Resets and places an instantiated that isn't being used in the <see cref="timerPool"/> Queue.
        /// </summary>
        /// <param name="timer">The unused timer.</param>
        private void ReturnTimer(Timer timer)
        {
            if (!timerPool.Contains(timer))
            {
                timer.inUse = false;
                timer.timeThreshold = 0;
                timer.repeatCount = 0;
                timer.interval = 0;
                timerPool.Enqueue(timer);
            }
        }

        /// <summary>
        /// The total number of BehaviourTrees registered to receive updates.
        /// </summary>
        /// <returns>The total number of trees.</returns>
        public int TreeCount()
        {
            return trees.Count + treesToAdd.Count - treesToRemove.Count;
        }

        /// <summary>
        /// The total number of timers that are registered to update.
        /// </summary>
        /// <returns>The total number of running timers.</returns>
        public int RunningTimerCount()
        {
            return timers.Values.Count + timersToAdd.Count - timersToRemove.Count;
        }

        /// <summary>
        /// The total number of timers that are either running or sitting in the timer pool.
        /// </summary>
        /// <returns>The total number of timers.</returns>
        public int TotalTimerCount()
        {
            return RunningTimerCount() + timerPool.Count;
        }

        private class Timer
        {
            // Is the timer currently being used
            public bool inUse = false;
            // The frequency the timer should update
            public double interval = 0f;
            // The next time the timer should at
            public double timeThreshold = 0f;
            // The amount of times the timer should repeat
            public int repeatCount = 0;

            public void Schedule(double elapsedTime)
            {
                timeThreshold = elapsedTime + interval;
            }

            public bool IsThresholdMet(double elapsedTime)
            {
                return timeThreshold <= elapsedTime;
            }
        }
    }
}

