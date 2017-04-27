using UnityEngine;
using System.Collections; using FastCollections;

namespace Lockstep
{
    /// <summary>
    /// Interface for data that gets registered for Commands. Note that Command data must createable via System.Activator.CreateInstance (Type)
    /// </summary>
    public interface ICommandData
    {
        /// <summary>
        /// Function for custom writing of the data of this object.
        /// </summary>
        /// <param name="writer">Writer.</param>
        void Write(Lockstep.Writer writer);
        /// <summary>
        /// Function for reading the custom data. After reading, this object should have the same values as the object that wrote the data.
        /// </summary>
        /// <param name="reader">Reader.</param>
        void Read (Reader reader);
    }
}