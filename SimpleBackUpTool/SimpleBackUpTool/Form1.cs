using System;
using System.IO;
using System.Windows.Forms;

namespace SimpleBackUpTool
{
	public partial class Form1 : Form
	{
		public Form1(BackUpSettings settings)
		{
			InitializeComponent();

			if (settings != null)
			{
				InitializeWithDefaultValues(settings);
			}
		}


		private void InitializeWithDefaultValues(BackUpSettings settings)
		{
			textBox1.Text = settings.TargetDirectory;
			foreach(string path in settings.OriginDirectories)
			{
				richTextBox1.Text += path + '\n';
			}

			if (settings.DefaultOverwriteState == ActionState.Yes)
			{
				radioButton1.Checked = true;
			}
			else if (settings.DefaultOverwriteState == ActionState.No)
			{
				radioButton2.Checked = true;
			}
			else
			{
				radioButton3.Checked = true;
			}

			if (settings.DefaultSkipState == ActionState.Yes)
			{
				radioButton4.Checked = true;
			}
			else if (settings.DefaultSkipState == ActionState.No)
			{
				radioButton5.Checked = true;
			}
			else
			{
				radioButton6.Checked = true;
			}
		}


		private void button1_Click(object sender, EventArgs e)
		{
			// Target Directories.
			FolderBrowserDialog folderDialog = new FolderBrowserDialog();
			DialogResult result = folderDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				textBox1.Text = folderDialog.SelectedPath;
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			// Origin Directories.
			FolderBrowserDialog folderDialog = new FolderBrowserDialog();
			DialogResult result = folderDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				richTextBox1.Text += folderDialog.SelectedPath + "\n";
			}
		}

		private void button3_Click(object sender, EventArgs e)
		{
			string name = textBox2.Text;

			string targetPath = textBox1.Text;
			string[] originPaths = richTextBox1.Text.Split('\n');

			ActionState overwriteState = radioButton1.Checked 
				? ActionState.Yes
				: radioButton2.Checked 
					? ActionState.No
					: ActionState.Unset;

			ActionState skipState = radioButton4.Checked
				? ActionState.Yes
				: radioButton5.Checked
					? ActionState.No
					: ActionState.Unset;

			BackUpSettings settings = new BackUpSettings()
			{
				TargetDirectory = targetPath,
				OriginDirectories = originPaths,
				DefaultOverwriteState = overwriteState,
				DefaultSkipState = skipState
			};

			foreach(string path in originPaths)
			{
				if(!Directory.Exists(path))
				{
					throw new DirectoryNotFoundException();
				}
			}

			Settings.Save(settings, name);
		}
	}
}
