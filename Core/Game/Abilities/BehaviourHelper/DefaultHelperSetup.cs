using UnityEngine;
using System.Collections;
using Lockstep.Data;
namespace Lockstep
{
    [UnityEngine.ExecuteInEditMode]
    [RequireComponent(typeof(MovementGroupHelper))]
    [RequireComponent(typeof(ScanGroupHelper))]
    [RequireComponent(typeof (EnvironmentHelper))]
    public class DefaultHelperSetup : MonoBehaviour
    {

        void Awake()
        {
            DestroyImmediate (this);
        }
    }
}