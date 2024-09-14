using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types;
using Substrate.NetApi.Model.Types.Metadata.V14;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Substrate.DotNet.Service.Node
{
   public class EnumBuilder : TypeBuilderBase
   {
      private EnumBuilder(string projectName, uint id, NodeTypeVariant typeDef, NodeTypeResolver typeDict)
          : base(projectName, id, typeDef, typeDict)
      {
      }

      public static EnumBuilder Init(string projectName, uint id, NodeTypeVariant typeDef, NodeTypeResolver typeDict)
      {
         return new EnumBuilder(projectName, id, typeDef, typeDict);
      }

      public override TypeBuilderBase Create()
      {
         var typeDef = TypeDef as NodeTypeVariant;

         string enumName = $"{typeDef.Path.Last()}";

         ClassName = $"Enum{enumName}";
         ReferenzName = $"{NamespaceName}.{ClassName}";
         var typeNamespace = new CodeNamespace(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         // Create the enum itself
         var enumType = new CodeTypeDeclaration(enumName)
         {
            IsEnum = true
         };
         enumType.Comments.AddRange(GetComments(typeDef.Docs, null, enumName));

         // Detect if this enum has associated types
         bool hasAssociatedTypes = false;

         if (typeDef.Variants != null)
         {
            foreach (TypeVariant variant in typeDef.Variants)
            {
               var enumMember = new CodeMemberField(ClassName, variant.Name)
               {
                  InitExpression = new CodePrimitiveExpression(variant.Index)
               };
               enumMember.Comments.AddRange(GetComments(variant.Docs, null, variant.Name));
               enumType.Members.Add(enumMember);

               // Check if this variant has associated types
               if (variant.TypeFields != null && variant.TypeFields.Length > 0)
               {
                  hasAssociatedTypes = true;
               }
            }
         }
         typeNamespace.Types.Add(enumType);

         // Generate the appropriate class based on whether there are associated types
         var targetClass = new CodeTypeDeclaration(ClassName)
         {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
         };
         targetClass.Comments.AddRange(GetComments(typeDef.Docs, typeDef));

         if (hasAssociatedTypes)
         {
            // Generate the class based on BaseEnumRust (with associated types)
            var codeConstructor = new CodeConstructor
            {
               Attributes = MemberAttributes.Public
            };
            codeConstructor.Comments.AddRange(GetComments(new string[] { "Initializes a new instance of the class." }, null));

            foreach (TypeVariant variant in typeDef.Variants)
            {
               string decoderType;
               if (variant.TypeFields == null || variant.TypeFields.Length == 0)
               {
                  decoderType = "BaseVoid";
               }
               else if (variant.TypeFields.Length == 1)
               {
                  NodeTypeResolved item = GetFullItemPath(variant.TypeFields[0].TypeId);
                  decoderType = item.ToString();
               }
               else
               {
                  string tupleType = $"BaseTuple<{string.Join(", ", variant.TypeFields.Select(f => GetFullItemPath(f.TypeId)))}>";
                  decoderType = tupleType;
               }

               codeConstructor.Statements.Add(
                   new CodeSnippetStatement($"\t\t\t\tAddTypeDecoder<{decoderType}>({enumName}.{HandleReservedKeyword(variant.Name)});")
               );
            }

            targetClass.BaseTypes.Add(new CodeTypeReference($"BaseEnumRust<{enumName}>"));
            targetClass.Members.Add(codeConstructor);
         }
         else
         {
            // Generate the class based on BaseEnum (without associated types)
            targetClass.BaseTypes.Add(new CodeTypeReference($"BaseEnum<{enumName}>"));
         }

         typeNamespace.Types.Add(targetClass);

         return this;
      }
   }
}