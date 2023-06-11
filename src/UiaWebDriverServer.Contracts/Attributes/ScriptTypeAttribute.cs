using System;

namespace UiaWebDriverServer.Contracts.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ScriptTypeAttribute : Attribute
    {
        public ScriptTypeAttribute(string type)
        {
            Type = type;
        }

        public string Type { get; set; }
    }
}
