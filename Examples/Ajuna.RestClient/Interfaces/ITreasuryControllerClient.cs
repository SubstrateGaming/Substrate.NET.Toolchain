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
   using Ajuna.NetApi.Model.Types.Primitive;
   using Ajuna.NetApi.Model.PalletTreasury;
   using Ajuna.NetApi.Model.FrameSupport;
   
   public interface ITreasuryControllerClient
   {
      Task<U32> GetProposalCount();
      Task<Proposal> GetProposals(string key);
      Task<BoundedVecT9> GetApprovals();
   }
}
