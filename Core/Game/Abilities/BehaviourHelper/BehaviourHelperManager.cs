using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BehaviorHelperManager")]

namespace Lockstep
{
    public static class BehaviourHelperManager {
        private static PriorityQueue<BehaviourHelper> Helpers { get; set; }

        /// <summary>
        /// The list of helpers that have been modified
        /// </summary>
        private static List<PriorityQueueNode<BehaviourHelper>> _modifiedHelpers;

        public static void Initialize (BehaviourHelper[] helpers) {
            Helpers = new PriorityQueue<BehaviourHelper>(helpers.Length);
            _modifiedHelpers = new List<PriorityQueueNode<BehaviourHelper>>(helpers.Length);

            foreach (var helper in helpers) {
                Helpers.AddWithPriority(helper, helper.Priority);
            }
            foreach (var helper in Helpers) {
                helper.Initialize ();
            }
            UpdateModifiedHelpers();
        }

        /// <summary>
        /// Update the behaviour helpers queue if somehow their priority is changed
        /// </summary>
        private static void UpdateModifiedHelpers()
        {
            foreach (var node in Helpers.Nodes)
            {
                if(node.Priority != node.Item.Priority)
                {
                    _modifiedHelpers.Add(node);
                }
            }
            foreach(var node in _modifiedHelpers)
            {
                Helpers.RemoveWithPriority(node.Item, node.Priority);
                Helpers.AddWithPriority(node.Item, node.Item.Priority);
            }
            _modifiedHelpers.Clear();
        }
        
        public static void LateInitialize () {
            foreach (var helper in Helpers) {
                helper.LateInitialize();
            }
            UpdateModifiedHelpers();
        }

        public static void GameStart () {
            foreach (var helper in Helpers) {
                helper.GameStart();
            }
            UpdateModifiedHelpers();
        }

        public static void Simulate () {
            foreach (var helper in Helpers) {
                helper.Simulate ();
            }
            UpdateModifiedHelpers();
        }

        public static void LateSimulate () {
            foreach (var helper in Helpers) {
                helper.LateSimulate ();
            }
            UpdateModifiedHelpers();
        }

        public static void Execute (Command com) {
            foreach (var helper in Helpers) {
                if (helper.CanExecuteOnCommand(com)) {
                    helper.Execute (com);
                }
                helper.RawExecute(com);
            }
            UpdateModifiedHelpers();
        }

        public static void Visualize () {
            foreach (var helper in Helpers) {
                helper.Visualize ();
            }
            UpdateModifiedHelpers();
        }

        public static void Deactivate () {
            foreach (var helper in Helpers) {
                helper.Deactivate ();
            }
            UpdateModifiedHelpers();
        }
    }
}