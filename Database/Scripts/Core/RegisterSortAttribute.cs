using UnityEngine;
using System.Collections; using FastCollections;
using Lockstep;
using System;
using System.Reflection;
namespace Lockstep.Data
{
    [System.AttributeUsage(
        AttributeTargets.Field,
        AllowMultiple = true,
        Inherited = true)
        ]
    public class RegisterSortAttribute : System.Attribute
    {

        public RegisterSortAttribute (string name, Type degreeGetterHolder, string methodName) {
            Name = name;
            DegreeGetter = (DataItemSorter)Delegate.CreateDelegate(typeof (DataItemSorter), degreeGetterHolder.GetMethod(methodName, (BindingFlags)~0));
        }

        public string Name {get; private set;}
        public DataItemSorter DegreeGetter {get; private set;}
    }
}