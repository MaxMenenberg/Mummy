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
using System.Windows.Forms;
using System.Reflection.Metadata;

namespace Mummy
{
    /// <summary>
    /// Interaction logic for DigitalSig_Window.xaml
    /// </summary>
    public partial class DigitalSig_Window : Window
    {

        string privateSigningKeyFile = "PrivateSigningKey.key";
        bool signingKeyExists, fileToSignSelected;
        string fileToSign;
        MxKey signingKey, verificationKey;

        public event RoutedEventHandler updateActivityLog;

        public DigitalSig_Window()
        {
            InitializeComponent();
            signingKeyExists = false;
            fileToSignSelected = false;
            SignFileButton.IsEnabled = false;
            GenerateVerKeyButton.IsEnabled = false;
            signingKey = new MxKey();
            verificationKey = new MxKey();
            checkForSigningKey();
        }

        private void checkForSigningKey() {
            if (File.Exists(privateSigningKeyFile))
            {
                signingKey.ImportKeyFromFile(privateSigningKeyFile);
                signingKeyExists = true;
                signingKeyTextBox.Text = Convert.ToHexString(signingKey.Key);
                GenerateVerKeyButton.IsEnabled = true;
                consoleTextBox_DigSig.Text = "Successfully loaded private signing key";
            }
            else {
                CngKey PrivSignKey = CngKey.Create(CngAlgorithm.ECDsaP256, "KeySign",
                   new CngKeyCreationParameters {
                       ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                       KeyUsage = CngKeyUsages.AllUsages,
                       UIPolicy = new CngUIPolicy(CngUIProtectionLevels.None),
                       KeyCreationOptions = CngKeyCreationOptions.OverwriteExistingKey});

                byte[] signingKeyBytes = PrivSignKey.Export(CngKeyBlobFormat.EccPrivateBlob);
                signingKey.Key = signingKeyBytes;
                signingKey.IsPrivate = true;
                signingKey.KeyType = MxKeyType.ECDSA;
                signingKey.ExportKeyToFile(privateSigningKeyFile);
                consoleTextBox_DigSig.Text = "Could not find a private signing key and created new one.";
                signingKeyTextBox.Text = Convert.ToHexString(signingKey.Key);
                signingKeyExists = true;
                GenerateVerKeyButton.IsEnabled = true;
            }
        }

        private void UpdateSigButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("If you update your private" +
                " signing key all verifiers will no longer be able to verify your signatures without updating their " +
                "verification keys. Are you sure you want to continue?", "Update Private Signing Key?", MessageBoxButtons.YesNo);

            if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                File.Delete(privateSigningKeyFile);
                CngKey PrivSignKey = CngKey.Create(CngAlgorithm.ECDsaP256, "KeySign",
                   new CngKeyCreationParameters
                   {
                       ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                       KeyUsage = CngKeyUsages.AllUsages,
                       UIPolicy = new CngUIPolicy(CngUIProtectionLevels.None),
                       KeyCreationOptions = CngKeyCreationOptions.OverwriteExistingKey
                   });

                byte[] signingKeyBytes = PrivSignKey.Export(CngKeyBlobFormat.EccPrivateBlob);
                signingKey.Key = signingKeyBytes;
                signingKey.IsPrivate = true;
                signingKey.KeyType = MxKeyType.ECDSA;
                signingKey.ExportKeyToFile(privateSigningKeyFile);
                clearConsole();
                consoleTextBox_DigSig.Text = "Deleted old signing key and successfully created new signing key!";
                signingKeyTextBox.Text = Convert.ToHexString(signingKey.Key);
                signingKeyExists = true;
            }
        }

        private void DigSigSelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            fileToSign = Utils.GetFileFromDialog();
            if (!fileToSign.Equals("default"))
            {
                fileToSignTextBox.Text = fileToSign;
                fileToSignSelected = true;
            }

            if (signingKeyExists && fileToSignSelected)
            {
                SignFileButton.IsEnabled = true;
            }
            else
            {
                SignFileButton.IsEnabled = false;
            }
        }

        private void SignFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] document = File.ReadAllBytes(fileToSign);
                ECDsaCng dsaSign = new ECDsaCng(CngKey.Import(signingKey.Key, CngKeyBlobFormat.EccPrivateBlob));
                dsaSign.HashAlgorithm = CngAlgorithm.Sha256;
                byte[] signature = dsaSign.SignData(SHA256.HashData(document));

                MxKey sig = new MxKey();
                sig.Key = signature;
                sig.IsPrivate = false;
                sig.KeyType = MxKeyType.ECDSA;

                //Save the signature to a file
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Digital Signature"; // Default file name
                dlg.DefaultExt = ".sig"; // Default file extension
                dlg.Filter = "Digital Signature (.sig)|*.sig"; // Filter files by extension

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    string digSigFileName = dlg.FileName;
                    sig.ExportKeyToFile(digSigFileName);

                    RecentActivityLogEntry logEntry = new RecentActivityLogEntry();
                    logEntry.action = "DigitalSignature";
                    logEntry.time = DateTime.Now;
                    logEntry.input = fileToSign;
                    logEntry.ouput = digSigFileName;
                    Utils.writeEntryToLog(logEntry, true);
                    updateActivityLog(this, null);

                    clearConsole();
                    consoleTextBox_DigSig.Text = "Successfully saved signature to " + digSigFileName;
                }
            }
            catch(Exception ex)
            {
                clearConsole();
                consoleTextBox_DigSig.Text = "Could not sign " + fileToSign;
                consoleTextBox_DigSig.AppendText("\n\n" + ex.StackTrace);
            }
        }

        private void GenerateVerKeyButton_Click(object sender, RoutedEventArgs e)
        {

            ECDsaCng dsaSign = new ECDsaCng(CngKey.Import(signingKey.Key, CngKeyBlobFormat.EccPrivateBlob));
            byte[] Key_Ver = dsaSign.Key.Export(CngKeyBlobFormat.EccPublicBlob);

            verificationKey.Key = Key_Ver;
            verificationKey.IsPrivate = false;
            verificationKey.KeyType = MxKeyType.ECDSA;

            //Save the verification key to a file
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "VerificationKey"; // Default file name
            dlg.DefaultExt = ".ver"; // Default file extension
            dlg.Filter = "Signature Verification Key (.ver)|*.ver"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string digSigFileName = dlg.FileName;
                verificationKey.ExportKeyToFile(digSigFileName);

                clearConsole();
                consoleTextBox_DigSig.Text = "Successfully saved verification key to to " + digSigFileName;
            }
        }

        private void clearConsole()
        {
            consoleTextBox_DigSig.Text = null;
        }
    }
}
