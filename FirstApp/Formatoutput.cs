// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Text;


class Program
{
    static void Main(string[] args)
    {
        bool result;
        result = parsing();
        Console.WriteLine(result);

    }

    static bool parsing()
    {
        List<String> stringList = new List<string>(); //List où seront contenus les keywords

        string strings; 
        StreamReader sr = new StreamReader(@"C:\Users\Fred\Desktop\hashmap.txt"); // les keywords nous indiquerons si le fichier log nous retourne un fichier malveillants 
        strings = sr.ReadToEnd();

        string text;
        StreamReader tr = new StreamReader(@"C:\Users\Fred\Desktop\loki_WINDEV2112EVAL_2022-02-14_11-31-14.log"); // le fichier log à analyser
        text = tr.ReadToEnd();

        string[] keywords = strings.Split(','); // séparations des strings par des virgules
             
        foreach (string i in keywords) //ajout des keywords dans listes
        {
            if (i.StartsWith('"'))
            {  
                stringList.Add(i.Substring(1,i.Length - 2));
            }
            else if (i == "!!")
            {
                stringList.Add(i);
            }
            
        }
       
        foreach (string y in stringList) // on cherche si le fichier log contiens un ou des keywords
        {
           if(text.Contains(y))
           {
                return true;
           }
           else if(y.Contains("!!")) // quitte la boucle à partir des "!!"
           {
                break;
           }
        }

        int lengthlist = stringList.Count;
        int index = stringList.IndexOf("!!");
        index += 1;
        for (int count = index; count < lengthlist; count++) // pour ensuite rechercher si les keywords restants de la liste ne sont pas présents dans le log
        {
            if (!text.Contains(stringList.ElementAt(count)))
            {   
                return true;
            }                   
        }
        return false; // si rien n'est détecté
    }
}

