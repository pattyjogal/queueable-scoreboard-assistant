using Newtonsoft.Json;
using queueable_scoreboard_assistant.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
                var streamSocketListener = new StreamSocketListener();
                streamSocketListener.ConnectionReceived += StreamSocketListener_ConnectionReceived;

                await streamSocketListener.BindServiceNameAsync(App.PortNumber);


                foreach (HostName localHostName in NetworkInformation.GetHostNames())
                {
                    if (localHostName.IPInformation != null)
                    {
                        if (localHostName.Type == HostNameType.Ipv4)
                        {
                            IPAddrText.Text = localHostName.ToString();
                            break;
                        }
                    }
                }

                OkStatusPanel.Visibility = Visibility.Visible;

                App.networkStateHandler.NetworkStatus = NetworkState.HostingIdle;
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                HostErrorMessage.Text = webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message;
                BadStatusPanel.Visibility = Visibility.Visible;

                App.networkStateHandler.NetworkStatus = NetworkState.HostFailure;
            }
        }

        private async void StreamSocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            {
                DataReader reader = new DataReader(args.Socket.InputStream);
                try
                {
                    while (true)
                    {
                        uint sizeFieldLen = await reader.LoadAsync(sizeof(uint));

                        if (sizeFieldLen != sizeof(uint))
                        {
                            // Socket closed unexpectedly
                            return;
                        }

                        // Read the sent result
                        uint stringLen = reader.ReadUInt32();
                        uint stringLenRead = await reader.LoadAsync(stringLen);
                        if (stringLen != stringLenRead)
                        {
                            // Socket closed unexpectedly
                            return;
                        }

                        string message = reader.ReadString(stringLenRead);
                        Debug.WriteLine(message);

                        QueueRequest request = JsonConvert.DeserializeObject<QueueRequest>(message);
                        DataWriter writer = new DataWriter(args.Socket.OutputStream);
                        switch (request.Action)
                        {
                            case RequestAction.HELLO:
                                Debug.WriteLine("Sending P O N G");
                                QueueRequest response = new QueueRequest(JsonConvert.SerializeObject("pong"), RequestAction.HELLO);
                                response.Send(writer);
                                break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                    {
                        throw;
                    }
                }
            }
        }

        private async void StartChildPeer(string address)
        {
            try
            {
                // Create the StreamSocket and establish a connection to the echo server.
                using (var streamSocket = new StreamSocket())
                {
                    var hostName = new HostName(address);

                    ConnectingClientStatusPanel.Visibility = Visibility.Visible;
                    

                    await streamSocket.ConnectAsync(hostName, App.PortNumber);

                    ConnectingClientStatusPanel.Visibility = Visibility.Collapsed;
                    OkClientStatusPanel.Visibility = Visibility.Visible;

                    App.socket = streamSocket;
                    App.networkStateHandler.NetworkStatus = NetworkState.ClientConnectedToServer;


                    // Consider caching a datawriter                   
                    DataWriter writer = new DataWriter(streamSocket.OutputStream);

                    // Write first the length of the string as UINT32 value followed up by the string. 
                    // Writing data to the writer will just store data in memory.
                    QueueRequest request = new QueueRequest(JsonConvert.SerializeObject("ping"), RequestAction.HELLO);
                    request.Send(writer);
                    
                }
            }
            catch (Exception ex)
            {
                ConnectingClientStatusPanel.Visibility = Visibility.Collapsed;
                OkClientStatusPanel.Visibility = Visibility.Collapsed;
                BadClientStatusPanel.Visibility = Visibility.Visible;

                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                ClientErrorMessage.Text = webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message;

                App.networkStateHandler.NetworkStatus = NetworkState.ClientFailure;
            }
        }
    }
}
