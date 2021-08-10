using System.Runtime.Serialization;

namespace UiaDriverServer.Contracts
{
    [DataContract]
    internal class Location
    {
        [DataMember]
        public int Top { get; set; }
        
        [DataMember]
        public int Left { get; set; }
    }
}
