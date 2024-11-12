using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Formats.Asn1;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.CompilerServices;

namespace Annulaire_Serveur.DB
{
    internal class AppDbContext : IDisposable
    {
        private static AppDbContext instance;
        private static readonly string conString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Users\\Mathi\\source\\repos\\Annulaire_Serveur\\DB\\Annulaire.accdb;Persist Security Info=True";
        private OleDbConnection connection;
        public AppDbContext() 
        {
            connection = new OleDbConnection(conString);
        }

        //1.Lister membre catégorie donnée
        public async Task<List<Membre>> GetMembreCategorie(string categorie)
        {
            string query = @"SELECT Num, Nom, Prenom, Catégorie, Matricule, Email, Téléphone, [ListeRouge], Domaine FROM Annulaire WHERE Catégorie = ?";

            return await FiltreListeRouge(query, "Catégorie", categorie);
        }
        //2.Lister les prof dans un domaine d'activite
        public async Task<List<Membre>> GetProfDomaine(string domaine)
        {

            string query = @"SELECT Num, Nom, Prenom, Catégorie, Matricule, Email, Téléphone, [ListeRouge], Domaine FROM Annulaire WHERE Categorie = 'professeur' AND Domaine = ?";

            return await FiltreListeRouge(query, "@Domaine", domaine);
        }

        //3.Rechercher un membre
        public async Task<List<Membre>> GetMembre(string nom)
        {
            string query = @"SELECT Num, Nom, Prenom, Catégorie, Matricule, Email, Téléphone, [ListeRouge], Domaine FROM Annulaire WHERE Nom LIKE '%' + ? + '%' OR Prenom LIKE '%' + ? + '%'";

            return await FiltreListeRouge(query, "@Nom", nom);
        }

        //Applique le filtre de la liste rouge pour afficher les bonnes choses.
        public async Task<List<Membre>> FiltreListeRouge(string query, string parametre, string valeur)
        {
            List<Membre> membres = new List<Membre>();

            await using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("?", valeur);
                await connection.OpenAsync();
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        bool listeRouge = reader.GetBoolean(reader.GetOrdinal("ListeRouge"));
                        membres.Add(new Membre
                        {
                            Num = reader.GetInt32(reader.GetOrdinal("Num")),
                            Nom = reader.GetString(reader.GetOrdinal("Nom")),
                            Prenom = reader.GetString(reader.GetOrdinal("Prenom")),
                            Catégorie = listeRouge ? null : reader.GetString(reader.GetOrdinal("Catégorie")),
                            Matricule = listeRouge ? null : reader.GetString(reader.GetOrdinal("Matricule")),
                            Email = listeRouge ? null : reader.GetString(reader.GetOrdinal("Email")),
                            Téléphone = listeRouge ? null : reader.GetString(reader.GetOrdinal("Téléphone")),
                            ListeRouge = listeRouge,
                            Domaine = listeRouge ? null : reader.GetString(reader.GetOrdinal("Domaine"))
                        });
                    }
                }
                connection.Close();
            }
            return membres;
        }

        //4.Ajouter un membre
        public async void AddMembre(string nom, string prenom, string categorie, string matricule, string email, string telephone, bool listeRouge, string domaine)
        {

        }

        //5.Supprimer un membre
        public async void DeleteMember(int num)
        {

        }

        //6.Modififer un membre
        public async void ModifyMember(int num, string nom, string prenom, string categorie, string matricule, string email, string telephone, bool listeRouge, string domaine)
        { 

        }

        //7.Mettre un membre sur la liste rouge
        public async void AddRougeMember(int num)
        {

        }

        //8.Enlever un membre de la liste rouge
        public async void RemoveRougeMember(int num)
        {

        }

        //Verifier la si utilisateur est admin
        public async Task<bool> VerifyAdminCredential(string password)
        {

            return false;
        }

        // Implemente IDisposable pour automatiquement jeter la connection quand demande fini
        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        // Public static property to get the single instance
        public static AppDbContext Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AppDbContext();
                }
                return instance;
            }
        }
    }

    public class Membre
    {
        public int Num { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Catégorie { get; set; }
        public string Matricule { get; set; }
        public string Email { get; set; }
        public string Téléphone { get; set; }
        public bool ListeRouge { get; set; }
        public string Domaine { get; set; }
    }
}
