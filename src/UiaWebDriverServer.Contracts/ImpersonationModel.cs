using System.Runtime.Serialization;

namespace UiaWebDriverServer.Contracts
{
    [DataContract]
    public class ImpersonationModel
    {
        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public bool Enabled { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string Username { get; set; }
    }
}
