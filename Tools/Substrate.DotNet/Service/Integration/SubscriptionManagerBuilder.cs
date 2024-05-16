using Substrate.DotNet.Service.Node.Base;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace Substrate.DotNet.Service.Node
{
   public class SubscriptionManagerBuilder : IntegrationBuilderBase
   {
      private SubscriptionManagerBuilder(string projectName, uint id, List<string> moduleNames, NodeTypeResolver typeDict) :
          base(projectName, id, moduleNames, typeDict)
      {
      }

      public static SubscriptionManagerBuilder Init(string projectName, uint id, List<string> moduleNames, NodeTypeResolver typeDict)
      {
         return new SubscriptionManagerBuilder(projectName, id, moduleNames, typeDict);
      }

      public override SubscriptionManagerBuilder Create()
      {
         ClassName = "SubscriptionManager";
         NamespaceName = $"{ProjectName}.Client";

         string literalCode = $@"
using System;
using Serilog;
using Substrate.NetApi.Model.Rpc;
using {ProjectName}.Helper;

namespace {NamespaceName}
{{
    public delegate void SubscriptionOnEvent(string subscriptionId, StorageChangeSet storageChangeSet);

    public class {ClassName}
    {{
        public bool IsSubscribed {{ get; set; }}

        public event SubscriptionOnEvent SubscrptionEvent;

        public {ClassName}()
        {{
            SubscrptionEvent += OnSystemEvents;
        }}

        /// <summary>
        /// Simple extrinsic tester
        /// </summary>
        /// <param name=""subscriptionId""></param>
        /// <param name=""storageChangeSet""></param>
        public void ActionSubscrptionEvent(string subscriptionId, StorageChangeSet storageChangeSet)
        {{
            IsSubscribed = false;

            Log.Information(""System.Events: {{0}}"", storageChangeSet);

            SubscrptionEvent?.Invoke(subscriptionId, storageChangeSet);
        }}

        /// <summary>
        /// Handle system events
        /// </summary>
        /// <param name=""subscriptionId""></param>
        /// <param name=""storageChangeSet""></param>
        private void OnSystemEvents(string subscriptionId, StorageChangeSet storageChangeSet)
        {{
            Log.Debug(""OnExtrinsicUpdated[{{id}}] updated {{state}}"",
                subscriptionId,
                storageChangeSet);
        }}
    }}
}}
";

         var csu = new CodeSnippetCompileUnit(literalCode);
         TargetUnit = csu;

         return this;
      }
   }
}