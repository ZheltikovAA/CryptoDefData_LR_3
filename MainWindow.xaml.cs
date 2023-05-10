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
using System.Security.Cryptography;
using System.Security.Permissions;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Ink;
using Microsoft.Win32;

namespace CryptoDefData_LR_3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private X509Store CertificateStore;
        private X509Certificate2 selectedCertificate;
        private SignDocument signDocument;
        public MainWindow()
        {
            InitializeComponent();
            ChangeFlag(false);
            UserName.IsEnabled = false;
            CertificateStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        }

        public void ChangeFlag(bool flag)
        {
            CertDepartamentMenu.IsEnabled =
            SaveDoc.IsEnabled =
            SaveDocMenu.IsEnabled =
            TextSquare.IsEnabled =
            CreateDocMenu.IsEnabled =
            InpDocMenu.IsEnabled =
            InputDoc.IsEnabled = flag;
        }
        private void ChouseCertClick(object sender, RoutedEventArgs e)
        {
            ChangeFlag(false);
            try
            {
                
                CertificateStore.Open(OpenFlags.ReadOnly | OpenFlags.IncludeArchived);
                X509Certificate2Collection selectedCertificates = X509Certificate2UI.SelectFromCollection(CertificateStore.Certificates, "Выберите сертификат", "Выберите сертификат из списка ниже:", X509SelectionFlag.SingleSelection);
                if (selectedCertificates.Count > 0)
                {
                    selectedCertificate = selectedCertificates[0];
                    MessageBox.Show($"Выбран сертификат:\n\nSubject: {selectedCertificate.Subject}");
                    ChangeFlag(true);
                    UserName.Text = selectedCertificate.Subject.ToString();
                }
                else
                {
                    MessageBox.Show("Сертификат не выбран.");
                }
                CertificateStore.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе сертификата: {ex.Message}");
            }
        }

        private void InputDocMenuClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == false) return;
            
            SignDocument document;

            try 
            {
                document = new SignDocument(dlg.FileName);
            }catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке документа: {ex.Message}");
                return;
            }

            //if (document.VerifyChain()) 
            //{
                if (document.VerifySign()) 
                {
                    signDocument = document;
                    this.Title = "Подписано: " + signDocument.certificate.Subject;
                    TextRange lenContent = new TextRange(TextSquare.Document.ContentStart, TextSquare.Document.ContentEnd);
                    lenContent.Text = signDocument.contentDoc;
                } else { MessageBox.Show("Ошибка при проверке целостности документа"); return; }

            //}
            //else { MessageBox.Show("Ошибка при проверке цепочки сертификации"); return; }
        }

        private void SaveDocMenuClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.FileName = "Message";
            if (save.ShowDialog() == false) return;
            TextRange lenContent = new TextRange(TextSquare.Document.ContentStart, TextSquare.Document.ContentEnd);
            signDocument = new SignDocument(selectedCertificate, lenContent.Text);
            signDocument.SaveInFile(save.FileName);
            this.Title = "Подписанный документ:" + signDocument.certificate.Subject;
        }

        private void CreateDocMenuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextSquare.Document.Blocks.Clear();
                TextSquare.Focus();
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Ошибка при создании нового документа: {ex.Message}");
            }
        }

        private void ExitMenuClick(object sender, RoutedEventArgs e)
        {
            CertificateStore.Close(); 
            Close();
        }

        private void AboutMenuClick(object sender, RoutedEventArgs e)
        {
            new AboutBox().Show();
        }

        private void DeleteCertificate(object sender, RoutedEventArgs e)
        {
            try 
            {
                CertificateStore.Open(OpenFlags.ReadWrite | OpenFlags.IncludeArchived);

                CertificateStore.Remove(selectedCertificate);
                CertificateStore.Close();
                MessageBox.Show("Сертификат успешно удален.");
                selectedCertificate = null;
                UserName.Text = "";
                ChangeFlag(false);
            }
            catch(Exception ex) 
            {
                MessageBox.Show($"Ошибка при удалении сертификата: {ex.Message}");
            }
        }
        

    }
}
