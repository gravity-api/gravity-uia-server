using System;

namespace UiaWebDriverServer.Contracts.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class UiaConstantAttribute : Attribute
    {
        public UiaConstantAttribute(int constant)
        {
            Constant = constant;
        }

        public int Constant { get; }
        public string Name { get; set; }
    }
}
