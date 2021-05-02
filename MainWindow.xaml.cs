using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NPC_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> Variables = new();
        List<string> Result = new();
        string InputText = "";
        Random ran = new Random();
        public MainWindow()
        {
            InitializeComponent();
            TBAmount.Text = Properties.Settings.Default.amount;
            TBInputText.Text = Properties.Settings.Default.input;
        }

        private void TBAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!int.TryParse(TBAmount.Text, out int result))
                {

                    TBAmount.Foreground = new SolidColorBrush(Colors.Red);
                    LbLInfo.Content = "Amount needs to be a number!";
                    BtnGo.IsEnabled = false;

                }
                else
                {
                    if (result <= 0)
                    {
                        TBAmount.Foreground = new SolidColorBrush(Colors.Red);
                        LbLInfo.Content = "Amount can't be less then 1!";
                        BtnGo.IsEnabled = false;
                    }
                    else
                    {
                        TBAmount.Foreground = new SolidColorBrush(Colors.Black);
                        BtnGo.IsEnabled = true;
                        LbLInfo.Content = "";
                        Properties.Settings.Default.amount = TBAmount.Text;
                        Properties.Settings.Default.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sorry something went wrong: \n" + ex.Message);
            }
        }

        private void BtnOptionsFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", $"{Directory.GetCurrentDirectory()}\\Options");
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Clipboard.SetText($"{ Directory.GetCurrentDirectory()}\\Options");
                MessageBox.Show($"Sorry some error occourd\n If the folder exists you can find it here: { Directory.GetCurrentDirectory()}\\Options");
            }

        }

        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            BtnGo.IsEnabled = false;
            PB.Visibility = Visibility.Visible;
            PB.Minimum = 0;
            int amount = Convert.ToInt32(TBAmount.Text);
            PB.Maximum = amount;
            Result = new();
            Variables = new();
            InputText = TBInputText.Text.ToString();
            foreach (Match match in Regex.Matches(InputText, "{[^}]+}"))
            {
                Variables.Add(match.Value.Replace("{", "").Replace("}", ""));
            }
            new Thread(() =>
               {
                   try
                   {
                       for (int i = 0; i < amount; i++)
                       {
                           Dispatcher.BeginInvoke(new Action(() =>
                           {
                               PB.Value = i;
                           }));
                           GenerateSentence();
                       }
                       var s = String.Join(System.Environment.NewLine, Result.ToHashSet().ToArray());
                       File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Result.txt", s);

                       MessageBoxResult r = MessageBox.Show(amount == 1 ? "The single sentence has been generated! Would you like to open the result file now?" : $"All {amount.ToString()} sentences have been generated! Would you like to open the result file now?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                       if (MessageBoxResult.Yes == r)
                       {
                           Process.Start("Notepad.exe", $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Result.txt");
                       }
                   }
                   catch (Exception ex)
                   {
                       MessageBox.Show("Sorry something went wrong!\n" + ex.Message);
                   }
                   finally
                   {
                       Dispatcher.BeginInvoke(new Action(() =>
                       {
                           PB.Visibility = Visibility.Collapsed;
                           BtnGo.IsEnabled = true;
                       }));
                   }
               }).Start();
        }

        private void GenerateSentence()
        {
            var temp = InputText;
            List<string> j = new();
            //
            foreach (var currentVar in Variables)
            {
                temp = temp.Replace($"{{{currentVar}}}", GetOneVarFromFile(currentVar));
            }
            Result.Add(Regex.Replace(temp, @"\s+", " "));

        }

        private string GetOneVarFromFile(string FileName)
        {
            var x = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\Options\\{FileName}.txt").Split(Environment.NewLine).ToList();
            var i = x[ran.Next(0, x.Count)];
            return i;
        }

        private void TBInputText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.input = TBInputText.Text;
            Properties.Settings.Default.Save();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Use {Head} in input text box if you want {Head} replaces with an Item from a file called Head.txt. You can find the Folder to put said file when you click 'options Folder'. One row is one item. ");
        }
    }
}
