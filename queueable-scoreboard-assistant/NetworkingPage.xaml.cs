using Newtonsoft.Json;
using queueable_scoreboard_assistant.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace queueable_scoreboard_assistant
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NetworkingPage : Page
    {
        public NetworkingPage()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            StartChildPeer(ServerAddress.Text);
        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            StartRootPeer();
        }

        private async void StartRootPeer()
        {
            try
            {
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
            }
        }

        private async void HandleNewPeer(HostName senderAddress, string senderPort)
        {
            Debug.WriteLine("New conn from: " + senderAddress + ":" + senderPort);
            App.attachedClientAddresses.Add((senderAddress, senderPort));

            // Send back an acknowledgment
            QueueRequest queueRequest = new QueueRequest("pong", RequestAction.HELLO);
            string jsonRequest = JsonConvert.SerializeObject(queueRequest);
            await SendMessageToPeer(senderAddress, senderPort, jsonRequest).ConfigureAwait(false);
        }
        
        private async void StartChildPeer(string address)
        {
            try
            {
                DatagramSocket clientDatagramSocket = new DatagramSocket();
                
                clientDatagramSocket.MessageReceived += ChildPeerDatagramSocket_MessageReceived;

                var hostName = new HostName(address);

                await clientDatagramSocket.BindServiceNameAsync("9000");

                // The ping request to register with the server
                Dictionary<string, string> data = new Dictionary<string, string>() 
                {
                    { "type", "ping" },
                    { "port", "9000" }
                };
                QueueRequest queueRequest = new QueueRequest(JsonConvert.SerializeObject(data), RequestAction.HELLO);
                string requestJson = JsonConvert.SerializeObject(queueRequest);
                await SendMessageToPeer(hostName, App.PortNumber, requestJson).ConfigureAwait(false);

                App.networkStateHandler.NetworkStatus = NetworkState.HostingIdle;
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
                    // Ensure that the response was a "pong"
                    if (queueRequest.JsonData == "pong")
                    {
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            App.networkStateHandler.NetworkStatus = NetworkState.ClientConnectedToServer);
                    }
                    break;
            }
        }

        private async Task SendMessageToPeer(HostName hostName, string port, string message)
        {
            using (var serverDatagramSocket = new DatagramSocket())
            {
                using (Stream outputStream = (await serverDatagramSocket.GetOutputStreamAsync(hostName, port)).AsStreamForWrite())
                {
                    using (var streamWriter = new StreamWriter(outputStream))
                    {
                        await streamWriter.WriteLineAsync(message).ConfigureAwait(false);
                        await streamWriter.FlushAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        private QueueRequest ReceiveQueueRequest(DatagramSocketMessageReceivedEventArgs args)
        {
            string request;
            using (DataReader dataReader = args.GetDataReader())
            {
                request = dataReader.ReadString(dataReader.UnconsumedBufferLength).Trim();
            }
            Debug.WriteLine(request);
            // Attempt to read the incoming message
            List<string> deserializationErrors = new List<string>();
            var queueRequest = JsonConvert.DeserializeObject<QueueRequest>(request,
                new JsonSerializerSettings
                {
                    Error = delegate (object errorSender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
                    {
                        deserializationErrors.Add(errorArgs.ErrorContext.Error.Message);
                        errorArgs.ErrorContext.Handled = true;
                    },
                });

            // We cannot handle this message if it couldn't properly deserialize
            if (deserializationErrors.Count > 0)
            {
                Debug.Fail("Could not deserialize!");
                throw new InvalidDataException();
            }

            return queueRequest;
        }
    }
}
