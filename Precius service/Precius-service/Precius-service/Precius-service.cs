using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using System.Text.RegularExpressions;


namespace Precius_service
{
    public partial class Precius : ServiceBase
    {
        private string precius_files_directory_path = "C:\\Program Files\\Precius";
        private string modules_file_path = "\\modules.conf";
        private string sectors_file_path = "\\sectors.conf";

        private int eventId = 1;

        // définition des différents mot clés
        public const string NAME = "name";
        public const string FILESCAN = "filescan";
        public const string QUARANTINE = "quarantine";

        public const string eco_binary = "<binary>"; // eco obligatoire
        public const string eco_filepath = "<filepath>";
        //public const string filelog = 

        // Ajout des mots clés dans un tableau en commun
        public string[] tabKeyWord = { NAME, FILESCAN, QUARANTINE };

        // définition de la structure Sector, représentant les secteurs définis par l'utilisateur
        struct Sector
        {
            public IDictionary<string, string> rules;
            public List<string> pathList;

            public Sector(string name, string[] tabkeyword)
            {
                this.pathList = new List<string>(); // la liste des paths de ce secteurs
                this.rules = new Dictionary<string, string>(); // les règles de redirection du secteurs
                for (int i = 0; i < tabkeyword.Length; i++)
                {
                    this.rules.Add(tabkeyword[i], "0"); // par défaut, tous est à redirigé vers aucun module
                }
                this.rules[NAME] = name;
            }

        }
        struct Precius_Module
        {
            public string filepath; // le chemin de l'executable/script du module
            public string executable_chain; // la chaine d'execution du module
            public string bad_outputs; // les mauvais output lors de son execution

            public Precius_Module(string filepath, string executable_chain, string bad_outputs)
            {
                this.filepath = filepath;
                this.executable_chain = executable_chain;
                this.bad_outputs = bad_outputs;
            }
        }

        // définition de la structure de log pour un filescan
        struct filescan_log
        {
            public string sector;
            public string sectorName;
            public string fileScanned;
            public string scanModule;
            public string evil;
            public string quarantined;
            public string quarantineModule;
            public string timeSpent;
            
            public string build_log()
            {
                string log = "";
                log += FILESCAN + '\n';
                log += "    sector=" + this.sector + '\n';
                log += "    sectorName=" + this.sectorName + '\n';
                log += "    fileScanned=" + this.fileScanned + '\n';
                log += "    scanModule=" + this.scanModule + '\n';
                log += "    evil=" + this.evil + '\n';
                log += "    quarantineModule=" + this.quarantineModule + '\n';
                log += "    quarantined=" + this.quarantined + '\n';
                log += "    timeSpent=" + this.timeSpent + '\n';

                return log;
            }
        }

        private List<Sector> sectors = new List<Sector>(); // la liste de tous les sectors
        private IDictionary<string, Precius_Module> modules = new Dictionary<string, Precius_Module>();


        private bool OutPutCheckFormat(string text, string bad_outputs)
        {
            //eventLog1.WriteEntry("text : " + text + " bad_outputs : " + bad_outputs);
            if (bad_outputs.Trim() == "" || text.Trim() == "")
            {
                return false;
            }

            List<String> stringList = new List<string>(); //List où seront contenus les keywords

            string[] keywords = bad_outputs.Split(','); // séparations des strings par des virgules

            foreach (string i in keywords) //ajout des keywords dans listes
            {
                if (i.StartsWith("\""))
                {
                    stringList.Add(i.Substring(1, i.Length - 2));
                }
                else if (i == "!!")
                {
                    stringList.Add(i);
                }

            }

            foreach (string y in stringList) // on cherche si le fichier log contiens un ou des keywords
            {
                if (text.Contains(y))
                {
                    //eventLog1.WriteEntry("Oh no : " + y);
                    return true;
                }
                else if (y.Contains("!!")) // quitte la boucle à partir des "!!"
                {
                    break;
                }
            }

            if (!stringList.Contains("!!")) {
                return false;
            }

            int lengthlist = stringList.Count;
            int index = stringList.IndexOf("!!");
            index += 1;
            for (int count = index; count < lengthlist; count++) // pour ensuite rechercher si les keywords restants de la liste ne sont pas présents dans le log
            {
                if (!text.Contains(stringList[count]))
                {
                    return true;
                }
                //eventLog1.WriteEntry("Oh yeeaa : " + stringList[count]);
            }
            return false; // si rien n'est détecté
        }

