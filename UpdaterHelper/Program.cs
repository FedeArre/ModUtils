using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace UpdaterHelper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string DirectoryPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
                string ModsFolderPath = Directory.GetParent(DirectoryPath).FullName;

                Console.WriteLine(DirectoryPath);
                Console.WriteLine(ModsFolderPath);

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
                    if(f.Name == "_SimplePartLoader.dll")
                    {
                        Console.WriteLine("[ModUtils/Autoupdating]: ModUtils has been detected, will try to check for BepInEx installation.");
                        string RootFolderPath = Directory.GetParent(ModsFolderPath).FullName;
                        if ((File.Exists(Path.Combine(RootFolderPath,"libdoorstop.so")) || File.Exists(Path.Combine(RootFolderPath, "winhttp.dll"))) && Directory.Exists(Path.Combine(RootFolderPath, "BepInEx")))
                        {
                            Console.WriteLine("[ModUtils/Autoupdating]: Found BepInEx installation");
                        }
                        else
                        {
                            //Console.Clear();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("NOTE FOR THE USER - READ!");

                            Console.Write("BepInEx installation not present or broken. This is required to install ");
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write("new ModUtils versions.\n");

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("PRESS ANY KEY");
                            Console.ForegroundColor= ConsoleColor.White;
                            Console.Write(" to confirm you want to download it, otherwise close the installer.");

                            Console.ReadKey();
                            //Console.Clear();

                            Console.WriteLine("Downloading required data from the official ModUtils server - This may take a bit");
                            using (HttpClient client = new HttpClient())
                            {
                                // Determinate path and URL based on OS
                                string tempFilePath = Path.Combine(RootFolderPath, "tempFileModUtils.zip");
                                string urlModutils = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "https://docs.fedes.uy/resources/general/bepInExWindows.zip" : "https://docs.fedes.uy/resources/general/bepInExLinux.zip"; // Check for Windows, if not assume Linux.

                                // Download
                                using (HttpResponseMessage response = client.GetAsync(urlModutils).Result)
                                {
                                    response.EnsureSuccessStatusCode();
                                    using (Stream stream = response.Content.ReadAsStreamAsync().Result)
                                    {
                                        using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                                        {
                                            stream.CopyTo(fileStream);
                                        }
                                    }
                                }

                                Console.WriteLine("[ModUtils/Autoupdating]: Download complete, now unpacking the file.");
                                using (FileStream zipToOpen = new FileStream(tempFilePath, FileMode.Open))
                                {
                                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                                    {
                                        foreach (ZipArchiveEntry entry in archive.Entries)
                                        {
                                            string destinationFileName = Path.Combine(RootFolderPath, entry.FullName);
                                            if (entry.FullName.EndsWith("/"))
                                            {
                                                Directory.CreateDirectory(destinationFileName);
                                            }
                                            else
                                            {
                                                // Ensure the destination directory exists
                                                Directory.CreateDirectory(Path.GetDirectoryName(destinationFileName));

                                                // Overwrite the file if it exists, otherwise create a new one
                                                entry.ExtractToFile(destinationFileName, true);
                                            }
                                        }
                                    }
                                }

                                // Delete the temporary file
                                File.Delete(tempFilePath);
                                Console.WriteLine("[ModUtils/Autoupdating]: Sucess! BepInEx installed");
                            }
                        }
                    }
                }

                DirectoryInfo ModsFolder = new DirectoryInfo(ModsFolderPath);
                if(ModsFolder.Exists) 
                {
                    Console.WriteLine("[ModUtils/Autoupdating]: Mods folder was found");
                }

                Console.WriteLine("[ModUtils/Autoupdating]: Starting updating process");
                foreach(FileInfo f in Files)
                {
                    string path = Path.Combine(ModsFolderPath,f.Name);
                    if (File.Exists(path))
                    {
                        Console.WriteLine($"[ModUtils/Autoupdating]: Found mod {f.Name}, replacing with new version");
                        File.SetAttributes(path, FileAttributes.Normal);
                        File.Copy(f.FullName, path, true);
                    }
                    else
                    {
                        Console.Write("\n\n");
                        Console.WriteLine($"[ModUtils/Autoupdating/Warning]: Could not find mod {f.Name} on Mods folder ({path}).");
                        Console.WriteLine($"*** {f.Name} HAS NOT BEEN UPDATED - PRESS ANY KEY TO CONTINUE UPDATING THE OTHER MODS ***");
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
