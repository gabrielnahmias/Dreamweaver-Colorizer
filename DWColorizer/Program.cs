using DWColorizer.Properties;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Utilities;

/* TODO:
 *  - Detect the parent process (CMD or Explorer) and conditionally show "Done." message.
 */

namespace DWColorizer
{
    class Program
    {
        static void Main(string[] args)
        {
            string sTitle = ApplicationInfo.Title,
                   sAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                   sSuffix = @"\Adobe",
                   sPath;

            // Constants and constant-like strings.
            string TEXT_USAGE = "Adds a new color theme to Adobe Dreamweaver.\n\n" +
                                "DWC [/?] [/u]\n\n" +
                                "\t/?\t\tDisplays this help message." +
                                "\t/u\tUndos the theme replacement.",

                   TEXT_WELCOME = String.Format("Welcome to the {0}!\n" +
                                                "This will add a new color theme to Adobe Dreamweaver.\n\n", sTitle);
            
            List<string> lArgs = new List<string>(args);

            Process Dreamweaver = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C start dreamweaver";
            Dreamweaver.StartInfo = startInfo;

            // Set the foreground to red and the icon to the program's.
            Console.ForegroundColor = ConsoleColor.Red;
            //ConsoleHelper.SetConsoleFont(4);
            ConsoleHelper.SetConsoleIcon(Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location));
            
            Console.Title = sTitle + " v" + ApplicationInfo.Version.ToString();

            if (lArgs.Count >= 1 && lArgs[0].Equals("/?"))
            {
                Console.Write(TEXT_USAGE);
                goto end;
            }

            Console.Write(TEXT_WELCOME);
            ConsoleHelper.Pause();
            Console.Write("\n\n");

            if (Directory.Exists(sAppDataDir + sSuffix))
            {
                sPath = sAppDataDir + sSuffix;
            }
            else
            {
                Console.WriteLine("Adobe application data directory not found.");
                goto end;
            }

            DirectoryInfo diInstall = new DirectoryInfo(sPath);

            string sColorsFile = "Colors.xml",
                    sColorsXML = Resources.Colors;

            string[] aDirs = Directory.GetDirectories(sPath, "CodeColoring", SearchOption.AllDirectories);

            // Handle command-line arguments.
            if (lArgs.Count >= 1)
            {
                string sCommand = lArgs[0].ToLower();
                switch (sCommand)
                {
                    case "/u":
                        // If /u is specified, backup the current Colors.xml and restore the .bak version.
                        if (lArgs.Count >= 1 && lArgs[0].Equals("/u"))
                            Console.Write("Reverting color scheme...\n\n");
            
                        foreach(string sDir in aDirs)
                        {
                            string sColorsPath = String.Format(@"{0}\{1}", sDir, sColorsFile),
                                   sBackupPath = sColorsPath + ".bak",
                                   sCustomColors = File.ReadAllText(sColorsPath);

                            FileInfo fiColors = new FileInfo(sColorsPath);
                            bool bInitiallyExists = fiColors.Exists;

                            if (File.Exists(sBackupPath))
                            {
                                Console.WriteLine("Restoring old \"Colors.xml\" file...");
                                File.Copy(sBackupPath, sColorsPath, true);
                            }
                            else
                            {
                                Console.WriteLine("No backup exists.");
                                goto end;
                            }

                            Console.WriteLine("Backing up custom \"Colors.xml\" file...");
                            File.WriteAllText(sBackupPath, sCustomColors);

                            goto done;
                        }
                        break;
                }
            }
            
            foreach(string sDir in aDirs)
            {
                string sColorsPath = String.Format(@"{0}\{1}", sDir, sColorsFile);
                FileInfo fiColors = new FileInfo(sColorsPath);
                bool bInitiallyExists = fiColors.Exists;

                if (bInitiallyExists)
                {
                    Console.WriteLine("Backing up old \"Colors.xml\" file...");
                    File.Copy(sColorsPath, sColorsPath + ".bak", true);
                }

                Console.WriteLine("{0} \"Colors.xml\" file...", ((bInitiallyExists) ? "Overwriting" : "Creating" ));
                File.WriteAllText(sColorsPath, sColorsXML);
            }

            done:
            Console.Write("\nDone. Press enter and Dreamweaver will be opened to allow\n" +
                          "you to go to go to Preferences (Ctrl+U), click Code Coloring, and\n" +
                          "change the default background to #{0}.", (lArgs.Count >= 1 && lArgs[0].Equals("/u")) ? "FFFFFF" : "252A32" );

            pause:
            Console.ReadKey(true);
            
            Dreamweaver.Start();

            end:
            Console.WriteLine();
            Application.Exit();
        }
    }
}
