using Ajuna.DotNet.Service.Node.Base;
using Ajuna.NetApi.Model.Meta;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ajuna.DotNet.Service.Node
{
   public class RunetimeBuilder : TypeBuilderBase
   {
      private RunetimeBuilder(string projectName, uint id, NodeTypeVariant typeDef, Dictionary<uint, (string, List<string>)> typeDict)
          : base(projectName, id, typeDef, typeDict)
      {
      }

      public static RunetimeBuilder Init(string projectName, uint id, NodeTypeVariant typeDef, Dictionary<uint, (string, List<string>)> typeDict)
      {
         return new RunetimeBuilder(projectName, id, typeDef, typeDict);
      }

      public override TypeBuilderBase Create()
      {
         var typeDef = TypeDef as NodeTypeVariant;

         string runtimeType = $"{typeDef.Path.Last()}";
         string enumName = $"Node{runtimeType}";

         ClassName = $"Enum{enumName}";
         ReferenzName = $"{NamespaceName}.{ClassName}";
         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         CodeTypeDeclaration TargetType = new(enumName)
         {
            IsEnum = true
         };

         if (typeDef.Variants != null)
         {
            foreach (string enumFieldName in typeDef.Variants.Select(p => p.Name))
            {
               TargetType.Members.Add(new CodeMemberField(ClassName, enumFieldName));
            }
         }
         typeNamespace.Types.Add(TargetType);

         var targetClass = new CodeTypeDeclaration(ClassName)
         {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
         };
         targetClass.BaseTypes.Add(new CodeTypeReference($"BaseEnum<{enumName}>"));

         // add comment to class if exists
         targetClass.Comments.AddRange(GetComments(typeDef.Docs, typeDef));
         AddTargetClassCustomAttributes(targetClass, typeDef);

         typeNamespace.Types.Add(targetClass);

         return this;
      }
   }
}
