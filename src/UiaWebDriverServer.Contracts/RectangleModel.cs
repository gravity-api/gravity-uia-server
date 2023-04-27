using System.Runtime.Serialization;

namespace UiaWebDriverServer.Contracts
{
    [DataContract]
    public class RectangleModel
    {
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public int X { get; set; }

        [DataMember]
        public int Y { get; set; }
    }
}
