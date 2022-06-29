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
        private int eventId = 1;

        // définition des différents mot clés
        public const string NAME = "name";
        public const string FILESCAN = "filescan";
        public const string QUARANTINE = "quarantine";

        // Ajout des mots clés dans un tableau en commun
        public string[] tabKeyWord = { NAME, FILESCAN, QUARANTINE };

        // définition de la structure Sector, représentant les secteurs définis par l'utilisateur
        struct Sector
        {
            public IDictionary<string,string> rules;
            public List<string> pathList;

            public Sector(string name,string[] tabkeyword)
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
            public string bad_output; // les mauvais output lors de son execution

            public Precius_Module(string filepath, string executable_chain)
            {
                this.filepath = filepath;
                this.executable_chain = executable_chain;
                this.bad_output = "";
            }
        }

        private List<Sector> sectors = new List<Sector>(); // la liste de tous les sectors
        private IDictionary<string, Precius_Module> modules = new Dictionary<string,Precius_Module>();


        private void OutPutCheckFormat(string text)
        {
            //List<string> hashm = new List<string>();
            //List<string> txt = new List<string>();
            //hashmap
            int result = 0;

            foreach (string i in text)
            {
                foreach (string y in hashm)
                {
                    if (i == y)
                    {
                        int buff = +1;
                        result = buff;
                    }
                    else
                    {
                        result = 0;
                    }
                    if (result >= 1)
                    {
                        string logpath = @"C:\Users\Fred\Desktop\log.txt";
                        using (StreamWriter writer = new StreamWriter(logpath, true))
                        {
                            writer.WriteLine($"{DateTime.Now} : {result}");
                        }
                        break;
                    }

                 }

            }
        }

        private void lireFichierTest(string chemin)
        {
            String line;
            try
            {
                StreamReader sr = new StreamReader(chemin);
                line = sr.ReadToEnd();
                eventLog1.WriteEntry(line, EventLogEntryType.SuccessAudit, eventId++);
                sr.Close();
            }
            catch(Exception e)
            {
                eventLog1.WriteEntry("Une Exception à été catch ! " + e.Message, EventLogEntryType.Error, eventId++);
            }
            finally
            {
                eventLog1.WriteEntry("in Finally block", EventLogEntryType.Information, eventId++);
            }
        }

        private void LoadSectors()
        {
            String all;
            try
            {
                StreamReader sr = new StreamReader("C:\\Users\\vadgo\\Documents\\Precius\\sector-config.txt");
                all = sr.ReadToEnd();
                eventLog1.WriteEntry("Lecture du fichier de config sector.\n" + "config recu : \n" + all, EventLogEntryType.Information, eventId++);
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
                            if(subTab[j].First() == '|')
                            {
                                subTab[j] = subTab[j].Replace("|", "");
                                s.pathList.Add(subTab[j]); // on ajoute un path
                                //eventLog1.WriteEntry("sector [" + (i - 1) + "] : new path (" + j + ") : " + subTab[j], EventLogEntryType.Information, eventId++);
                            }
                            else if (this.tabKeyWord.Contains(subTab[j].Split('=')[0].Trim()))
                            {
                                s.rules[subTab[j].Split('=')[0].Trim()] = subTab[j].Split('=')[1].Trim(); // on ajoute une règle
                                //eventLog1.WriteEntry("sector [" + (i - 1) + "] : new rule (" + j + ") : " + subTab[j], EventLogEntryType.Information, eventId++);
                            }
                            
                        }
                    }
                    this.sectors.Add(s);
                }
                //eventLog1.WriteEntry(tab[i], EventLogEntryType.Information, eventId++);

                sr.Close();
            }
            catch (Exception e)
            {
                
                eventLog1.WriteEntry("Une Exception à été catch ! " + e.Message, EventLogEntryType.Error, eventId++);
            }
            finally
            {
                eventLog1.WriteEntry("in Finally block", EventLogEntryType.Information, eventId++);
            }
        }

        private void LoadModules()
        {
            String all;
            try
            {
                StreamReader sr = new StreamReader("C:\\Users\\vadgo\\Documents\\Precius\\module-config.txt");
                all = sr.ReadToEnd();
                eventLog1.WriteEntry("Lecture du fichier de config module.\n" + "config recu : \n" + all, EventLogEntryType.Information, eventId++);
                all = all.Trim();
                string[] tab = all.Split(new string[] { "[new module]" }, StringSplitOptions.None);
                string[] subTab = { };

                //eventLog1.WriteEntry("taille tableau : " + tab.Length , EventLogEntryType.Information, eventId++);


                for (int i = 1; i < tab.Length; i++) // i est à 1 car il y à toujours un string vide
                {
                    //eventLog1.WriteEntry("i be like : " + i, EventLogEntryType.Information, eventId++);
                    tab[i] = tab[i].Trim().Replace("\n\n", "");
                    subTab = tab[i].Split('\n');

                    
                    if (subTab.Length == 3)
                    {
                        Precius_Module m = new Precius_Module(subTab[1].Trim(), subTab[2].Trim());  // on génére un module vide
                        //eventLog1.WriteEntry(subTab[0].Trim() + " file path : " + m.filepath, EventLogEntryType.Information, eventId++);
                        //eventLog1.WriteEntry(subTab[0].Trim() + " chaine executable : " + m.executable_chain, EventLogEntryType.Information, eventId++);

                        this.modules.Add(subTab[0].Trim(),m);

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
               
                for(int i = 0; i < sectors.Count(); i++)
                {
                    seeSectors += "--------------------------------------------------------------\n";
                    seeSectors += "sector numéro : " + i + "\n";
                    seeSectors += " ------ All path :\n";
                    for(int j = 0; j < sectors[i].pathList.Count(); j++)
                    {
                        seeSectors += "     " + sectors[i].pathList[j] + "\n";
                    }

                    seeSectors += " ------ All rules :\n";

                    for (int j = 0; j < tabKeyWord.Length; j++)
                    {
                        seeSectors += "     " + tabKeyWord[j] + "=" + sectors[i].rules[tabKeyWord[j]] + "\n";
                    }
                }
                //eventLog1.WriteEntry(seeSectors, EventLogEntryType.Information, eventId++);

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
                    seeModules += "bad outputs : " + this.modules.ElementAt(i).Value.bad_output + "\n";
                }
                //eventLog1.WriteEntry(seeModules, EventLogEntryType.Information, eventId++);
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
            res += this.modules[module].bad_output + "\n";

            return res;
        }

        private void executeModule(string moduleName)
        {

        }
        
        private bool SearchLog(string stringToFind,ref string write, int last = 10)
        {
            EventLogEntryCollection elec = this.eventLog1.Entries;

            if (elec.Count > 0)
            {
                for (int i = elec.Count - 1 ; i >= elec.Count - 1 - last; i--)
                {
                    if (i < 0)
                    {
                        i = 0;
                        break;
                    }
                    eventLog1.WriteEntry("OMG message a " + i + " est : " + elec[i].Message);
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

            for(int i = 0; i < this.sectors.Count; i++)
            {
                foreach(string s in this.sectors[i].pathList){
                    //splitedPath = path.Split('*');
                    string temp = ConvertPathToStandarsPath(s);
                    if (temp == "*")
                    {
                        return i;
                    }

                    splitedSectorPath = new List<string>(temp.Split('*'));

                    for(int k = 0;k < splitedSectorPath.Count; k++) // on supprime les espace vide
                    {
                        if(splitedSectorPath[k] == "")
                        {
                            splitedSectorPath.RemoveAt(k);
                            k--;
                        }
                    }

                    int nbDetected = 0; // le nombre de string détécté
                    int index = 0; // l'index de recherche
                    int result = -2; // le resultat de la recherche
                    for(int j = 0; j < splitedSectorPath.Count; j++)
                    {
                        if(path.Length <= index)
                        {
                            goto nextpath;
                        }
                        result = path.IndexOf(splitedSectorPath[j],index);
                        if(result == -1 || result < index)
                        {
                            goto nextpath;
                        }
                        index = result + splitedSectorPath[j].Length;
                        nbDetected++;
                    }
                    if(nbDetected == splitedSectorPath.Count)
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
            path = path.Replace("\\","/");
            path = path.Replace("c:", "C:");
            path = path.Replace("|","");
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
            eventLog1.WriteEntry("Custom command reçu, numéro : " + command, EventLogEntryType.Information, eventId++);
            string log = "";
            switch (command)
            {
                case 128: // show sectors
                    log = "CustomCommand.ShowSectors\n";
                    eventLog1.WriteEntry(log + ShowSectors(), EventLogEntryType.SuccessAudit, eventId++);
                    break;

                case 129: // show modules
                    log = "CustomCommand.ShowModules\n";
                    eventLog1.WriteEntry(log + ShowModules(),EventLogEntryType.SuccessAudit, eventId++);
                    break;
                case 130: // module information command
                    string write = "";
                    if(SearchLog("Talker.ModuleInformation", ref write))
                    {
                        string[] temp = write.Split('\n');
                        if(temp.Length == 2)
                        {
                            log = "CustomCommand.ModuleInformation\n";
                            eventLog1.WriteEntry(log + ModuleInformation(temp[1]), EventLogEntryType.SuccessAudit, eventId++);
                        }
                        
                    }
                    break;
                case 131:// convert path to sector
                    string write2 = "";
                    if (SearchLog("Talker.ConvertPathToSector",ref write2))
                    {
                        string[] temp = write2.Split('\n');
                        if (temp.Length == 2)
                        {
                            log = "CustomCommand.ConvertPathToSector";
                            int response = this.PathToSector(temp[1]);
                            eventLog1.WriteEntry(log + '\n' + response + '\n' + this.sectors[response], EventLogEntryType.SuccessAudit, eventId++);
                        }
                    }
                    break;
                case 255: // fonction de test bip
                    log = "CustomCommand.Bip\n";
                    eventLog1.WriteEntry(log + ModuleInformation("helloworld"), EventLogEntryType.SuccessAudit,eventId++);
                    break;
                default:
                    eventLog1.WriteEntry("Custom command inconnu reçu, numéro : " + command, EventLogEntryType.Error, eventId++);
                    break;
            }
        }

        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }
    }
}
