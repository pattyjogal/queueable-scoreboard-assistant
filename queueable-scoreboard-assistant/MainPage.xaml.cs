﻿using queueable_scoreboard_assistant.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            App.mainContentFrame = ContentFrame;
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


        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationView navView = (NavigationView)sender;
            App.nagivationView = navView;
            navView.IsPaneOpen = false;
            navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        }


        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem != null)
            {
                switch (((NavigationViewItem)args.SelectedItem).Tag.ToString())
                {
                    case "Schedule Match":
                        ContentFrame.Navigate(typeof(ScheduleMatchPage));
                        break;

                }
            }
        }

        private void Button_Click_QueuePop(object sender, RoutedEventArgs e)
        {
            if (App.scheduledMatches.Count > 0)
            {
                App.activeMatch = App.scheduledMatches.First();
                App.scheduledMatches.RemoveAt(0); 

                ActiveMatchPlayerOneAutocomplete.Text = App.activeMatch.FirstPlayer;
                ActiveMatchPlayerTwoAutocomplete.Text = App.activeMatch.SecondPlayer;

                // Now that something has been dequeued, we alow requeuing
                IsScoreboardPopulated = true;
            }
        }

        private void Button_Click_Requeue(object sender, RoutedEventArgs e)
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
            }
        }
    }
}
