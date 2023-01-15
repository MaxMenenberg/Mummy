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
using System.Windows.Shapes;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Security.Cryptography.Xml;

namespace Mummy
{
    /// <summary>
    /// Interaction logic for DigitalVer_Window.xaml
    /// </summary>
    public partial class DigitalVer_Window : Window
    {
        bool fileToVerifyLoaded, signatureLoaded, verKeyLoaded;
        MxKey digSig, verKey;
        string fileToVerify, digSigFile, verKeyFile;


        public DigitalVer_Window()
        {
            InitializeComponent();
            fileToVerifyLoaded = false;
            signatureLoaded = false;
            verKeyLoaded = false;
            digSig = new MxKey();
            verKey = new MxKey();
            CheckSigButton.IsEnabled = false;
        }

        private void DigVerSelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            fileToVerify = Utils.GetFileFromDialog();
            if (!fileToVerify.Equals("default"))
            {
                fileToVerTextBox.Text = fileToVerify;
                fileToVerifyLoaded = true;
            }

            if (fileToVerifyLoaded && signatureLoaded && verKeyLoaded)
            {
                CheckSigButton.IsEnabled = true;
            }
            else
            {
                CheckSigButton.IsEnabled = false;
            }
        }

        private void DigSigSelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            digSigFile = Utils.GetFileFromDialog("sig");
            if (!digSigFile.Equals("default"))
            {
                digSig.ImportKeyFromFile(digSigFile);
                digSigTextBox.Text = Convert.ToHexString(digSig.Key);
                signatureLoaded = true;
            }

            if (fileToVerifyLoaded && signatureLoaded && verKeyLoaded)
            {
                CheckSigButton.IsEnabled = true;
            }
            else
            {
                CheckSigButton.IsEnabled = false;
            }
        }

        private void DigVerKeySelectButton_Click(object sender, RoutedEventArgs e)
        {
            verKeyFile = Utils.GetFileFromDialog("ver");
            if (!verKeyFile.Equals("default"))
            {
                verKey.ImportKeyFromFile(verKeyFile);
                verKeyTextBox.Text = Convert.ToHexString(verKey.Key);
                verKeyLoaded = true;
            }

            if (fileToVerifyLoaded && signatureLoaded && verKeyLoaded)
            {
                CheckSigButton.IsEnabled = true;
            }
            else
            {
                CheckSigButton.IsEnabled = false;
            }
        }

        private void CheckSigButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(fileToVerify);
                ECDsaCng dsaVer = new ECDsaCng(CngKey.Import(verKey.Key, CngKeyBlobFormat.EccPublicBlob));

                if (dsaVer.VerifyData(SHA256.HashData(fileBytes), digSig.Key))
                {
                    consoleTextBox_VerSig.Text = "Signature Verification: PASS";
                }
                else
                {
                    consoleTextBox_VerSig.Text = "Signature Verification: FAIL";
                }


            }
            catch (Exception ex)
            {
                consoleTextBox_VerSig.Text = "Error verifying digital signature";
                consoleTextBox_VerSig.AppendText("\n\n" + ex.StackTrace);
            }
        }

        private void clearConsole()
        {
            consoleTextBox_VerSig.Text = null;
        }

    }
}
