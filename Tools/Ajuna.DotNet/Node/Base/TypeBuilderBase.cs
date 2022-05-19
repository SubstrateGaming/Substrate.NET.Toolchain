using Ajuna.DotNet.Extensions;
using Ajuna.NetApi.Model.Meta;
using System.Collections.Generic;

namespace Ajuna.DotNet.Node.Base
{
   public abstract class TypeBuilderBase : BuilderBase
   {
      public NodeType TypeDef { get; }

      public TypeBuilderBase(uint id, NodeType typeDef, Dictionary<uint, (string, List<string>)> typeDict)
          : base(id, typeDict)
      {
         TypeDef = typeDef;
         NameSpace = typeDef.Path != null && typeDef.Path[0].Contains("_")
             ? $"{BASE_NAMESPACE}.Model.{typeDef.Path[0].MakeMethod()}"
             : $"{BASE_NAMESPACE}.Model.Base";
      }
   }
}