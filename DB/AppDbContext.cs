using System.Data.OleDb;
using System.Diagnostics.PerformanceData;

namespace Annulaire_Serveur.DB
{
    internal class AppDbContext : IDisposable
    {
        private static AppDbContext instance;
        private OleDbConnection connection;
        public AppDbContext() 
        {
            string dbPath = @"../../../DB/Annulaire.accdb";
            string conString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=True";
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

            string query = @"SELECT Num, Nom, Prenom, Catégorie, Matricule, Email, Téléphone, [ListeRouge], Domaine FROM Annulaire WHERE Catégorie = 'Professeur' AND Domaine = ?";

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
            string query = @"INSERT INTO Annulaire (Nom, Prenom, Catégorie, Matricule, Email, Téléphone, [ListeRouge], Domaine) 
                         VALUES (@Nom, @Prenom, @Catégorie, @Matricule, @Email, @Téléphone, @ListeRouge, @Domaine)";

            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Nom", nom);
                command.Parameters.AddWithValue("@Prenom", prenom);
                command.Parameters.AddWithValue("@Catégorie", categorie);
                if(categorie == "Etudiant")
                {
                    command.Parameters.AddWithValue("@Matricule", matricule);
                }
                else
                {
                    command.Parameters.AddWithValue("@Matricule", "Null");
                }
                command.Parameters.AddWithValue("@Email", email);
                if(categorie == "Professeur")
                {
                    command.Parameters.AddWithValue("@Téléphone", telephone);
                }
                else
                {
                    command.Parameters.AddWithValue("@Téléphone", "Null");
                }
                command.Parameters.AddWithValue("@ListeRouge", listeRouge);
                command.Parameters.AddWithValue("@Domaine", domaine);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }

        //5.Supprimer un membre
        public async void DeleteMember(int num)
        {
            string query = "DELETE FROM Annulaire WHERE Num = @Num";

            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Num", num);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }

        //6.Modififer un membre
        public async void ModifyMember(int num, string nom, string prenom, string categorie, string matricule, string email, string telephone, bool listeRouge, string domaine)
        {
            string query = @"UPDATE Annulaire SET Nom = @Nom, Prenom = @Prenom, Catégorie = @Catégorie, 
                         Matricule = @Matricule, Email = @Email, Téléphone = @Téléphone, [ListeRouge] = @ListeRouge, 
                         Domaine = @Domaine WHERE Num = @Num";

            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Nom", nom);
                command.Parameters.AddWithValue("@Prenom", prenom);
                command.Parameters.AddWithValue("@Catégorie", categorie);
                if(categorie == "Etudiant")
                {
                    command.Parameters.AddWithValue("@Matricule", matricule);
                }
                else
                {
                    command.Parameters.AddWithValue("@Matricule", "Null");
                }
                command.Parameters.AddWithValue("@Email", email);
                if(categorie == "Professeur")
                {
                    command.Parameters.AddWithValue("@Téléphone", telephone);
                }
                else
                {
                    command.Parameters.AddWithValue("@Téléphone", "Null");
                }
                command.Parameters.AddWithValue("@ListeRouge", listeRouge);
                command.Parameters.AddWithValue("@Domaine", domaine);
                command.Parameters.AddWithValue("@Num", num);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }

        //7.Mettre un membre sur la liste rouge
        public async void SetRougeMember(int num)
        {
            string query = "UPDATE Annulaire SET [ListeRouge] = @ListeRouge WHERE Num = @Num";

            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ListeRouge", true);
                command.Parameters.AddWithValue("@Num", num);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }

        //8.Enlever un membre de la liste rouge
        public async void RemoveRougeMember(int num)
        {
            string query = "UPDATE Annulaire SET [ListeRouge] = @ListeRouge WHERE Num = @Num";

            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ListeRouge", false);
                command.Parameters.AddWithValue("@Num", num);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }
        }

        //Verifier la si utilisateur est admin
        public async Task<bool> VerifyAdminCredential(string password)
        {
            string query = "SELECT [mot de passe] FROM Admin";
            using (var command = new OleDbCommand(query, connection))
            {
                await connection.OpenAsync();
                var motDePasseBD = await command.ExecuteScalarAsync() as string;
                connection.Close();

                // Comparer le mot de passe de la base de données avec le mot de passe fourni
                return motDePasseBD != null && motDePasseBD == password;
            }
        }

        //Vérifier que le num envoyer par le client existe dans la base de donnée
        public async Task<bool> VerifyIsNumValid(int num)
        {
            string query = "SELECT COUNT(1) FROM Annulaire WHERE Num = ?";
            using (var command = new OleDbCommand(query, connection))
            {
                command.Parameters.AddWithValue("?", num);
                await connection.OpenAsync();
                int count = (int)command.ExecuteScalar();
                connection.Close();
                if (count > 0) { return true; }
                else { return false; }
            }
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
