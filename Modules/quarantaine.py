import os
import shutil
import crypto
import sys
import hashlib,sys,string
from subprocess import getoutput
from cryptography.fernet import Fernet
import datetime
from tinydb import TinyDB, Query, where


# C:\ProgramData\Microsoft\precius\ 
# requierement : pip install python-dev-tools
#  python -m pip install cryptography

# http://dotmobo.github.io/tinydb.html
# https://webdevdesigner.com/q/python-replace-single-backslash-with-double-backslash-48585/
# BASE DE DONNEE : pip install tinydb
# https://www.delftstack.com/fr/howto/python/replace-characters-in-strings-python/
####################### Entrer BDD .JSON

########################################## Repertoire
def repertoire():    
# vérifier l’existence du répertoire et le créer s’il n’existe pas
    try:
        os.mkdir('C:\ProgramData\Microsoft\precius')
        os.mkdir('C:\ProgramData\Microsoft\precius\Quarantine')
    except:
        print('\n\n***** Le répertoire Quarantaine existe déjà ****')
    print(' [+] repertoire quarantaine : OK !\n')

    try:
        os.mkdir('C:\ProgramData\Microsoft\precius\key')
    except:
            print('***** Le répertoire Key existe déjà ****')
    print(' [+] repertoire Key : OK !')
#############################################################################################
################################################## in BDD
def in_bdd(): 

    db = TinyDB('C:/ProgramData/Microsoft/precius/Quarantine/emplacement.json')
    table = db.table('fichier')
    virus_name = sys.argv[2]
    #virus_nameA = virus_name.replace('\\','/')
    virus_name = virus_name.split("\\")[-1]
    position_virus = str(sys.argv[2])
    position_virus = position_virus.replace('\\','/')
    print("position_virus :", position_virus)

    destinationf = 'C:/ProgramData/Microsoft/precius/Quarantine/' + virus_name

    table.insert({'status' : 'affectation', 'virus': virus_name, 'source': position_virus, 'destination' : destinationf})
    ##################################################################################################################
    ##################################################################################################################
    ##################################################################################################################
    ##################################################################################################################
####################### Sortie BDD .JSON
def out_bdd():
   
    db = TinyDB('C:/ProgramData/Microsoft/precius/Quarantine/emplacement.json')
    table = db.table('fichier')
    
    virus_name = sys.argv[2]
    print(" virus_name : ",virus_name)
    source = 'C:/ProgramData/Microsoft/precius/Quarantine/' + virus_name
  
    destinationbdd = table.search(where('virus') == virus_name and where('status') == 'affectation' and where('destination') == source)
    destinationbdd = str(destinationbdd)
    print(" destination1 : ",destinationbdd)

    destinationbdd = destinationbdd.split("'source':")[-1].split(',')[0]
    destinationbdd = destinationbdd.replace("'",'')
    destinationbdd = destinationbdd.replace(" ","")
    destinationbdd = str(destinationbdd)
    print("source:", destinationbdd, "\n\n ================================= \n ")

    table.insert({ 'status' : 'supression' , 'virus': virus_name, 'source': source, 'destination' : destinationbdd})

    shutil.move(source, destinationbdd)
    print("[-][-] suppression de", virus_name, "de là quarantaine [-][-]")
    print("Emplacement \t ==>", destinationbdd)
    if os.path.isfile(source):
        print("[WARNING] echec de là suppression de là quarantaine [WARNING] ")
    else:

        fichier2 = open("c:\ProgramData\Microsoft\precius\Quarantine\quarantaine.log", "a")
        fichier2.write("\n[-][-] \t [")
        fichier2.write(datetime.datetime.now().ctime())
        fichier2.write("]\n=====> réaffectation de [ "+ virus_name + " ] au répertoire : [ "+ destinationbdd +" ]  [*] \n ")
        fichier2.close()   

##################################################################################################################
##################################################################################################################
######################################################## Move
    # Déplacer le fichier virus
def move(): 
    global virus_name, destination, position_virus, destinationf, positionf, positionOut
    virus_name = sys.argv[2]
    virus_name = virus_name.split("\\")[-1]
    print(': virus_name :', virus_name)
    position_virus = str(sys.argv[2])
    position_virus = position_virus.replace('\\','/')
    print("position_virus :", position_virus)
    destination = r'C:\ProgramData\Microsoft\precius\Quarantine\\'
    destinationf = destination + virus_name
    #positionf = os.getcwd() + "\\" + sys.argv[2]
    if os.path.isfile(destinationf):
        print("fichier du même type déjà en quarantaine\n")
    else :
        shutil.move(position_virus, destinationf)
        
