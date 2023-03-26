using System;

namespace Substrate.ServiceLayer.Attributes
{
   [AttributeUsage(AttributeTargets.Class)]
   public class AjunaControllerIgnoreAttribute : Attribute
   {
      public AjunaControllerIgnoreAttribute()
      {
      }
   }
}
