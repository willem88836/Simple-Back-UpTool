using Framework.Utils;
using SimpleJsonLibrary;
using System;
using System.IO;
using System.Net.Configuration;

namespace SimpleBackUpTool
{
	public class BackUp
	{
		private const string virtualDirectoryPath = "virtualDirectory.json"; 

		private BackUpSettings settings;
		private ActionState overwriteState;
		private ActionState skipState;


		public void Start(BackUpSettings settings)
		{
			this.settings = settings;
			this.overwriteState = settings.DefaultOverwriteState;
			this.skipState = settings.DefaultSkipState;

			if (!Directory.Exists(settings.TargetDirectory))
			{
				throw new DirectoryNotFoundException("Target Directory Non-existent");
			}

			// Filters all non-existing directories from the origin. 
			foreach (string dir in settings.OriginDirectories)
			{
				if (!Directory.Exists(dir))
				{
					throw new DirectoryNotFoundException("Origin Directory Non-Existent");
				}
			}

			// TODO: when two origin folders have the same name all non-first ones are removed here because the names don't match the original.
			foreach(string originDirectory in this.settings.OriginDirectories)
			{
				string targetDirectory = Path.Combine(this.settings.TargetDirectory, Path.GetFileName(originDirectory));
				DirectoryUtilities.EnsureDirectory(targetDirectory);

				VirtualDirectory virtualDirectory = LoadVirtualDirectory(targetDirectory);
				virtualDirectory.Pointer = 0;
				RemoveAbandonedFiles(originDirectory, virtualDirectory);
				CreateBackUp(originDirectory, virtualDirectory);
			}
		}


		/// <summary>
		///		Loads the virtual Directory from target directory. 
		///		Ensures virtual directory file exists. 
		/// </summary>
		private VirtualDirectory LoadVirtualDirectory(string targetDirectory)
		{
			string fullVirtualDirectoryPath = Path.Combine(targetDirectory, virtualDirectoryPath);

			if (!File.Exists(fullVirtualDirectoryPath))
			{
				LoggingUtilities.LogFormat("Virtual directory file not found, creating new one at: {0}", fullVirtualDirectoryPath);
				VirtualDirectory emptyVirtualDirectory = new VirtualDirectory()
				{
					Children = new VirtualDirectoryEntry[0],
					Name = Path.GetFileName(targetDirectory)
				};
				string json = JsonUtility.ToJson(emptyVirtualDirectory);
				File.WriteAllText(fullVirtualDirectoryPath, json);
				File.SetAttributes(fullVirtualDirectoryPath, FileAttributes.Hidden);
			}

			string virtualDirectoryJson = File.ReadAllText(fullVirtualDirectoryPath);
			VirtualDirectory virtualDirectory = JsonUtility.FromJson<VirtualDirectory>(virtualDirectoryJson);
			virtualDirectory.Pointer = 0;
			LoggingUtilities.LogFormat("Loaded virtual directory file: {0}", fullVirtualDirectoryPath);
			return virtualDirectory;
		}


		/// <summary>
		///		Removes all files that no longer are. 
		/// </summary>
		private void RemoveAbandonedFiles(string originPath, VirtualDirectory virtualDirectory)
		{
			originPath = Path.GetDirectoryName(originPath);
			while (virtualDirectory.HasNext())
			{
				VirtualDirectoryEntry next = virtualDirectory.GetNext();

				string originFilePath = Path.Combine(originPath, next.Name);

				if (!File.Exists(originFilePath) && !Directory.Exists(originFilePath))
				{
					// remove the file.
					string targetPath = Path.Combine(this.settings.TargetDirectory, next.Name);
					Directory.Delete(targetPath, true);
					// remove the entry from the virtual directory.
					virtualDirectory.SkipLast();
				}
			}
		}

		private void CreateBackUp(
			string originDirectory, // base directory.
			VirtualDirectory virtualTargetDirectory) // base directory, no root. Note: settings.targetDir + targetDir == targetPath.
		{
			CopyDirectories(originDirectory, virtualTargetDirectory);
			CopyFiles(originDirectory, virtualTargetDirectory);
		}

		private void CopyDirectories(string originDirectory, VirtualDirectory virtualTargetDirectory)
		{
			string[] directories = Directory.GetDirectories(originDirectory);

			// creates a virtual directory for each of the child directories. 
			// recursively calls Copy() on each of them.
			for (int i = 0; i < directories.Length; i++)
			{
				VirtualDirectory childDirectory = new VirtualDirectory()
				{
					Name = Path.Combine(virtualTargetDirectory.Name, Path.GetFileName(directories[i]))
				};

				string destinationDirectory = Path.Combine(this.settings.TargetDirectory, childDirectory.Name);

				// the existence of all directories is ensured. 
				if (!virtualTargetDirectory.Contains(childDirectory.Name))
				{
					Directory.CreateDirectory(destinationDirectory);
				}

				virtualTargetDirectory.UpdateChild(childDirectory);

				CreateBackUp(directories[i], childDirectory);
			}
		}

		//TODO: make this work with individual virtual directories as well. 
		private void CopyFiles(string originDirectory, VirtualDirectory virtualTargetDirectory)
		{
			string[] files = Directory.GetFiles(originDirectory);
			for (int i = 0; i < files.Length; i++)
			{
				string currentFile = files[i];

				string fileName = Path.GetFileName(currentFile);
				VirtualDirectoryEntry virtualFile;
				if (virtualTargetDirectory.Contains(fileName, out virtualFile))
				{
					FileInfo fileInfo = new FileInfo(currentFile);
					if (virtualFile.LastModifiedOn != fileInfo.LastWriteTime)
					{
						string targetPath = Path.Combine(
							this.settings.TargetDirectory,
							virtualTargetDirectory.Name,
							fileInfo.Name);

						try
						{
							File.Copy(currentFile, targetPath, true);


							FileAttributes attributes = File.GetAttributes(targetPath);
							if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
							{
								attributes = attributes & ~FileAttributes.ReadOnly;
								File.SetAttributes(targetPath, attributes);
							}
						}
						catch (Exception e)
						{
							LoggingUtilities.LogFormat("ERROR: {0} | {1}\n", e.GetType(), e.Message);
							bool skip = skipState == ActionState.Yes;
							if (skipState == ActionState.Unset)
							{
								skip = RequestActionState(ref skipState, string.Format("Can't reach file due to error ({0}). Skip", e.Message), targetPath);
							}

							if (skip)
							{
								LoggingUtilities.Log("Skipped\n");
								return;
							}
						}
					}
				}
			}
		}


		private bool RequestActionState(ref ActionState state, string e, string path) {

			string input = BackUpProgram.RequestInput("{0} file ({1})?\n[y]es/[n]o/[a]ll/n[o]ne", e, path);

			if (input == "y")
			{
				return true;
			}
			else if (input == "n")
			{
				return false;
			}
			else if (input == "a")
			{
				state = ActionState.Yes;
				return true;
			}
			else if (input == "o")
			{
				state = ActionState.No;
				return false;
			}

			Console.WriteLine("Invalid Input");
			return RequestActionState(ref state, e, path);
		}
	}
}
