using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    public class Blackboard
    {        
        private Blackboard parent;
        private Dictionary<string, object> data = new Dictionary<string, object>();
        private Dictionary<string, List<System.Action<Type, object>>> listeners = new Dictionary<string, List<System.Action<Type, object>>>();

        private Dictionary<string, HashSet<System.Action<Type, object>>> listenersToAdd = new Dictionary<string, HashSet<System.Action<Type, object>>>();
        private Dictionary<string, HashSet<System.Action<Type, object>>> listenersToRemove = new Dictionary<string, HashSet<System.Action<Type, object>>>();
        private bool isUpdating = false;

        private List<Notification> notifications = new List<Notification>();

        private PineTreeManager treeManager;

        public Blackboard()
        {}

        public Blackboard(Blackboard parent)
        {
            this.parent = parent;
        }

        public void Enable()
        {
            treeManager = PineTreeUnityContext.GetInstance().TreeManager;
        }

        public void Disable()
        {
            // TODO: Is clean necessary?
            // Remove all listeners
            // if (treeManager != null)
            //     treeManager.UnregisterTimer(TriggerListeners);
        }

        public object this[string key]
        {
            get {return Get(key);}
            set {Add(key, value);}
        }

        public T Get<T>(string key)
        {
            object value = Get(key);
            if (value == null)
                return default(T);
            return (T)value;
        }

        public object Get(string key)
        {
            if (data.ContainsKey(key))
                return data[key];
            else if (parent != null)
                return parent.Get(key);
            else
                return null;
        }

        public bool Has(string key)
        {
            return data.ContainsKey(key) || (parent != null && parent.Has(key));
        }

        public void Add(string key, object value)
        {
            // If the parent has the key, set its value
            if (parent != null && parent.Has(key))
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

        public void Remove(string key)
        {
            if (data.ContainsKey(key))
            {
                data.Remove(key);
                notifications.Add(new Notification(key, null, Type.REMOVE));
                RegisterNotifications();
            }
        }

        private void RegisterNotifications()
        {
            if (notifications.Count == 0)
                return;

            treeManager.RegisterTimer(0, 1, TriggerListeners);
        }

        public void TriggerListeners()
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
                List<System.Action<Type, object>> currentListeners = listeners[notification.key];

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
                Debug.Log($"Adding Listener {key}");
                if (!listeners.ContainsKey(key))
                    listeners.Add(key, new List<System.Action<Type, object>>());
                
                listeners[key].AddRange(listenersToAdd[key]);
            }

            // Remove listeners
            foreach(string key in listenersToRemove.Keys)
            {
                Debug.Log($"Removing Listener {key}");
                if (!listeners.ContainsKey(key))
                    continue;

                foreach(System.Action<Type, object> rmvListener in listenersToRemove[key])
                {
                    listeners[key].Remove(rmvListener);
                } 
            }

            listenersToAdd.Clear();
            listenersToRemove.Clear();
            notifications.Clear();
            isUpdating = false;
        }

        public void RegisterListener(string key, System.Action<Type, object> action)
        {
            if (!isUpdating)
            {
                // If the listeners dict doesn't have the key
                // Add the key to the listeners and create the listeners function list
                if (!listeners.ContainsKey(key))
                    listeners.Add(key, new List<System.Action<Type, object>>());
                listeners[key].Add(action);
            }
            else
            {
                if (!listenersToAdd.ContainsKey(key))
                {
                    listenersToAdd.Add(key, new HashSet<System.Action<Type, object>>());
                    listenersToAdd[key].Add(action);
                }
                else if (!listenersToAdd[key].Contains(action))
                {
                    listenersToAdd[key].Add(action);
                }

                if (listenersToRemove.ContainsKey(key) && listenersToRemove[key].Contains(action))
                    listenersToRemove[key].Remove(action);
            }
        }

        public bool HasListener(string key)
        {
            if (listeners.ContainsKey(key) || listenersToAdd.ContainsKey(key))
                return true;
            else if (listenersToRemove.ContainsKey(key))
                return false;
            return false;
        }

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
                {
                    listenersToRemove.Add(key, new HashSet<System.Action<Type, object>>());
                    listenersToRemove[key].Add(action);
                }
                else if (!listenersToRemove[key].Contains(action))
                {
                    listenersToRemove[key].Add(action);
                }

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

