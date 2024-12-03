using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Annulaire_Serveur
{
    internal class DonneeClient
    {
        public static int Compteur = 0;
        public int id { get; private set; }
        public bool admin { get; set; }
        public Socket socketClient { get; private set; }
        public Thread threadClient {get; private set; }
        public bool flag { get; set; }

        public DonneeClient(Socket nSocketClient) 
        {
            this.id = Compteur;
            Compteur++;
            this.socketClient = nSocketClient;
            this.threadClient = new Thread(ClientThread);
            this.threadClient.Start(this.socketClient);   //Join dans Program.cs
        }

        public void ClientThread(object socket)
        {
            flag = true;
            byte[] buffer;
            int bufferSize;
            while (flag)
            {
                buffer = new byte[this.socketClient.ReceiveBufferSize];
                try
                {
                    bufferSize = this.socketClient.Receive(buffer);
                } catch (System.Net.Sockets.SocketException e)
                {
                    socketClient.Close();
                    break;
                }
       
                if (bufferSize > 0)
                {
                    Paquet paquet = new Paquet(buffer);
                    if (paquet != null)
                    {
                        PaquetController.DataController(paquet, this);
                    }
                }
            }
            Console.WriteLine("Thread client fermé.");
        }
    }
}
