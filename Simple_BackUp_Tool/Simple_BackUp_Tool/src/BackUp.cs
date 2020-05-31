using Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

public class BackUp
{
	private BackUpSettings settings;
	private ActionState overwriteState;
	private ActionState skipState;

	public void Start(BackUpSettings settings)
	{
		this.settings = settings;
		this.overwriteState = settings.DefaultOverwriteState;
		this.skipState = settings.DefaultSkipState;
		Console.Clear();

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
		Thread t = StartLoadingIcon();
		RemoveAbandonedFiles();
		CreateBackUp();
		t.Abort();
	}

	/// <summary>
	///		Does it help at all? Not really.. 
	///		Does it soothe the mind while waiting? yes!
	/// </summary>
	private Thread StartLoadingIcon()
	{
		Thread t = new Thread(() =>
		{
			int i = 0; 
			while (true)
			{
				i++;
				i %= 5;
				string l = "Backing Up All The Things";
				for (int j = 0; j < i; j++)
				{
					l += ".";
				}
				Console.Clear();
				Console.Write(l);
				Thread.Sleep(1000);
			}
		});

		t.Start();
		return t;
	}

	/// <summary>
	///		Removes all files that no longer are. 
	/// </summary>
	private void RemoveAbandonedFiles()
	{
		DirectoryUtilities.ForeachFolderAt(settings.TargetDirectory, (string dir) =>
		{
			string dirName = Path.GetFileName(dir);

			string origin = null;
			foreach (string originDir in settings.OriginDirectories)
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

		foreach (string dir in settings.OriginDirectories)
		{
			string destinationDir = TestDirectoryName(dir, settings.TargetDirectory, ref processedNames);
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
				bool overwrite = overwriteState == ActionState.Yes;
				if (overwriteState == ActionState.Unset)
				{
					overwrite = RequestActionState(ref overwriteState, "overwrite", targetPath);
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
			catch (Exception e)
			{
				bool skip = skipState == ActionState.Yes;
				if (skipState == ActionState.Unset)
				{
					LoggingUtilities.LogFormat("ERROR: {0}\n", e.Message);
					skip = RequestActionState(ref skipState, string.Format("Can't reach file due to error ({0}). Skip", e.Message), targetPath);
				}

				if (skip)
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


	private bool RequestActionState(ref ActionState state, string e, string path) {

		string input = Program.RequestInput("{0} file ({1})?\n[y]es/[n]o/[a]ll/n[o]ne", e, path);

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
