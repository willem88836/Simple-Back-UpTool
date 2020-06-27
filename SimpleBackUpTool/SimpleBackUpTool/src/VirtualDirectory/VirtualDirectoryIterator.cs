using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleBackUpTool
{
	public sealed class VirtualDirectoryIterator
	{
		private VirtualDirectory root;
		// pointer with value -1 points to the root.
		private int pointer;


		public VirtualDirectoryIterator(VirtualDirectory virtualDirectory, bool isRoot)
		{
			this.root = virtualDirectory;
			this.pointer = isRoot ? 0 : -1;
		}

		public bool HasNext()
		{
			return pointer < root.Children.Length;
		}

		public VirtualDirectoryEntry Next()
		{
			if (pointer == -1)
			{
				return root;
			}
			else
			{
				VirtualDirectoryEntry child = this.root.Children[this.pointer];
				if (child.GetType() == typeof(VirtualDirectory))
				{

				}
			}
		}
	}
}
