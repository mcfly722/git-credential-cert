using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Runtime.Serialization.Json;


namespace Vault
{
    public class NoCertWithPrivateKeyException : Exception
    {
        public NoCertWithPrivateKeyException(string message) : base(message) { }
    }

    public class NoCertFoundException : Exception
    {
        public NoCertFoundException(string message) : base(message) { }
    }

    [DataContract]
    public class SignedContainer
    {
        [DataMember]
        internal Container container;

        [DataMember]
        internal string thumbprint;

        [DataMember]
        internal string signature;

        public UriBuilder GetURL()
        {
            return new UriBuilder(container.url);
        }

        public string GetUserName()
        {
            return container.username;
        }

        public string GetCertificateThumbprint()
        {
            return thumbprint;
        }

        public string GetCertificateSubject()
        {
            return container.certificateSubject;
        }

        public DateTime GetCreatedDateTime() {
            return container.created;
        }

        public SignedContainer(UriBuilder url, string username, string password)
        {

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
                string.Format("Select certificate to protect your credentials for {0}", url),
                "Select a certificate from the following list", X509SelectionFlag.SingleSelection);

            if (scollection.Count == 0)
            {
                throw new NoCertWithPrivateKeyException("to sign, encrypt and store your credentials you have to choose appropriate certificate with private key");
            }

            var certificate = scollection[0];

            var encryptedPassword = Convert.ToBase64String(certificate.GetRSAPublicKey().Encrypt((new UTF8Encoding()).GetBytes(password), RSAEncryptionPadding.OaepSHA512));

            var csp = certificate.GetRSAPrivateKey();
            if (csp == null)
            {
                throw new NoCertFoundException("no valid cert was found");
            }

            container = new Container(url, username, encryptedPassword, certificate.Subject);

            this.signature = Convert.ToBase64String(csp.SignData((new UTF8Encoding()).GetBytes(container.ToJSON()), HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1));

            this.thumbprint = certificate.Thumbprint;

        }

        internal X509Certificate2 getCertificate(string thumbprint)
        {
            X509Store certsStore = new X509Store("MY", StoreLocation.CurrentUser);
            certsStore.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection collection = (X509Certificate2Collection)certsStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (collection.Count < 1)
            {
                throw new NoCertFoundException(string.Format("Could not found certificate {0}({1}) to check container signature", container.certificateSubject, thumbprint));
            }
            return collection[0];
        }

        internal bool SignatureIsCorrect()
        {
            return getCertificate(thumbprint).GetRSAPublicKey().VerifyData((new UTF8Encoding()).GetBytes(container.ToJSON()), System.Convert.FromBase64String(signature), HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }

        internal string GetDecryptedPassword()
        {
            return System.Text.Encoding.UTF8.GetString(
                getCertificate(thumbprint).GetRSAPrivateKey().Decrypt(System.Convert.FromBase64String(container.password), RSAEncryptionPadding.OaepSHA512)
            );
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
