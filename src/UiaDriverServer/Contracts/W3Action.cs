using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UiaDriverServer.Contracts
{
    [DataContract]
    public class W3Action
    {
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public int Duration { get; set; }

        [DataMember]
        public int X { get; set; }

        [DataMember]
        public int Y { get; set; }

        [DataMember]
        public IDictionary<string, string> Origin { get; set; }
    }
}
