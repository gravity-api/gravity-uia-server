﻿using System.Runtime.Serialization;

namespace UiaWebDriverServer.Contracts
{
    [DataContract]
    public class ClickablePoint
    {
        public ClickablePoint()
            : this(xpos: 0, ypos: 0)
        { }

        public ClickablePoint(int xpos, int ypos)
        {
            XPos = xpos;
            YPos = ypos;
        }

        [DataMember]
        public int XPos { get; set; }

        [DataMember]
        public int YPos { get; set; }
    }
}