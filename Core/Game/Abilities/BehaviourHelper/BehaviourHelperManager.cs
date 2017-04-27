using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep {
    public static class BehaviourHelperManager {
        private static BehaviourHelper[] Helpers { get; set; }

        public static void Initialize (BehaviourHelper[] helpers) {
            Helpers = helpers;
            foreach (BehaviourHelper helper in Helpers) {
                helper.EarlyInitialize ();
            }
        }

        public static void LateInitialize () {
			foreach (BehaviourHelper helper in Helpers) {
				helper.Initialize();
			}
            foreach (BehaviourHelper helper in Helpers) {
                helper.LateInitialize();
            }
        }

        public static void GameStart () {
            foreach (var helper in Helpers) {
                helper.GameStart();
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
                if (helper.CachedListenInput == com.InputCode) {
                    helper.GlobalExecute (com);
                }
                helper.RawExecute(com);
            }
        }

        public static void Visualize () {
            foreach (BehaviourHelper helper in Helpers) {
                helper.Visualize ();
            }
        }

		public static void LateVisualize () {
			foreach ( var helper in Helpers) {
				helper.LateVisualize();
			}
		}

        public static void Deactivate () {
            foreach (BehaviourHelper helper in Helpers) {
                helper.Deactivate ();
            }
        }
    }
}