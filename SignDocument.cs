using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CryptoDefData_LR_3
{
    internal class SignDocument
    {
        public X509Certificate2 certificate;
        public byte[] esign;
        public string contentDoc;

        public SignDocument()
        {
            esign = new byte[] { };
            certificate = null;
            contentDoc = "";
        }

        public SignDocument(string fileName) 
        {
            try
            {
                using (BinaryReader br = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read))) 
                {
                    int certificateLength, esignLength;
                    
                    certificateLength = br.ReadInt32();
                    esignLength = br.ReadInt32();

                    byte[] cert = br.ReadBytes(certificateLength);
                    certificate = new X509Certificate2(cert);

                    byte[] es = br.ReadBytes(esignLength);
                    esign = es;

                    using (StreamReader sr = new StreamReader(br.BaseStream, Encoding.UTF8)) { contentDoc = sr.ReadToEnd(); }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка файла: {ex.Message}");
            }
        
        }
        public SignDocument(X509Certificate2 cert, string text)
        {
            contentDoc = text;
            certificate = cert;
            byte[] txt = Encoding.UTF8.GetBytes(text);
            byte[] hash = new SHA1Managed().ComputeHash(txt);

            var dsacsp = (DSACryptoServiceProvider)cert.PrivateKey;

            esign = dsacsp.SignHash(hash, "SHA1");
        }

        public bool VerifyChain()
        {
            X509Chain chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            if (!chain.Build(certificate)) { return false; }
            return true;
        }
    
        public bool VerifySign()
        {
            byte[] hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(contentDoc));
            var dsacsp = (DSACryptoServiceProvider)certificate.PublicKey.Key;

            if(!dsacsp.VerifyHash(hash, "SHA1", esign)) { return false; }
            return true;
        }
        public void SaveInFile(string fileName)
        {
            try
            {
                using (BinaryWriter bw = new BinaryWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write)))
                {

                    bw.Write(certificate.GetRawCertData().Length);
                    bw.Write(esign.Length);
                    bw.Write(certificate.GetRawCertData());
                    bw.Write(esign);
                    bw.Write(Encoding.UTF8.GetBytes(contentDoc));
                }
            } catch(Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}");
            }
        }
    }
}
