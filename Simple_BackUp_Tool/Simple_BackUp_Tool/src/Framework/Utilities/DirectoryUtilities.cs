using System;
using System.IO;

namespace Framework.Utils
{
	/// <summary>
	///		Contains utility methods for directories.
	/// </summary>
	public class DirectoryUtilities
	{
		/// <summary>
		///		Executes action at every folder within the provided path
		///		and the folders within those folders.
		/// </summary>
		public static void ForeachFolderIn(string path, Action<string> action, Action<string> onFinish = null)
		{
			action.Invoke(path);

			string[] folders = Directory.GetDirectories(path);

			for (int i = 0; i < folders.Length; i++)
			{
				string current = folders[i];
				ForeachFolderIn(current, action, onFinish);
			}

			onFinish.SafeInvoke(path);
		}

		/// <summary>
		///		Executes action at every folder within the provided path in reversed order
		///		and the folders within those folders.
		/// </summary>
		public static void ReversedForeachFolderIn(string path, Action<string> action, Action<string> onStart = null)
		{
			onStart.SafeInvoke(path);

			string[] folders = Directory.GetDirectories(path);

			for (int i = folders.Length - 1; i >= 0; i--)
			{
				string current = folders[i];
				ReversedForeachFolderIn(current, action, onStart);
			}

			action.Invoke(path);
		}

		/// <summary>
		///		Executes action at every folder within the provided path.
		/// </summary>
		public static void ForeachFolderAt(string path, Action<string> action, Action<string> onFinish = null)
		{
			string[] folders = Directory.GetDirectories(path);

			for (int i = 0; i < folders.Length; i++)
			{
				string current = folders[i];
				action.Invoke(current);
			}

			onFinish.SafeInvoke(path);
		}

		/// <summary>
		///		Executes action at every folder within the provided path in reversed order.
		/// </summary>
		public static void ReversedForeachFolderAt(string path, Action<string> action, Action<string> onStart = null)
		{
			onStart.SafeInvoke(path);

			string[] folders = Directory.GetDirectories(path);

			for (int i = folders.Length - 1; i >= 0; i--)
			{
				string current = folders[i];
				action.Invoke(current);
			}
		}


		/// <summary>
		///		Executes action for every file within the provided path in alphabetical order.
		/// </summary>
		public static void ForeachFileAt(string path, Action<FileInfo> action)
		{
			string[] fileNames = Directory.GetFiles(path);

			foreach (string n in fileNames)
			{
				FileInfo info = new FileInfo(n);
				action.Invoke(info);
			}
		}

		/// <summary>
		///		Executes action for every file within 
		///		the provided path in reversed alphabetical order.
		/// </summary>
		public static void ReversedForeachFileAt(string path, Action<FileInfo> action)
		{
			string[] fileNames = Directory.GetFiles(path);

			for (int i = fileNames.Length - 1; i >= 0; i--)
			{
				string n = fileNames[i];
				FileInfo info = new FileInfo(n);
				action.Invoke(info);
			}
		}

		/// <summary>
		///		Executes action for every file contained in the provided folder
		///		and the folders within that folder.
		/// </summary>
		public static void ForeachFileIn(string path, Action<FileInfo> action)
		{
			ForeachFolderIn(path, (string p) => ForeachFileAt(p, action));
		}

		/// <summary>
		///		Executes action for every file contained in the provided folder
		///		and the folders within that folder, in reversed order.
		/// </summary>
		public static void ReversedForeachFileIn(string path, Action<FileInfo> action)
		{
			ReversedForeachFolderIn(path, (string p) => ReversedForeachFileAt(p, action));
		}


		/// <summary>
		///		Ensures that the selected directory exists. 
		/// </summary>
		public static void EnsureDirectory(string path)
		{
			if(!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
	}
}
