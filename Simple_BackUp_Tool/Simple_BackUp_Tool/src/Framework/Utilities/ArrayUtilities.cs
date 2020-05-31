using System;
using System.Collections.Generic;

namespace Framework.Utils
{
	public static class ArrayUtilities
	{
		// TODO: rename these methods. They don't actually insert anything, they override things.
		/// <summary>
		///		Inserts range of variables in the provided array.
		/// </summary>
		public static void Insert<T>(this T[] array, int startIndex, T[] range)
		{
			if (startIndex + range.Length > array.Length)
				throw new IndexOutOfRangeException();

			for(int i = 0; i < range.Length; i++)
			{
				array[startIndex + i] = range[i];
			}
		}
		public static void Insert<T>(this T[] array, ref int startIndex, T[] range)
		{
			Insert(array, startIndex, range);
			startIndex += range.Length;
		}
		public static void Insert<T>(this T[] array, int startIndex, List<T> range)
		{
			if (startIndex + range.Count > array.Length)
				throw new IndexOutOfRangeException();

			for (int i = 0; i < range.Count; i++)
			{
				array[startIndex + i] = range[i];
			}
		}
		public static void Insert<T>(this T[] array, ref int startIndex, List<T> range)
		{
			Insert(array, startIndex, range);
			startIndex += range.Count;
		}

		/// <summary>
		///		Removes the element from the given list 
		///		only if it exists.
		/// </summary>
		public static void SafeRemove<T>(this List<T> list, T obj)
		{
			int i = list.IndexOf(obj);
			if (i != -1)
				list.Remove(obj);
		}
		/// <summary>
		///		Adds the object to the given list
		///		only if it hasn't been added already. 
		/// </summary>
		public static void SafeAdd<T>(this List<T> list, T obj)
		{
			if (!list.Contains(obj))
				list.Add(obj);
		}

		/// <summary>
		///		Returns a segment of the provided array.
		/// </summary>
		public static T[] SubArray<T>(this T[] array, int startIndex, int length)
		{
			T[] subArray = new T[length];
			for (int i = 0; i < length; i++)
			{
				subArray[i] = array[startIndex + i];
			}
			return subArray;
		}
		public static T[] SubArray<T>(this T[] array, ref int startIndex, int length)
		{
			T[] subArray = SubArray(array, startIndex, length);
			startIndex += length;
			return subArray;
		}
		/// <summary>
		///		Returns a segment of the provided list.
		/// </summary>
		public static List<T> SubList<T>(this List<T> list, int startIndex, int length)
		{
			List<T> subList = new List<T>();
			for (int i = 0; i < length; i++)
			{
				subList.Add(list[startIndex + i]);
			}
			return subList;
		}
		public static List<T> SubList<T>(this List<T> list, ref int startIndex, int length)
		{
			List<T> subList = SubList(list, startIndex, length);
			startIndex += length;
			return subList;
		}
	}
}
