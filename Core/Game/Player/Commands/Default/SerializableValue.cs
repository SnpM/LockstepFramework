using System;
using System.Runtime.Serialization;

namespace Lockstep
{
	public class SerializableValue<TValue> : BaseSerializableValue where TValue : new()
	{
		public SerializableValue(TValue value)
		{
			if (typeof(TValue).IsSerializable || typeof(TValue).IsAssignableFrom(typeof(ISerializable)))
			{
				Value = value;
			}
			else
			{
				throw new InvalidOperationException("A serializable Type is required");
			}
		}

		private TValue Value;

		private TValue _ObjectValue
		{
			get
			{
				return Value;
			}
			set
			{
				Value = value;
			}
		}

		public override object ObjectValue
		{
			get
			{
				return (object)this._ObjectValue;
			}
			protected set
			{
				_ObjectValue = (TValue)value;
			}
		}

		public static implicit operator TValue(SerializableValue<TValue> serializable)
		{
			return serializable._ObjectValue;
		}

		public static implicit operator SerializableValue<TValue>(TValue value)
		{
			SerializableValue<TValue> serializable = new SerializableValue<TValue>(value);
			return serializable;
		}

	}
}