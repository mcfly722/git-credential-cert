using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Vault
{
    [DataContract]
    internal class Container
    {
        [DataMember]
        internal string url;

        [DataMember]
        internal string username;

        [DataMember]
        internal string password;

        [DataMember]
        internal string certificateSubject;

        [DataMember]
        internal DateTime created;

        internal Container(UriBuilder url, string username, string password, string certificateSubject) {
            this.url = url.ToString();
            this.username = username;
            this.password = password;
            this.certificateSubject = certificateSubject;
            this.created = DateTime.Now;
        }

        internal string ToJSON()
        {
            MemoryStream stream = new MemoryStream();
            var serializer = new DataContractJsonSerializer(typeof(Container), new DataContractJsonSerializerSettings()
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
