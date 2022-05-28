using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Linq;

namespace Vault
{
    public class CredentialsAlreadyExistsException : Exception
    {
        public CredentialsAlreadyExistsException(string message) : base(message) { }
    }

    public class CouldNotFoundCredentialsException : Exception
    {
        public CouldNotFoundCredentialsException(string message) : base(message) { }
    }

    public class SignatureIsIncorrectException : Exception
    {
        public SignatureIsIncorrectException(string message) : base(message) { }
    }

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

        public List<SignedContainer> GetList()
        {
            return signedContainers;
        }

        public Boolean Exist(UriBuilder url)
        {
            return signedContainers.Where(signedContainer => new UriBuilder(signedContainer.container.url).ToString() == url.ToString()).Count() > 0;
        }

        public void Add(UriBuilder url, string username, string password)
        {
            if (Exist(url))
            {
                throw new CredentialsAlreadyExistsException(string.Format("Credentials with {0} url already exist. Please, delete it before add new one", url));
            }

            signedContainers.Add(new SignedContainer(url, username, password));
        }

        public void Remove(UriBuilder url)
        {
            if (!Exist(url)) {
                throw new CouldNotFoundCredentialsException(string.Format("could not found credentials for url={0}", url.ToString()));
            }

            signedContainers = signedContainers.Where(signedContainer => new UriBuilder(signedContainer.container.url).ToString() != url.ToString()).ToList();
        }

        public void Save()
        {
            File.WriteAllText(STORE_FILE, ToJSON());
        }

        public (string protocol, string host, string path, string username, string password) GetCredentialsFor(string url)
        {
            UriBuilder uriBuilder = new UriBuilder(url);

            string username = "", password = "";

            var credentialsContainers = signedContainers.Where(signedContainer => signedContainer.container.url == url).ToList();

            if (credentialsContainers.Count() > 0)
            {
                var credentialsContainer = credentialsContainers[0];
                if (!credentialsContainer.SignatureIsCorrect())
                {
                    throw new SignatureIsIncorrectException(string.Format("signature for url={0} credentials is incorrect", url));
                }
                else
                {
                    var verifiedCredentialsContainer = credentialsContainer;
                    password = verifiedCredentialsContainer.GetDecryptedPassword();
                    username = verifiedCredentialsContainer.container.username;
                }
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
