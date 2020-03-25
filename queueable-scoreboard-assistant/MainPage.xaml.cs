using Newtonsoft.Json;
using queueable_scoreboard_assistant.Common;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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

            // Listen for common state changes
            App.networkStateHandler.PropertyChanged += NetworkStateHandler_PropertyChanged;
            App.scoreboardStateHandler.PropertyChanged += ScoreboardStateHandler_PropertyChanged;
        }

        private void ScoreboardStateHandler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            LeftScore.Text = App.scoreboardStateHandler.ScoreboardState.LeftScore.ToString();
            RightScore.Text = App.scoreboardStateHandler.ScoreboardState.RightScore.ToString();
            ActiveMatchPlayerOneAutocomplete.Text = App.scoreboardStateHandler.ScoreboardState.LeftPlayerName.ToString();
            ActiveMatchPlayerTwoAutocomplete.Text = App.scoreboardStateHandler.ScoreboardState.RightPlayerName.ToString();
        }

        public static async void PropagateQueue(object sender, NotifyCollectionChangedEventArgs e)
        {
            string scheduledMatchesJson = JsonConvert.SerializeObject(App.scheduledMatches);
            QueueRequest queueRequest = new QueueRequest(scheduledMatchesJson, RequestAction.QUEUE_PROPAGATE);
            string requestJson = JsonConvert.SerializeObject(queueRequest);
            
            // If there are attached client addresses, we're the root
            if (App.attachedClientAddresses != null)    
            {
                foreach ((var host, var port) in App.attachedClientAddresses)
                {
                    await NetworkingPage.SendMessageToServer(requestJson);
                }
            }
            // Otherwise, we're a normal peer, and are sending this to the root
            else
            {
                Debug.WriteLine("I have been invoked");
                await NetworkingPage.SendMessageToServer(requestJson);
            }
        }

        private async void NetworkStateHandler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("NetworkStatus"))
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
                    });
            }
        }

        private void ScheduledMatches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            QueuePullButton.IsEnabled = App.scheduledMatches.Count > 0;
        }

        private void Button_Click_LeftDecrement(object sender, RoutedEventArgs e)
        {
            ScoreboardState updatedScoreboardState = App.scoreboardStateHandler.ScoreboardState;
            updatedScoreboardState.LeftScore -= 1;
            App.scoreboardStateHandler.ScoreboardState = updatedScoreboardState;
        }

        private void Button_Click_RightDecrement(object sender, RoutedEventArgs e)
        {
            ScoreboardState updatedScoreboardState = App.scoreboardStateHandler.ScoreboardState;
            updatedScoreboardState.RightScore -= 1;
            App.scoreboardStateHandler.ScoreboardState = updatedScoreboardState;
        }

        private void Button_Click_LeftIncrement(object sender, RoutedEventArgs e)
        {
            ScoreboardState updatedScoreboardState = App.scoreboardStateHandler.ScoreboardState;
            updatedScoreboardState.LeftScore += 1;
            App.scoreboardStateHandler.ScoreboardState = updatedScoreboardState;
        }

        private void Button_Click_RightIncrement(object sender, RoutedEventArgs e)
        {
            ScoreboardState updatedScoreboardState = App.scoreboardStateHandler.ScoreboardState;
            updatedScoreboardState.RightScore += 1;
            App.scoreboardStateHandler.ScoreboardState = updatedScoreboardState;
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
                ScoreboardState updatedScoreboardState = App.scoreboardStateHandler.ScoreboardState;

                App.activeMatch = App.scheduledMatches.First();
                App.scheduledMatches.RemoveAt(0);

                updatedScoreboardState.LeftPlayerName = App.activeMatch.FirstPlayer;
                updatedScoreboardState.RightPlayerName = App.activeMatch.SecondPlayer;

                // Now that something has been dequeued, we alow requeuing
                IsScoreboardPopulated = true;

                // Write the new names out to file
                try
                {
                    await UpdateStreamFileAsync("p1_name.txt", updatedScoreboardState.LeftPlayerName);
                    await UpdateStreamFileAsync("p2_name.txt", updatedScoreboardState.RightPlayerName);
                    await UpdateStreamFileAsync("p1_score.txt", "0");
                    await UpdateStreamFileAsync("p2_score.txt", "0");
                    await UpdateStreamFileAsync("match_name.txt", App.activeMatch.MatchName);
                    ActiveMatchPlayerOneAutocomplete.QueryIcon = null;
                    ActiveMatchPlayerTwoAutocomplete.QueryIcon = null;
                }
                catch (System.IO.FileNotFoundException)
                {
                    await WarnUserNoDirectoryOutput();
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

                App.scoreboardStateHandler.ScoreboardState = updatedScoreboardState;
            }
        }

        private async void Button_Click_RequeueAsync(object sender, RoutedEventArgs e)
        {
            if (IsScoreboardPopulated)
            {
                ScoreboardState updatedScoreboardState = App.scoreboardStateHandler.ScoreboardState;

                ScheduledMatch nextMatch = new ScheduledMatch(
                    ActiveMatchPlayerOneAutocomplete.Text,
                    ActiveMatchPlayerTwoAutocomplete.Text,
                    App.activeMatch.MatchName
                    );

                App.scheduledMatches.Insert(0, nextMatch);
                App.activeMatch = null;

                updatedScoreboardState.LeftPlayerName = "";
                updatedScoreboardState.RightPlayerName = "";

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
                catch (System.IO.FileNotFoundException)
                {
                    await WarnUserNoDirectoryOutput();
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

                App.scoreboardStateHandler.ScoreboardState = updatedScoreboardState;
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
            ScoreboardState updatedScoreboardState = App.scoreboardStateHandler.ScoreboardState;
            try
            {
                updatedScoreboardState.LeftScore = int.Parse((sender as TextBox).Text);
            }
            catch (FormatException)
            {
                updatedScoreboardState.LeftScore = 0;
            }

            App.scoreboardStateHandler.ScoreboardState = updatedScoreboardState;

            try
            {
                await UpdateStreamFileAsync("p1_score.txt", (sender as TextBox).Text);
            }
            catch (System.IO.FileNotFoundException)
            {
                await WarnUserNoDirectoryOutput();
            }
        }

        private async void RightScore_TextChangedAsync(object sender, TextChangedEventArgs e)
        {
            ScoreboardState updatedScoreboardState = App.scoreboardStateHandler.ScoreboardState;
            try
            {
                updatedScoreboardState.RightScore = int.Parse((sender as TextBox).Text);
            }
            catch (FormatException)
            {
                updatedScoreboardState.RightScore = 0;
            }

            App.scoreboardStateHandler.ScoreboardState = updatedScoreboardState;

            try
            {
                await UpdateStreamFileAsync("p2_score.txt", (sender as TextBox).Text);
            }
            catch (System.IO.FileNotFoundException)
            {
                await WarnUserNoDirectoryOutput();
            }
        }

        private async void ActiveMatchPlayerOneAutocomplete_QuerySubmittedAsync(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            playerNamesAutocomplete.QuerySubmitted(sender, args);
            try
            {
                await UpdateStreamFileAsync("p_name.txt", sender.Text);
            }
            catch (System.IO.FileNotFoundException)
            {
                await WarnUserNoDirectoryOutput();
            }
        }

        private async void ActiveMatchPlayerTwoAutocomplete_QuerySubmittedAsync(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            playerNamesAutocomplete.QuerySubmitted(sender, args);
            try
            {
                await UpdateStreamFileAsync("p2_name.txt", sender.Text);
            }
            catch (System.IO.FileNotFoundException)
            {
                await WarnUserNoDirectoryOutput();
            }
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
                throw new System.IO.FileNotFoundException();
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

            try
            {
                await UpdateStreamFileAsync("p1_name.txt", ActiveMatchPlayerOneAutocomplete.Text);
                await UpdateStreamFileAsync("p2_name.txt", ActiveMatchPlayerTwoAutocomplete.Text);
                await UpdateStreamFileAsync("p1_score.txt", LeftScore.Text);
                await UpdateStreamFileAsync("p2_score.txt", RightScore.Text);
            } 
            catch (System.IO.FileNotFoundException)
            {
                await WarnUserNoDirectoryOutput();
            }
        }

        private static async Task WarnUserNoDirectoryOutput()
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
}
