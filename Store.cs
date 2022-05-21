using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

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

        public void Add(SignedContainer signedContainer) {
            signedContainers.Add(signedContainer);
        }

        internal void Save()
        {
            File.WriteAllText(STORE_FILE, ToJSON());
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
