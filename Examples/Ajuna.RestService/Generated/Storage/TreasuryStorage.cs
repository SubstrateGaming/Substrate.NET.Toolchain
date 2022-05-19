//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.FrameSupport;
using Ajuna.NetApi.Model.PalletTreasury;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using Ajuna.ServiceLayer.Attributes;
using Ajuna.ServiceLayer.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Ajuna.RestService.Generated.Storage
{
    
    
    /// <summary>
    /// ITreasuryStorage interface definition.
    /// </summary>
    public interface ITreasuryStorage : IStorage
    {
        
        /// <summary>
        /// >> ProposalCount
        ///  Number of proposals that have been made.
        /// </summary>
        Ajuna.NetApi.Model.Types.Primitive.U32 GetProposalCount();
        
        /// <summary>
        /// >> Proposals
        ///  Proposals that have been made.
        /// </summary>
        Ajuna.NetApi.Model.PalletTreasury.Proposal GetProposals(string key);
        
        /// <summary>
        /// >> Approvals
        ///  Proposal indices that have been approved but not yet awarded.
        /// </summary>
        Ajuna.NetApi.Model.FrameSupport.BoundedVecT9 GetApprovals();
    }
    
    /// <summary>
    /// TreasuryStorage class definition.
    /// </summary>
    public sealed class TreasuryStorage : ITreasuryStorage
    {
        
        /// <summary>
        /// _proposalCountTypedStorage typed storage field
        /// </summary>
        private TypedStorage<Ajuna.NetApi.Model.Types.Primitive.U32> _proposalCountTypedStorage;
        
        /// <summary>
        /// _proposalsTypedStorage typed storage field
        /// </summary>
        private TypedMapStorage<Ajuna.NetApi.Model.PalletTreasury.Proposal> _proposalsTypedStorage;
        
        /// <summary>
        /// _approvalsTypedStorage typed storage field
        /// </summary>
        private TypedStorage<Ajuna.NetApi.Model.FrameSupport.BoundedVecT9> _approvalsTypedStorage;
        
        /// <summary>
        /// TreasuryStorage constructor.
        /// </summary>
        public TreasuryStorage(IStorageChangeDelegate storageChangeDelegate)
        {
            this.ProposalCountTypedStorage = new TypedStorage<Ajuna.NetApi.Model.Types.Primitive.U32>("Treasury.ProposalCount", storageChangeDelegate);
            this.ProposalsTypedStorage = new TypedMapStorage<Ajuna.NetApi.Model.PalletTreasury.Proposal>("Treasury.Proposals", storageChangeDelegate);
            this.ApprovalsTypedStorage = new TypedStorage<Ajuna.NetApi.Model.FrameSupport.BoundedVecT9>("Treasury.Approvals", storageChangeDelegate);
        }
        
        /// <summary>
        /// _proposalCountTypedStorage property
        /// </summary>
        public TypedStorage<Ajuna.NetApi.Model.Types.Primitive.U32> ProposalCountTypedStorage
        {
            get
            {
                return _proposalCountTypedStorage;
            }
            set
            {
                _proposalCountTypedStorage = value;
            }
        }
        
        /// <summary>
        /// _proposalsTypedStorage property
        /// </summary>
        public TypedMapStorage<Ajuna.NetApi.Model.PalletTreasury.Proposal> ProposalsTypedStorage
        {
            get
            {
                return _proposalsTypedStorage;
            }
            set
            {
                _proposalsTypedStorage = value;
            }
        }
        
        /// <summary>
        /// _approvalsTypedStorage property
        /// </summary>
        public TypedStorage<Ajuna.NetApi.Model.FrameSupport.BoundedVecT9> ApprovalsTypedStorage
        {
            get
            {
                return _approvalsTypedStorage;
            }
            set
            {
                _approvalsTypedStorage = value;
            }
        }
        
        /// <summary>
        /// Connects to all storages and initializes the change subscription handling.
        /// </summary>
        public async Task InitializeAsync(Ajuna.NetApi.SubstrateClient client)
        {
            await ProposalCountTypedStorage.InitializeAsync(client, "Treasury", "ProposalCount");
            await ProposalsTypedStorage.InitializeAsync(client, "Treasury", "Proposals");
            await ApprovalsTypedStorage.InitializeAsync(client, "Treasury", "Approvals");
        }
        
        /// <summary>
        /// Implements any storage change for Treasury.ProposalCount
        /// </summary>
        [StorageChange("Treasury", "ProposalCount")]
        public void OnUpdateProposalCount(string data)
        {
            ProposalCountTypedStorage.Update(data);
        }
        
        /// <summary>
        /// >> ProposalCount
        ///  Number of proposals that have been made.
        /// </summary>
        public Ajuna.NetApi.Model.Types.Primitive.U32 GetProposalCount()
        {
            return ProposalCountTypedStorage.Get();
        }
        
        /// <summary>
        /// Implements any storage change for Treasury.Proposals
        /// </summary>
        [StorageChange("Treasury", "Proposals")]
        public void OnUpdateProposals(string key, string data)
        {
            ProposalsTypedStorage.Update(key, data);
        }
        
        /// <summary>
        /// >> Proposals
        ///  Proposals that have been made.
        /// </summary>
        public Ajuna.NetApi.Model.PalletTreasury.Proposal GetProposals(string key)
        {
            if ((key == null))
            {
                return null;
            }
            if (ProposalsTypedStorage.Dictionary.TryGetValue(key, out Ajuna.NetApi.Model.PalletTreasury.Proposal result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Implements any storage change for Treasury.Approvals
        /// </summary>
        [StorageChange("Treasury", "Approvals")]
        public void OnUpdateApprovals(string data)
        {
            ApprovalsTypedStorage.Update(data);
        }
        
        /// <summary>
        /// >> Approvals
        ///  Proposal indices that have been approved but not yet awarded.
        /// </summary>
        public Ajuna.NetApi.Model.FrameSupport.BoundedVecT9 GetApprovals()
        {
            return ApprovalsTypedStorage.Get();
        }
    }
}