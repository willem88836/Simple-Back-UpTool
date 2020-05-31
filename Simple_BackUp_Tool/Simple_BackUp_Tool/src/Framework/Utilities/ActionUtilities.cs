using System;

namespace Framework.Utils
{
	/// <summary>
	///		Contains methods for Action.
	/// </summary>
	public static class ActionUtilities
	{
		// Ty 8D
		public static void SafeInvoke(this Action a)
		{
			if (a != null)
			{ a.Invoke(); }
		}
		public static void SafeInvoke<T>(this Action<T> a, T t)
		{
			if (a != null)
			{ a.Invoke(t); }
		}
		public static void SafeInvoke<T0, T1>(this Action<T0, T1> a, T0 t0, T1 t1)
		{
			if (a != null)
			{ a.Invoke(t0, t1); }
		}
		public static void SafeInvoke<T0, T1, T2>(this Action<T0, T1, T2> a, T0 t0, T1 t1, T2 t2)
		{
			if (a != null)
			{ a.Invoke(t0, t1, t2); }
		}
		public static void SafeInvoke<T0, T1, T2, T3>(this Action<T0, T1, T2, T3> a, T0 t0, T1 t1, T2 t2, T3 t3)
		{
			if (a != null)
			{ a.Invoke(t0, t1, t2, t3); }
		}

		public static TReturn SafeInvoke<TReturn>(this Func<TReturn> a)
		{
			return a == null ? default(TReturn) : a.Invoke();
		}
		public static TReturn SafeInvoke<T0, TReturn>(this Func<T0, TReturn> a, T0 t0)
		{
			return a == null ? default(TReturn) : a.Invoke(t0);
		}
		public static TReturn SafeInvoke<T0, T1, TReturn>(this Func<T0, T1, TReturn> a, T0 t0, T1 t1)
		{
			return a == null ? default(TReturn) : a.Invoke(t0, t1);
		}
		public static TReturn SafeInvoke<T0, T1, T2, TReturn>(this Func<T0, T1, T2, TReturn> a, T0 t0, T1 t1, T2 t2)
		{
			return a == null ? default(TReturn) : a.Invoke(t0, t1, t2);
		}
	}
}