        private void CréationSystemFile()
        {
            try
            {
                if (!System.IO.Directory.Exists(precius_files_directory_path))
                {
                    System.IO.Directory.CreateDirectory(precius_files_directory_path);
                }

                if (!System.IO.File.Exists(precius_files_directory_path + sectors_file_path))
                {
                    System.IO.File.Create(precius_files_directory_path + sectors_file_path).Close();
                }

                if (!System.IO.File.Exists(precius_files_directory_path + modules_file_path))
                {
                    System.IO.File.Create(precius_files_directory_path + modules_file_path).Close();
                }
            }
            catch(Exception e)
            {
                eventLog1.WriteEntry("Error - " + e.Message, EventLogEntryType.SuccessAudit, eventId++);
            }
           
        }

        private void LoadSectors()
        {
            String all;
            try
            {
                StreamReader sr = new StreamReader(precius_files_directory_path + sectors_file_path);
                all = sr.ReadToEnd();
                sr.Close();
                eventLog1.WriteEntry("sectors.conf readed.\n" + "conf : \n" + all, EventLogEntryType.Information, eventId++);
                all = all.Trim();
                string[] tab = all.Split(new string[] { "[new sector]" }, StringSplitOptions.None);
                string[] subTab = { };
                for (int i = 1; i < tab.Length; i++) // i est à 1 car il y à toujours un string vide
                {
                    tab[i] = tab[i].Trim().Replace("\n\n", "");
                    subTab = tab[i].Split('\n');

                    Sector s = new Sector("unnamed", this.tabKeyWord); // par défaut ils sont sans nom

                    for (int j = 0; j < subTab.Length; j++)
                    {
                        if (subTab[j] != "")
                        {
                            subTab[j] = subTab[j].Trim();
                            if (subTab[j].First() == '|')
                            {
                                subTab[j] = subTab[j].Replace("|", "");
                                s.pathList.Add(subTab[j]); // on ajoute un path
                            }
                            else if (this.tabKeyWord.Contains(subTab[j].Split('=')[0].Trim()))
                            {
                                s.rules[subTab[j].Split('=')[0].Trim()] = subTab[j].Split('=')[1].Trim(); // on ajoute une règle
                            }

                        }
                    }
                    this.sectors.Add(s);
                }

               
            }
            catch (Exception e)
            {

                eventLog1.WriteEntry("Une Exception à été catch ! " + e.Message, EventLogEntryType.Error, eventId++);
            }
        }

        private void LoadModules()
        {
            String all;
            try
            {
                StreamReader sr = new StreamReader(precius_files_directory_path + modules_file_path);
                all = sr.ReadToEnd();
                sr.Close();
                eventLog1.WriteEntry("modules.conf readed.\n" + "conf : \n" + all, EventLogEntryType.Information, eventId++);
                all = all.Trim();
                string[] tab = all.Split(new string[] { "[new module]" }, StringSplitOptions.None);
                string[] subTab = { };



                for (int i = 1; i < tab.Length; i++) // i est à 1 car il y à toujours un string vide
                {
                    tab[i] = tab[i].Trim().Replace("\n\n", "");
                    subTab = tab[i].Split('\n');


                    if (subTab.Length >= 3 && subTab.Length <= 4)
                    {
                        string filepath = "";
                        string e_chain = "";
                        string bad_o = "";
                        if (subTab.Length >= 2)
                            filepath = subTab[1].Trim();
                        if (subTab.Length >= 3)
                            e_chain = subTab[2].Trim();
                        if (subTab.Length == 4)
                            bad_o = subTab[3].Trim();

                        Precius_Module m = new Precius_Module(filepath,e_chain,bad_o);  // on génére un module vide

                        this.modules.Add(subTab[0].Trim(), m);

                    }
                }

            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Une Exception à été catch ! - LoadModules" + e.Message, EventLogEntryType.Error, eventId++);
            }
            finally
            {

            }
        }

