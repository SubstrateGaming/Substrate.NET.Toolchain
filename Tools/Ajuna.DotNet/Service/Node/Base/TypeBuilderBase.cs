using Ajuna.DotNet.Extensions;
using Ajuna.NetApi.Model.Meta;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ajuna.DotNet.Service.Node.Base
{
   public abstract class TypeBuilderBase : BuilderBase
   {
      public NodeType TypeDef { get; }

      public TypeBuilderBase(string projectName, uint id, NodeType typeDef, NodeTypeResolver resolver)
          : base(projectName, id, resolver)
      {
         TypeDef = typeDef;
         NamespaceName = resolver.GetNamespace(id);
      }
   }
}