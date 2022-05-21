using System.Runtime.Serialization;
using System;
using System.Windows.Forms;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Linq;


namespace Vault
{
    [DataContract]
    public class SignedContainer
    {
        [DataMember]
        internal Container container;

        [DataMember]
        internal string thumbprint;

        [DataMember]
        internal string signature;


        public SignedContainer(string url, string username, string password) {

            X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
            X509Certificate2Collection certsOnlyWithPrivateKey = new X509Certificate2Collection();
            foreach (X509Certificate2 cert in collection)
            {
                if (cert.HasPrivateKey)
                {
                    certsOnlyWithPrivateKey.Add(cert);
                }
            }
            X509Certificate2Collection fcollection = (X509Certificate2Collection)certsOnlyWithPrivateKey.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
            X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(
                fcollection,
                "Select certificate to protect your credentials",
                "Select a certificate from the following list", X509SelectionFlag.SingleSelection);

            if (scollection.Count == 0)
            {
                throw new Exception("to sign your credentials you have to choose appropriate certificate with private key");
            }

            var certificate = scollection[0];

            var encryptedPassword = Convert.ToBase64String(certificate.GetRSAPublicKey().Encrypt((new UTF8Encoding()).GetBytes(password), RSAEncryptionPadding.OaepSHA512));

            var csp = certificate.GetRSAPrivateKey();
            if (csp == null)
            {
                throw new Exception("no valid cert was found");
            }

            container = new Container(url, username, encryptedPassword);

            this.signature = Convert.ToBase64String(csp.SignData((new UTF8Encoding()).GetBytes(container.ToJSON()), HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1));

            this.thumbprint = certificate.Thumbprint;

        }

        internal string ToJSON()
        {
            MemoryStream stream = new MemoryStream();
            var serializer = new DataContractJsonSerializer(typeof(SignedContainer), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            serializer.WriteObject(stream, this);
            stream.Position = 0;
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}
