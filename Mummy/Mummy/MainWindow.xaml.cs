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

namespace Mummy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void encryptButton_Click(object sender, RoutedEventArgs e)
        {
            Encryption_Window encryptWin = new Encryption_Window();
            encryptWin.Show();
        }

        private void decryptButton_Click(object sender, RoutedEventArgs e)
        {
            Decryption_Window decryptWin = new Decryption_Window();
            decryptWin.Show();
        }

        private void keyExchangeButton_Click(object sender, RoutedEventArgs e)
        {
            KeyExchange_Window keyExWin = new KeyExchange_Window();
            keyExWin.Show();
        }

        private void SignatureButton_Click(object sender, RoutedEventArgs e)
        {
            DigitalSig_Window digSigWin = new DigitalSig_Window();
            digSigWin.Show();
        }

        private void VerifySigButton_Click(object sender, RoutedEventArgs e)
        {
            DigitalVer_Window digVerWin = new DigitalVer_Window();
            digVerWin.Show();
        }
    }
}
