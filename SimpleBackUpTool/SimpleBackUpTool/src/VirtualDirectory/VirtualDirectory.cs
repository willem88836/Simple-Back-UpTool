using Framework.Utils;
using SimpleJsonLibrary;

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
				if(!(child as VirtualDirectory).SkipLast())
				{
					DropElementAt(j);
				}
			}
			else
			{
				DropElementAt(j);
			}

			return true;
		}

		private void DropElementAt(int i)
		{
			// TODO: there's a neater way to do this.
			Children = new VirtualDirectoryEntry[Children.Length - 1];
			Children.Insert(0, Children.SubArray(0, i - 1));
			Children.Insert(i + 1, Children.SubArray(i + 1, Children.Length - i - 1));
		}
	

		public void UpdateChild(VirtualDirectoryEntry newChild)
		{
			for(int i = 0; i < this.Children.Length; i++)
			{
				VirtualDirectoryEntry entry = this.Children[i];
				if (entry.Name == newChild.Name)
				{
					this.Children[i] = newChild;
					return;
				}
			}

			//TODO: this is underperformant.
			VirtualDirectoryEntry[] newChildren = new VirtualDirectoryEntry[this.Children.Length + 1];
			newChildren.Insert(0, this.Children);
			newChildren[this.Children.Length] = newChild;
			this.Children = newChildren;
		}

		public bool Contains(string name, out VirtualDirectoryEntry child)
		{
			foreach(VirtualDirectoryEntry entry in Children)
			{
				if(entry.Name == name)
				{
					child = entry;
					return true;
				}
			}

			child = null;
			return false;
		}

		public bool Contains(string name)
		{
			foreach(VirtualDirectory entry in Children)
			{
				if(entry.Name == name)
				{
					return true;
				}
			}
			return false;
		}
	}
}
