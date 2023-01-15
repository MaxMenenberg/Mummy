using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Diagnostics;
using System.Security.Cryptography;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Mummy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public ObservableCollection<RecentActivityLogEntry> log;
        string logFile = "RecentActivityLog.csv";

        public MainWindow()
        {
            InitializeComponent();
            log = Utils.importLogActivity(logFile);
            LogDataGrid.ItemsSource = log;
        }

        private void encryptButton_Click(object sender, RoutedEventArgs e)
        {
            Encryption_Window encryptWin = new Encryption_Window();
            encryptWin.updateActivityLog += updateLogList;
            encryptWin.Show();
        }

        private void decryptButton_Click(object sender, RoutedEventArgs e)
        {
            Decryption_Window decryptWin = new Decryption_Window();
            decryptWin.updateActivityLog += updateLogList;
            decryptWin.Show();
        }

        private void keyExchangeButton_Click(object sender, RoutedEventArgs e)
        {
            KeyExchange_Window keyExWin = new KeyExchange_Window();
            keyExWin.updateActivityLog += updateLogList;
            keyExWin.Show();
        }

        private void SignatureButton_Click(object sender, RoutedEventArgs e)
        {
            DigitalSig_Window digSigWin = new DigitalSig_Window();
            digSigWin.updateActivityLog += updateLogList;
            digSigWin.Show();
        }

        private void VerifySigButton_Click(object sender, RoutedEventArgs e)
        {
            DigitalVer_Window digVerWin = new DigitalVer_Window();
            digVerWin.updateActivityLog += updateLogList;
            digVerWin.Show();
        }

        private void updateLogList(object sender, System.Windows.RoutedEventArgs e) {
            log = Utils.importLogActivity(logFile);
            LogDataGrid.ItemsSource = log;
            Debug.WriteLine(log.Count);
        }
    }
}
