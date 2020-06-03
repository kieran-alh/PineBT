using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    /// <summary>
    /// <para>Blackboard is a data store that can be used by a <see cref="BehaviourTree"/> 
    /// or any entity to hold data for decision making or general storage.</para>
    /// <para>A Blackboard can be used by one or more <see cref="BehaviourTree"/> 
    /// and can act as a centeralized data store between all the trees.</para>
    /// <para>Event listeners can be added and will receive a callback when the data is 
    /// first added, updated, or removed from the Blackboard.</para>
    /// </summary>
    public class Blackboard
    {
        /// <summary>The Blackboard's name.</summary>
        private string name;
        /// <summary>The Blackboard's parent.</summary>
        private Blackboard parent;
        /// <summary>The primary data store for the Blackboard.</summary>
        private Dictionary<string, object> data = new Dictionary<string, object>();
        /// <summary>The primary set of event listeners for the Blackboard data.</summary>
        private Dictionary<string, HashSet<System.Action<Type, object>>> listeners = new Dictionary<string, HashSet<System.Action<Type, object>>>();

        /// <summary>Listeners to be added in the TriggerListeners cycle.</summary>
        private Dictionary<string, HashSet<System.Action<Type, object>>> listenersToAdd = new Dictionary<string, HashSet<System.Action<Type, object>>>();
        /// <summary>Listeners to be removed in the TriggerListeners cycle.</summary>
        private Dictionary<string, HashSet<System.Action<Type, object>>> listenersToRemove = new Dictionary<string, HashSet<System.Action<Type, object>>>();
        /// <summary>Is the Blackboard currenttly triggering its event listeners.</summary>
        private bool isUpdating = false;

        /// <summary>List of data change notifications.</summary>
        private List<Notification> notifications = new List<Notification>();

        private PineTreeManager treeManager;

        /// <summary>
        /// Constructs a Blackboard with a default name.
        /// </summary>
        public Blackboard() : this("Blackboard")
        {}

        /// <summary>
        /// Constructs a Blackboard with a custom name.
        /// </summary>
        public Blackboard(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Constructs a Blackboard with a parent.
        /// </summary>
        public Blackboard(Blackboard parent) : this("Blackboard", parent)
        {}

        /// <summary>
        /// Constructs a Blackboard with a custom name and parent.
        /// </summary>
        public Blackboard(string name, Blackboard parent)
        {
            this.name = name;
            this.parent = parent;
        }

        /// <summary>
        /// Enables the Blackboard for listeners.
        /// </summary>
        public void Enable()
        {
            treeManager = PineTreeUnityContext.Instance().TreeManager;
        }

        /// <summary>
        /// Disables the Blackboard and Unregisters any listeners.
        /// </summary>
        public void Disable()
        {
            // Remove all listeners
            if (treeManager != null)
                treeManager.UnregisterTimer(TriggerListeners);
        }

        public Blackboard Parent
        {
            get {return parent;}
            set {parent = value;}
        }

        public object this[string key]
        {
            get {return Get(key);}
            set {Add(key, value);}
        }

        /// <summary>
        /// Gets the value for the specified key. If the key does not exist, null is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The value's key.</param>
        /// <returns>Typed value or null.</returns>
        public T Get<T>(string key)
        {
            object value = Get(key);
            if (value == null)
                return default(T);
            return (T)value;
        }

        /// <summary>
        /// Gets the object value for the specified key. If the key does not exist, null is returned.
        /// </summary>
        /// <param name="key">The value's key.</param>
        /// <returns>Object value or null.</returns>
        public object Get(string key)
        {
            if (data.ContainsKey(key))
                return data[key];
            else if (parent != null)
                return parent.Get(key);
            else
                return null;
        }
        
        /// <summary>
        /// Whether the Blackboard contains the specified key.
        /// If the Blackboard does not have the key and has a parent, the parent will be checked.
        /// </summary>
        /// <param name="key">The key to be checked.</param>
        /// <returns>Whether the Blackboard contains the key.</returns>
        public bool HasKey(string key)
        {
            return data.ContainsKey(key) || (parent != null && parent.HasKey(key));
        }

        /// <summary>
        /// Whether the Blackboard has a non null value for the specified key.
        /// If the Blackboard does not have a non null value and has a parent, the parent will be checked.
        /// </summary>
        /// <param name="key">The value's key.</param>
        /// <returns>Whether a non null value is contained in the Blackboard.</returns>
        public bool HasValue(string key)
        {
            return (data.ContainsKey(key) && data[key] != null) || (parent != null && parent.HasValue(key));
        }

        /// <summary>
        /// Adds a key value pair to the Blackboard.
        /// If the Blackboard has a parent and the parent contains the key, the value will be added or updated in the parent.
        /// </summary>
        /// <param name="key">The value's key.</param>
        /// <param name="value">The value itself.</param>
        public void Add(string key, object value)
        {
            // If the parent has the key, set its value
            if (parent != null && parent.HasKey(key))
            {
                parent.Add(key, value);
            }
            else
            {
                if (!data.ContainsKey(key))
                {
                    // Add
                    data.Add(key, value);
                    notifications.Add(new Notification(key, value, Type.ADD));
                }
                else
                {
                    // Update
                    data[key] = value;
                    notifications.Add(new Notification(key, value, Type.UPDATE));
                }
                RegisterNotifications();
            }
        }

        /// <summary>
        /// Removes the key and value from the Blackboard.
        /// </summary>
        /// <param name="key">The key to be removed.</param>
        public void Remove(string key)
        {
            if (data.ContainsKey(key))
            {
                data.Remove(key);
                notifications.Add(new Notification(key, null, Type.REMOVE));
                RegisterNotifications();
            }
        }

        /// <summary>
        /// Registers the <see cref="TriggerListeners"/> function with the <see cref="PineTreeManager"/>
        /// as a timer to be Invoked on the next update cycle.
        /// </summary>
        private void RegisterNotifications()
        {
            if (notifications.Count == 0)
                return;

            treeManager.RegisterTimer(0, 1, TriggerListeners);
        }

        /// <summary>
        /// Iterates through the pending Notifications and Invokes their corresponding Listener with the <see cref="Type"/> and value.
        /// Adds and removes listeners from the primary listener dictionary.
        /// </summary>
        private void TriggerListeners()
        {
            isUpdating = true;
            if (notifications.Count == 0)
                return;
            
            // Go through all notification and trigger listeners registered with its key
            foreach(Notification notification in notifications)
            {
                // If there is no listener for the curren key, skip
                if (!listeners.ContainsKey(notification.key))
                    continue;

                // Get all the listener actions for the current notification key
                HashSet<System.Action<Type, object>> currentListeners = listeners[notification.key];

                // Invoke all the listener actions
                foreach(System.Action<Type, object> listener in currentListeners)
                {
                    if (listenersToRemove.ContainsKey(notification.key) && listenersToRemove[notification.key].Contains(listener))
                        continue;
                    
                    listener.Invoke(notification.type, notification.value);
                }
            }

            // Add new listeners
            foreach(string key in listenersToAdd.Keys)
            {
                if (!listeners.ContainsKey(key))
                    listeners.Add(key, new HashSet<System.Action<Type, object>>());
                
                foreach(System.Action<Type, object> listener in listenersToAdd[key])
                {
                    listeners[key].Add(listener);
                }
            }

            // Remove listeners
            foreach(string key in listenersToRemove.Keys)
            {
                if (!listeners.ContainsKey(key))
                    continue;

                foreach(System.Action<Type, object> rmvListener in listenersToRemove[key])
                {
                    listeners[key].Remove(rmvListener);
                } 
            }

            // Triggering the listeners is complete
            isUpdating = false;

            listenersToAdd.Clear();
            listenersToRemove.Clear();
            notifications.Clear();
        }

        /// <summary>
        /// Register's a <see cref="System.Action"/> to listen and respond when there is a change to the key's value.
        /// If the listener is already registered for the specified key, it will NOT be registered again.
        /// </summary>
        /// <param name="key">The key to listen on.</param>
        /// <param name="action">The action to Invoke on value change.</param>
        public void RegisterListener(string key, System.Action<Type, object> action)
        {
            if (!isUpdating)
            {
                // If the listeners dict doesn't have the key
                // Add the key to the listeners and create the listeners function list
                if (!listeners.ContainsKey(key))
                    listeners.Add(key, new HashSet<System.Action<Type, object>>());
                listeners[key].Add(action);
            }
            else
            {
                if (!listenersToAdd.ContainsKey(key))
                    listenersToAdd.Add(key, new HashSet<System.Action<Type, object>>());
                listenersToAdd[key].Add(action);
                
                if (listenersToRemove.ContainsKey(key) && listenersToRemove[key].Contains(action))
                    listenersToRemove[key].Remove(action);
            }
        }

        /// <summary>
        /// Is the listener already registered on the specified key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <param name="action">The listener to check.</param>
        /// <returns>Whether the listener is already registered with the specified key.</returns>
        public bool IsListenerRegistered(string key, System.Action<Type, object> action)
        {
            if ((listeners.ContainsKey(key) && listeners[key].Contains(action)) || (listenersToAdd.ContainsKey(key) && listenersToAdd[key].Contains(action)))
                return true;
            else if (listenersToRemove.ContainsKey(key) && listenersToRemove[key].Contains(action))
                return false;
            return false;
        }

        /// <summary>
        /// Does the specified key have any listeners.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether the key has any listeners.</returns>
        public bool HasListener(string key)
        {
            if (NumberOfListeners(key) > 0)
                return true;
            else 
                return false;
        }

        /// <summary>
        /// How many listeners does the specified key have.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Number of listeners registered with the specified key.</returns>
        public int NumberOfListeners(string key)
        {
            int count = 0;
            if (listeners.ContainsKey(key))
                count += listeners[key].Count;
            if (listenersToAdd.ContainsKey(key))
                count += listenersToAdd[key].Count;
            if (listenersToRemove.ContainsKey(key))
                count -= listenersToRemove[key].Count;
            return count;
        }

        /// <summary>
        /// Unregisters the listener from the specified key.
        /// </summary>
        /// <param name="key">The key to not listen on.</param>
        /// <param name="action">The listener to unregister.</param>
        public void UnregisterListener(string key, System.Action<Type, object> action)
        {
            if (!isUpdating)
            {
                if (listeners.ContainsKey(key) && listeners[key].Contains(action))
                    listeners[key].Remove(action);
            }
            else
            {
                if (!listenersToRemove.ContainsKey(key))
                    listenersToRemove.Add(key, new HashSet<System.Action<Type, object>>());
                listenersToRemove[key].Add(action);

                if (listenersToAdd.ContainsKey(key) && listenersToAdd[key].Contains(action))
                    listenersToAdd[key].Remove(action);
            }
        }

        public enum Type
        {
            ADD,
            UPDATE,
            REMOVE
        }
        
        private struct Notification
        {
            public string key;
            public object value;
            public Type type;

            public Notification(string key, object value, Type type)
            {
                this.key = key;
                this.value = value;
                this.type = type;
            }
        }
    }
}

