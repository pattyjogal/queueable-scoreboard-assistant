using Newtonsoft.Json;
using queueable_scoreboard_assistant.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace queueable_scoreboard_assistant
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NetworkingPage : Page
    {
        private DatagramSocket clientDatagramSocket;

        public NetworkingPage()
        {
            InitializeComponent();

            ConnectedHostsListView.ItemsSource = App.attachedClientAddresses;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var hostName = new HostName(ServerAddress.Text);
            App.rootHostName = hostName;
            StartChildPeer(hostName);
        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            StartRootPeer();
        }

        private async void StartRootPeer()
        {
            try
            {
                App.attachedClientAddresses = new ObservableCollection<(HostName, string)>();

                var serverDatagramSocket = new DatagramSocket();

                serverDatagramSocket.MessageReceived += RootPeerDatagramSocket_MessageReceived;

                await serverDatagramSocket.BindServiceNameAsync(App.PortNumber);

                OkStatusPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                HostErrorMessage.Text = webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message;
                BadStatusPanel.Visibility = Visibility.Visible;

                App.networkStateHandler.NetworkStatus = NetworkState.HostFailure;
            }
        }

        private async void RootPeerDatagramSocket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            QueueRequest queueRequest = ReceiveQueueRequest(args);

            Debug.WriteLine(queueRequest.JsonData);

            switch (queueRequest.Action)
            {
                case RequestAction.HELLO:
                    // Make sure that the hello message was a "ping"
                    var connData = JsonConvert.DeserializeObject<Dictionary<string, string>>(queueRequest.JsonData);

                    if (connData["type"] == "ping")
                    {
                        HandleNewPeer(args.RemoteAddress, connData["port"]);
                    }

                    break;

                case RequestAction.QUEUE_PROPAGATE:
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        HandleQueueUpdate(queueRequest));

                    break;
            }
        }

        private static void HandleQueueUpdate(QueueRequest queueRequest)
        {
            App.scheduledMatches.CollectionChanged -= MainPage.PropagateQueue;

            App.scheduledMatches.Clear();
            var newScheduledMatches
                = JsonConvert.DeserializeObject<ObservableCollection<ScheduledMatch>>(queueRequest.JsonData);

            foreach (var item in newScheduledMatches) 
            {
                App.scheduledMatches.Add(item);
            }

            App.scheduledMatches.CollectionChanged += MainPage.PropagateQueue;

            Debug.WriteLine(App.scheduledMatches);
        }

        private static void HandleServerAck(QueueRequest queueRequest)
        {
            App.scheduledMatches.CollectionChanged -= MainPage.PropagateQueue;

            App.scheduledMatches.Clear();
            
            try
            {
                var newScheduledMatches
                    = JsonConvert.DeserializeObject<ObservableCollection<ScheduledMatch>>(queueRequest.JsonData);

                foreach (var item in newScheduledMatches)
                {
                    App.scheduledMatches.Add(item);
                }

                App.networkStateHandler.NetworkStatus = NetworkState.ClientConnectedToServer;
            }
            catch (JsonSerializationException)
            {
                // TODO: Handle this
            }
  

            App.scheduledMatches.CollectionChanged += MainPage.PropagateQueue;
        }

        private async void HandleNewPeer(HostName senderAddress, string senderPort)
        {
            Debug.WriteLine("New conn from: " + senderAddress + ":" + senderPort);
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                App.attachedClientAddresses.Add((senderAddress, senderPort));
            });

            // Send back an acknowledgment
            QueueRequest queueRequest = new QueueRequest("pong", RequestAction.HELLO);
            string jsonRequest = JsonConvert.SerializeObject(queueRequest);
            await SendMessageToServer(jsonRequest).ConfigureAwait(false);
        }
        
        private async void StartChildPeer(HostName hostName)
        {
            try
            {
                DatagramSocket clientDatagramSocket = new DatagramSocket();
                
                clientDatagramSocket.MessageReceived += ChildPeerDatagramSocket_MessageReceived;

                await clientDatagramSocket.ConnectAsync(hostName, App.PortNumber);

                CoreApplication.Properties.Add("clientSocket", clientDatagramSocket);

                // The ping request to register with the server
                Dictionary<string, string> data = new Dictionary<string, string>() 
                {
                    { "type", "ping" },
                };
                QueueRequest queueRequest = new QueueRequest(JsonConvert.SerializeObject(data), RequestAction.HELLO);
                string requestJson = JsonConvert.SerializeObject(queueRequest);
                await SendMessageToServer(requestJson).ConfigureAwait(false);

                App.networkStateHandler.NetworkStatus = NetworkState.NoConnection;
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                Debug.WriteLine(ex);
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
                {
                    ClientErrorMessage.Visibility = Visibility.Visible;
                    ClientErrorMessage.Text = webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message;
                    App.networkStateHandler.NetworkStatus = NetworkState.ClientFailure;
                });
            }
        }

        private async void ChildPeerDatagramSocket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            QueueRequest queueRequest = ReceiveQueueRequest(args);

            switch (queueRequest.Action)
            {
                case RequestAction.HELLO:
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        HandleServerAck(queueRequest));
                    
                    break;

                case RequestAction.QUEUE_PROPAGATE:
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        HandleQueueUpdate(queueRequest));

                    break;
            }
        }

        public static async Task SendMessageToServer(string message)
        {
            object outValue;
            if (CoreApplication.Properties.TryGetValue("clientSocket", out outValue) && outValue is DatagramSocket)
            {
                DatagramSocket datagramSocket = outValue as DatagramSocket;

                if (!CoreApplication.Properties.TryGetValue("clientOutputWriter", out outValue) || !(outValue is DataWriter))
                {
                    outValue = new DataWriter(datagramSocket.OutputStream);
                }
                DataWriter outputWriter = outValue as DataWriter;

                outputWriter.WriteString(message);
                await outputWriter.StoreAsync();
            }
        }

        private static QueueRequest ReceiveQueueRequest(DatagramSocketMessageReceivedEventArgs args)
        {
            string request;
            using (DataReader dataReader = args.GetDataReader())
            {
                request = dataReader.ReadString(dataReader.UnconsumedBufferLength).Trim();
            }
            Debug.WriteLine(request);
            // Attempt to read the incoming message
            try
            {
                Debug.WriteLine("Deserializing with this json: " + request);
                var queueRequest = JsonConvert.DeserializeObject<QueueRequest>(request);
                return queueRequest;
            }
            catch (JsonSerializationException e)
            {
                // We cannot handle this message if it couldn't properly deserialize
                Debug.Fail("Could not deserialize!");
                throw e;
            }
        }
    }
}
