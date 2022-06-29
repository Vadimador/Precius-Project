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

/*
    static bool parsingfile()
    {
        List<String> stringList = new List<string>();

        string badoutput;
        StreamReader sr = new StreamReader(@"C:\Users\Fred\Desktop\hashmap.txt");
        badoutput = sr.ReadToEnd();

        string text;
        StreamReader tr = new StreamReader(@"C:\Users\Fred\Desktop\loki_WINDEV2112EVAL_2022-02-14_11-31-14.log");
        text = tr.ReadToEnd();

        string[] hashmap = badoutput.Split(',');
             
        foreach (string i in hashmap)
        {
            //Console.WriteLine(i);
            if (i.StartsWith('"'))
            {  
                stringList.Add(i.Substring(1,i.Length - 2));
            }
            else if (i == "!!")
            {
                stringList.Add(i);
            }
            
        }


        foreach (string i in stringList)
        {
             
            if (i.Contains("!!"))
            {
                //recupère la taille de la liste
                int lengthlist = stringList.Count;

                int index = stringList.IndexOf(i);
                index += 1;
                
                for (int count = index; count < lengthlist; count++)
                {
                    
                    //Console.WriteLine(stringList.ElementAt(count));
                   if (!text.Contains(stringList.ElementAt(count)))
                    {   
                        return true;
                        //buff += 1;
                        //Console.WriteLine(" the buff value : {0}", buff);
                    }
                    
                    

                }
            
            }
        }   return false;
    }

}


/*
    static bool parsingfile(string[] hashmap)
    {
        string text;
        StreamReader sr = new StreamReader(@"C:\Users\Fred\Desktop\loki_WINDEV2112EVAL_2022-02-14_11-31-14.log");
        text = sr.ReadToEnd();

        foreach (string i in hashmap)
        {
           
           
        }



        return false;
    }


    static bool goodresult(string[] hashmap)
    {
         foreach (string i in stringList)
        {
            if (i.Contains("!"))
            {
                //recupère la taille de la liste
                int lengthlist = stringList.Count;

                int index = stringList.IndexOf(i);
                index += 1;
                
                
                for (int count = index; count < lengthlist; count++)
                {
                   Console.WriteLine(stringList.ElementAt(count));
                   if (!test.Contains(stringList.ElementAt(count)))
                    {
                        return true;
                    }
                }
            }
        }

    }

}
/*

else if(i.StartsWith("!"))
            {
                
            }
     static bool negation(string[] hashmap)
    {
        
        foreach (string i in hashmap)
        {
            if (i =="!")
            {
                return true;
                
            }
           
        }



        return false;
    }


/*
    static bool printlist(string[] stringList)
    {
        Console.WriteLine("content of first list :");

        foreach (string i in stringList)
        {
            Console.WriteLine(i);
        }
        return;
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