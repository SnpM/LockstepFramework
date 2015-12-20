using UnityEngine;
using System.Collections;

namespace Lockstep {
    public static class BehaviourHelperManager {
        private static BehaviourHelper[] Helpers { get; set; }

        public static void Initialize (BehaviourHelper[] helpers) {
            Helpers = helpers;
            foreach (BehaviourHelper helper in Helpers) {
                helper.Initialize ();
            }
        }

        public static void LateInitialize () {
            foreach (BehaviourHelper helper in Helpers) {
                helper.LateInitialize();
            }
        }

        public static void Simulate () {
            foreach (BehaviourHelper helper in Helpers) {
                helper.Simulate ();
            }
        }

        public static void LateSimulate () {
            foreach (BehaviourHelper helper in Helpers) {
                helper.LateSimulate ();   
            }
        }

        public static void Execute (Command com) {
            foreach (BehaviourHelper helper in Helpers) {
                if (helper.ListenInput == com.LeInput) {
                    helper.Execute (com);
                }
            }
        }

        public static void Visualize () {
            foreach (BehaviourHelper helper in Helpers) {
                helper.Visualize ();
            }
        }

        public static void Deactivate () {
            foreach (BehaviourHelper helper in Helpers) {
                helper.Deactivate ();
            }
        }
    }
}