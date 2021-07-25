using System.Runtime.Serialization;

namespace UiaDriverServer.Dto
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
