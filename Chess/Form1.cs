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
			if( fileName == null || fileName.Equals( "" ) )
			{
				MessageBox.Show( "Error! Must select XML file!", "Select File", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			Tournament T = new Tournament( fileName );
			if( File.Exists( fileName + ".bkp" ) )
				File.Delete( fileName + ".bkp" );
			File.Move( fileName, fileName + ".bkp" );

			T.printNewFile( fileName );
			T.GenerateMatchups();
			T.printMatchups( Directory.GetParent(fileName).FullName );
			MessageBox.Show( "Success!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information );
		}
    }
}
