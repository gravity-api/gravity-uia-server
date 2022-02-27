using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UiaDriverServer.Contracts
{
    [DataContract]
    public class W3Actions
    {
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public IEnumerable<W3Action> Actions { get; set; }

        [DataMember]
        public IDictionary<string, object> Parameters { get; set; }
    }
}
