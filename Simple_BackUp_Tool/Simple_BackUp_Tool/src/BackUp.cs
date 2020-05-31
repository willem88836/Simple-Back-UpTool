using Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleBackUp.src
{
	public class BackUp
	{
		public enum OverwriteState { Unset, OverwriteAll, OverwriteNone };

		private Settings settings;
		private OverwriteState overwriteState;
		private List<string> originDirectories;
		private string targetDirectory;
		private bool useShortNames; // TODO: actually use this.


		public BackUp()
		{
			settings = Settings.Instance;
			overwriteState = settings.DefaultOverwriteState;
			useShortNames = settings.UseShortNames;

			if (!Directory.Exists(settings.TargetDirectory))
			{
				throw new Exception("Target Directory Non-existent");
			}

			targetDirectory = settings.TargetDirectory;

			// Filters all non-existing directories from the origin. 
			originDirectories = new List<string>();
			for (int i = 0; i < settings.OriginDirectories.Length; i++)
			{
				string dir = settings.OriginDirectories[i];
				if (Directory.Exists(dir))
				{
					originDirectories.Add(dir);
				}
				else
				{
					LoggingUtilities.LogFormat("Directory ({0}) does not exist", dir);
				}
			}

			RemoveAbandonedFiles(); // TODO: when two origin folders have the same name all non-first ones are removed here because the names don't match the original. 
			CreateBackUp();
		}


		/// <summary>
		///		Removes all files that no longer are. 
		/// </summary>
		private void RemoveAbandonedFiles()
		{
			DirectoryUtilities.ForeachFolderAt(targetDirectory, (string dir) =>
			{
				string dirName = Path.GetFileName(dir);

				string origin = null;
				foreach(string originDir in originDirectories)
				{
					string originDirName = Path.GetFileName(originDir);
					if (dirName == originDirName)
					{
						origin = originDir;
						break;
					}
				}

				if (origin == null)
				{
					Directory.Delete(dir, true);
				}
				else
				{
					ClearAbandonedObjects(origin, dir);
				}
			});
		}

		private void CreateBackUp()
		{
			// Dictionary is used to tack duplicate names (and adjust those according to their index)
			Dictionary<string, int> processedNames = new Dictionary<string, int>();

			foreach(string dir in originDirectories)
			{
				string destinationDir = TestDirectoryName(dir, targetDirectory, ref processedNames);
				Copy(dir, destinationDir);
			}
		}


		private void ClearAbandonedObjects(
			string originDirectory, 
			string targetDirectory)
		{
			// Removes abandoned directories.
			DirectoryUtilities.ForeachFolderAt(targetDirectory, (string childPath) =>
			{
				string targetName = Path.GetFileName(childPath);

				string allegedOriginPath = Path.Combine(originDirectory, targetName);
				if (!Directory.Exists(allegedOriginPath))
				{
					// recursively deletes all files within folder. 
					DirectoryUtilities.ReversedForeachFolderIn(childPath, (string f) =>
					{
						DirectoryUtilities.ReversedForeachFileAt(f, (FileInfo i) =>
						{
							File.SetAttributes(i.FullName, FileAttributes.Normal);
							File.Delete(i.FullName);
						});
						Directory.Delete(f);
					});
				}
				else
				{
					ClearAbandonedObjects(allegedOriginPath, childPath);
				}
			});

			// Removes abandoned files. 
			DirectoryUtilities.ForeachFileAt(targetDirectory, (FileInfo file) =>
			{
				string allegedOriginPath = Path.Combine(originDirectory, file.Name);

				if (!File.Exists(allegedOriginPath))
				{
					File.Delete(file.FullName);
				}
			});
		}

		private void Copy(
			string originDirectory,
			string targetDirectory)
		{
			DirectoryUtilities.EnsureDirectory(targetDirectory);

			DirectoryUtilities.ForeachFolderAt(originDirectory, (string childDir) => 
				Copy(childDir, Path.Combine(targetDirectory, Path.GetFileName(childDir))));
	
			DirectoryUtilities.ForeachFileAt(originDirectory, (FileInfo file) =>
			{
				// TODO: add sub folders to settings to autoskip (e.g. ".git" folders). 
				string targetPath = Path.Combine(targetDirectory, file.Name);

				// if it's the same file, but a different version.
				if (File.Exists(targetPath) && file.LastWriteTime != File.GetLastWriteTime(targetPath))
				{
					bool overwrite = overwriteState == OverwriteState.OverwriteAll;
					if (overwriteState == OverwriteState.Unset)
					{
						overwrite = RequestOverwriteState(targetPath);
					}

					if (!overwrite)
					{
						return;
					}
				}

				try
				{
					File.Copy(file.FullName, targetPath, true);
				}
				catch(Exception e)
				{
					// TODO: Add auto-skip.
					LoggingUtilities.LogFormat("ERROR: {0}\n", e.Message);
					string i = Program.RequestInput(string.Format("Can't reach file due to error ({0}). Skip file? [y]es/[n]o", e.Message));
					if (i == "y")
					{
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


		private bool RequestOverwriteState(
			string path)
		{
			string input = Program.RequestInput(
				"Overwrite file ({0})?\n[y]es/[n]o/[a]ll/n[o]ne", 
				path);

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
				overwriteState = OverwriteState.OverwriteAll;
				return true;
			}
			else if (input == "o")
			{
				overwriteState = OverwriteState.OverwriteNone;
				return false;
			}

			return RequestOverwriteState(path);
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
