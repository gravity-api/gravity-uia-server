using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UiaDriverServer.Contracts
{
    [DataContract]
    public class W3ActionsContract
    {
        [DataMember]
        public IEnumerable<W3Actions> Actions { get; set; }
    }
}
