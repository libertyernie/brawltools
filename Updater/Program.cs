using Octokit;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;

namespace Updater
{
    class Program
    {
        // Base url to search for in the release asset stream
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

        static void Main(string[] args)
        {
            // -r is the overwrite switch.
            if (args.Length > 0 && args[0] == "-r")
            {
                Task t = UpdateCheck(true);
                t.Wait();
            }
            else if (args.Length > 0 && args[0] != "-r")
                Console.WriteLine("Usage: -r = Overwrite files in directory");
            else
            {
                Task t = UpdateCheck();
                t.Wait();
            }
        }
    }
}