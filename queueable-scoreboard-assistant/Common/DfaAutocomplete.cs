using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace queueable_scoreboard_assistant.Common
{
    public class DfaAutocomplete
    {
        private const string DfaStoreFilename = "names.dfa";
        private readonly Windows.Storage.StorageFolder storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;
    
        public LanguageDfa languageDfa;


        public async void Init()
        {
            Windows.Storage.StorageFile dfaFile;
            try
            {
                dfaFile = await storageFolder.GetFileAsync(DfaStoreFilename);
            }
            catch (FileNotFoundException)
            {
                // Create the store if it does not exist
                dfaFile = await storageFolder.CreateFileAsync(DfaStoreFilename);
            }

            Stream stream = await dfaFile.OpenStreamForReadAsync();
            languageDfa = new LanguageDfa();
            languageDfa.ReadPrefixStates(stream);
        }

        public void TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string[] matches = languageDfa.ListPossibleStrings(sender.Text);
                sender.ItemsSource = matches.ToList();
            }
        }

        public async void QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
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

    }
}
