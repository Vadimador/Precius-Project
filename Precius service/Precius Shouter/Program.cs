using System;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading;

namespace Precius_Shouter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                if (args[1] == Key())
                {
                    ToastContentBuilder tcb = new ToastContentBuilder();
                    tcb.AddText("Précius Notification")
                        .AddAppLogoOverride(new Uri(@"D:\Documents\COURS_ESGI\PA\Precius-Project\Precius service\Precius Shouter\bin\Release\netcoreapp3.1\logo.png"), ToastGenericAppLogoCrop.Circle)
                        .AddText(args[0]);
                    tcb.Show(); // Not seeing the Show() method? Make sure you have version 7.0, and if you're using .NET 6 (or later), then your TFM must be net6.0-windows10.0.17763.0 or greater
                    Thread.Sleep(1000);
                }
                else
                {
                    throw new Exception("Key invalid");
                }

            }
            else
            {
                throw new Exception("Arguments incorrect");
            }
        }

        static string Key()
        {
            return "Precius";
        }
    }
}
