using Lockstep.Data;
using System;
namespace Lockstep.Data {
    public struct DataItemInfo {
        public DataItemInfo (
            Type targetType,
            string displayName,
            string codeName,
            string fieldName,
            params SortInfo[] sorts)
        {
            this.TargetType = targetType;
            this.DisplayName = displayName;
            this.CodeName = codeName;
            this.FieldName = fieldName;
            this.Sorts = sorts;
        }
        public Type TargetType;
        public string DisplayName;
        public string CodeName;
        public string FieldName;
        public SortInfo[] Sorts;
    }
}