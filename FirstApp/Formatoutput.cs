// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Text;


class Program
{
    static void Main(string[] args)
    {
        List<string> hashm = new List<string>();
        List<string> output = new List<string>();
        
        int result = 0; 
        output.Add("tetetetet");
        output.Add("tetetetet");
        output.Add("tetet");

        hashm.Add("alert 0");
        hashm.Add("warning 0");
        hashm.Add("tetetetet");
        foreach(string i in hashm)
        {
            foreach(string y in output)
            {

                if (i == y)
                {
                    int buff =+ 1;
                    result = buff;
                }
                else
                {
                    result = 0; 
                }
                if (result >= 1)
                {
                    string logpath = @"C:\Users\Fred\Desktop\log.txt";
                    using(StreamWriter writer = new StreamWriter(logpath, true))
                    {
                        writer.WriteLine($"{DateTime.Now} : {result}");
                    }           
                    break;
                }
                
            }
            

        }
        
    }
}
/*
        //string fileinfec= "FILE:";
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
    }
}
*/