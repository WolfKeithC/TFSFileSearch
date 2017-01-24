using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TFSFileSearch
{
    public partial class MainForm : Form
    {
        static string[] textPatterns = null;        

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string tfsUrl = ConfigurationManager.AppSettings["TFSurl"];
            txtTFSurl.Text = tfsUrl;
            string Pattern = ConfigurationManager.AppSettings["Pattern"];
            txtSearch.Text = Pattern;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            btnSearch.Enabled = false;

            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                MessageBox.Show("Please enter the search pattern.", this.Text);
                txtSearch.Focus();
            }

            textPatterns = txtSearch.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);  //Text to search

            //string filePath = @"C:\temp\Find.txt";
            //StreamWriter outputFile = null;

            try
            {
                Search s = new Search(txtTFSurl.Text, textPatterns);
                s.Connect();
                s.PercentComplete += S_PercentComplete;
                s.Completed += S_Completed;

                //outputFile = new StreamWriter(filePath);

                Thread t = new Thread(new ThreadStart(s.Execute));

                t.Start();

                /*
                pbFiles.Style = ProgressBarStyle.Marquee;
                pbFiles.MarqueeAnimationSpeed = 100;






                pbFiles.Style = ProgressBarStyle.Blocks;
                pbFiles.MarqueeAnimationSpeed = 0;
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, this.Text);
            }
        }

        private void S_PercentComplete(object o, ThresholdReachedEventArgs e)
        {
            pbFiles.Value = e.Value;
        }

        private void S_Completed(object sender, EventArgs e)
        {
            pbFiles.Style = ProgressBarStyle.Blocks;
            pbFiles.MarqueeAnimationSpeed = 0;
            pbFiles.Value = 100;
            this.Cursor = Cursors.Default;
            btnSearch.Enabled = true;
            MessageBox.Show("Finished", this.Text);
        }

        // Define other methods and classes here

    }
}