using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
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
            this.InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            StartClient(ServerAddress.Text);
        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            StartServer();
        }

        private async void StartServer()
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
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                HostErrorMessage.Text = webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message;
                BadStatusPanel.Visibility = Visibility.Visible;
            }
        }

        private async void StreamSocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            string request;
            using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            {
                request = await streamReader.ReadLineAsync();
            }

            //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server received the request: \"{0}\"", request)));

            // Echo the request back as the response.
            using (Stream outputStream = args.Socket.OutputStream.AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }

            //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server sent back the response: \"{0}\"", request)));

            sender.Dispose();

            //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add("server closed its socket"));
        }

        private async void StartClient(string address)
        {
            try
            {
                // Create the StreamSocket and establish a connection to the echo server.
                using (var streamSocket = new Windows.Networking.Sockets.StreamSocket())
                {
                    // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                    var hostName = new HostName(address);

                    ConnectingClientStatusPanel.Visibility = Visibility.Visible;
                    await streamSocket.ConnectAsync(hostName, App.PortNumber);

                    ConnectingClientStatusPanel.Visibility = Visibility.Collapsed;
                    OkClientStatusPanel.Visibility = Visibility.Visible;

                    // Send a request to the echo server.
                    /*                   string request = "Hello, World!";
                                       using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                                       {
                                           using (var streamWriter = new StreamWriter(outputStream))
                                           {
                                               await streamWriter.WriteLineAsync(request);
                                               await streamWriter.FlushAsync();
                                           }
                                       }

                                       this.clientListBox.Items.Add(string.Format("client sent the request: \"{0}\"", request));

                                       // Read data from the echo server.
                                       string response;
                                       using (Stream inputStream = streamSocket.InputStream.AsStreamForRead())
                                       {
                                           using (StreamReader streamReader = new StreamReader(inputStream))
                                           {
                                               response = await streamReader.ReadLineAsync();
                                           }
                                       }

                                       this.clientListBox.Items.Add(string.Format("client received the response: \"{0}\" ", response));
                                   }

                                   this.clientListBox.Items.Add("client closed its socket");*/
                }
            }
            catch (Exception ex)
            {
                ConnectingClientStatusPanel.Visibility = Visibility.Collapsed;
                OkClientStatusPanel.Visibility = Visibility.Collapsed;
                BadClientStatusPanel.Visibility = Visibility.Visible;
    
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                ClientErrorMessage.Text = webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message;
            }
        }
    }
}
