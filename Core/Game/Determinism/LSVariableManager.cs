using UnityEngine;
using System.Collections; using FastCollections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
namespace Lockstep
{
    public static class LSVariableManager
    {
        static readonly FastList<string> bufferPropertyNames = new FastList<string>();
        private static Dictionary<Type,string[]> CachedLockstepPropertyNames = new Dictionary<Type, string[]>();
        private static readonly FastBucket<LSVariableContainer> Containers = new FastBucket<LSVariableContainer>();

        /// <summary>
        /// Registers an object and returns a ticket to access variable info about the object.
        /// Note: Ticket may vary on multiple clients and sessions.
        /// </summary>
        /// <param name="lockstepObject">Lockstep object.</param>
        public static int Register (object lockstepObject) {
            Type type = lockstepObject.GetType();
            string[] propertyNames;
            LSVariableContainer container;
            if (!CachedLockstepPropertyNames.TryGetValue(type, out propertyNames)) {
                bufferPropertyNames.FastClear();
                container = new LSVariableContainer(GetVariables (lockstepObject, type));
                foreach (LSVariable info in container.Variables) {
                    bufferPropertyNames.Add(info.Info.Name);
                }
                CachedLockstepPropertyNames.Add (type, bufferPropertyNames.ToArray());
            }
            else {
                container = new LSVariableContainer(GetVariables (lockstepObject, type, propertyNames));
            }
            return Containers.Add(container);
        }
        private static IEnumerable <LSVariable> GetVariables (object lockstepObject, Type type, string[] propertyNames) {
            //Getting target variables with cache
            foreach (string name in propertyNames) {
                yield return new LSVariable(lockstepObject, type.GetProperty(name,(BindingFlags)~0));
            }
        }

        private static IEnumerable<LSVariable> GetVariables (object lockstepObject, Type type) {
            foreach (PropertyInfo info in type.GetProperties((BindingFlags)~0)) {
                //Make sure the type is something we can work with

                //Getting vars without cache
                object[] attributes = info.GetCustomAttributes(typeof (LockstepAttribute), true);

                if (attributes != null && attributes.Length > 0) {
                    Type propType = info.PropertyType;
                    if (propType.IsArray)
                    {
                        //Currently arrays can't be tracked
                        Debug.LogErrorFormat ("'{0}' of type '{1}' cannot be tracked since it's an array.", info, propType);
                        continue;
                    }

                    yield return new LSVariable(lockstepObject, info, attributes.FirstOrDefault() as LockstepAttribute);
                }
            }
        }

        public static LSVariableContainer GetContainer (int ticket) {
            return Containers[ticket];
        }

        public static int GetHash (int ticket) {
            return GetContainer(ticket).Hash();
        }

        public static IEnumerable<LSVariable> GetObjectDesyncs (int ticket,int[] compareHashes) {
            foreach (LSVariable variable in GetContainer (ticket).GetDesyncs (compareHashes))
                yield return variable;
        }
    }
}