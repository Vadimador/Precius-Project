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

        private List<Sector> sectors = new List<Sector>(); // la liste de tous les sectors
        private int eventId = 1;
        // tab module
        // tab secteurs

        
        private bool OutPutCheckFormat(string args)
        {

            return false;
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
                            if(subTab[j].Trim().First() == '*' || subTab[j].Trim().First() == '/')
                            {
                                s.pathList.Add(subTab[j].Trim()); // on ajoute un path
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

        private void ShowSectors()
        {
            try
            {
                string seeSectors = "Affichage des secteurs : \n\n";
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
                eventLog1.WriteEntry(seeSectors, EventLogEntryType.Information, eventId++);

            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Une Exception à été catch ! " + e.Message, EventLogEntryType.Error, eventId++);
            }
            finally
            {

            }
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
            ShowSectors();
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
    }
}