        private string ShowSectors()
        {
            string seeSectors = "Affichage des secteurs : \n\n";
            try
            {

                for (int i = 0; i < sectors.Count(); i++)
                {
                    seeSectors += "==============================================================\n";
                    seeSectors += "sector numéro : " + i + "\n";
                    seeSectors += " ------ All path :\n";
                    for (int j = 0; j < sectors[i].pathList.Count(); j++)
                    {
                        seeSectors += "     " + sectors[i].pathList[j] + "\n";
                    }

                    seeSectors += " ------ All rules :\n";

                    for (int j = 0; j < tabKeyWord.Length; j++)
                    {
                        seeSectors += "     " + tabKeyWord[j] + "=" + sectors[i].rules[tabKeyWord[j]] + "\n";
                    }
                }

            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Une Exception à été catch ! " + e.Message, EventLogEntryType.Error, eventId++);
            }
            finally
            {

            }
            return seeSectors;
        }

        private string ShowModules()
        {
            string seeModules = "Affichage des modules : \n\n";
            try
            {
                for (int i = 0; i < modules.Count(); i++)
                {
                    seeModules += "--------------------------------------------------------------\n";
                    seeModules += " ====== nom modules : " + this.modules.ElementAt(i).Key + "\n";
                    seeModules += "file path : " + this.modules.ElementAt(i).Value.filepath + "\n";
                    seeModules += "chaine d'execution : " + this.modules.ElementAt(i).Value.executable_chain + "\n";
                    seeModules += "bad outputs : " + this.modules.ElementAt(i).Value.bad_outputs + "\n";
                }
                return seeModules;

            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Une Exception à été catch ! " + e.Message, EventLogEntryType.Error, eventId++);
            }
            finally
            {

            }
            return seeModules;
        }

        private string ModuleInformation(string module)
        {
            string res = "";
            if (!this.modules.ContainsKey(module))
            {
                res = "Module \"" + module + "\" can't be found.";
                return res;
            }

            res += module + "\n";
            res += this.modules[module].filepath + "\n";
            res += this.modules[module].executable_chain + "\n";
            res += this.modules[module].bad_outputs + "\n";

            return res;
        }

        private bool SearchLog(string stringToFind, ref string write, int last = 10)
        {
            EventLogEntryCollection elec = this.eventLog1.Entries;

            if (elec.Count > 0)
            {
                for (int i = elec.Count - 1; i >= elec.Count - 1 - last; i--)
                {
                    if (i < 0)
                    {
                        i = 0;
                        break;
                    }
                    if (elec[i].Message.Contains(stringToFind))
                    {
                        write += elec[i].Message;
                        return true;
                    }
                }
            }


            return false;
        }

        // Retourne l'index du sector du path donnée, ou -1 si rien
        private int PathToSector(string path)
        {
            int sector = -1;
            path = ConvertPathToStandarsPath(path);
            List<string> splitedSectorPath;

            for (int i = 0; i < this.sectors.Count; i++)
            {
                foreach (string s in this.sectors[i].pathList) {
                    //splitedPath = path.Split('*');
                    string temp = ConvertPathToStandarsPath(s);
                    if (temp == "*")
                    {
                        return i;
                    }

                    splitedSectorPath = new List<string>(temp.Split('*'));

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
                        if (path.Length <= index)
                        {
                            goto nextpath;
                        }
                        result = path.IndexOf(splitedSectorPath[j], index);
                        if (result == -1 || result < index)
                        {
                            goto nextpath;
                        }
                        index = result + splitedSectorPath[j].Length;
                        nbDetected++;
                    }
                    if (nbDetected == splitedSectorPath.Count)
                    {
                        return i; // YATAAAAAAAAA
                    }
                nextpath:;

                }

            }

            return sector;
        }

        // Convertit une chaine de caractère reprèsentant un chemin en Standars path de la sorte "C:/***/***"
        private string ConvertPathToStandarsPath(string path)
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

