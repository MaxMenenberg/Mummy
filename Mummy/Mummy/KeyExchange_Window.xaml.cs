using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace Mummy
{
    /// <summary>
    /// Interaction logic for KeyExchage_Window.xaml
    /// </summary>
    public partial class KeyExchange_Window : Window
    {

        MxKey PrivA, PubA, PubB, ShrdScrt;
        bool keyALoaded, keyBLoaded;

        public KeyExchange_Window()
        {
            InitializeComponent();
            keyALoaded = false;
            ComputeSharedKey_Button.IsEnabled = false;
            PrivA = new MxKey();
            PubA = new MxKey();
            PubB = new MxKey();
            ShrdScrt = new MxKey();
        }

        private void PubB_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PubB_TextBox.Text.Length == 144)
            {
                keyBLoaded = true;
                string PubBHexString = PubB_TextBox.Text;
                PubB.Key = Utils.StringToByteArray(PubBHexString);
                PubB.IsPrivate = false;
                PubB.KeyType = MxKeyType.ECDH;
            }
            else {
                keyBLoaded = false;
            }
            if (keyALoaded && keyBLoaded)
            {
                ComputeSharedKey_Button.IsEnabled = true;
            }
            else {
                ComputeSharedKey_Button.IsEnabled = false;
            }
        }


        private void GenPrivPubKey_Button_Click(object sender, RoutedEventArgs e)
        {
            CngKey KeyA = CngKey.Create(CngAlgorithm.ECDiffieHellmanP256, "KeyA",
             new CngKeyCreationParameters
             {
                 ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                 KeyUsage = CngKeyUsages.AllUsages,
                 UIPolicy = new CngUIPolicy(CngUIProtectionLevels.None),
                 KeyCreationOptions = CngKeyCreationOptions.OverwriteExistingKey

             }
             );

            byte[] KeyA_PrivateBytes = KeyA.Export(CngKeyBlobFormat.EccPrivateBlob);
            byte[] KeyA_PublicBytes = KeyA.Export(CngKeyBlobFormat.EccPublicBlob);

           
            PrivA.Key = KeyA_PrivateBytes;
            PrivA.IsPrivate = true;
            PrivA.KeyType = MxKeyType.ECDH;

            PubA.Key = KeyA_PublicBytes;
            PubA.IsPrivate = false;
            PubA.KeyType = MxKeyType.ECDH;

            PrivA_TextBox.Text = Convert.ToHexString(KeyA_PrivateBytes);
            PubA_TextBox.Text = Convert.ToHexString(KeyA_PublicBytes);

            keyALoaded = true;

            if (keyALoaded && keyBLoaded)
            {
                ComputeSharedKey_Button.IsEnabled = true;
            }
            else
            {
                ComputeSharedKey_Button.IsEnabled = false;
            }

            consoleTextBox_keyEx.Text = "Enter other party's public key in Pulic B field. Public B must be a 144 hex character string";
        }

        private void ComputeSharedKey_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ECDiffieHellmanCng ecdhCng = new ECDiffieHellmanCng(CngKey.Import(PrivA.Key, CngKeyBlobFormat.EccPrivateBlob));
                ECDiffieHellmanCng ecdhCngB = new ECDiffieHellmanCng(CngKey.Import(PubB.Key, CngKeyBlobFormat.EccPublicBlob));

                byte[] ShrdScrtBytes = ecdhCng.DeriveKeyFromHash(ecdhCngB.PublicKey, new HashAlgorithmName("SHA512"));


                //The shared secret key is too large for AES and must be shrunk to 32 bytes
                byte[] ShrdSecrtAES = new byte[32];
                for (int n = 0; n < ShrdSecrtAES.Length; n++) {
                    ShrdSecrtAES[n] = ShrdScrtBytes[n];
                }

                ShrdScrt.Key = ShrdSecrtAES;
                ShrdScrt.IsPrivate = true;
                ShrdScrt.KeyType = MxKeyType.AES;

                //Save the final shared key to a file
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "SharedSecretAESKey"; // Default file name
                dlg.DefaultExt = ".key"; // Default file extension
                dlg.Filter = "Cryptography Keys (.key)|*.key"; // Filter files by extension

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    string shrdScrtKeyFileName = dlg.FileName;
                    ShrdScrt.ExportKeyToFile(shrdScrtKeyFileName);

                    //Put the hash of the shared secret byte on the screen so both users are confident their final keys are identical
                    SHA512 hashFunc = SHA512.Create();
                    byte[] hashResults = hashFunc.ComputeHash(ShrdScrtBytes);
                    SsHash_TextBox.Text = Convert.ToHexString(hashResults);

                    clearConsole();
                    consoleTextBox_keyEx.Text = "Successfully saved shared secret key to " + shrdScrtKeyFileName;
                }

              
            }
            catch {
                clearConsole();
                consoleTextBox_keyEx.Text = "Error generating shared secret key";
            }
        }

        private void clearConsole() {
            consoleTextBox_keyEx.Text = null;
        }
            

    }
}
