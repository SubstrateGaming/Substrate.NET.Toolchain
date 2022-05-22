using System;

namespace Ajuna.ServiceLayer.Attributes
{
   [AttributeUsage(AttributeTargets.Method)]
   public class StorageKeyBuilderAttribute : Attribute
   {
      public string ClassName { get; }
      public string MethodName { get; }

      public StorageKeyBuilderAttribute(string className, string methodName)
      {
         ClassName = className;
         MethodName = methodName;
      }
   }
}
