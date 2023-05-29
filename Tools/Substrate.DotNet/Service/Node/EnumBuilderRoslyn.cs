using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types;
using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using static Substrate.NetApi.Model.Meta.Storage;
using Serilog;

namespace Substrate.DotNet.Service.Node
{
   public class EnumBuilderRoslyn : TypeBuilderBaseRoslyn
   {
      private EnumBuilderRoslyn(string projectName, uint id, NodeTypeVariant typeDef, NodeTypeResolver typeDict)
          : base(projectName, id, typeDef, typeDict)
      {
      }

      public static EnumBuilderRoslyn Init(string projectName, uint id, NodeTypeVariant typeDef, NodeTypeResolver typeDict)
      {
         return new EnumBuilderRoslyn(projectName, id, typeDef, typeDict);
      }

      public override TypeBuilderBaseRoslyn Create()
      {
         var typeDef = TypeDef as NodeTypeVariant;
         string enumName = $"{typeDef.Path.Last()}";

         if (!Resolver.TypeNames.TryGetValue(Id, out NodeTypeResolved nodeTypeResolved))
         {
            throw new NotSupportedException($"Could not find type {Id}");
         }

         ClassName = nodeTypeResolved.ClassName;

         ReferenzName = $"{NamespaceName}.{ClassName}";

         NamespaceDeclarationSyntax typeNamespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(NamespaceName));

         // only add the enumeration in the first variations.
         if (string.IsNullOrEmpty(nodeTypeResolved.Name.ClassNameSufix) || nodeTypeResolved.Name.ClassNameSufix == "1")
         {
               EnumDeclarationSyntax targetType = SyntaxFactory
                  .EnumDeclaration(enumName)
                  .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

               if (typeDef.Variants != null)
               {
                  foreach (TypeVariant variant in typeDef.Variants)
                  {
                     targetType = targetType.AddMembers(
                         SyntaxFactory.EnumMemberDeclaration(variant.Name)
                           .WithEqualsValue(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(variant.Index))))
                     );
                  }
               }
               typeNamespace = typeNamespace.AddMembers(targetType);
         }


         ClassDeclarationSyntax targetClass = SyntaxFactory.ClassDeclaration(ClassName)
             .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword));
         targetClass = targetClass.WithLeadingTrivia(GetCommentsRoslyn(typeDef.Docs, typeDef));

         if (typeDef.Variants == null || typeDef.Variants.All(p => p.TypeFields == null))
         {
            targetClass = targetClass.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"BaseEnum<{enumName}>")));
            typeNamespace = typeNamespace.AddMembers(targetClass);
         }
         else
         {
            var genericTypeArguments = new List<TypeSyntax> { SyntaxFactory.ParseTypeName(enumName) };

            int highIndex = typeDef.Variants.Max(p => p.Index);
            if (highIndex < 256)
            {
               for (int i = 0; i < highIndex + 1; i++)
               {
                  TypeVariant variant = typeDef.Variants.FirstOrDefault(p => p.Index == i);

                  if (variant == null || variant.TypeFields == null)
                  {
                     // add void type
                     genericTypeArguments.Add(SyntaxFactory.ParseTypeName("BaseVoid"));
                  }
                  else
                  {
                     if (variant.TypeFields.Length == 1)
                     {
                        NodeTypeResolved item = GetFullItemPath(variant.TypeFields[0].TypeId);
                        genericTypeArguments.Add(SyntaxFactory.ParseTypeName(item.ToString()));
                     }
                     else
                     {
                        var baseTupleArgs = new List<TypeSyntax>();
                        foreach (NodeTypeField field in variant.TypeFields)
                        {
                           NodeTypeResolved item = GetFullItemPath(field.TypeId);
                           baseTupleArgs.Add(SyntaxFactory.ParseTypeName(item.ToString()));
                        }
                        GenericNameSyntax baseTuple = SyntaxFactory.GenericName(SyntaxFactory.Identifier("BaseTuple"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(baseTupleArgs)));
                        genericTypeArguments.Add(baseTuple);
                     }
                  }
               }
            }
            else
            {
               throw new NotImplementedException("Enum extension can't handle such big sized typed rust enumeration, please create a manual fix for it.");
            }

            GenericNameSyntax baseEnumExt = SyntaxFactory.GenericName(SyntaxFactory.Identifier("BaseEnumExt"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(genericTypeArguments)));
            targetClass = targetClass.AddBaseListTypes(SyntaxFactory.SimpleBaseType(baseEnumExt));
            typeNamespace = typeNamespace.AddMembers(targetClass);
         }

         TargetUnit = TargetUnit.AddMembers(typeNamespace);

         return this;
      }
   }
}