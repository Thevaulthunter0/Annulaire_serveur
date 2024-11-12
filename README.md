# Communication Client-Serveur #
Le client et le serveur doivent utiliser la classe Paquet.cs pour communiquer entre eux.

Le paquet est un objet compos� de :
- intInfo
- boolInfo
- TypePaquet
	-Connexion : Demande de connexion d'un administrateur, ne pas utiliser pour faire une demande de connexion
	-Demande : Les 8 requ�tes possibles que le serveur doit effectu� selon les consignes
	-Deconnexion : Client indique qu'il se d�connecte pour que serveur libere thread
- donnee : List<List<String>>

*Certain param�tre ne seront peut-etre pas toujours utilis�...
**L'utilisation de List<List<String>> n'est probablement pas obtimal 
mais pour partager une liste de personne dans l'annulaire c'est le plus facile.

# Transformer paquet #
Pour envoyer sur socket doit transformer l'objet paquet en byte[].
Pour ce faire utiliser la fonction .bytes()
ex : Paquet p = new Paquet(1, 0, TypePaquet.Demande, new List<List<String>>(), false)
	 byte[] bytes = p.bytes()

Lorsque des donnees sont recu sur le socket nous devont transformer les bytes en objet Paquet.
Pour ce faire utiliser le constructeur qui accepte en parametre des byte[]
ex : Paquet p = new Paquet(buffer[])

## Utilisation normal des paquets ##
Apres etre connecte au serveur un client veut faire la demande #1 avec parametre "Etudiant"
1. Client construit le paquet et le parametre:
List<List<String>> parametre = new List<List<String>>();
parametre.Add(new List<String>)
parametre[0] = "Etudiant";
Paquet p = Paquet(1, 0, TypePaquet.Demande, parametre, false)
2. Transforme le paquet en bytes
var bytes = p.bytes()
3. Envoie au serveur

Maintenant le client recoit un paquet du serveur.
1. Client construit le paquet
Paquet p = Paquet(byteRecuServeur);
2. Client peut acceder au donnee
p.donnee

## Comment? ##
1. On Serialize() un objet Paquet dans un string (JsonSerializer.Serialize()).
2. Encode le string en Byte[] (Encoding.UTF8.GetBytes())
3. Recoit des bytes qu'on decode en string (Encoding.UTF8.GetString())
4. On Desezialize le string dans un objet json(JsonSerializer.Deserialize<Paquet>())
5. Construit le paquet avec l'objet json

# Construction des paquets #
Paquet p = Paquet(intInfo, idClient, TypePaquet, donnee, boolInfo)

- Demande de connexion administrateur
Donne le mot de passe
Client envoie :
List<List<String>> password = new List<List<String>>();
password.Add(new List<String>)
password[0] = "1234";
Paquet(0,0,TypePaquet.Connexion, password, false)

- (1) Lister les membres d�une cat�gorie donn�e
Cat�gorie peut etre : 
	- "Etudiant"
	- "Professeur"
Client envoie :
List<List<String>> Cat�gorie = new List<List<String>>();
Cat�gorie.Add(new List<String>)
Cat�gorie[0] = "Etudiant"
Paquet(1,0,TypePaquet.Demande, Cat�gorie, false)

- (2) Lister les professeurs dans un domaine d�activit� donn�
Domaine peut etre :
	-A DETERMINER
List<List<String>> Domaine = new List<List<String>>();
Domaine.Add(new List<String>)
Domaine[0] = "???"
Paquet(2,0,TypePaquet.Demande, Domaine, false)

- (3) Rechercher un membre
Recherche � partir d'un nom OU prenom
List<List<String>> Nom = new List<List<String>>();
Nom.Add(new List<String>)
Nom[0] = "Trystan"
Paquet p = Paquet(3,0,TypePaquet.Demande, Nom, false)

- (4) Ajouter un membre
Doit envoyer les parametres d'un membre
List<List<String>> Parametre = new List<List<String>>();
Parametre.Add(new List<String>)
Parametre[0] = "@Nom"
Parametre[1] = "@Prenom"
Parametre[2] = "@Cat�gorie"
Parametre[3] = "@Matricule"
Parametre[4] = "@Email"
Parametre[5] = "@Telephone"
Parametre[6] = "@ListRouge"
Parametre[7] = "@Domaine"
Paquet p = Paquet(4,0,TypePaquet.Demande, Parametre, false)

- (5) Supprimer un membre
Doit envoyer le num du membre
List<List<String>> Num = new List<List<String>>();
Num.Add(new List<String>)
Num[0] = "52"
- Paquet p = Paquet(5,0,TypePaquet.Demande, Num, false)

- (6) Modifier (mettre � jour) un membre
PAS ENCORE SUR DE COMMENT LE FAIRE

- (7) Mettre un membre sur la liste rouge
Doit envoyer le num du membre
List<List<String>> Num = new List<List<String>>();
Num.Add(new List<String>)
Num[0] = "52"
Paquet p = Paquet(7,0,TypePaquet.Demande, Num, false)

- (8) Enlever un membre de la liste rouge
- Doit envoyer le num du membre
List<List<String>> Num = new List<List<String>>();
Num.Add(new List<String>)
Num[0] = "52"
Paquet p = Paquet(8,0,TypePaquet.Demande, Num, false)

## Reponse du serveur ##
Le serveur devrait repondre a chaque paquet sauf le paquet de deconnexion

Pour connexion : Le serveur va renvoye le meme paquet mais .boolInfo = true ou .boolInfo = false selon succes ou echec. 

Pour demande 1 a 3 : Le serveur renvoie dans .donnee la liste ou le membre avec .boolInfo = true.

Pour demande 4 a 8 : Le serveur renvoie le meme paquet mais .boolInfo = true ou .boolInfo = false selon succes ou echec. 
