using System;
using System.ServiceProcess;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

// Idée pour l'execution actif : cmd.exe /c python.exe "D:\Documents\pythonProject\precius-helloWorld\precius-hellworld.py" -n "my name is bobby"
namespace Precius_Talker
{

    class Program
    {
        private const string show_modules = "show_modules"; // montre la table des modules
        private const string show_sectors = "show_sectors"; // montre la table des secteurs
        private const string module = "module"; // execute de manière actif un module existant
        private const string convert_path_to_sector = "convert_path_to_sector"; // execute de manière actif un module existant
        private const string send_signal = "send_signal"; // simule un signal envoyé depuis le minifilter
        private const string bip = "bip";

        private const string sourceName = "Précius";
        private const string journalName = "precius-log";
        private const float timerWait = 4;
        static ServiceController Controller = null;
        static EventLog el = null;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    initialiseServiceController();
                    initialiseArgLog();

                    switch (args[0])
                    {
                        case show_modules:
                            ShowModules();
                            break;
                        case show_sectors:
                            ShowSectors();
                            break;
                        case module:
                            List<string> arglist = new List<string>();
                            for (int i = 1; i < args.Length; i++)
                                arglist.Add(args[i]);

                            ExecuteModule(ref arglist);
                            break;
                        case convert_path_to_sector:
                            ConvertPathToSector(args[1]);
                            break;
                        case send_signal:
                            SendSignal(args[1]);
                            break;
                        case bip:
                            //Console.WriteLine("args : " + args[1]);
                            Console.WriteLine("Bip Result : " + Bip(args[1]));
                            break;
                        default:
                            throw new Exception("Unknowed command");
                            //break;

                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("/!\\ -- Error : " + e.Message);
                    Console.WriteLine(help());
                }
            }
            else
            {
                Console.WriteLine(help());
            }
        }

        // Fonction Utilitaire : 
        static private void initialiseServiceController()
        {
            Controller = new ServiceController("Precius");
            Console.WriteLine("Service Status : " + Controller.Status); // on l'utilise pour
            if(Controller.Status != ServiceControllerStatus.Running)
            {
                throw new Exception("The service is not running.");
            }
        }
        static private void initialiseArgLog()
        {
            el = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(sourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(sourceName, journalName);
            }
            el.Source = sourceName;
            el.Log = journalName;
        }
        static private bool WaitForLog(int executeCommand, string stringToFind, ref string write, float timerTime = timerWait)
        {
            EventLogEntryCollection elec = el.Entries;
            int counter = elec.Count - 1;
            if (counter == -1)
                counter = 0;

            Controller.ExecuteCommand(executeCommand);


            float timer = timerWait;
            Console.WriteLine("Waiting for service response... \n");
            bool success = false;
            while (timer > 0)
            {
                elec = el.Entries;

                for (int i = elec.Count - 1; i >= counter; i--)
                {
                    if (elec[i].Message.Contains(stringToFind))
                    {
                        //Console.WriteLine(elec[i].Message);
                        write += elec[i].Message;
                        timer = 0;
                        success = true;
                        return true;
                    }
                }
                if (success)
                    break;
                Thread.Sleep(1000);
                Console.WriteLine("...");
                timer--;
            }

            if (!success)
            {
                Console.WriteLine("\nTime out !");

            }
            return false;

        }
        static private void WriteLog(string log) {
            el.WriteEntry(log, EventLogEntryType.SuccessAudit, 0);
        }

        // Service Custom fonction :
        static private void ShowSectors()
        {
            string write = "";
            if (WaitForLog(128, "CustomCommand.ShowSectors", ref write))
            {
                Console.WriteLine(write);
            }
        }

        static private void ShowModules()
        {
            string write = "";
            if(WaitForLog(129, "CustomCommand.ShowModules",ref write))
            {
                Console.WriteLine(write);
            }
        }

        static private void ExecuteCommand(string command){
           try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                if(error.Length > 0)
                {
                    result += "\n --- error --- \n";
                    result += error;
                }
                
                // Display the command output.
                Console.WriteLine(result);
            }
            catch (Exception objException)
            {
                Console.WriteLine("Exception : " + objException.Message);
            }
        }

        static private void ExecuteModule(ref List<string> arg)
        {
            if (arg.Count == 0)
            {
                throw new Exception("bad argument for \"module\" command.");
            }

            string write = "";
            string log = "Talker.ModuleInformation\n" + arg[0];
            WriteLog(log);

            if (WaitForLog(130, "CustomCommand.ModuleInformation", ref write))
            {
                //Console.WriteLine(write);
                string[] result = write.Split("\n"); // chaine d'exécution
                write = result[3];

                if(!write.Contains("<binary>")){ // test de flag
                    throw new Exception("The flag \"<binary>\" was not found in the execution chain.");
                }

                string temp = result[2];
                temp = "\"" + temp + "\"";
                //Console.WriteLine(write);
                string finalChain = "";

                foreach(string s in write.Split(" "))
                {
                    finalChain += s;
                    if(s.Contains("<binary>")){
                        break;
                    }
                    finalChain += " ";
                }
                write = finalChain;
                for(int i = 1; i < arg.Count; i++){
                    
                    write += " " + arg[i];
                }

                write = write.Replace("<binary>", temp);
                //Console.WriteLine(write);
                
                ExecuteCommand(write);

            }
        }

        static private void ConvertPathToSector(string path)
        {
            if (path.Length != 0)
            {
                string write = "";
                WriteLog("Talker.ConvertPathToSector\n" + path);
                if(WaitForLog(131, "CustomCommand.ConvertPathToSector",ref write))
                {
                    string[] temp = write.Split('\n');
                    if (temp.Length >= 2)
                    {
                        Console.WriteLine("Sector attribué à ce path : " + temp[1] + ", named : " + temp[2] );
                    }
                    else
                    {
                        throw new Exception("Bad return from Precius-service");
                    }

                    
                }
            }
            else
            {
                throw new Exception("Le chemin donné est vide");
            }
        }

        static private void SendSignal(string signal)
        {
            if(signal.Length != 0)
            {
                WriteLog("Talker.SendSignal\n" + signal);
                Controller.ExecuteCommand(132);
                Console.WriteLine("/!\\ warning, this command simulates a signal from the minifilter, so it doesn't  wait for any response from the service.");
            }
        }

        static private bool Bip(string path)
        {
            Console.WriteLine("\n\n\n");
            string sector = "*D:*/important/*/banane";
            path = ConvertPathToStandarsPath(path);
            Console.WriteLine("Sector path : " + sector);
            Console.WriteLine("path : " + path);
            string tempPath = path;
            List<string> splitedSectorPath;

            if(sector == "*")
            {
                return true;
            }
            splitedSectorPath = new List<string>(sector.Split('*'));

            for (int k = 0; k < splitedSectorPath.Count; k++) // on supprime les espace vide
            {
                if (splitedSectorPath[k] == "")
                {
                    splitedSectorPath.RemoveAt(k);
                    k--;
                }
            }

            int nbDetected = 0; // le nombre de string détécté
            int index = 0; // l'index de recherche
            int result = -2; // le resultat de la recherche
            for (int j = 0; j < splitedSectorPath.Count; j++)
            {
                if (tempPath.Length <= index)
                {
                    return false;
                }
                result = tempPath.IndexOf(splitedSectorPath[j], index);
                if (result == -1 || result < index)
                {
                    return false;
                }
                index = result + splitedSectorPath[j].Length;
                nbDetected++;
            }
            if (nbDetected == splitedSectorPath.Count)
            {
                return true; // YATAAAAAAAAA
            }
            Console.WriteLine("oh no !");
            return false;

        }

        static private string ConvertPathToStandarsPath(string path)
        {
            path = path.Replace("\\", "/");
            path = path.Replace("c:", "C:");
            path = path.Replace("|", "");
            while (path.Contains("**"))
            {
                path = path.Replace("**", "*");
            }
            path = path.Trim();
            return path;
        }

        static private string help()
        {
            string h = "commandes :\n" +
                "       show_sectors --> show the sectors table of the Précius service\n" +
                "       show_modules --> show the modules table of the Précis service\n" +
                "       module <module name> [argument] --> execute a module configured in Precius service\n" +
                "       convert_path_to_sector <path> --> Renvoi le numéro de secteur attribué à ce chemin\n" +
                "       send_signal <signal> --> Simule un signal du minifilter\n" +
                "           => signal exemple : \"filescan|C:\\path\\to\\file.txt\"\n";



            return h;
        }
    }
}
 