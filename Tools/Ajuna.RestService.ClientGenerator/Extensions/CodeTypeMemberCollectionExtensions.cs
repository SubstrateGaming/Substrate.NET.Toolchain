using System;
using System.CodeDom;
using System.Net.Http;

namespace Ajuna.RestService.ClientGenerator.Extensions
{
   /// <summary>
   /// Simplifies access to CodeTypeMemberCollection
   /// </summary>
   internal static class CodeTypeMemberCollectionExtensions
   {
      /// <summary>
      /// Addds a private member to the given target.
      /// </summary>
      /// <param name="target">The target code where to add the member to.</param>
      /// <param name="type">The member type.</param>
      /// <param name="name">The member name.</param>
      /// <param name="ns">The namespace that we may need to add the given member type to.</param>
      internal static void AddPrivateFieldAssignableFromConstructor(this CodeTypeMemberCollection target, Type type, string name, CodeNamespace ns)
      {
         ns.Imports.Add(new CodeNamespaceImport(type.Namespace));

         target.Add(new CodeMemberField()
         {
            Attributes = MemberAttributes.Private,
            Name = name,
            Type = new CodeTypeReference(type)
         });
      }

      /// <summary>
      /// Adds a "private HttpClient _httpClient" member to the given target.
      /// </summary>
      /// <param name="target">The target code where to add the member to.</param>
      /// <param name="ns">The current namespaces that we may need to add HttpClient usage.</param>
      internal static void AddHttpClientPrivateMember(this CodeTypeMemberCollection target, CodeNamespace ns)
      {
         target.AddPrivateFieldAssignableFromConstructor(typeof(HttpClient), "_httpClient", ns);
      }
   }
}
