using System;

namespace UiaDriverServer.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    internal class UiaConstantAttribute : Attribute
    {
        public UiaConstantAttribute(int constant)
        {
            Constant = constant;
        }

        public int Constant { get; }
        public string Name { get; set; }
    }
}