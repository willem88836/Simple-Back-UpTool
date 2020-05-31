using System.Collections.Generic;
using System.IO;
using Framework.Utils;

namespace Framework.Storage
{
	///
	///		The data in the .zip has the following structure: 
	///		
	///		|---------------------------|
	///				Format Length			(the length in bytes of the data that describes the zipped data)
	///		|---------------------------|
	///				File  Lengths			(the length in bytes of the zipped data)
	///		|---------------------------|
	///				Name  Lengths			(the length in bytes of the names of each file)
	///		|---------------------------|	
	///				File    Names			(all the filenames)
	///		|---------------------------|	
	///				File     Data			(all the files)
	///		|---------------------------|	
	///		



	/// <summary>
	///		Zips a set of files or directory into a singular file. 
	/// </summary>
	public class SimpleZipper
	{
		private const int LONGLENGTH = 58;
		private const int SHORTLENGTH = 52;


		#region Zipping

		/// <summary>
		///		Zips all files within the provided 
		///		directory into the provided extraction path.
		/// </summary>
		public FileInfo Zip(string directory, string extractionPath)
		{
			List<string> files = new List<string>();
			DirectoryUtilities.ForeachFileIn(directory, (FileInfo info) => files.Add(info.FullName));
			return Zip(files.ToArray(), extractionPath, directory);
		}

		/// <summary>
		///		Zips all provided files into the provided extraction path.
		/// </summary>
		public FileInfo Zip(string[] files, string extractionPath, string rootPath = null)
		{
			// Contains the individual byte size of each file that is zipped.
			long[] fileSizes = new long[files.Length];

			// Contains all filenames, transformed to their byte data.
			List<byte> listedFileNames = new List<byte>();
			// Contains the individual byte size of each file's name.
			short[] fileNameSizes = new short[files.Length];

			// Contains the size of the complete zip file. 
			long completeFileSize = 0;

			// Iterates through all files to incorporate these into the above data containers.
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo info = new FileInfo(files[i]);

				// Incorporates the current file into the completeFileSize and the stored fileSizes.
				long l = info.Length;
				completeFileSize += l;
				fileSizes[i] = l;


				// Adds the fileName data to the listedFileNames, and stores its length.
				string fileName = rootPath == null ? info.Name : info.FullName.Substring(rootPath.Length);
				byte[] fileNameData = ObjectUtilities.ToByteArray(fileName);
				listedFileNames.AddRange(fileNameData);
				fileNameSizes[i] = (short)fileNameData.Length;
			}


			// Calculates the byte length of the zip's format.
			long formatLength = LONGLENGTH + (files.Length * LONGLENGTH);

			// Calculates the byte length of the entire zip.
			completeFileSize += formatLength + listedFileNames.Count + (fileNameSizes.Length * SHORTLENGTH);


			// Writes all data into the byte array (in the order mentioned at the start of this file.
			byte[] zipData = new byte[completeFileSize];
			int insertIndex = 0;

			zipData.Insert(ref insertIndex, formatLength.ToByteArray());

			foreach (long l in fileSizes)
				zipData.Insert(ref insertIndex, l.ToByteArray());

			foreach (short l in fileNameSizes)
				zipData.Insert(ref insertIndex, l.ToByteArray());


			zipData.Insert(ref insertIndex, listedFileNames);

			foreach (string file in files)
				zipData.Insert(ref insertIndex, File.ReadAllBytes(file));


			// Writes the byte array to the provided path.
			File.WriteAllBytes(extractionPath, zipData);
			return new FileInfo(extractionPath);
		}

		#endregion


		#region Unzipping

		/// <summary>
		///		Unzips all files contained in the 
		///		zip into the provided extraction directory.
		/// </summary>
		public DirectoryInfo Unzip(string zip, string extractionDirectory)
		{
			byte[] data = File.ReadAllBytes(zip);
			return Unzip(data, extractionDirectory);
		}

		/// <summary>
		///		Unzips all files contained in the byte array.
		/// </summary>
		public DirectoryInfo Unzip(byte[] data, string extractionDirectory)
		{
			// Determines the number of files that is in the zip.
			int dataIndex = 0;
			long fileCount = ((long)data.SubArray(ref dataIndex, LONGLENGTH).ToObject()) / LONGLENGTH - 1;

			// Collects file sizes.
			long[] fileSizes = new long[fileCount];
			for (int i = 0; i < fileCount; i++)
			{
				fileSizes[i] = (long)data.SubArray(ref dataIndex, LONGLENGTH).ToObject();
			}

			// Collects name sizes.
			short[] namelengths = new short[fileCount];
			for (int i = 0; i < fileCount; i++)
			{
				namelengths[i] = (short)data.SubArray(ref dataIndex, SHORTLENGTH).ToObject();
			}

			// Collects names. 
			string[] fileNames = new string[fileCount];
			for (int i = 0; i < fileCount; i++)
			{
				short length = namelengths[i];
				fileNames[i] = (string)data.SubArray(ref dataIndex, length).ToObject();
			}

			// Unzips the individual files.
			for (int i = 0; i < fileSizes.Length; i++)
			{
				string extractPath = extractionDirectory + '\\' + fileNames[i];

				string directory = Path.GetDirectoryName(extractPath);
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				byte[] item = data.SubArray(ref dataIndex, (int)fileSizes[i]);
				File.WriteAllBytes(extractPath, item);
			}

			return new DirectoryInfo(extractionDirectory);
		}

		#endregion
	}
}
