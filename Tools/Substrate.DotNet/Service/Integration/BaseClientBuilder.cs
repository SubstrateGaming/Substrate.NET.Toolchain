using Substrate.DotNet.Service.Node.Base;
using Substrate.NetApi;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Meta;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Substrate.DotNet.Extensions;
using System.Xml.Linq;

namespace Substrate.DotNet.Service.Node
{
   public class BaseClientBuilder : BaseClientBuilderBase
   {
      private BaseClientBuilder(string projectName, uint id, List<string> moduleNames, NodeTypeResolver typeDict) :
          base(projectName, id, moduleNames, typeDict)
      {
      }

      public static BaseClientBuilder Init(string projectName, uint id, List<string> moduleNames, NodeTypeResolver typeDict)
      {
         return new BaseClientBuilder(projectName, id, moduleNames, typeDict);
      }

      public override BaseClientBuilder Create()
      {
         ClassName = "BaseClient";
         NamespaceName = $"{ProjectName}.Generated";

         CodeNamespace typeNamespace = new(NamespaceName);
         TargetUnit.Namespaces.Add(typeNamespace);

         // Import necessary namespaces
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Substrate.NetApi.Model.Meta"));
         ImportsNamespace.Imports.Add(new CodeNamespaceImport("Substrate.NetApi.Model.Extrinsics"));

         var targetClass = new CodeTypeDeclaration(ClassName)
         {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
         };
         targetClass.Comments.AddRange(GetComments(null, null, $"Base Client, Boilerplate."));
         targetClass.BaseTypes.Add(new CodeTypeReference(typeof(SubstrateClient)));
         typeNamespace.Types.Add(targetClass);

         // Add fields
         var maxConcurrentCallsField = new CodeMemberField(typeof(int), "_maxConcurrentCalls")
         {
            Attributes = MemberAttributes.Private
         };
         targetClass.Members.Add(maxConcurrentCallsField);

         var chargeTypeField = new CodeMemberField("ChargeType", "_chargeTypeDefault")
         {
            Attributes = MemberAttributes.Private
         };
         targetClass.Members.Add(chargeTypeField);

         targetClass.Members.Add(CreateAutoProperty("ExtrinsicManager", "ExtrinsicManager", 
            "Extrinsic manager, used to manage extrinsic subscriptions and the corresponding states."));
         targetClass.Members.Add(CreateAutoProperty("SubscriptionManager", "SubscriptionManager", 
            "Subscription manager, used to manage subscriptions of storage elements."));
         targetClass.Members.Add(CreateAutoProperty("SubstrateClientExt", "SubstrateClientExt", 
            "Substrate Extension Client."));
         targetClass.Members.Add(CreateAutoProperty("NetworkType", "NetworkType",
            "Network type."));

         // Define the IsConnected property
         var isConnectedProperty = new CodeMemberProperty
         {
            Name = "IsConnected",
            Type = new CodeTypeReference(typeof(bool)),
            Attributes = MemberAttributes.Public | MemberAttributes.Final,
            HasGet = true,
            HasSet = false
         };
         isConnectedProperty.Comments.AddRange(GetComments(null, null, "Is connected to the network."));
         isConnectedProperty.GetStatements.Add(new CodeMethodReturnStatement(
             new CodePropertyReferenceExpression(
                 new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "substrateClient"),
                 "IsConnected"
             )
         ));
         targetClass.Members.Add(isConnectedProperty);

         // Define constructor
         var constructor = new CodeConstructor
         {
            Attributes = MemberAttributes.Public,
            Parameters =
            {
                new CodeParameterDeclarationExpression(typeof(string), "url"),
                new CodeParameterDeclarationExpression(typeof(int), "networkType"),
                new CodeParameterDeclarationExpression(typeof(int), "maxConcurrentCalls")
            }
         };
         targetClass.Members.Add(constructor);

         // Modifying constructor for initialization
         constructor.Statements.Add(new CodeAssignStatement(
             new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_chargeTypeDefault"),
             new CodeMethodInvokeExpression(
                 new CodeTypeReferenceExpression("ChargeType"),
                 "Default" // Assuming a static method Default() based on context
             )
         ));

         constructor.Statements.Add(new CodeAssignStatement(
             new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "extrinsicManager"),
             new CodeObjectCreateExpression("ExtrinsicManager")
         ));

         constructor.Statements.Add(new CodeAssignStatement(
             new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "subscriptionManager"),
             new CodeObjectCreateExpression("SubscriptionManager")
         ));

         constructor.Statements.Add(new CodeAssignStatement(
             new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "substrateClient"),
             new CodeObjectCreateExpression("SubstrateClientExt", new CodeVariableReferenceExpression("uri"), new CodeVariableReferenceExpression("_chargeTypeDefault"))
         ));


         // Add methods
         var connectMethod = new CodeMemberMethod
         {
            Name = "ConnectAsync",
            Attributes = MemberAttributes.Public | MemberAttributes.Final,
            ReturnType = new CodeTypeReference(typeof(Task<bool>)),
            Parameters =
            {
                new CodeParameterDeclarationExpression(typeof(bool), "useMetadata"),
                new CodeParameterDeclarationExpression(typeof(bool), "standardSubstrate"),
                new CodeParameterDeclarationExpression(typeof(CancellationToken), "token")
            }
         };
         targetClass.Members.Add(connectMethod);

         return this;
      }

      public CodeTypeMember CreateAutoProperty(string propertyName, string propertyType, string comment)
      {
         string propertyCode = $"        public {propertyType} {propertyName.MakeMethod()} {{ get; }}";

         // Using CodeSnippetTypeMember to inject a raw string of code.
         CodeSnippetTypeMember autoProperty = new(propertyCode);
         autoProperty.Comments.AddRange(GetComments(null, null, comment));
         return autoProperty;
      }
   }

}
