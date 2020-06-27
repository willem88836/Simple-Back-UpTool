using Framework.Storage;
using Framework.Utils;
using SimpleJsonLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SimpleBackUpTool
{
	public class BackUp
	{
		private const string virtualDirectoryPath = "virtualDirectory.json"; 

		private BackUpSettings settings;
		private ActionState overwriteState;
		private ActionState skipState;

		private VirtualDirectory virtualDirectory;


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

			LoadVirtualDirectory();
			RemoveAbandonedFiles();
			CreateBackUp();
		}


		/// <summary>
		///		Loads the virtual Directory from target directory. 
		///		Ensures virtual directory file exists. 
		/// </summary>
		private void LoadVirtualDirectory()
		{
			string fullVirtualDirectoryPath = Path.Combine(this.settings.TargetDirectory, virtualDirectoryPath);

			if (!File.Exists(fullVirtualDirectoryPath))
			{
				LoggingUtilities.LogFormat("Virtual directory file not found, creating new one at: {0}", fullVirtualDirectoryPath);
				FileStream stream = File.Create(fullVirtualDirectoryPath);
				File.SetAttributes(fullVirtualDirectoryPath, FileAttributes.Hidden);
				stream.Close();
			}

			string virtualDirectoryJson = File.ReadAllText(fullVirtualDirectoryPath);
			this.virtualDirectory = JsonUtility.FromJson<VirtualDirectory>(virtualDirectoryJson);
			this.virtualDirectory.Pointer = 0;
			LoggingUtilities.LogFormat("Loaded virtual directory file: {0}", fullVirtualDirectoryPath);
		}


		/// <summary>
		///		Removes all files that no longer are. 
		/// </summary>
		private void RemoveAbandonedFiles()
		{
			while (this.virtualDirectory.HasNext())
			{
				VirtualDirectoryEntry next = this.virtualDirectory.GetNext();
				string originPath = Path.GetPathRoot(next.Name);

				foreach(string originDirectory in this.settings.OriginDirectories)
				{
					string directoryName = Path.GetFileName(originDirectory);
					if (directoryName == originPath)
					{
						originPath = originDirectory;
						break;
					}
				}

				originPath = Path.Combine(originPath, next.Name);

				if (!File.Exists(originPath) && !Directory.Exists(originPath))
				{
					// remove the file.
					string targetPath = Path.Combine(this.settings.TargetDirectory, next.Name);
					Directory.Delete(targetPath, true);
					// remove the entry from the virtual directory.
					this.virtualDirectory.SkipLast();
				}
			}
		}

		private void CreateBackUp()
		{
			VirtualDirectory newVirtualDirectory = new VirtualDirectory()
			{
				Children = new VirtualDirectoryEntry[this.settings.OriginDirectories.Length]
			};

			for(int i = 0; i < this.settings.OriginDirectories.Length; i++)
			{
				string originDirectory = this.settings.OriginDirectories[i];
				string originName = Path.GetFileName(originDirectory);
				//string destinationDirectory = Path.Combine(this.settings.TargetDirectory, originName);

				VirtualDirectory virtualDirectory = (VirtualDirectory) newVirtualDirectory.Children[i];
				virtualDirectory.Name = originName;

				this.virtualDirectory.Children[i] = virtualDirectory;

				Copy(originDirectory, originName, virtualDirectory);
			}

			this.virtualDirectory = newVirtualDirectory;
		}

		private void Copy(
			string originDirectory,
			string targetDirectory,
			VirtualDirectory virtualDirectory)
		{
			DirectoryUtilities.EnsureDirectory(targetDirectory);

			string[] directories = Directory.GetDirectories(originDirectory);
			
			for(int i = 0; i < directories.Length; i++)
			{
				string destinationDirectory = Path.Combine(
					targetDirectory,
					Path.GetFileName(directories[i]));

				VirtualDirectory childDirectory = new VirtualDirectory()
				{
					Name = destinationDirectory
				};

				virtualDirectory.Children[i] = childDirectory;

				Copy(directories[i], destinationDirectory, childDirectory);
			}

			string[] files = Directory.GetFiles(originDirectory);





			DirectoryUtilities.ForeachFolderAt(originDirectory, (string childDir) =>
				Copy(childDir, Path.Combine(targetDirectory, Path.GetFileName(childDir))));

			DirectoryUtilities.ForeachFileAt(originDirectory, (FileInfo file) =>
			{
				// TODO: add sub folders to settings to autoskip (e.g. ".git" folders). 
				string targetPath = Path.Combine(targetDirectory, file.Name);

				// if it's the same file, but a different version.
				if (File.Exists(targetPath) && file.LastWriteTime != File.GetLastWriteTime(targetPath))
				{
					LoggingUtilities.LogFormat("DUPLICATE FILE: {0}\n", targetPath);

					bool overwrite = overwriteState == ActionState.Yes;
					if (overwriteState == ActionState.Unset)
					{
						overwrite = RequestActionState(ref overwriteState, "overwrite", targetPath);
					}

					if (!overwrite)
					{
						LoggingUtilities.LogFormat("Skipped\n");
						return;
					}

					LoggingUtilities.LogFormat("Overwritten\n");
				}

				try
				{
					File.Copy(file.FullName, targetPath, true);
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

				FileAttributes attributes = File.GetAttributes(targetPath);
				if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					attributes = attributes & ~FileAttributes.ReadOnly;
					File.SetAttributes(targetPath, attributes);
				}
			});
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

		private string TestDirectoryName(
			string originDirectory,
			string targetDirectory,
			ref Dictionary<string, int> processedDirectories)
		{
			string originDir = Path.GetFileName(originDirectory);


			var keys = processedDirectories.Keys;

			if (keys.Contains(originDir))
			{
				processedDirectories[originDir]++;
				originDir = string.Format(
					"{0} ({1}){2}",
					Path.GetFileNameWithoutExtension(originDir),
					processedDirectories[originDir],
					Path.GetExtension(originDir));
			}

			processedDirectories.Add(originDir, 0);

			originDir = Path.Combine(targetDirectory, originDir);
			return originDir;
		}
	}
}
