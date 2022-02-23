import os
import shutil
import crypto
import sys
import hashlib,sys,string
from subprocess import getoutput
from cryptography.fernet import Fernet
import datetime

# C:\ProgramData\Microsoft\precius\ 
# requierement : pip install python-dev-tools
#  python -m pip install cryptography

virus = 1
if virus == 1: 
########################################## Repertoire
 def repertoire():    
# vérifier l’existence du répertoire et le créer s’il n’existe pas
    try:
       os.mkdir('C:\ProgramData\Microsoft\precius')
       os.mkdir('C:\ProgramData\Microsoft\precius\Quarantine')
    except:
        print('\n\n***** Le répertoire Quarantaine existe déjà ****')
    print(' [+] repertoire quarantaine : OK !\n\n')

    try:
        os.mkdir('C:\ProgramData\Microsoft\precius\key')
    except:
            print('***** Le répertoire Key existe déjà ****')
    print(' [+] repertoire Key : OK !\n\n\n')

######################################################## Move
    # Déplacer le fichier virus
def move(): 
    global virus_name, destination, position_virus, positionf, destinationf
    virus_name = sys.argv[2]
    #virus_name = input("entrez le nom de votre fichier : ")
    position_virus=os.getcwd() + "\\" 
    destination = 'C:\ProgramData\Microsoft\precius\Quarantine\\'
    destinationf = destination + virus_name
    positionf = position_virus + virus_name
    if os.path.isfile(destinationf):
        print("fichier du même type déjà en quarantaine\n")
    else :
        shutil.move(positionf, destinationf)

############################################## chiffrement 

def chiffrement(): 
######################################
# https://fr.acervolima.com/crypter-et-decrypter-des-fichiers-a-laide-de-python/

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
         os.system('echo                   [==========]      log quarantaine    [==========] >> c:\ProgramData\Microsoft\precius\Quarantine\quarantaine.log')

    if os.path.isfile(destinationf):
            fichier = open("c:\ProgramData\Microsoft\precius\Quarantine\quarantaine.log", "a")
            fichier.write("\n[+][+] \t [")
            fichier.write(datetime.datetime.now().ctime())
            fichier.write("]\n=====> Ajout de  [ "+ virus_name + " ] en provenance de : [ "+ positionf +" ]  [:] \n ")
            fichier.close()



#################################################### dechiffrement 

def dechiffrement():       

####  Décrypter le fichier crypté
    virus_name = sys.argv[2]
    position_virus=os.getcwd() + "\\" 
    destination = 'C:\ProgramData\Microsoft\precius\Quarantine\\'
    destinationf = destination + virus_name
    positionf = position_virus + virus_name
    
    with open('C:/ProgramData/Microsoft/precius/key/filekey.key', 'rb') as filekey: 
        key = filekey.read() 
        print("virus_name: ", virus_name)
    fernet = Fernet(key)
    with open('C:/ProgramData/Microsoft/precius/Quarantine/'+ virus_name, 'rb') as enc_file: 
        encrypted = enc_file.read() 
    decrypted = fernet.decrypt(encrypted)       
    with open('C:/ProgramData/Microsoft/precius/Quarantine/'+ virus_name, 'wb') as dec_file: 
        dec_file.write(decrypted)
    
    shutil.move(destinationf, positionf)
    print("[-][-] suppression de", virus_name, "de là quarantaine [-][-]")
    print("Emplacement \t ==>", positionf)
    if os.path.isfile(destinationf):
        print("[WARNING] echec de là suppression de là quarantaine [WARNING] ")
    else:
        fichier2 = open("c:\ProgramData\Microsoft\precius\Quarantine\quarantaine.log", "a")
        fichier2.write("\n[-][-] \t [")
        fichier2.write(datetime.datetime.now().ctime())
        fichier2.write("]\n=====> réaffectation de [ "+ virus_name + " ] au répertoire : [ "+ positionf +" ]  [*] \n ")
        fichier2.close()    
               
###################################################### Manuel
def manuel():
    print('\n\tBienvenue dans le Module Quarantaine\n ')
    print('***** Manuel de commandes *****')
    print(' \n\n pour chiffrer :\t"chiffrement" ou "-chiffrement" suivie du nom du fichier à chiffrer')
    print('pour dechiffrer : \t"dechiffrement" ou "-dechiffrement" suivie du nom du fichier à chiffrer\n')
    print('exemple : \t python quarantaine.py chiffrement image.jpg')
    print('\t \t python quarantaine.py -dechiffrement image.jpg \n')

#################################################### Commande

if sys.argv[1] == 'chiffrement' or sys.argv[1] == '-chiffrement' :
    repertoire()
    move() 
    chiffrement()
elif  sys.argv[1] == 'dechiffrement'or sys.argv[1] == '-dechiffrement':
    dechiffrement()

elif sys.argv[1] == '-help' or sys.argv[1] == '-h'or sys.argv[1] == '--h' or sys.argv[1] == '--help':
    manuel()
