using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep.Data;
namespace Lockstep
{
    public struct EmptyData : ICommandData
    {
        public void Read (Reader reader) {

        }
        public void Write (Writer writer) {

        }
    }
}