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
using System.Security.Cryptography.Xml;

namespace Mummy
{
    /// <summary>
    /// Interaction logic for Decryption_Window.xaml
    /// </summary>
    public partial class Decryption_Window : Window
    {

        private string fileToDecrypt;
        bool fileSelected, keySelected;
        private System.Security.Cryptography.Aes cipher;
        private MxKey k;
        private byte[] IV = new byte[16];
        private string expectedFileExtension = ".crypt";

        public Decryption_Window()
        {
            InitializeComponent();
            fileToDecrypt = "";
            fileSelected = false;
            keySelected = false;
            DecryptFileButton.IsEnabled = false;
            k = new MxKey();
        }

        private void DecryptSelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            fileToDecrypt = Utils.GetFileFromDialog();
            
            string fileExtention = fileToDecrypt.Substring(fileToDecrypt.Length - 6, 6);
            if (fileExtention.Equals(expectedFileExtension))
            {
                fileToDecryptTextBox.Text = fileToDecrypt;
                fileSelected = true;
            }
            else if (fileToDecrypt.Equals("default")) {
                //The user x'd out of the window
                return;
            }
            else
            {
                consoleTextBox_decrypt.Text = "Files to be decrypted must end in .crypt";
            }

            if (keySelected && fileSelected)
            {
                DecryptFileButton.IsEnabled = true;
            }
            else
            {
                DecryptFileButton.IsEnabled = true;
            }
        }

        private void DecryptImportKeyButton_Click(object sender, RoutedEventArgs e)
        {
            string keyFile = Utils.GetFileFromDialog();

            bool KeyImportSuccess = k.ImportKeyFromFile(keyFile);

            if (KeyImportSuccess && k.KeyType == MxKeyType.AES)
            {
                decryptionKeyTextBox.Text = k.KeyToHexString();
                keySelected = true;

                if (keySelected && fileSelected)
                {
                    DecryptFileButton.IsEnabled = true;
                }
                else
                {
                    DecryptFileButton.IsEnabled = false;
                }
            }
            else
            {
                keySelected = false;
                decryptionKeyTextBox.Text = "";
                clearConsole();
                if (k.KeyType != MxKeyType.AES)
                {
                    consoleTextBox_decrypt.Text = keyFile + " is not a valid AES Decryption Key";
                }
                else
                {
                    consoleTextBox_decrypt.Text = "Error importing key from " + keyFile;
                }
            }

        }

        private void DecryptFileButton_Click(object sender, RoutedEventArgs e)
        {
            string decryptedFileName = fileToDecrypt.Substring(0, fileToDecrypt.Length - 6);
            try
            {
                File.Create(decryptedFileName).Close();

                cipher = System.Security.Cryptography.Aes.Create();
                cipher.Key = k.Key;

                byte[] rawCipherData = File.ReadAllBytes(fileToDecrypt);

                byte[] decryptedFileData = cipher.DecryptCbc(rawCipherData, IV, PaddingMode.PKCS7);

                File.WriteAllBytes(decryptedFileName, decryptedFileData);

                clearConsole();
                consoleTextBox_decrypt.Text = "Successfully Decrypted " + fileToDecrypt + " to " + decryptedFileName;
            }
            catch {
                File.Delete(decryptedFileName);
                clearConsole();
                consoleTextBox_decrypt.Text = "Could not decrypt " + fileToDecrypt;
                consoleTextBox_decrypt.AppendText("\nMake sure the file/key pair is valid");
            }
        }

        public void clearConsole()
        {
            consoleTextBox_decrypt.Text = null;
        }

    }
}
