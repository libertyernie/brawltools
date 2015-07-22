//================================================================\\
//  Simple application containing most functions for interfacing  \\
//      with Github API, including Updater and BugSquish.         \\
//================================================================\\
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace Net
{
    public static class Updater
    {
        public static readonly string BaseURL = "https://github.com/libertyernie/brawltools/releases/download/";
        public static string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static async Task UpdateCheck() { await UpdateCheck(false); }
        public static async Task UpdateCheck(bool Overwrite)
        {
            // check to see if the user is online, and that github is up and running.
            Console.WriteLine("Checking connection to server.");
            using (Ping s = new Ping())
                Console.WriteLine(s.Send("www.github.com").Status);

            // Initiate the github client.
            GitHubClient github = new GitHubClient(new Octokit.ProductHeaderValue("Brawltools"));

            // get repo, Release, and release assets
            Repository repo = await github.Repository.Get("libertyernie", "brawltools");
            Release release = (await github.Release.GetAll(repo.Owner.Login, repo.Name))[0];
            ReleaseAsset Asset = (await github.Release.GetAssets("libertyernie", repo.Name, release.Id))[0];

            // Check if we were passed in the overwrite paramter, and if not create a new folder to extract in.
            if (!Overwrite)
            {
                Directory.CreateDirectory(AppPath + "/" + release.TagName);
                AppPath += "/" + release.TagName;
            }
            else
            {
                //Find and close the brawlbox application that will be overwritten
                Process[] px =  Process.GetProcessesByName("BrawlBox");
                Process p = px.FirstOrDefault(x => x.MainModule.FileName.StartsWith(AppPath));
                if (p != null && p != default(Process))
                {
                    p.CloseMainWindow();
                    p.Close();
                }
            }

            using (WebClient client = new WebClient())
            {
                // Add the user agent header, otherwise we will get access denied.
                client.Headers.Add("User-Agent: Other");

                // Full asset streamed into a single string
                string html = client.DownloadString(Asset.Url);

                // The browser download link to the self extracting archive, hosted on github
                string URL = html.Substring(html.IndexOf(BaseURL)).TrimEnd(new char[] { '}', '"' });

                Console.WriteLine("\nDownloading");
                client.DownloadFile(URL, AppPath + "/Update.exe");
                Console.WriteLine("\nSuccess!");

                Console.Clear();
                Console.WriteLine("Starting install");

                Process update = Process.Start(AppPath + "/Update.exe", "-d\"" + AppPath + "\"");
            }
        }

        public static async Task CheckUpdates(string releaseTag, bool manual = true)
        {
            try
            {
                var github = new GitHubClient(new Octokit.ProductHeaderValue("Brawltools"));
                IReadOnlyList<Release> release = null;
                try
                {
                    release = await github.Release.GetAll("libertyernie", "brawltools");
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    MessageBox.Show("Unable to connect to the internet.");
                    return;
                }

                if (release != null &&
                    release.Count > 0 &&
                    !String.Equals(release[0].TagName, releaseTag, StringComparison.InvariantCulture) && //Make sure the most recent version is not this version
                    release[0].Name.IndexOf("BrawlBox", StringComparison.InvariantCultureIgnoreCase) >= 0) //Make sure this is a BrawlBox release
                {
                    DialogResult UpdateResult = MessageBox.Show(release[0].Name + " is available! Update now?", "Update", MessageBoxButtons.YesNo);
                    if (UpdateResult == DialogResult.Yes)
                    {
                        DialogResult OverwriteResult = MessageBox.Show("Overwrite current installation?", "", MessageBoxButtons.YesNoCancel);
                        if (OverwriteResult != DialogResult.Cancel)
                        {
                            Task t = UpdateCheck(OverwriteResult == DialogResult.Yes);
                            t.Wait();
                        }
                    }
                }
                else if (manual)
                    MessageBox.Show("No updates found.");
            }
            catch (Exception e)
            {
                if (manual)
                    MessageBox.Show(e.Message);
            }
        }
    }

    public static class BugSquish
    {
        public static async Task CreateIssue(string Title, string IssueBody)
        {
            Octokit.Credentials s = new Credentials("6c6b2a56408a04a1b1a002d60202df2b520c88a4");
            GitHubClient github = new GitHubClient(new Octokit.ProductHeaderValue("Brawltools")) { Credentials = s };

            // get repo, Release, and release assets
            Repository repo = await github.Repository.Get("libertyernie", "brawltools");
            NewIssue issue = new NewIssue(Title) { Body = IssueBody };
            Issue x = await github.Issue.Create("libertyernie", "brawltools", issue);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            bool somethingDone = false;

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "-r": //overwrite
                        somethingDone = true;
                        Task t = Updater.UpdateCheck(true);
                        t.Wait();
                        break;
                    case "-b": //brawlbox call
                        somethingDone = true;
                        Task t2 = Updater.CheckUpdates(args[1], args[2] != "0");
                        t2.Wait();
                        break;
                }
            }
            else if (args.Length == 0)
            {
                somethingDone = true;
                Task t = Updater.UpdateCheck();
                t.Wait();
            }

            if (!somethingDone)
                Console.WriteLine("Usage: -r = Overwrite files in directory");
        }
    }
}