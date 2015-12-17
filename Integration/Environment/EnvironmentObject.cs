using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public class EnvironmentObject : MonoBehaviour
    {
        public void Initialize () {
            this.OnInitialize();
        }
        protected virtual void OnInitialize () {

        }
    }
}