using System.IO;
using Framework.Utils;

namespace Framework.Storage
{
	public static class SaveLoad
	{
		private static string savePath;
		public static string SavePath
		{
			get { return savePath; } 
			set { EnsureDirectoryExistence(savePath = value); }
		}
		public static string Extention = "";
		public static bool EncryptData = true;


		/// <summary>
		///		Removes all files at the savepath.
		/// </summary>
		internal static void CleanPath(string path = "")
		{
			if (path == "")
				path = savePath;

			DirectoryUtilities.ForeachFileAt(path, (FileInfo info) =>
			{
				File.Delete(info.FullName);
			});
		}


		/// <summary>
		///		Saves the provided byte[] to the set path.
		/// </summary>
		public static FileInfo Save(byte[] data, string name)
		{
			if (!Directory.Exists(savePath))
				Directory.CreateDirectory(savePath);

			if (EncryptData) FileEncryption.Encrypt(ref data);
			string path = Path.Combine(savePath, name);
			path = Path.ChangeExtension(path, Extention);
			File.WriteAllBytes(path, data);

			LoggingUtilities.LogFormat("Saved at: {0}", path);

			return new FileInfo(path);
		}

		/// <summary>
		///		Saves the provided string to the set path.
		/// </summary>
		public static FileInfo Save(string data, string name)
		{
			if (!Directory.Exists(savePath))
				Directory.CreateDirectory(savePath);

			if (EncryptData) FileEncryption.Encrypt(ref data);
			string path = Path.Combine(savePath, name);
			path = Path.ChangeExtension(path, Extention);
			
			File.WriteAllText(path, data);

			LoggingUtilities.LogFormat("Saved at: {0}", path);

			return new FileInfo(path);
		}


		/// <summary>
		///		Loads a string from the set path.
		/// </summary>
		public static void Load(string name, out string data)
		{
			string path = Path.ChangeExtension(Path.Combine(savePath, name), Extention);
			if (File.Exists(path))
			{
				data = File.ReadAllText(path);
				if(EncryptData)
				{
					FileEncryption.Decrypt(ref data);
				}
			}
			else
			{
				data = null;
			}
		}

		/// <summary>
		///		Loads a byte[] from the set path.
		/// </summary>
		public static void Load(string name, out byte[] data)
		{
			string path = Path.ChangeExtension(Path.Combine(savePath, name), Extention);
			if (File.Exists(path))
			{
				data = File.ReadAllBytes(path);
				if (EncryptData)
				{
					FileEncryption.Decrypt(ref data);
				}
			}
			else
			{
				data = null;
			}
		}

		public static bool Exists(string name)
		{
			string path = Path.ChangeExtension(Path.Combine(savePath, name), Extention);
			return File.Exists(path);
		}

		/// <summary>
		///		Deletes the file  with the specified name.
		/// </summary>
		public static bool Remove(string name)
		{
			string path = Path.ChangeExtension(Path.Combine(savePath, name), Extention);
			try
			{
				File.Delete(path);
				return true;
			}
			catch (System.Exception ex)
			{
				LoggingUtilities.LogFormat("Couldn't delete file ({0}). Halted with error ({1})", path, ex.Message);
				return false;
			}
		}


		/// <summary>
		///		Appends the specified file.
		/// </summary>
		public static void Append(string name, string data)
		{
			string path = Path.ChangeExtension(Path.Combine(savePath, name), Extention);
			if (!File.Exists(path))
			{
				File.WriteAllText(path, data);
				return;
			}

			File.AppendAllText(path, data);
		}


		/// <summary>
		///		Creates a directory if the filename 
		///		mentioned's path has none yet.
		/// </summary>
		public static void EnsureDirectoryExistenceOfFile(string path)
		{
			string dir = Path.Combine(savePath, Path.GetDirectoryName(path));
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
		}

		/// <summary>
		///		Creates a directory if the directory doesn't exist.
		/// </summary>
		public static void EnsureDirectoryExistence(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
	}
}
