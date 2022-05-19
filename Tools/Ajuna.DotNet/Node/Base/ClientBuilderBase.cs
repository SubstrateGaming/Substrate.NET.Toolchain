using System.Collections.Generic;

namespace Ajuna.DotNet.Node.Base
{
   public abstract class ClientBuilderBase : BuilderBase
   {
      public List<(string, List<string>)> ModuleNames { get; }

      public ClientBuilderBase(uint id, List<(string, List<string>)> moduleNames,
          Dictionary<uint, (string, List<string>)> typeDict)
          : base(id, typeDict)
      {
         ModuleNames = moduleNames;
         NameSpace = BASE_NAMESPACE;
      }
   }
}