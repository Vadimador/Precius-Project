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

namespace Precius_service
{
    public partial class Precius : ServiceBase
    {
        private int eventId = 1;
        // tab module
        // tab secteurs

        
        private bool OutPutCheckFormat(string args)
        {
            string path = @"C:\Users\Fred\Desktop\loki_WINDEV2112EVAL_2022-02-14_11-31-14.log";
            string pathfilewrite = @"C:\Users\Fred\Desktop\";
            int buff = 0;
     
            if (File.Exists(path))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    if (line.Contains(alert))
                    {
                        buff = buff + 1;
                        //if(line.Contains(fileinfec))                     
                    }
                    else if (line.Contains(warnings))
                    {
                        buff = buff + 1;
                        //if(line.Contains(fileinfec))
                    }
                }

                if (buff > 0)
                {
                    Console.WriteLine("INFECTED");
                    // creation d'un fichier qui contiendra la reponse de sortie du programme
                }
                else
                {
                    Console.WriteLine("NEGATIF");
                }
            }
            else
            {
                Console.WriteLine("file existe pas");
            }
            return false;
        }

        public Precius()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
        }

        protected override void OnStart(string[] args) // il faut que la fonction se termine pour ne pas bloquer tout l'os
        {
            eventLog1.WriteEntry("In OnStart.");

            // Set up a timer that triggers every minute.
            Timer timer = new Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
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
