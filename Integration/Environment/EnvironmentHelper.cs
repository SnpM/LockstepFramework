using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public class EnvironmentHelper : BehaviourHelper
    {
        public override InputCode ListenInput
        {
            get
            {
                return InputCode.None;
            }
        }

        public EnvironmentSaver Saver { get; private set; }

        public EnvironmentHelper(EnvironmentSaver saver)
        {
            this.Saver = saver;
        }

        protected override void OnInitialize()
        {
            if (Saver.EnvironmentBodies == null)
            {
                Debug.Log("No EnvironmentBodies found in Saver - Ignoring.");
            } else
            {
                foreach (EnvironmentBodyInfo info in Saver.EnvironmentBodies)
                {
                    info.Body.Initialize(info.Position, info.Rotation);
                }
            }
        }
    }
}