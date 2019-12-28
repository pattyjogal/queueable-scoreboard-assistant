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
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

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
    }
}
