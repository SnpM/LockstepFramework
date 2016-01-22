using UnityEngine;
using System.Collections;

namespace Lockstep.Data
{
    public interface IInputDataProvider
    {
        InputDataItem[] InputData { get; }
    }
}