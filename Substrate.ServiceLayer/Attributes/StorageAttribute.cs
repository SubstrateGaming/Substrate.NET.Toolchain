using System;

namespace Substrate.ServiceLayer.Attributes
{
   [AttributeUsage(AttributeTargets.Class)]
   public class StorageAttribute : Attribute
   {
      public StorageAttribute()
      {
      }
   }
}
