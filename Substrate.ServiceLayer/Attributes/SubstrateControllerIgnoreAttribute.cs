using System;

namespace Substrate.ServiceLayer.Attributes
{
   [AttributeUsage(AttributeTargets.Class)]
   public class SubstrateControllerIgnoreAttribute : Attribute
   {
      public SubstrateControllerIgnoreAttribute()
      {
      }
   }
}
