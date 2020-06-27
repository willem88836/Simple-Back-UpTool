using System;

namespace SimpleBackUpTool
{
	public class VirtualDirectoryEntry 
	{
		public VirtualDirectoryEntry Parent;
		public DateTime LastModifiedOn;
		public string Name;

		public VirtualDirectoryEntry() {}
	}
}
