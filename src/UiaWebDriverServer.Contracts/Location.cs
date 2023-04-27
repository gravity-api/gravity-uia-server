using System.Runtime.Serialization;

namespace UiaWebDriverServer.Contracts
{
    [DataContract]
    public class Location
    {
        [DataMember]
        public int Top { get; set; }

        [DataMember]
        public int Left { get; set; }

        [DataMember]
        public int Right { get; set; }

        [DataMember]
        public int Bottom { get; set; }
    }
}