# https://fr.acervolima.com/crypter-et-decrypter-des-fichiers-a-laide-de-python/

############################################## chiffrement 

def chiffrement(): 


    #Générer la clé et la sauvegarder
    fileName = 'C:/ProgramData/Microsoft/precius/key/filekey.key'
    if os.path.isfile(fileName):
        print("[++] Key : OK ! [++]")
    else :
        key = Fernet.generate_key() 
        with open('C:/ProgramData/Microsoft/precius/key/filekey.key', 'wb') as filekey: 
           filekey.write(key)
        print("[+] création de là clé [+]")

    # Crypter le fichier à l’aide de la clé générée
    with open('C:/ProgramData/Microsoft/precius/key/filekey.key', 'rb') as filekey: 
        key = filekey.read() 
      
    fernet = Fernet(key) 
     ## fichier a chiffrer 
    with open('C:/ProgramData/Microsoft/precius/Quarantine/'+ virus_name, 'rb') as file: 
        original = file.read() 
          
    encrypted = fernet.encrypt(original) 
      
    with open('C:\ProgramData\Microsoft\precius\Quarantine\\' + virus_name, 'wb') as encrypted_file: 
        encrypted_file.write(encrypted) 
        print("[+] fichier en quarantaine [+]")
        print("Quarantaine \t  ==> C:\ProgramData\Microsoft\precius\Quarantine")

    # crée fichier log 
    log  = 'c:\ProgramData\Microsoft\precius\Quarantine\quarantaine.log'
    if os.path.isfile(log):
        print("[++] logs : OK ! [++]")
    else :
         print("[++] création du fichier de logs [++]")
         os.system('echo       [==========]      log quarantaine    [==========] >> c:\ProgramData\Microsoft\precius\Quarantine\quarantaine.log')

    if os.path.isfile(destinationf):
            fichier = open("c:\ProgramData\Microsoft\precius\Quarantine\quarantaine.log", "a")
            fichier.write("\n[+][+] \t [")
            fichier.write(datetime.datetime.now().ctime())
            fichier.write("]\n=====> Ajout de  [ "+ virus_name + " ] en provenance de : [ "+ position_virus +" ]  [:] \n ")
            fichier.close()



#################################################### dechiffrement 

def dechiffrement():       

####  Décrypter le fichier crypté

    ##########################################################

    virus_name = sys.argv[2]
    virus_name = virus_name.split("\\")[-1]

    position_virus = str(sys.argv[2])
   # print(position_virus," : position virus ***********")
    destination = r'C:\ProgramData\Microsoft\precius\Quarantine\\'
    destinationf = destination + virus_name
    with open('C:/ProgramData/Microsoft/precius/key/filekey.key', 'rb') as filekey: 
        key = filekey.read() 
       # print("virus_name: ", virus_name)
    fernet = Fernet(key)
    with open('C:/ProgramData/Microsoft/precius/Quarantine/'+ virus_name, 'rb') as enc_file: 
        encrypted = enc_file.read() 
    decrypted = fernet.decrypt(encrypted)       
    with open('C:/ProgramData/Microsoft/precius/Quarantine/'+ virus_name, 'wb') as dec_file: 
        dec_file.write(decrypted)
    
     
               
###################################################### Manuel
def manuel():
    print('\n\tBienvenue dans le Module Quarantaine\n ')
    print('***** Manuel de commandes *****')
    print(' \n\n pour chiffrer :\t"chiffrement" ou "-chiffrement" suivie du nom du chemin complet du fichier à chiffrer')
    print('pour dechiffrer : \t"dechiffrement" ou "-dechiffrement" suivie du nom du fichier à chiffrer\n')
    print('exemple : \t python quarantaine.py chiffrement C:\\Users\\bob\\Desktop\\image.jpg')
    print('\t \t python quarantaine.py -dechiffrement image.jpg \n')

#################################################### Commande

if len(sys.argv) > 1:
 
    if sys.argv[1] == 'chiffrement' or sys.argv[1] == '-chiffrement' :
        repertoire()
        move() 
        chiffrement()
        in_bdd()
        

    elif  sys.argv[1] == 'dechiffrement'or sys.argv[1] == '-dechiffrement':
        dechiffrement()
        out_bdd()   
   
    elif sys.argv[1] == '-help' or sys.argv[1] == '-h'or sys.argv[1] == '--h' or sys.argv[1] == '--help':
        manuel()

    else:
       print("Erreur syntaxe. \nPour plus d'information utilisez la commande : --h")


else:
    print("Entrez un argument s'il vous plaît !")
