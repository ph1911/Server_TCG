using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace fctServer
{
    public partial class Server : Form
    {
        public static Server Instance;

        public const int port = 1911;
        public TcpListener server;
        public bool serverRunning;

        private List<ServerClient> connectedClients = new List<ServerClient>();
        private List<ServerClient> waitingClients = new List<ServerClient>();
        private List<Battle> currentBattles = new List<Battle>();

        public Server()
        {
            Instance = this;
            InitializeComponent();
            StartButton_Click(this, default(EventArgs));
        }

        //start server
        public void Init()
        {
            try {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                serverRunning = true;
                server.BeginAcceptTcpClient(AcceptTcpClient, server);
                OutputTB.Text = "server has been started on port " + port.ToString();
                firstMessage = false;
            }
            catch (Exception e) {
                OutputTB.Text = "socket error: " + e.Message;
            }
        }

        private void AcceptTcpClient(IAsyncResult ar)
        {
            if (!serverRunning)
                return;
            TcpListener listener = (TcpListener)ar.AsyncState;
            ServerClient client = new ServerClient(listener.EndAcceptTcpClient(ar));
            Task.Run(() => WaitForIncomingData(client));
            server.BeginAcceptTcpClient(AcceptTcpClient, server);
        }

        private void WaitForIncomingData(ServerClient client)
        {
            NetworkStream stream = client.tcp.GetStream();
            //who is the client?
            Send("SWHO", client);
            connectedClients.Add(client);
            Invoke((MethodInvoker)delegate {
                ClientListBox.Items.Add("Waiting for authentification...");
            });
            while (client.connected && serverRunning) {
                if (stream.DataAvailable) {
                    byte[] data = new byte[512];
                    stream.Read(data, 0, 1);
                    byte dataLength = data[0];
                    stream.Read(data, 0, dataLength);
                    OnIncomingData(client, Encoding.UTF8.GetString(data, 0, dataLength));
                }
                if (client.timeSinceLastMessage > 20) {
                    client.connected = false;
                }
            }
            Invoke((MethodInvoker)delegate {
                if (client.authenticated)
                    ClientListBox.Items.Remove(client.playerName);
                else
                    ClientListBox.Items.Remove("Waiting for authentification...");
            });

            Send("SDISC", client);
            if (!client.opponent.Equals(default(ServerClient))) {
                Send("OpLEAVE", client.opponent);
            }
            waitingClients.Remove(client);
            connectedClients.Remove(client);
            stream.Close();
            client.tcp.Close();
        }

        private bool firstMessage;
        //read data from client
        private void OnIncomingData(ServerClient client, string data)
        {
            client.timeSinceLastMessage = 0;
            string[] allData = data.Split('|');
            if (allData[0] == "PlFocus" || allData[0] == "PlStrike" || allData[0] == "PlBlock") {
                firstMessage = !firstMessage;
            }
            if (firstMessage) {
                Invoke((MethodInvoker)delegate {
                    SendListBox.Items.Clear();
                    IncomingDataListBox.Items.Clear();
                });
            }
            Invoke((MethodInvoker)delegate {
                IncomingDataListBox.Items.Add(client.playerName + ": " + data);
            });
            switch (allData[0]) {
                case "CWHO":
                    bool nameAllowed = true;
                    foreach (ServerClient player in connectedClients) {
                        if (player.playerName == allData[1])
                            nameAllowed = false;
                        break;
                    }
                    if (nameAllowed == false) {
                        Send("SWrongName", client);
                        client.connected = false;
                    }
                    else {
                        Send("SAuthenticated", client);
                        client.authenticated = true;
                        client.playerName = allData[1];
                        Invoke((MethodInvoker)delegate {
                            ClientListBox.Items.Remove("Waiting for authentification...");
                            ClientListBox.Items.Add(client.playerName);
                        });
                    }
                    break;
                case "CSearchGame":
                    //start waiting if no one is there
                    if (waitingClients.Count == 0)
                        waitingClients.Add(client);
                    //else start a game
                    else {
                        ServerClient otherPlayer = waitingClients[0];
                        client.opponent = otherPlayer;
                        otherPlayer.opponent = client;
                        waitingClients.Remove(client);
                        waitingClients.Remove(otherPlayer);
                        Send("SRDY|" + otherPlayer.playerName, client);
                        Send("SRDY|" + client.playerName, otherPlayer);
                        Battle newBattle = new Battle(client, otherPlayer);
                        currentBattles.Add(newBattle);
                    }
                    break;
                case "CStopSearching":
                    waitingClients.Remove(client);
                    break;
                case "PlFocus":
                    client.currentBattle.PlayerAction("Focus", client.battleIdentity);
                    break;
                case "PlStrike":
                    client.currentBattle.PlayerAction("Strike", client.battleIdentity);
                    break;
                case "PlBlock":
                    client.currentBattle.PlayerAction("Block", client.battleIdentity);
                    break;
                case "CKeepAlive":
                    Send("SKeepAlive", client);
                    break;
                case "Chat":
                    Send("Chat|" + client.playerName + ": " + allData[1], client.opponent);
                    break;
                case "LEAVE":
                    Send("END|OpLEAVE", client.opponent);
                    break;
                case "CDISC":
                    client.connected = false;
                    break;
            }
        }

        //send data to one client
        public void Send(string data, ServerClient client)
        {
            try {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] sendBytes = new byte[dataBytes.Length + 1];
                Array.Copy(dataBytes, 0, sendBytes, 1, dataBytes.Length);
                sendBytes[0] = Convert.ToByte(dataBytes.Length);
                client.tcp.GetStream().Write(sendBytes, 0, sendBytes.Length);
                Invoke((MethodInvoker)delegate {
                    OutputTB.Text = data.Length.ToString();
                    SendListBox.Items.Add("server to " + client.playerName + ": " + data);
                });
            }
            catch {
                Invoke((MethodInvoker)delegate {
                    SendListBox.Items.Add("FAIL server to " + client.playerName + ": " + data);
                });
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            Init();
            StartButton.Enabled = false;
            StopButton.Enabled = true;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            foreach (ServerClient client in connectedClients) {
                client.connected = false;
            }
            waitingClients = new List<ServerClient>();
            connectedClients = new List<ServerClient>();
            serverRunning = false;
            StartButton.Enabled = true;
            StopButton.Enabled = false;
            server.Stop();
            ClientListBox.Items.Clear();
            OutputTB.Text = "server stopped";
        }

        private void ConnectionTimer_Tick(object sender, EventArgs e)
        {
            if (connectedClients != null)
                foreach (ServerClient client in connectedClients)
                    client.timeSinceLastMessage++;
        }

        public class ServerClient
        {
            public TcpClient tcp;
            public bool connected;
            public bool authenticated;
            public string playerName;
            public SavedPlayerData playerData;
            public Battle currentBattle;
            public int battleIdentity;

            public ServerClient opponent;
            public int timeSinceLastMessage;

            public struct SavedPlayerData
            {
                public string[] deckCardIds;
            }

            public ServerClient(TcpClient clientSocket)
            {
                tcp = clientSocket;
                connected = true;
                playerData.deckCardIds = new string[15];

                XmlSerializer serializer = new XmlSerializer(typeof(SavedPlayerData));
                //XmlWriterSettings settings = new XmlWriterSettings();
                //settings.Indent = true;
                //XmlWriter writer = XmlWriter.Create("C:\\_TCT\\somePlayer1.xml", settings);
                //serializer.Serialize(writer, playerData);
                //writer.Close();
                using (StreamReader reader = new StreamReader(@"C:\FCT\somePlayer1.xml")) {
                    playerData = (SavedPlayerData)serializer.Deserialize(reader);
                }
            }
        }

        private void BattleTimer_Tick(object sender, EventArgs e)
        {
            if (currentBattles != null) {
                foreach (Battle _battle in currentBattles) {
                    _battle.battleTimer++;
                    if (_battle.battleTimer > 15) {
                        if (_battle.fighter1.action == null) {
                            _battle.fighter1.action = "Focus";
                        }
                        if (_battle.fighter2.action == null) {
                            _battle.fighter2.action = "Focus";
                        }
                    }
                }
            }
        }
    }
}