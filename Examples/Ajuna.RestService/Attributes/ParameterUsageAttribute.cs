using System;

namespace Ajuna.RestService.Attributes
{
    public class ParameterUsageAttribute : Attribute
    {
        public string ClassName { get; set; }
        public string MethodName { get; set; }

        public ParameterUsageAttribute(string className, string methodName)
        {
            ClassName = className;
            MethodName = methodName;
        }
    }
}
