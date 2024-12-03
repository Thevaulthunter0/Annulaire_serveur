using System.Net;
using System.Net.Sockets;

namespace Annulaire_Serveur
{
    internal class Serveur
    {
        static int port = 3434;                   //Notre port dedier a l'ecoute de nouvelle connexion
        static Socket? conSocket;                 //Notre socket qui va recevoir les demandes de connexion des clients
        static List<DonneeClient>? clients;       //Notre list contenant les informations de nos clients
        static bool flag = true;

        //Point de depart de notre serveur
        static void Main(string[] args)
        {
            Console.WriteLine("Configuration du serveur...");
            //Utilise InterNetwork pour l'utilisation d'IPV4 
            conSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clients = new List<DonneeClient>();

            //Trouver notre adresse IP ou bien utiliser loopback + port
            //IPAddress? ip = Dns.GetHostAddresses(Dns.GetHostName())
            //    .FirstOrDefault(i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            //
            //if (ip == null)
            //{
            //    ip = IPAddress.Parse("127.0.0.0");
            //}

            IPAddress ip = IPAddress.Parse("127.0.0.1");

            //Assigner nos informations IPV4 a conSocket
            IPEndPoint hoteEndPoint = new IPEndPoint(ip, port);
            conSocket.Bind(hoteEndPoint);

            //Commencer un thread qui ne fait qu'ecouter sur conSocket pour de nouvelle connexion.
            Thread conThread = new Thread(ConThread);
            conThread.Start();

            //Thread d'ecoute du clavier pour la lettre q pour fermer serveur
            Thread fermerThread = new Thread(FermerThreadPrinc);
            fermerThread.Start();

            Console.WriteLine("Configuration du serveur terminé. 'Q' pour fermer le serveur.");

            fermerThread.Join();
            conThread.Join();

            Console.WriteLine("Serveur fermé.");
        }

        //Methode du thread qui ecoute sur conSocket
        private static void ConThread() 
        {
            while(flag)
            {
                try
                {
                    conSocket.Listen();
                    Socket newSocket = conSocket.Accept();
                    clients.Add(new DonneeClient(newSocket));
                    Console.WriteLine($"Nouveau client id : {clients.Last().id}");
                }
                catch (SocketException e)
                {
                    //Empecher l'application de planter quand utilisateur quitte avec 'Q'
                    if(flag == false && e.SocketErrorCode == SocketError.Interrupted)
                    {
                        break;
                    }
                }
                
            }
            
            //Attendre que chaque thread client se ferme(join) avant de fermer le conThread
            //Signifie que le serveur attend que les clients deja connecte quitte avant de completement fermer le serveur
            //Pendant ce temps aucune autre connexion peut se faire
            if(clients != null)
            {
                foreach (DonneeClient client in clients)
                {
                    client.threadClient.Join();
                }
            }
        }

        private static void FermerThreadPrinc()
        {
            while(flag)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                {
                    flag = false;
                    conSocket.Close();
                }
                Thread.Sleep(100);
            }
        }
    }
}
