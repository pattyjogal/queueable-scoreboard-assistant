using queueable_scoreboard_assistant.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        private const string DfaStoreFilename = "names.dfa";
        private readonly Windows.Storage.StorageFolder storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;


        private LanguageDfa languageDfa;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Run only once
            if (languageDfa == null)
            {
                // Load the existing autocomplete language from the stored file
                Windows.Storage.StorageFile dfaFile;
                try
                {
                    dfaFile = await storageFolder.GetFileAsync(DfaStoreFilename);
                } catch (FileNotFoundException)
                {
                    // Create the store if it does not exist
                    dfaFile = await storageFolder.CreateFileAsync(DfaStoreFilename);
                }

                Stream stream = await dfaFile.OpenStreamForReadAsync();
                languageDfa = new LanguageDfa();
                languageDfa.ReadPrefixStates(stream);
            }
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

        private void AutoSuggestBox_TextChanged_Player(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string[] matches = languageDfa.ListPossibleStrings(sender.Text);
                sender.ItemsSource = matches.ToList();
            }
        }
        
        private async void AutoSuggestBox_QuerySubmitted_Player(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
            }
            else
            {
                // The user wants to commit a new player name
                languageDfa.AddNewString(args.QueryText);

                // TODO: Intoduce an option to save to file on each new name vs. saving when the program closes
                Windows.Storage.StorageFile dfaFile = await storageFolder.GetFileAsync(DfaStoreFilename);
                using (Stream stream = await dfaFile.OpenStreamForWriteAsync())
                    languageDfa.WritePrefixStates(stream);
            }
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationView navView = (NavigationView)sender;
            navView.IsPaneOpen = false;
            navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        }
    }
}
