﻿using Newtonsoft.Json;
using queueable_scoreboard_assistant.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace queueable_scoreboard_assistant
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly DfaAutocomplete playerNamesAutocomplete = App.playerNamesAutocomplete;
        private bool _isScoreboardPopulated = false;
        public bool IsScoreboardPopulated
        { 
            get
            {
                return _isScoreboardPopulated;
            }

            set
            {
                _isScoreboardPopulated = value;
                RequeueButton.IsEnabled = value;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            IsScoreboardPopulated = false;
            ScheduledMatchesListView.ItemsSource = App.scheduledMatches;

            App.scheduledMatches.CollectionChanged += ScheduledMatches_CollectionChanged;
            App.scheduledMatches.CollectionChanged += PropagateQueue;
            App.mainContentFrame = ContentFrame;
            UpdateStreamFileAsync("p1_score.txt", "0");
            UpdateStreamFileAsync("p2_score.txt", "0");

            // Observe the change in network connection state
            App.networkStateHandler.PropertyChanged += NetworkStateHandler_PropertyChanged;
        }

        private void PropagateQueue(object sender, NotifyCollectionChangedEventArgs e)
        {
            //string json = JsonConvert.SerializeObject(App.scheduledMatches);
            //QueueRequest req = new QueueRequest(json, RequestAction.QUEUE_PROPAGATE);
            //Debug.WriteLine("Sending: " + JsonConvert.SerializeObject(req));
            // Only write to socket if the connection has been established
            if (App.socket != null)
            {
                // Serialize the queue
               /* string json = JsonConvert.SerializeObject(App.scheduledMatches);
                QueueRequest req = new QueueRequest(json, RequestAction.QUEUE_PROPAGATE);
                Debug.WriteLine("Sending: " + json);*/

                //App.socket.OutputStream.AsStreamForWrite().Write(Encoding.UTF8.GetBytes(json), 0, json.Length);
            }
        }

        private void NetworkStateHandler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("NetworkStatus"))
            {
                switch ((sender as NetworkStateHandler).NetworkStatus)
                {
                    case NetworkState.NoConnection:
                        NetworkStatusBar.Visibility = Visibility.Collapsed;
                        break;
                    case NetworkState.HostingIdle:
                        NetworkStatusBar.Visibility = Visibility.Visible;
                        NetworkStatusBar.Background = new SolidColorBrush(Colors.Gold);
                        NetworkStatusText.Foreground = new SolidColorBrush(Colors.Black);
                        NetworkStatusText.Text = "Hosting; Waiting for clients...";
                        break;
                    case NetworkState.HostingClient:
                        NetworkStatusBar.Visibility = Visibility.Visible;
                        NetworkStatusBar.Background = new SolidColorBrush(Colors.LawnGreen);
                        NetworkStatusText.Foreground = new SolidColorBrush(Colors.White);
                        NetworkStatusText.Text = "Hosting; Client connected";
                        break;
                    case NetworkState.HostFailure:
                        NetworkStatusBar.Visibility = Visibility.Visible;
                        NetworkStatusBar.Background = new SolidColorBrush(Colors.DarkRed);
                        NetworkStatusText.Foreground = new SolidColorBrush(Colors.White);
                        NetworkStatusText.Text = "Hosting Error";
                        break;
                    case NetworkState.ClientConnectedToServer:
                        NetworkStatusBar.Visibility = Visibility.Visible;
                        NetworkStatusBar.Background = new SolidColorBrush(Colors.LawnGreen);
                        NetworkStatusText.Foreground = new SolidColorBrush(Colors.White);
                        NetworkStatusText.Text = "Connected to server";
                        break;
                    case NetworkState.ClientFailure:
                        NetworkStatusBar.Visibility = Visibility.Visible;
                        NetworkStatusBar.Background = new SolidColorBrush(Colors.DarkRed);
                        NetworkStatusText.Foreground = new SolidColorBrush(Colors.White);
                        NetworkStatusText.Text = "Failed to connect to server";
                        break;

                }
            }
        }

        private void ScheduledMatches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            QueuePullButton.IsEnabled = App.scheduledMatches.Count > 0;
        }

        private void Button_Click_LeftDecrement(object sender, RoutedEventArgs e)
        {
            try
            {
                LeftScore.Text = (int.Parse(LeftScore.Text) - 1).ToString();
            } catch
            {
                LeftScore.Text = "NaN";
            }
        }

        private void Button_Click_RightDecrement(object sender, RoutedEventArgs e)
        {
            try
            {
                RightScore.Text = (int.Parse(RightScore.Text) - 1).ToString();
            }
            catch
            {
                RightScore.Text = "NaN";
            }
        }

        private void Button_Click_LeftIncrement(object sender, RoutedEventArgs e)
        {
            try
            {
                LeftScore.Text = (int.Parse(LeftScore.Text) + 1).ToString();
            }
            catch
            {
                LeftScore.Text = "NaN";
            }
        }

        private void Button_Click_RightIncrement(object sender, RoutedEventArgs e)
        {
            try
            {
                RightScore.Text = (int.Parse(RightScore.Text) + 1).ToString();
            }
            catch
            {
                RightScore.Text = "NaN";
            }
        }


        private async void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationView navView = (NavigationView)sender;
            App.nagivationView = navView;
            navView.IsPaneOpen = false;
            navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        }


        private void NavigationView_Navigate(NavigationViewItem item)
        {
            if (item != null)
            {
                switch (item.Tag.ToString())
                {
                    case "Schedule Match":
                        ContentFrame.Navigate(typeof(ScheduleMatchPage));
                        break;
                    case "Networking Settings":
                        ContentFrame.Navigate(typeof(NetworkingPage));
                        break;
                }
            }
        }

        private async void Button_Click_QueuePopAsync(object sender, RoutedEventArgs e)
        {
            if (App.scheduledMatches.Count > 0)
            {
                App.activeMatch = App.scheduledMatches.First();
                App.scheduledMatches.RemoveAt(0); 

                ActiveMatchPlayerOneAutocomplete.Text = App.activeMatch.FirstPlayer;
                ActiveMatchPlayerTwoAutocomplete.Text = App.activeMatch.SecondPlayer;

                // Now that something has been dequeued, we alow requeuing
                IsScoreboardPopulated = true;

                // Write the new names out to file
                try
                {
                    await UpdateStreamFileAsync("p1_name.txt", ActiveMatchPlayerOneAutocomplete.Text);
                    await UpdateStreamFileAsync("p2_name.txt", ActiveMatchPlayerTwoAutocomplete.Text);
                    await UpdateStreamFileAsync("p1_score.txt", "0");
                    await UpdateStreamFileAsync("p2_score.txt", "0");
                    await UpdateStreamFileAsync("match_name.txt", App.activeMatch.MatchName);
                    ActiveMatchPlayerOneAutocomplete.QueryIcon = null;
                    ActiveMatchPlayerTwoAutocomplete.QueryIcon = null;
                }
                catch (Exception ex)
                {
                    if (ex.HResult != 0x80070497)
                    {
                        throw ex;
                    }

                    ContentDialog alert = new ContentDialog
                    {
                        Title = "Could not save",
                        Content = "One of the files was in use. Please try again.",
                        CloseButtonText = "Ok"
                    };

                    ContentDialogResult result = await alert.ShowAsync();
                }
            }
        }

        private async void Button_Click_RequeueAsync(object sender, RoutedEventArgs e)
        {
            if (IsScoreboardPopulated)
            {
                ScheduledMatch nextMatch = new ScheduledMatch(
                    ActiveMatchPlayerOneAutocomplete.Text,
                    ActiveMatchPlayerTwoAutocomplete.Text,
                    App.activeMatch.MatchName
                    );

                App.scheduledMatches.Insert(0, nextMatch);
                App.activeMatch = null;

                ActiveMatchPlayerOneAutocomplete.Text = "";
                ActiveMatchPlayerTwoAutocomplete.Text = "";

                // Once a match has been dequeued, the scoreboard is blank
                IsScoreboardPopulated = false;

                // Clear the names in the files
                try
                {
                    await UpdateStreamFileAsync("p1_name.txt", ActiveMatchPlayerOneAutocomplete.Text);
                    await UpdateStreamFileAsync("p2_name.txt", ActiveMatchPlayerTwoAutocomplete.Text);
                    await UpdateStreamFileAsync("p1_score.txt", "0");
                    await UpdateStreamFileAsync("p2_score.txt", "0");
                    await UpdateStreamFileAsync("match_name.txt", "");

                }
                catch (Exception ex)
                {
                    if (ex.HResult != 0x80070497)
                    {
                        throw ex;
                    }

                    ContentDialog alert = new ContentDialog
                    {
                        Title = "Could not save",
                        Content = "One of the files was in use. Please try again.",
                        CloseButtonText = "Ok"
                    };

                    ContentDialogResult result = await alert.ShowAsync();
                }

            }
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
                NavigationView_Navigate(item as NavigationViewItem);
            }
        }

        private async void LeftScore_TextChangedAsync(object sender, TextChangedEventArgs e)
        {
            await UpdateStreamFileAsync("p1_score.txt", (sender as TextBox).Text);
        }

        private async void RightScore_TextChangedAsync(object sender, TextChangedEventArgs e)
        {
            await UpdateStreamFileAsync("p2_score.txt", (sender as TextBox).Text);

        }

        private async void ActiveMatchPlayerOneAutocomplete_QuerySubmittedAsync(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            playerNamesAutocomplete.QuerySubmitted(sender, args);
            await UpdateStreamFileAsync("p1_name.txt", sender.Text);

        }

        private async void ActiveMatchPlayerTwoAutocomplete_QuerySubmittedAsync(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            playerNamesAutocomplete.QuerySubmitted(sender, args);
            await UpdateStreamFileAsync("p2_name.txt", sender.Text);

        }

        private async System.Threading.Tasks.Task UpdateStreamFileAsync(string filename, string value)
        {
            object outputFolder;
            if (App.localSettings.Values.TryGetValue("output_folder", out outputFolder))
            {
                Windows.Storage.StorageFolder storageFolder = 
                    await Windows.Storage.StorageFolder.GetFolderFromPathAsync(outputFolder as string);
                Windows.Storage.StorageFile outFile =
                    await storageFolder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(outFile, value);
            } 
            else
            {
                ContentDialog alert = new ContentDialog
                {
                    Title = "Could not save",
                    Content = "You must set an output directory to write files for streaming software.",
                    CloseButtonText = "Ok"
                };

                ContentDialogResult result = await alert.ShowAsync();
            }
        }

        private async void SwitchButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            string temp = ActiveMatchPlayerOneAutocomplete.Text;
            ActiveMatchPlayerOneAutocomplete.Text = ActiveMatchPlayerTwoAutocomplete.Text;
            ActiveMatchPlayerTwoAutocomplete.Text = temp;

            temp = LeftScore.Text;
            LeftScore.Text = RightScore.Text;
            RightScore.Text = temp;

            await UpdateStreamFileAsync("p1_name.txt", ActiveMatchPlayerOneAutocomplete.Text);
            await UpdateStreamFileAsync("p2_name.txt", ActiveMatchPlayerTwoAutocomplete.Text);
            await UpdateStreamFileAsync("p1_score.txt", LeftScore.Text);
            await UpdateStreamFileAsync("p2_score.txt", RightScore.Text);
        }
    }
}
