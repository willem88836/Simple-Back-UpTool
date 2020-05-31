using System;

namespace Framework.Utils
{
	public static class TypeUtilities
	{
		public static bool IsPrimitive(this Type t)
		{
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
