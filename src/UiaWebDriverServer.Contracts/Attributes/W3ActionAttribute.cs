using System;

namespace UiaWebDriverServer.Contracts.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class W3ActionAttribute : Attribute
    {
        public W3ActionAttribute(string type)
        {
            Type = type;
        }

        public string Type { get; set; }
    }
}
