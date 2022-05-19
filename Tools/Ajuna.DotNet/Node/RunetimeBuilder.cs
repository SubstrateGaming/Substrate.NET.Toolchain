﻿using Ajuna.DotNet.Node.Base;
using Ajuna.NetApi.Model.Meta;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ajuna.DotNet.Node
{
   public class RunetimeBuilder : TypeBuilderBase
   {
      private RunetimeBuilder(uint id, NodeTypeVariant typeDef, Dictionary<uint, (string, List<string>)> typeDict)
          : base(id, typeDef, typeDict)
      {
      }

      public static RunetimeBuilder Init(uint id, NodeTypeVariant typeDef, Dictionary<uint, (string, List<string>)> typeDict)
      {
         return new RunetimeBuilder(id, typeDef, typeDict);
      }

      public override TypeBuilderBase Create()
      {
         var typeDef = TypeDef as NodeTypeVariant;

         #region CREATE

         var runtimeType = $"{typeDef.Path.Last()}";
         var enumName = $"Node{runtimeType}";

         ClassName = $"Enum{enumName}";
         ReferenzName = $"{NameSpace}.{ClassName}";
         CodeNamespace typeNamespace = new(NameSpace);
         TargetUnit.Namespaces.Add(typeNamespace);

         CodeTypeDeclaration TargetType = new(enumName)
         {
            IsEnum = true
         };

         if (typeDef.Variants != null)
         {
            foreach (var enumFieldName in typeDef.Variants.Select(p => p.Name))
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

         typeNamespace.Types.Add(targetClass);

         #endregion

         return this;
      }
   }
}