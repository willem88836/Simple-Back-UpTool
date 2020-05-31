using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Framework.Utils
{
	/// <summary>
	///		Contains utility methods for objects.
	/// </summary>
	public static class ObjectUtilities
	{
		// The array length of these variables is always the same.
		public const int INT_BYTEARRAYLENGTH = 54;
		public const int SHORT_BYTEARRAYLENGTH = 52;


		/// <summary>
		///		Convert an object to a byte array.
		/// </summary>
		public static byte[] ToByteArray(this object obj)
		{
			if (obj == null)
				return null;

			// TODO: this is memory wastefull. Make an attempt to not create a new object every time this is called.
			BinaryFormatter bf = new BinaryFormatter();
			MemoryStream ms = new MemoryStream();
			bf.Serialize(ms, obj);

			return ms.ToArray();
		}

		/// <summary>
		///		Convert a byte array to an Object.
		/// </summary>
		public static object ToObject(this byte[] byteArray)
		{
			// TODO: this is memory wastefull. Make an attempt to not create a new object every time this is called.
			MemoryStream memStream = new MemoryStream();
			BinaryFormatter binForm = new BinaryFormatter();
			memStream.Write(byteArray, 0, byteArray.Length);
			memStream.Seek(0, SeekOrigin.Begin);
			object obj = binForm.Deserialize(memStream);
			return obj;
		}

		public static T ToObject<T>(this byte[] byteArray) 
		{
			return (T)ToObject(byteArray);
		}

		public static bool IsNumber(this object obj)
		{
			return IsDecimalNumber(obj) || IsNonDecimalNumber(obj);
		}

		public static bool IsDecimalNumber(this object obj)
		{
			return obj is decimal
				|| obj is double
				|| obj is float;
		}

		public static bool IsNonDecimalNumber(this object obj)
		{
			return obj is byte
				|| obj is sbyte
				|| obj is short
				|| obj is ushort
				|| obj is int
				|| obj is uint
				|| obj is long
				|| obj is ulong;
		}


		public static bool IsPrimitive<T>(this T obj)
		{
			if (obj == null)
				return false;

			Type t = obj.GetType();
			return t == typeof(bool)
				|| t == typeof(byte)
				|| t == typeof(sbyte)
				|| t == typeof(char)
				|| t == typeof(decimal)
				|| t == typeof(double)
				|| t == typeof(float)
				|| t == typeof(int)
				|| t == typeof(uint)
				|| t == typeof(long)
				|| t == typeof(ulong)
				|| t == typeof(short)
				|| t == typeof(ushort)
				|| t == typeof(string);
		}
	}
}