        public Precius()
        {
            InitializeComponent();
            //sectors = new List<Sector>();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("Précius"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "Précius", "precius-log");
            }
            eventLog1.Source = "Précius";
            eventLog1.Log = "precius-log";
        }

        protected override void OnStart(string[] args) // il faut que la fonction se termine pour ne pas bloquer tout l'os
        {
            eventLog1.WriteEntry("In OnStart.");

            // Set up a timer that triggers every minute.
            Timer timer = new Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

            CréationSystemFile();
            LoadSectors(); // on génére les secteurs de précius
            LoadModules(); // on génére les modules de précius
            //ShowModules();
            //ShowSectors();
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);

        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop.");
        }

        protected override void OnCustomCommand(int command)
        {
            eventLog1.WriteEntry("Custom command : " + command, EventLogEntryType.Information, eventId++);
            string log = "";
            switch (command)
            {
                case 128: // show sectors
                    log = "CustomCommand.ShowSectors\n";
                    eventLog1.WriteEntry(log + ShowSectors(), EventLogEntryType.SuccessAudit, eventId++);
                    break;

                case 129: // show modules
                    log = "CustomCommand.ShowModules\n";
                    eventLog1.WriteEntry(log + ShowModules(), EventLogEntryType.SuccessAudit, eventId++);
                    break;
                case 130: // module information command
                    string write = "";
                    if (SearchLog("Talker.ModuleInformation", ref write))
                    {
                        string[] temp = write.Split('\n');
                        if (temp.Length == 2)
                        {
                            log = "CustomCommand.ModuleInformation\n";
                            eventLog1.WriteEntry(log + ModuleInformation(temp[1]), EventLogEntryType.SuccessAudit, eventId++);
                        }

                    }
                    break;
                case 131:// convert path to sector
                    string write2 = "";
                    if (SearchLog("Talker.ConvertPathToSector", ref write2))
                    {
                        string[] temp = write2.Split('\n');
                        if (temp.Length == 2)
                        {
                            log = "CustomCommand.ConvertPathToSector";
                            int response = this.PathToSector(temp[1]);
                            eventLog1.WriteEntry(log + '\n' + response + '\n' + (string)this.sectors[response].rules[NAME], EventLogEntryType.SuccessAudit, eventId++);
                        }
                    }
                    break;
                case 132: // send signal
                    string write3 = "";
                    if(SearchLog("Talker.SendSignal",ref write3))
                    {
                        string[] temp = write3.Split('\n');
                        if(temp.Length == 2)
                        {
                            receiveSignal(temp[1]);
                        }
                    }
                    break;
                /*case 255: // fonction de test bip
                    log = "CustomCommand.Bip\n";
                    receiveSignal(FILESCAN + "|D:\\Documents\\COURS_ESGI\\PA\\Precius-Project\\Modules\\virus.txt");
                    break;*/
                default:
                    eventLog1.WriteEntry("Unknowed Custom command : " + command, EventLogEntryType.Error, eventId++);
                    break;
            }
        }

        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }

        // ================= SCAN-FLOW =========================
        /// <summary>
        /// parse the receive signal to choose the right flow
        /// </summary>
        /// <param name="signal"> the signal as : signal_name|arg1|arg2|...</param>
        private void receiveSignal(string signal)
        {
            string signal_name = signal.Split('|')[0];
            string[] args = signal.Split('|');
            try
            {
                switch (signal_name)
                {
                    case FILESCAN:
                        {
                            /*
                             * FILESCAN ARGUMENT EXAMPLE
                             * arg1 : <filepath> : C:/the/directory/file.txt
                             */
                            Stopwatch stopWatch = new Stopwatch();
                            stopWatch.Start();
                            filescan_log log = new filescan_log();
                            string response = "";
                            string filepath = args[1];
                            int sector_num = this.PathToSector(filepath);
                            if (sector_num == -1)
                            {
                                eventLog1.WriteEntry("FILESCAN - This files " + filepath + " doesnt trigger any sectors", EventLogEntryType.Error, eventId++);
                                break;
                            }
                            log.sector = sector_num.ToString();
                            log.sectorName = this.sectors[sector_num].rules[NAME];
                            log.fileScanned = filepath;
                            string module_name = this.sectors[sector_num].rules[FILESCAN];
                            //eventLog1.WriteEntry("filepath : " + filepath + " sector_num : " + sector_num + " module_name : " + module_name, EventLogEntryType.SuccessAudit, eventId++);
                            if (module_name != "0") // si le module est activé
                            {
                                log.scanModule = module_name;
                                // fonction d'execution de module
                                string executable_chain = this.modules[module_name].executable_chain;

                                if (!executable_chain.Contains(eco_binary))
                                {
                                    eventLog1.WriteEntry("FILESCAN - there is no <binary> in the executable chain for the module " + module_name, EventLogEntryType.Error, eventId++);
                                    break;
                                }

                                executable_chain = executable_chain.Replace(eco_binary, "\"" + this.modules[module_name].filepath + "\"");

                                if (executable_chain.Contains(eco_filepath))
                                {
                                    executable_chain = executable_chain.Replace(eco_filepath, filepath);
                                }

                                bool error = this.ExecuteExecutableChain(executable_chain,ref response); // Execution de la chaine formaté

                                bool outputCheckResponse = this.OutPutCheckFormat(response, this.modules[module_name].bad_outputs);

                                log.evil = outputCheckResponse.ToString();
                                //eventLog1.WriteEntry("OutPutCheckFormat responded : " + outputCheckResponse, EventLogEntryType.Information, eventId++);
                                if (outputCheckResponse)
                                {
                                    module_name = this.sectors[sector_num].rules[QUARANTINE]; // on va chercher le module du quarantine du secteur
                                    if (module_name != "0")
                                    {
                                        log.quarantineModule = module_name;
                                        executable_chain = this.modules[module_name].executable_chain;
                                        executable_chain = executable_chain.Replace(eco_binary,"\"" + this.modules[module_name].filepath + "\"");
                                        executable_chain = executable_chain.Replace(eco_filepath, "\"" + filepath + "\"");
                                        error = this.ExecuteExecutableChain(executable_chain,ref response); // on lance la quarantaine si le fichier est trigger
                                        outputCheckResponse = this.OutPutCheckFormat(response, this.modules[module_name].bad_outputs);

                                        if (!outputCheckResponse && !error)
                                        {
                                            log.quarantined = true.ToString();
                                        }
                                        else
                                        {
                                            log.quarantined = false.ToString();
                                        }

                                    }
                                }

                            }
                            stopWatch.Stop();
                            TimeSpan ts = stopWatch.Elapsed;
                            string elapsedtime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds,ts.Milliseconds / 10);
                            log.timeSpent = elapsedtime;
                            eventLog1.WriteEntry(log.build_log(), EventLogEntryType.Information, eventId++);

                        }
                        break;
                    /*case QUARANTINE: // quarantaine n'est pas un signal
                        {
                            string filepath = args[1];
                        }
                        break;*/
                    default:
                        break;
                }
            }
            catch(Exception e)
            {
                eventLog1.WriteEntry("Error - " + e.Message, EventLogEntryType.Error, eventId++);
            }

        }

        private bool ExecuteExecutableChain(string chain, ref string output)
        {
            try
            {
                //eventLog1.WriteEntry("Executable_chain : " + chain, EventLogEntryType.SuccessAudit, eventId++);
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c " + chain);

                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;

                procStartInfo.CreateNoWindow = true;

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                string result = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                if (result != "")
                {
                    eventLog1.WriteEntry(result, EventLogEntryType.SuccessAudit, eventId++);
                }
                if (error != "")
                {
                    eventLog1.WriteEntry(error, EventLogEntryType.Error, eventId++);
                }

                output = result + "\n" + error;
                return (error != "")?true:false;
            }
            catch(Exception e)
            {
                eventLog1.WriteEntry("Exception : " + e.Message, EventLogEntryType.Error, eventId++);
            }
            return true;
           
        }
    }
}
