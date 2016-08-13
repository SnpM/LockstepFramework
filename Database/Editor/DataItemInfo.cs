using Lockstep.Data;
using System;
namespace Lockstep.Data {
    public struct DataItemInfo {
        public DataItemInfo (
            Type targetType,
            string dataName,
            string fieldName
            )
        {
            this.TargetType = targetType;
            this.DataName = dataName;
            this.FieldName = fieldName;
            this.Sorts = new SortInfo[0];
        }
        public DataItemInfo (
            Type targetType,
            string dataName,
            string fieldName,
            params SortInfo[] sorts)
        {
            this.TargetType = targetType;
            this.DataName = dataName;
            this.FieldName = fieldName;
            this.Sorts = sorts;
        }
        public Type TargetType;
        public string DataName;
        public string FieldName;
        public SortInfo[] Sorts;
    }
}