//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ajuna.RestClient.Interfaces
{
   using System;
   using System.Threading.Tasks;
   using Ajuna.NetApi.Model.FrameSupport;
   using Ajuna.NetApi.Model.NodeRuntime;
   using Ajuna.NetApi.Model.PalletCollective;
   using Ajuna.NetApi.Model.Types.Primitive;
   using Ajuna.NetApi.Model.Types.Base;
   using Ajuna.NetApi.Model.SpCore;
   
   public interface ICouncilControllerClient
   {
      Task<BoundedVecT7> GetProposals();
      Task<EnumNodeCall> GetProposalOf(string key);
      Task<Votes> GetVoting(string key);
      Task<U32> GetProposalCount();
      Task<BaseVec<AccountId32>> GetMembers();
      Task<AccountId32> GetPrime();
   }
}
