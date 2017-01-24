using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSFileSearch
{
    class Search
    {
        private string tfsUrl;
        private string[] pattern;
        private string filePath = @"C:\temp\Find.txt";
        private VersionControlServer versionControl;
        private TfsTeamProjectCollection tfs;

        public static string[] filePatterns = new[] { "*.cs", "*.xml", "*.config", "*.asp", "*.aspx", "*.js", "*.htm", "*.html",
                                               "*.vb", "*.asax", "*.ashx", "*.asmx", "*.ascx", "*.master", "*.svc"}; //file extensions

        public Search(string Url, string[] Pattern)
        {
            this.tfsUrl = Url;
            this.pattern = Pattern;
        }

        public void Connect()
        {
            tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(this.tfsUrl));
            tfs.EnsureAuthenticated();
            versionControl = tfs.GetService<VersionControlServer>();
        }

        public void Execute()
        {
            string filePath = @"C:\temp\Find.txt";
            StreamWriter outputFile = outputFile = new StreamWriter(filePath);

            int total = 0, count = 0;

            try
            {
                var allProjs = versionControl.GetAllTeamProjects(true);//.Where( t => t.Name.Contains("ABC"));

                total = allProjs.Count() * filePatterns.Count();

                foreach (var teamProj in allProjs)
                {
                    foreach (var filePattern in filePatterns)
                    {
                        count++;
                        this.OnPercentComplete(new ThresholdReachedEventArgs() { TimeReached = DateTime.Now, Value = (count / total) });
                        var items = versionControl.GetItems(teamProj.ServerItem + "/" + filePattern, RecursionType.Full).Items
                                    .Where(i => !i.ServerItem.Contains("_ReSharper"));  //skipping resharper stuff
                        foreach (var item in items)
                        {
                            List<string> lines = SearchInFile(item);
                            if (lines.Count > 0)
                            {
                                outputFile.WriteLine("FILE:" + item.ServerItem);
                                outputFile.WriteLine(lines.Count.ToString() + " occurence(s) found.");
                                outputFile.WriteLine();
                            }
                            foreach (string line in lines)
                            {
                                outputFile.WriteLine(line);
                            }
                            if (lines.Count > 0)
                            {
                                outputFile.WriteLine();
                            }
                        }
                    }
                    outputFile.Flush();
                }
                //MessageBox.Show("Search is complete. Please visit " + filePath + " for the results.", this.Text);
            }
            catch (Exception ex)
            {
                outputFile.Write(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                if (outputFile != null)
                {
                    outputFile.Flush();
                    outputFile.Close();
                }
            }
            this.OnCompleted(new EventArgs());
        }

        private List<string> SearchInFile(Item file)
        {
            var result = new List<string>();

            try
            {
                var stream = new StreamReader(file.DownloadFile(), Encoding.Default);

                var line = stream.ReadLine();
                var lineIndex = 0;

                while (!stream.EndOfStream)
                {
                    if (this.pattern.Any(p => line.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
                        result.Add("=== Line " + lineIndex + ": " + line.Trim());

                    line = stream.ReadLine();
                    lineIndex++;
                }
            }
            catch (Exception e)
            {
                string ex = e.Message;
                Console.WriteLine("!!EXCEPTION: " + e.Message);
                Console.WriteLine("Continuing... ");
            }

            return result;
        }

        #region Events
        public event EventHandler Completed;
        public event ThresholdReachedEventHandler PercentComplete;
        public delegate void ThresholdReachedEventHandler(object o, ThresholdReachedEventArgs e);

        protected virtual void OnCompleted(EventArgs e)
        {
            EventHandler handler = Completed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPercentComplete(ThresholdReachedEventArgs e)
        {
            ThresholdReachedEventHandler handler = PercentComplete;
            if (handler != null)
            {
                handler.Invoke(this, e);
            }
        }
        #endregion
    }

    public class ThresholdReachedEventArgs : EventArgs
    {
        public int Value { get; set; }
        public DateTime TimeReached { get; set; }
    }
}
