using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;

namespace Lockstep.Data {
    public class RegisterDataAttribute : Attribute {
        public static IEnumerable<EditorLSDatabase.DataItemInfo> GetDataItemInfos () {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() ()) {
                foreach (Type type in assembly.GetTypes ()) {
                    object[] attributes = type.GetCustomAttributes (typeof(RegisterDataAttribute), false);
                    for (int i = 0; i < attributes.Length; i++) {
                        RegisterDataAttribute reg = attributes [i] as RegisterDataAttribute;
                        yield return reg.info;
                    }

                }
            }
            yield break;
        }

        public RegisterDataAttribute (
            Type targetType,
            string displayName,
            string codeName,
            string fieldName,
            params SortInfo[] sorts) {
            info.TargetType = targetType;
            info.DisplayName = displayName;
            info.CodeName = codeName;
            info.FieldName = fieldName;
            info.Sorts = sorts;
        }

        public EditorLSDatabase.DataItemInfo info;
    }
}