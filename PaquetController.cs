﻿using System.Configuration;

namespace Annulaire_Serveur
{
    internal class PaquetController
    {
        public static async void DataController(Paquet paquet, DonneeClient client)
        {
            Paquet nPaquet;
            //Si paquet est de type Connexion(Le client veut se connecter en tant qu'admin)
            if (paquet.type == TypePaquet.Connexion)
            {
                string pw = paquet.donnee[0][0];
                bool succes = await AppDbContext.Instance.VerifyAdminCredential(pw);
               
                if(succes == true)
                {
                    client.admin = true;
                    nPaquet = new Paquet(0, client.id, TypePaquet.Connexion, new List<List<string>>(), true);
                }
                else
                {
                    List<List<String>> Erreur = new List<List<String>>();
                    Erreur.Add(new List<String> { "Mauvais mot de passe." });
                    nPaquet = new Paquet(0, client.id, TypePaquet.Connexion, Erreur, false);
                }
                await client.socketClient.SendAsync(nPaquet.bytes());
            }
            //Si paquet est de type Demande(Le client veut effectué une opération)
            if (paquet.type == TypePaquet.Demande)
            {
                switch(paquet.intInfo)
                {
                    //List membre(caregorie)
                    case 1:
                        if(paquet.donnee[0].Count() == 1)
                        {
                            string Catégorie = paquet.donnee[0][0];
                            List<Membre> membres = await AppDbContext.Instance.GetMembreCategorie(Catégorie);
                            List<List<string>> strings = MembreToListString(membres);
                            nPaquet = new Paquet(1, client.id, TypePaquet.Demande, strings, true);
                        }
                        else
                        {
                            List<List<string>> Erreur = new List<List<string>>();
                            Erreur.Add(new List<string> { "Veuillez fournir une catégorie." });
                            nPaquet = new Paquet(1, client.id, TypePaquet.Demande, Erreur, false);
                        }
                        await client.socketClient.SendAsync(nPaquet.bytes());
                        break;
                    //Lise prof(domaine)
                    case 2:
                        if(paquet.donnee[0].Count() == 1)
                        {
                            string Domaine = paquet.donnee[0][0];
                            List<Membre> membres = await AppDbContext.Instance.GetProfDomaine(Domaine);
                            List<List<String>> strings = MembreToListString(membres);
                            nPaquet = new Paquet(2, client.id, TypePaquet.Demande, strings, true);
                        }
                        else
                        {
                            List<List<String>> Erreur = new List<List<String>>();
                            Erreur.Add(new List<String> { "Veuillez fournir un domaine." });
                            nPaquet = new Paquet(2, client.id, TypePaquet.Demande, Erreur, false);
                        }
                        await client.socketClient.SendAsync(nPaquet.bytes());
                        break;
                    //Rechercher un membre(nom/prenom)
                    case 3:
                        if (paquet.donnee[0].Count() == 1)
                        {
                            string Nom = paquet.donnee[0][0];
                            List<Membre> membre = await AppDbContext.Instance.GetMembre(Nom);
                            List<List<String>> strings = MembreToListString(membre);
                            nPaquet = new Paquet(3, client.id, TypePaquet.Demande, strings, true);
                        }
                        else
                        {
                            List<List<String>> Erreur = new List<List<String>>();
                            Erreur.Add(new List<String> { "Veuillez fournir un nom/prenom." });
                            nPaquet = new Paquet(3, client.id, TypePaquet.Demande, Erreur, false);
                        }
                        await client.socketClient.SendAsync(nPaquet.bytes());
                        break;
                    //Ajouter un membre(info pour membre)
                    case 4:
                        if(client.admin == true)
                        {
                            var data = paquet.donnee[0];
                            if (paquet.donnee[0].Count() == 8)
                            {
                                string nom = data[0];
                                string prenom = data[1];
                                string categorie = data[2];
                                string matricule = data[3];
                                string email = data[4];
                                string telephone = data[5];
                                bool listeRouge;
                                if (data[6] == "true" || data[6] == "True") { listeRouge = true; }
                                else { listeRouge = false; }
                                string domaine = data[7];
                                AppDbContext.Instance.AddMembre(nom, prenom, categorie, matricule, email, telephone, listeRouge, domaine);
                                nPaquet = new Paquet(4, client.id, TypePaquet.Demande, new List<List<String>>(), true);
                            }
                            else 
                            {
                                List<List<String>> Erreur = new List<List<String>>();
                                Erreur.Add(new List<String> { "Information manquante." });
                                nPaquet = new Paquet(4, client.id, TypePaquet.Demande, Erreur, false); 
                            }
                        }
                        else
                        {
                            List<List<String>> Erreur = new List<List<String>>();
                            Erreur.Add(new List<String> { "N'est pas administrateur" });
                            nPaquet = new Paquet(4, client.id, TypePaquet.Demande, Erreur, false);
                        }
                        await client.socketClient.SendAsync(nPaquet.bytes());
                        break;
                    //Supprimer un membre(Num)
                    case 5:
                        if(client.admin == true)
                        {
                            if (paquet.donnee[0].Count() == 1)
                            {
                                int num = int.Parse(paquet.donnee[0][0]);
                                //Verifier si num est valide
                                if (await AppDbContext.Instance.VerifyIsNumValid(num))
                                {
                                    AppDbContext.Instance.DeleteMember(num);
                                    nPaquet = new Paquet(5, client.id, TypePaquet.Demande, new List<List<String>>(), true);
                                }
                                else 
                                {
                                    List<List<String>> Erreur = new List<List<String>>();
                                    Erreur.Add(new List<String> { "Veuillez fournir un numero valide." });
                                    nPaquet = new Paquet(5, client.id, TypePaquet.Demande, Erreur, false);
                                }
                            }
                            else 
                            { 
                                List<List<String>> Erreur = new List<List<String>>();
                                Erreur.Add(new List<String> { "Veuillez fournir un numero." });
                                nPaquet = new Paquet(5, client.id, TypePaquet.Demande, Erreur, false);
                            }
                        }
                        else
                        {
                            List<List<String>> Erreur = new List<List<String>>();
                            Erreur.Add(new List<String> { "N'est pas administrateur" });
                            nPaquet = new Paquet(5, client.id, TypePaquet.Demande, Erreur, false);
                        }
                        await client.socketClient.SendAsync(nPaquet.bytes());
                        break;
                    //Modifier un membre
                    case 6:
                        if(client.admin == true)
                        {
                            var data = paquet.donnee[0];
                            if(await AppDbContext.Instance.VerifyIsNumValid(int.Parse(data[0])))
                            {
                                if (paquet.donnee[0].Count() == 9)
                                {
                                    int num = int.Parse(data[0]);
                                    string nom = data[1];
                                    string prenom = data[2];
                                    string categorie = data[3];
                                    string matricule = data[4];
                                    string email = data[5];
                                    string telephone = data[6];
                                    bool listeRouge;
                                    if (data[7] == "true" || data[7] == "True") { listeRouge = true; }
                                    else { listeRouge = false; }
                                    string domaine = data[8];
                                    AppDbContext.Instance.ModifyMember(num, nom, prenom, categorie, matricule, email, telephone, listeRouge, domaine);
                                    nPaquet = new Paquet(6, client.id, TypePaquet.Demande, new List<List<String>>(), true);
                                }
                                else
                                {
                                    List<List<String>> Erreur = new List<List<String>>();
                                    Erreur.Add(new List<String> { "Information manquante." });
                                    nPaquet = new Paquet(6, client.id, TypePaquet.Demande, Erreur, false);
                                }
                            }
                            else
                            {
                                List<List<String>> Erreur = new List<List<String>>();
                                Erreur.Add(new List<String> { "Veuillez fournir un numero valide." });
                                nPaquet = new Paquet(6, client.id, TypePaquet.Demande, Erreur, false);
                            }
                        }
                        else
                        {
                            List<List<String>> Erreur = new List<List<String>>();
                            Erreur.Add(new List<String> { "N'est pas administrateur" });
                            nPaquet = new Paquet(6, client.id, TypePaquet.Demande, Erreur, false);
                        }
                        await client.socketClient.SendAsync(nPaquet.bytes());
                        break;
                    //Mettre membre sur la liste rouge(Num)
                    case 7:
                        if(client.admin == true)
                        {
                            if (paquet.donnee[0].Count() == 1)
                            {
                                int num = int.Parse(paquet.donnee[0][0]);   
                                if(await AppDbContext.Instance.VerifyIsNumValid(num))
                                {
                                    AppDbContext.Instance.SetRougeMember(num);
                                    nPaquet = new Paquet(7, client.id, TypePaquet.Demande, new List<List<String>>(), true);
                                }
                                else
                                {
                                    List<List<String>> Erreur = new List<List<String>>();
                                    Erreur.Add(new List<String> { "Veuillez fournir un numero valide." });
                                    nPaquet = new Paquet(7, client.id, TypePaquet.Demande, Erreur, false);
                                }
                            }
                            else 
                            {
                                List<List<String>> Erreur = new List<List<String>>();
                                Erreur.Add(new List<String> { "Veuillez fournir un numero." });
                                nPaquet = new Paquet(7, client.id, TypePaquet.Demande, Erreur, false); 
                            }
                        }
                        else
                        {
                            List<List<String>> Erreur = new List<List<String>>();
                            Erreur.Add(new List<String> { "N'est pas administrateur" });
                            nPaquet = new Paquet(7, client.id, TypePaquet.Demande, Erreur, false);
                        }
                        await client.socketClient.SendAsync(nPaquet.bytes());
                        break;
                    //Enlever membre de la liste rouge(Num)
                    case 8:
                        if(client.admin == true)
                        {
                            if (paquet.donnee[0].Count() == 1)
                            {
                                int num = int.Parse(paquet.donnee[0][0]);
                                if(await AppDbContext.Instance.VerifyIsNumValid(num))
                                {
                                    AppDbContext.Instance.RemoveRougeMember(num);
                                    nPaquet = new Paquet(8, client.id, TypePaquet.Demande, new List<List<String>>(), true);
                                }
                                else
                                {
                                    List<List<String>> Erreur = new List<List<String>>();
                                    Erreur.Add(new List<String> { "Veuillez fournir un numero valide." });
                                    nPaquet = new Paquet(8, client.id, TypePaquet.Demande, Erreur, false);
                                }
                            }
                            else 
                            {
                                List<List<String>> Erreur = new List<List<String>>();
                                Erreur.Add(new List<String> { "Veuillez fournir un numero." });
                                nPaquet = new Paquet(8, client.id, TypePaquet.Demande, Erreur, false); 
                            }
                        }
                        else
                        {
                            List<List<String>> Erreur = new List<List<String>>();
                            Erreur.Add(new List<String> { "N'est pas administrateur" });
                            nPaquet = new Paquet(8, client.id, TypePaquet.Demande, Erreur, false);
                        }
                        await client.socketClient.SendAsync(nPaquet.bytes());
                        break;
                    default:
                        List<List<String>> ERR = new List<List<String>>();
                        ERR.Add(new List<String> { "ERREUR" });
                        nPaquet = new Paquet(9, client.id, TypePaquet.Demande, ERR, false);
                        break;
                }
            }
            //Si paquet est de type Deconnexion(Le client est déconnecté et a quitté l'application)
            if (paquet.type == TypePaquet.Deconnexion)
            {
                client.flag = false;
            }
        }

        private static List<List<String>> MembreToListString(List<Membre> membres)
        {
            List<List<String>> strings = new List<List<String>>();
            foreach(Membre membre in membres)
            {
                List<String> membreProriete = new List<String>();
                var nPropriete = typeof(Membre).GetProperties();
                foreach(var proprie in nPropriete)
                {
                    var value = proprie.GetValue(membre);
                    if (value == null)
                    {
                        membreProriete.Add("Fait partie de la Liste Rouge");
                        break;
                    }
                    membreProriete.Add(value.ToString());
                }
                strings.Add(membreProriete);
            }
            return strings;
        }
    }
}
