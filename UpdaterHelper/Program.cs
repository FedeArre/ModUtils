using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace UpdaterHelper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string DirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
                string ModsFolderPath = DirectoryPath + "/../";
                Console.WriteLine("[ModUtils/Autoupdating]: Autoupdater helper client started, hello!");

                Process[] processes = Process.GetProcessesByName("My Garage");
                Console.WriteLine($"[ModUtils/Autoupdating]: Awaiting game close");
                while (processes.Length != 0)
                {
                    System.Threading.Thread.Sleep(2000);
                    Console.WriteLine($"[ModUtils/Autoupdating]: Awaiting game close");
                    processes = Process.GetProcessesByName("My Garage");
                }

                Console.WriteLine($"[ModUtils/Autoupdating]: Game closed!");
                Console.WriteLine($"[ModUtils/Autoupdating]: Detected path: {DirectoryPath}");
                Console.WriteLine("[ModUtils/Autoupdating]: Detected the following files to be updated:");

                DirectoryInfo CurrentDirectory = new DirectoryInfo(DirectoryPath);
                FileInfo[] Files = CurrentDirectory.GetFiles("*.dll");

                foreach (FileInfo f in Files)
                {
                    Console.WriteLine($"[ModUtils/Autoupdating]: {f.Name}");
                    File.SetAttributes(f.FullName, FileAttributes.Normal);
                }

                DirectoryInfo ModsFolder = new DirectoryInfo(ModsFolderPath);
                if(ModsFolder.Exists) 
                {
                    Console.WriteLine("[ModUtils/Autoupdating]: Mods folder was found");
                }

                Console.WriteLine("[ModUtils/Autoupdating]: Starting updating process");
                foreach(FileInfo f in Files)
                {
                    if(File.Exists(ModsFolderPath + f.Name))
                    {
                        Console.WriteLine($"[ModUtils/Autoupdating]: Found mod {f.Name}, replacing with new version");
                        File.SetAttributes(ModsFolderPath + f.Name, FileAttributes.Normal);
                        File.Copy(f.FullName, ModsFolderPath + f.Name, true);
                        /*File.Delete(ModsFolderPath + f.Name);

                        Console.WriteLine($"[ModUtils/Autoupdating]: Moving new version into place");
                        File.Move(Path.Combine(DirectoryPath, f.Name), ModsFolderPath);*/
                    }
                    else
                    {
                        Console.WriteLine($"[ModUtils/Autoupdating/Warning]: Could not find mod {f.Name} on Mods folder ({ModsFolderPath + f.Name}). Press any key to continue");
                        Console.ReadKey();
                    }
                }

                Console.WriteLine("[ModUtils/Autoupdating]: Mods were succesfully updated, opening game now.");
                Process.Start("steam://rungameid/1578390");
                Environment.Exit(0);
            }
            catch(Exception ex)
            {
                Console.WriteLine("[ModUtils/Autoupdating/Error]: Something has gone very wrong, please report this. Mods were not updated probably");
                Console.WriteLine($"[ModUtils/Autoupdating/Error]: {ex.Message}");
                Console.WriteLine($"[ModUtils/Autoupdating/Error]: {ex.StackTrace}");
                Console.WriteLine($"[ModUtils/Autoupdating/Error]: {ex.InnerException}");
                Console.ReadKey();
            }
        }
    }
}
