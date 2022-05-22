using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Linq;

namespace Vault
{
    [DataContract]
    public class Store
    {
        private static string STORE_FILE = string.Format("{0}\\.git-credential-cert", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        [DataMember]
        internal List<SignedContainer> signedContainers = new List<SignedContainer>();

        public static Store Open()
        {
            if (!File.Exists(STORE_FILE))
            {
                var store = new Store();
                File.WriteAllText(STORE_FILE, store.ToJSON());
            }
            var json = File.ReadAllText(STORE_FILE);
            return FromJSON(json);
        }

        public class CredentialsAlreadyExistsException : Exception {
            public CredentialsAlreadyExistsException(string message) : base(message) { }
        }

        public void Add(string url, string username, string password)
        {
            {
                var sameContainers = signedContainers.Where(signedContainer => signedContainer.container.url == url);
                if (sameContainers.Count() > 0) {
                    throw new CredentialsAlreadyExistsException(string.Format("Credentials with {0} url already exist. Please, delete it before add new one", url));
                }
            }

            signedContainers.Add(new SignedContainer(url, username, password));
        }

        public void Remove(string url)
        {
            signedContainers = signedContainers.Where(signedContainer => signedContainer.container.url != url).ToList();
        }

        public void Save()
        {
            File.WriteAllText(STORE_FILE, ToJSON());
        }

        public (string protocol, string host, string path, string username, string password) GetCredentialsFor(string url)
        {
            UriBuilder uriBuilder = new UriBuilder(url);

            string username = "", password = "";

            var verifiedCredentialsContainers = signedContainers.Where(signedContainer => signedContainer.container.url == url).Where(credentialsContainer => credentialsContainer.SignatureIsCorrect()).ToList();

            if (verifiedCredentialsContainers.Count() > 0)
            {
                var verifiedCredentialsContainer = verifiedCredentialsContainers[0];

                password = verifiedCredentialsContainer.GetDecryptedPassword();
                username = verifiedCredentialsContainer.container.username;

            }

            if (username == "")
            {
                throw new Exception(string.Format("Credentials for {0} not found in .git-credential-cert store", url));
            }

            var path = uriBuilder.Path;
            if (path == "/") { path = ""; }

            return (uriBuilder.Scheme, uriBuilder.Host, path, username, password);
        }

        private string ToJSON()
        {
            MemoryStream stream = new MemoryStream();
            var serializer = new DataContractJsonSerializer(typeof(Store), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            serializer.WriteObject(stream, this);
            stream.Position = 0;
            StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        private static Store FromJSON(string json)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Store), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });

            Store store = (Store)serializer.ReadObject(stream);
            return store;
        }
    }
}
