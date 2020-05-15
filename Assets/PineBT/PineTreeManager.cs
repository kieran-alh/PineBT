using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    public class PineTreeManager
    {
        // TODO: Invesitage IndexOf Calls
        private List<BehaviourTree> trees;
        private List<BehaviourTree> treesToAdd;
        private List<BehaviourTree> treesToRemove;

        private bool isUpdating;
        private bool isFixedUpdating;
        private float updateElapsedTime;
        private float fixedUpdateElapsedTime;

        public PineTreeManager()
        {
            trees = new List<BehaviourTree>();
            treesToAdd = new List<BehaviourTree>();
            treesToRemove = new List<BehaviourTree>();

            isUpdating = false;
            isFixedUpdating = false;

            // TODO: Check if Initialized to 0
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
                if (treesToRemove.IndexOf(trees[i]) == -1)
                {
                    trees[i].Update();
                }
            }

            // Add all trees waiting to be added
            for (int i = 0; i < treesToAdd.Count; i++)
            {
                trees.Add(treesToAdd[i]);
            }

            // TODO: Remove is a O(n) operation
            // Maybe use a dictionary for indexing
            // Remove all trees waiting to be removed
            for (int i = 0; i < treesToRemove.Count; i++)
            {
                trees.Remove(treesToRemove[i]);
            }

            treesToAdd.Clear();
            treesToRemove.Clear();

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
                if (treesToAdd.IndexOf(tree) == -1)
                    treesToAdd.Add(tree);
                if (treesToRemove.IndexOf(tree) != -1)
                    treesToRemove.Remove(tree);
            }
        }

        /// <summary>Removes tree from receiving any Unity Updates.</summary>
        public void UnregisterTree(BehaviourTree tree)
        {
            if (!isUpdating && !isFixedUpdating)
            {
                
                if (trees.IndexOf(tree) != -1)
                    trees.Remove(tree);
                else
                {
                    #if UNITY_EDITOR
                        Debug.LogError("Tree already unregistered.");
                    #endif
                }
            }
            else
            {
                if (treesToAdd.IndexOf(tree) != -1)
                    treesToAdd.Remove(tree);
                if (treesToRemove.IndexOf(tree) == -1)
                    treesToRemove.Add(tree);
            }
        }
    }
}

