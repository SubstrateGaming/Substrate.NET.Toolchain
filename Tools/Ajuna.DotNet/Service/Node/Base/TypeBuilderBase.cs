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

      public TypeBuilderBase(string projectName, uint id, NodeType typeDef, Dictionary<uint, (string, List<string>)> typeDict)
          : base(projectName, id, typeDict)
      {
         TypeDef = typeDef;
         NamespaceName = $"{ProjectName}.Generated.Model.{TypeNameSpace(typeDef.Path)}";
      }

      private string TypeNameSpace(string[] path)
      {
         if (path == null || path.Length < 2)
         {
            return "Base";
         }

         // heck if we have a versioned name space
         IEnumerable<string> vWhere = path.Where(p => Regex.IsMatch(p, @"v[0-9]+"));
         if (vWhere.Any())
         {
            return path[0].MakeMethod() + "." + vWhere.First().MakeMethod();
         }

         return path[0].MakeMethod();
      }
   }
}