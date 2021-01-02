using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            TBAmount.Text = "1";
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
                MessageBox.Show($"Sorry some error occourd\n If the folder exists you can find it here: { Directory.GetCurrentDirectory()}\\Options");
            }

        }

        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            PB.Visibility = Visibility.Visible;
            Result = new();
            Variables = new();
            InputText = TBInputText.Text.ToString();
            foreach (Match match in Regex.Matches(InputText, "{[^}]+}"))
            {
                Variables.Add(match.Value.Replace("{", "").Replace("}", ""));
            }
            //Variables.ForEach(x => { MessageBox.Show(x); });
            int amount = Convert.ToInt32(TBAmount.Text);
            Thread th = new Thread(() =>
            {
                try
                {

                    for (int i = 0; i < amount; i++)
                    {
                        GenerateSentence();
                    }
                    var s = String.Join(System.Environment.NewLine, Result.ToArray());
                    //File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Result.txt",s);
                    MessageBox.Show(s);
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
                    }));
                }
            });
            th.Start();

        }

        private void GenerateSentence()
        {
            var temp = InputText;
            List<string> j = new();
            //
            foreach (var currentVar in Variables)
            {
                InputText = InputText.Replace($"{{{currentVar}}}", GetOneVarFromFile(currentVar));
            }
            Result.Add(InputText);

        }
        private string GetOneVarFromFile(string FileName)
        {
            var x = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\Options\\{FileName}.txt").Split(Environment.NewLine).ToList();
            ran = new Random(DateTime.Now.Millisecond);
            Thread.Sleep(100);
            return x[ran.Next(0, x.Count)];
        }
    }
}
