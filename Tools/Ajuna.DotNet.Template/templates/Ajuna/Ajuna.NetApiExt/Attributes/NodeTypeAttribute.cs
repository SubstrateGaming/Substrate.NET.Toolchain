using Ajuna.NetApi.Model.Types.Metadata.V14;
using System;

namespace Ajuna.NetApiExt.Attributes
{
   [AttributeUsage(AttributeTargets.Class)]
   public class NodeTypeAttribute : Attribute
   {
      public TypeDefEnum NodeType { get; set; }

      public NodeTypeAttribute(TypeDefEnum nodeType)
      {
         NodeType = nodeType;
      }
   }
}
