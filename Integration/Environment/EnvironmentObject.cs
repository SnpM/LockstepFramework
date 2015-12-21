using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public class EnvironmentObject : MonoBehaviour
    {
        internal void Initialize () {
            this.OnInitialize();
        }
        protected virtual void OnInitialize () {

        }
        internal void LateInitialize () {
            this.OnLateInitialize();
        }
        protected virtual void OnLateInitialize () {

        }
    }
}