
using SimpleJsonLibrary;
using System;
using System.Drawing;
using System.Runtime.Remoting.Contexts;

namespace SimpleBackUpTool
{
	public sealed class VirtualDirectory : VirtualDirectoryEntry
	{
		public VirtualDirectoryEntry[] Children;
		[JsonIgnore] public int Pointer;

		public VirtualDirectory() 
		{
			Pointer = -1;
		}


		public bool HasNext()
		{
			return Pointer < Children.Length;
		}

		public VirtualDirectoryEntry GetNext()
		{
			if (Pointer == -1)
			{
				Pointer++;
				return this;
			}
			else
			{
				VirtualDirectoryEntry entry = Children[Pointer]; 
				if(entry.GetType() == typeof(VirtualDirectory))
				{
					VirtualDirectory directoryEntry = entry as VirtualDirectory;
					VirtualDirectoryEntry returnValue = directoryEntry.GetNext();
					if (!directoryEntry.HasNext())
					{
						Pointer++;
					}

					return returnValue;
				}
				else
				{
					Pointer++;
					return entry;
				}
			}
		}

		public bool SkipLast()
		{	
			int j = Pointer - 1;

			if(j == -1)
			{
				return false;
			}

			VirtualDirectoryEntry child = Children[j];

			if (child.GetType() == typeof(VirtualDirectory))
			{
				VirtualDirectory childDirectory = (VirtualDirectory) child;
				if (!childDirectory.SkipLast())
				{
					childDirectory.Clear();
				}
			}

			return true;
		}

		public void Clear()
		{
			Pointer = Children.Length;
		}
	}
}
