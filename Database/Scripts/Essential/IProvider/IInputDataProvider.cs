using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep.Data
{
    public interface IInputDataProvider
    {
        InputDataItem[] InputData { get; }
    }
}