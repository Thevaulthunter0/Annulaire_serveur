using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel.Design;

namespace Annulaire_Serveur
{
    internal class Paquet
    {
        public int intInfo { get; set; }
        public bool boolInfo { get; set; }
        public int idClient { get; set; }
        public TypePaquet type { get; set; }
        public List<List<String>> donnee {  get; set; }

        public Paquet()
        {
            // Default constructor (parameterless)
        }

        //Creer un paquet a partir de byte
        //Utiliser quand recoit.
        public Paquet(byte[] bytePaquet)
        {
            string jsonStr = Encoding.UTF8.GetString(bytePaquet);
            jsonStr = jsonStr.Replace("\0", "");
            var paquet = JsonSerializer.Deserialize<Paquet>(jsonStr);
            if (paquet != null)
            {
                this.intInfo = paquet.intInfo;
                this.boolInfo = paquet.boolInfo;
                this.idClient = paquet.idClient;
                this.type = paquet.type;
                this.donnee = paquet.donnee;
            }
        }

        //Creer un paquet a partir de nouvelle propriete
        //Puis utiliser la fonciton bytes pour la transformer en byte.
        public Paquet(int intInfo,  int idClient, TypePaquet type, List<List<String>> donnee, bool boolInfo)
        {
            this.intInfo = intInfo;
            this.idClient = idClient;
            this.type = type;
            this.donnee = donnee;   
            this.boolInfo = boolInfo;
        }

        //Transformer this. en forme json byte
        public byte[] bytes()
        {
            string jsonStr = JsonSerializer.Serialize(this);
            return Encoding.UTF8.GetBytes(jsonStr);
        }
    }

    public enum TypePaquet
    {
        Connexion,
        Demande,
        Deconnexion
    }
}
/* A. Paquet de type Connexion
 *      - Le client envoie TypePaquet.Connexion au serveur pour acceder admin
 *          Met dans le premier element de la premiere list dans List<String> le mot de passe a verifier
 *      - Le serveur repond TypePaquet.Connexion en cas d'echec
 *          Met false dans boolInfo.
 *      - Le serveur repond TypePaquet.Connexion en cas de succes
 *          Met true dans boolInfo
 * 
 * B. Paquet de type Demande
 *      - Le client envoie TypePaquet.Demande avec la requete voulu(Voir devoir)
 *           Met dans intInfo le numero de la requete (1-8)
 *           Met dans la premiere list dans List<String> les filtres important pour chaque requete.
 *            ex: Requete 1, necessite la categorie(Etudiant ou Professeur)
 *                Requete 2, necessite le domaine d'activite(Logiciel, web mobile....)
 *                Requete 3, necessite un Nom
 * 
 * ...
 * ...
 * ...
 * 
 * C. Paquet de type Deconnexion
 *      - Le client envoie TypePaquet.Deconnexion c'est tout.
 */