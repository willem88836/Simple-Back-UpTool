using System;
using System.Text;

namespace Framework.Utils
{
	/// <summary>
	///		Contains utility methods for strings.
	/// </summary>
	public static class StringUtilities
	{
		/// <summary>
		///		Combines the provided objects in to one 
		///		string separating them by the provided char.
		/// </summary>
		public static string Combine(char separator, params object[] elements)
		{
			string s = "";
			foreach (object e in elements)
			{
				s += e.ToString() + separator;
			}
			return s;
		}


		public static string Format(this string s, params object[] elements)
		{
			return string.Format(s, elements);
		}


		public static string SubString(this string s, int startIndex, int length)
		{
			if (startIndex + length >= s.Length)
				throw new IndexOutOfRangeException();

			StringBuilder stringBuilder = new StringBuilder("");

			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append(s[startIndex + i]);
			}

			return stringBuilder.ToString();
		}
	}
}
