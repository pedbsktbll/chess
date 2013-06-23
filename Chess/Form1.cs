using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Chess
{
    public partial class Form1 : Form
    {
		private String fileName = null;

        public Form1()
		{
            InitializeComponent();
			return;
        }

		private void button1_Click( object sender, EventArgs e )
		{
			openFileDialog1.Filter = "XML Files (*.xml)|*.xml";
			openFileDialog1.FilterIndex = 1;
			openFileDialog1.RestoreDirectory = true;
			if( openFileDialog1.ShowDialog() == DialogResult.OK )
			{
				fileName = openFileDialog1.FileName;
				label1.Text = fileName;
			}
			else
				fileName = null;
		}

		private void button2_Click( object sender, EventArgs e )
		{
			if( fileName == null || fileName.Equals( "" ) || !File.Exists(fileName) )
			{
				MessageBox.Show( "Error! Must select XML file!", "Select File", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

            // Check if file open or otherwise in use:
			for( bool tryAgain = true; tryAgain; )
			{
				tryAgain = false;
				FileStream stream = null, stream2 = null;
				try
				{
					FileInfo file = new FileInfo(fileName);
					stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
					if( File.Exists(fileName + ".bkp") )
					{
						FileInfo file2 = new FileInfo(fileName + ".bkp");
						stream2 = file2.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
					}
				}
				catch( IOException )
				{
					if( MessageBox.Show("Error! Please close all instances of the file and try again", "File Open", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) 
						== DialogResult.Cancel )
						return;
					tryAgain = true;
				}
				finally
				{
					if( stream != null )
						stream.Close();
					if( stream2 != null )
						stream2.Close();
				}
			}

			try
			{
				Tournament T = new Tournament(fileName);
				if( File.Exists(fileName + ".bkp") )
					File.Delete(fileName + ".bkp");
				File.Move(fileName, fileName + ".bkp");

				T.printNewFile(fileName);
				T.GenerateMatchups();
				if( T.printMatchups(Directory.GetParent(fileName).FullName) )
					MessageBox.Show("Success!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
				else
					MessageBox.Show("Semi-Success...?", "Umm... Something didn't look right about those matchups. Might want to double check those...",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			catch( System.Exception ex )
			{
				MessageBox.Show("Error! " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
    }
}
