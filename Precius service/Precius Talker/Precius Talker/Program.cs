using System;
using System.ServiceProcess;
using System.Diagnostics;
using System.Threading;

namespace Precius_Talker
{

    class Program
    {
        public const string show_modules = "show_modules"; // montre la table des modules
        public const string show_sectors = "show_sectors"; // montre la table des secteurs

        public const string sourceName = "Précius";
        public const string journalName = "precius-log";
        public const float timerWait = 4;
        static ServiceController Controller = null;
        static EventLog el = null;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (!initialiseServiceController())
                {
                    return;
                }
                initialiseArgLog();

                switch (args[0])
                {
                    case show_modules:
                       ShowModules();
                        break;
                    case show_sectors:
                       ShowSectors();
                        break;
                    default:
                        Console.WriteLine("Commande inconnue.");
                        Console.WriteLine(help());
                        break;

                }
            }
            else
            {
                Console.WriteLine(help());
            }
        }

        static public bool initialiseServiceController()
        {
            try
            {
                Controller = new ServiceController("Precius");
                Console.WriteLine("Service Status : " + Controller.Status); // on l'utilise pour
            }
            catch (Exception e)
            {
                Console.WriteLine("An error have occur : " + e.Message);
                Console.WriteLine(help());
                return false;
            }
            return true;
        }
        static public void initialiseArgLog()
        {
            el = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(sourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(sourceName, journalName);
            }
            el.Source = sourceName;
            el.Log = journalName;
        }

        static public void ShowSectors()
        {
            EventLogEntryCollection elec = el.Entries;
            int counter = elec.Count - 1;
            if (counter == -1)
                counter = 0;

            Controller.ExecuteCommand(128);
            

            float timer = timerWait;
            Console.WriteLine("Waiting for service response... \n");
            bool success = false;
            while(timer > 0)
            {
                elec = el.Entries;

                for (int i = counter; i < elec.Count; i++)
                {
                    //Console.WriteLine(elec[i].Message);
                    if (elec[i].Message.Contains("CustomCommand.ShowSectors"))
                    {
                        Console.WriteLine(elec[i].Message);
                        timer = 0;
                        success = true;
                        break;
                    }
                }
                if (success)
                    break;
                Thread.Sleep(1000);
                Console.WriteLine('.');
                timer--;
            }

            if(!success)
            {
                Console.WriteLine("\nTime out !");
            }
        }

        static public void ShowModules()
        {
            EventLogEntryCollection elec = el.Entries;
            int counter = elec.Count - 1;
            if (counter == -1)
                counter = 0;

            Controller.ExecuteCommand(129);


            float timer = timerWait;
            Console.WriteLine("Waiting for service response... \n");
            bool success = false;
            while (timer > 0)
            {
                elec = el.Entries;

                for (int i = counter; i < elec.Count; i++)
                {
                    //Console.WriteLine(elec[i].Message);
                    if (elec[i].Message.Contains("CustomCommand.ShowModules"))
                    {
                        Console.WriteLine(elec[i].Message);
                        timer = 0;
                        success = true;
                        break;
                    }
                }
                if (success)
                    break;
                Thread.Sleep(1000);
                Console.WriteLine('.');
                timer--;
            }

            if (!success)
            {
                Console.WriteLine("\nTime out !");
            }
        }

        static public string help()
        {
            string h = "Something went wrong, here's the help : \ncommandes : show_sectors, show_modules";


            return h;
        }
    }
}
