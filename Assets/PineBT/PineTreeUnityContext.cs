using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    /// <summary> 
    /// The Singleton MonoBehaviour that receives Unity Update and FixedUpdate callbacks
    /// and calls the TreeManager to update the Behvaiour Trees.
    /// </summary>
    public class PineTreeUnityContext : MonoBehaviour
    {
        private static PineTreeUnityContext instance = null;
        private PineTreeManager treeManager = new PineTreeManager();

        public PineTreeManager TreeManager
        {
            get {return treeManager;}
        }

        /// <summary> 
        /// Returns the instance of PineTreeUnityContext. If the instance is null, and new GameObject
        /// is created with PineTreeUnityContext added as a component. 
        /// </summary>
        public static PineTreeUnityContext Instance()
        {
            if (instance == null)
            {
                // Creates a new GameObject in the scene to get Unity lifecycle calls
                GameObject pineGameObject = new GameObject();
                pineGameObject.name = "PineBTUnityContext";
                instance = (PineTreeUnityContext)pineGameObject.AddComponent(typeof(PineTreeUnityContext));
                pineGameObject.isStatic = true;
            }
            return instance;
        }

        void Update()
        {
            treeManager.Update(Time.deltaTime);
        }

        void FixedUpdate()
        {
            // treeManager.FixedUpdate(Time.fixedDeltaTime);
        }
    }
}

