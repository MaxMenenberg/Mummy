using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.IO;

namespace Mummy
{
    /// <summary>
    /// Interaction logic for Encryption_Window.xaml
    /// </summary>
    public partial class Encryption_Window : Window
    {

        private string fileToEncrypt;
        private int keySize;
        bool fileSelected, keySelected;
        private System.Security.Cryptography.Aes cipher;
        private MxKey k;
        private byte[] IV = new byte[16];

        public Encryption_Window()
        {
            InitializeComponent();
            keySize = int.Parse(((ComboBoxItem)KeySizeDropDown.SelectedItem).Content.ToString());
            fileToEncrypt = "";
            fileSelected = false;
            keySelected = false;
            EncryptFileButton.IsEnabled = false;
            k = new MxKey();
        }

        private void KeySizeDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            keySize = int.Parse(((ComboBoxItem)KeySizeDropDown.SelectedItem).Content.ToString());
        }

        private void EncryptImportKeyButton_Click(object sender, RoutedEventArgs e)
        {
            string keyFile = Utils.GetFileFromDialog();

            bool KeyImportSuccess = k.ImportKeyFromFile(keyFile);

            if (KeyImportSuccess)
            {
                encryptionKeyTextBox.Text = k.KeyToHexString();
                keySelected = true;

                if (keySelected && fileSelected)
                {
                    EncryptFileButton.IsEnabled = true;
                }
                else {
                    EncryptFileButton.IsEnabled = true;
                }
            }
            else {
                keySelected = false;
                encryptionKeyTextBox.Text = "";
                consoleTextBox.Text = "Error importing key from " + keyFile;
            }


        }

        private void EncryptGenerateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            cipher = System.Security.Cryptography.Aes.Create();
            cipher.KeySize = keySize;            
            k.Key = cipher.Key;
            k.KeyType = MxKeyType.AES;
            k.IsPrivate = true;

            encryptionKeyTextBox.Text = k.KeyToHexString();
            keySelected = true;

            if (keySelected && fileSelected)
            {
                EncryptFileButton.IsEnabled = true;
            }
            else
            {
                EncryptFileButton.IsEnabled = true;
            }

        }

        private void EncryptFileButton_Click(object sender, RoutedEventArgs e)
        {
            try {
                string EncryptedFileName = fileToEncrypt + ".crypt";
                string EncryptionKeyFileName = fileToEncrypt + ".key";

                if (File.Exists(EncryptedFileName))
                {
                    System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show(EncryptedFileName + " already exists, do you want to overwrite it?", "", MessageBoxButtons.YesNo);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        File.Delete(EncryptedFileName);

                        if (File.Exists(EncryptionKeyFileName))
                        {
                            File.Delete(EncryptionKeyFileName);
                        }

                    }
                    else if (dr == System.Windows.Forms.DialogResult.No)
                    {
                        //Don't do anything
                        clearConsole();
                        return;
                    }
                }

                File.Create(EncryptedFileName).Close();

                byte[] rawFileData = File.ReadAllBytes(fileToEncrypt);

                byte[] cipherData = cipher.EncryptCbc(rawFileData, IV, PaddingMode.PKCS7);

                File.WriteAllBytes(EncryptedFileName, cipherData);

                k.ExportKeyToFile(EncryptionKeyFileName);

                clearConsole();
                consoleTextBox.Text = "Successfully Encrypted " + fileToEncrypt + " to " + EncryptedFileName;
                consoleTextBox.AppendText("\nExported Key to " + EncryptionKeyFileName);

                

            }
            catch(Exception ex)
            {
                clearConsole();
                consoleTextBox.Text = "Error encrypting " + fileToEncrypt + ". " + ex.ToString();
            }
        }

        private void EncryptSelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            fileToEncrypt = Utils.GetFileFromDialog();
            fileToEncryptTextBox.Text = fileToEncrypt;
            fileSelected = true;

            if (keySelected && fileSelected)
            {
                EncryptFileButton.IsEnabled = true;
            }
            else
            {
                EncryptFileButton.IsEnabled = true;
            }
        }

        public void clearConsole() {
            consoleTextBox.Text = null;
        }
    }
}
