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
        bool fileSelected, keySelected, isKeyImported, usePlainTextPw;
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
            isKeyImported = false;
            EncryptFileButton.IsEnabled = false;
            usePlainTextPw = false;
            k = new MxKey();
        }

        private void KeySizeDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            keySize = int.Parse(((ComboBoxItem)KeySizeDropDown.SelectedItem).Content.ToString());
        }

        private void EncryptSelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            fileToEncrypt = Utils.GetFileFromDialog();
            if (!fileToEncrypt.Equals("default"))
            {
                fileToEncryptTextBox.Text = fileToEncrypt;
                fileSelected = true;
            }

            if (keySelected && fileSelected)
            {
                EncryptFileButton.IsEnabled = true;
            }
            else
            {
                EncryptFileButton.IsEnabled = false;
            }
        }

        private void EncryptImportKeyButton_Click(object sender, RoutedEventArgs e)
        {
            string keyFile = Utils.GetFileFromDialog("key");

            bool KeyImportSuccess = k.ImportKeyFromFile(keyFile);

            if (KeyImportSuccess)
            {
                encryptionKeyTextBox.Text = k.KeyToHexString();
                keySelected = true;
                isKeyImported = true;

                if (keySelected && fileSelected)
                {
                    EncryptFileButton.IsEnabled = true;
                }
                else {
                    EncryptFileButton.IsEnabled = false;
                }
            }
            else {
                keySelected = false;
                encryptionKeyTextBox.Text = "";
                consoleTextBox_encrypt.Text = "Error importing key from " + keyFile;
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
            isKeyImported = false;

            if (keySelected && fileSelected)
            {
                EncryptFileButton.IsEnabled = true;
            }
            else
            {
                EncryptFileButton.IsEnabled = false;
            }

        }

        private void usePlainTextPwCheckBox_encrypt_Unchecked(object sender, RoutedEventArgs e)
        {
            EncryptImportKeyButton.IsEnabled = true;
            EncryptGenerateKeyButton.IsEnabled = true;
            encryptionKeyTextBox.Text = null;
            KeySizeDropDown.IsEnabled = true;
            isKeyImported = false;
            keySelected = false;
            usePlainTextPw = false;

            if (keySelected && fileSelected)
            {
                EncryptFileButton.IsEnabled = true;
            }
            else
            {
                EncryptFileButton.IsEnabled = false;
            }
        }

        private void usePlainTextPwCheckBox_encrypt_Checked(object sender, RoutedEventArgs e)
        {
            EncryptImportKeyButton.IsEnabled = false;
            EncryptGenerateKeyButton.IsEnabled = false;
            KeySizeDropDown.IsEnabled = false;
            encryptionKeyTextBox.Text = "<Enter encryption password here>";
            keySelected = true;
            usePlainTextPw = true;

            if (keySelected && fileSelected)
            {
                EncryptFileButton.IsEnabled = true;
            }
            else
            {
                EncryptFileButton.IsEnabled = false;
            }
        }

        private void EncryptFileButton_Click(object sender, RoutedEventArgs e)
        {
            string EncryptedFileName = fileToEncrypt + ".crypt";
            string EncryptionKeyFileName = fileToEncrypt + ".key";
            try {
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

                //If we are using a plain text password create the key now
                if (usePlainTextPw) {
                    byte[] hashRes265Bit = Utils.plaintextPwTo265bitKey(encryptionKeyTextBox.Text);

                    k.Key = hashRes265Bit;
                    k.KeyType = MxKeyType.AES;
                    k.IsPrivate = true;
                }

                cipher = System.Security.Cryptography.Aes.Create();
                cipher.Key = k.Key;

                File.Create(EncryptedFileName).Close();

                byte[] rawFileData = File.ReadAllBytes(fileToEncrypt);

                byte[] cipherData = cipher.EncryptCbc(rawFileData, IV, PaddingMode.PKCS7);

                File.WriteAllBytes(EncryptedFileName, cipherData);
                

                clearConsole();
                consoleTextBox_encrypt.Text = "Successfully Encrypted " + fileToEncrypt + " to " + EncryptedFileName;

                //If the user imported the key don't export another copy. Only export the key if they generated it
                if (!isKeyImported && !usePlainTextPw)
                {
                    k.ExportKeyToFile(EncryptionKeyFileName);
                    consoleTextBox_encrypt.AppendText("\nExported Key to " + EncryptionKeyFileName);
                }



            }
            catch(Exception ex)
            {
                if (File.Exists(EncryptedFileName))
                {
                    File.Delete(EncryptedFileName);
                }
                clearConsole();
                consoleTextBox_encrypt.Text = "Error encrypting " + fileToEncrypt + ". " + ex.ToString();
            }
        }

        public void clearConsole() {
            consoleTextBox_encrypt.Text = null;
        }
    }
}
