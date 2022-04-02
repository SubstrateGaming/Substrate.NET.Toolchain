//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.Extrinsics;
using Ajuna.NetApi.Model.Meta;
using Ajuna.NetApi.Model.PalletBounties;
using Ajuna.NetApi.Model.SpCore;
using Ajuna.NetApi.Model.SpRuntime;
using Ajuna.NetApi.Model.Types;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Ajuna.NetApi.Model.PalletBounties
{
    
    
    public sealed class BountiesStorage
    {
        
        // Substrate client for the storage calls.
        private SubstrateClientExt _client;
        
        public BountiesStorage(SubstrateClientExt client)
        {
            this._client = client;
            _client.StorageKeyDict.Add(new System.Tuple<string, string>("Bounties", "BountyCount"), new System.Tuple<Ajuna.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>(null, null, typeof(Ajuna.NetApi.Model.Types.Primitive.U32)));
            _client.StorageKeyDict.Add(new System.Tuple<string, string>("Bounties", "Bounties"), new System.Tuple<Ajuna.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>(new Ajuna.NetApi.Model.Meta.Storage.Hasher[] {
                            Ajuna.NetApi.Model.Meta.Storage.Hasher.Twox64Concat}, typeof(Ajuna.NetApi.Model.Types.Primitive.U32), typeof(Ajuna.NetApi.Model.PalletBounties.Bounty)));
            _client.StorageKeyDict.Add(new System.Tuple<string, string>("Bounties", "BountyDescriptions"), new System.Tuple<Ajuna.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>(new Ajuna.NetApi.Model.Meta.Storage.Hasher[] {
                            Ajuna.NetApi.Model.Meta.Storage.Hasher.Twox64Concat}, typeof(Ajuna.NetApi.Model.Types.Primitive.U32), typeof(BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8>)));
            _client.StorageKeyDict.Add(new System.Tuple<string, string>("Bounties", "BountyApprovals"), new System.Tuple<Ajuna.NetApi.Model.Meta.Storage.Hasher[], System.Type, System.Type>(null, null, typeof(BaseVec<Ajuna.NetApi.Model.Types.Primitive.U32>)));
        }
        
        /// <summary>
        /// >> BountyCountParams
        ///  Number of bounty proposals that have been made.
        /// </summary>
        public static string BountyCountParams()
        {
            return RequestGenerator.GetStorage("Bounties", "BountyCount", Ajuna.NetApi.Model.Meta.Storage.Type.Plain);
        }
        
        /// <summary>
        /// >> BountyCount
        ///  Number of bounty proposals that have been made.
        /// </summary>
        public async Task<Ajuna.NetApi.Model.Types.Primitive.U32> BountyCount(CancellationToken token)
        {
            string parameters = BountiesStorage.BountyCountParams();
            return await _client.GetStorageAsync<Ajuna.NetApi.Model.Types.Primitive.U32>(parameters, token);
        }
        
        /// <summary>
        /// >> BountiesParams
        ///  Bounties that have been made.
        /// </summary>
        public static string BountiesParams(Ajuna.NetApi.Model.Types.Primitive.U32 key)
        {
            return RequestGenerator.GetStorage("Bounties", "Bounties", Ajuna.NetApi.Model.Meta.Storage.Type.Map, new Ajuna.NetApi.Model.Meta.Storage.Hasher[] {
                        Ajuna.NetApi.Model.Meta.Storage.Hasher.Twox64Concat}, new Ajuna.NetApi.Model.Types.IType[] {
                        key});
        }
        
        /// <summary>
        /// >> Bounties
        ///  Bounties that have been made.
        /// </summary>
        public async Task<Ajuna.NetApi.Model.PalletBounties.Bounty> Bounties(Ajuna.NetApi.Model.Types.Primitive.U32 key, CancellationToken token)
        {
            string parameters = BountiesStorage.BountiesParams(key);
            return await _client.GetStorageAsync<Ajuna.NetApi.Model.PalletBounties.Bounty>(parameters, token);
        }
        
        /// <summary>
        /// >> BountyDescriptionsParams
        ///  The description of each bounty.
        /// </summary>
        public static string BountyDescriptionsParams(Ajuna.NetApi.Model.Types.Primitive.U32 key)
        {
            return RequestGenerator.GetStorage("Bounties", "BountyDescriptions", Ajuna.NetApi.Model.Meta.Storage.Type.Map, new Ajuna.NetApi.Model.Meta.Storage.Hasher[] {
                        Ajuna.NetApi.Model.Meta.Storage.Hasher.Twox64Concat}, new Ajuna.NetApi.Model.Types.IType[] {
                        key});
        }
        
        /// <summary>
        /// >> BountyDescriptions
        ///  The description of each bounty.
        /// </summary>
        public async Task<BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8>> BountyDescriptions(Ajuna.NetApi.Model.Types.Primitive.U32 key, CancellationToken token)
        {
            string parameters = BountiesStorage.BountyDescriptionsParams(key);
            return await _client.GetStorageAsync<BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8>>(parameters, token);
        }
        
        /// <summary>
        /// >> BountyApprovalsParams
        ///  Bounty indices that have been approved but not yet funded.
        /// </summary>
        public static string BountyApprovalsParams()
        {
            return RequestGenerator.GetStorage("Bounties", "BountyApprovals", Ajuna.NetApi.Model.Meta.Storage.Type.Plain);
        }
        
        /// <summary>
        /// >> BountyApprovals
        ///  Bounty indices that have been approved but not yet funded.
        /// </summary>
        public async Task<BaseVec<Ajuna.NetApi.Model.Types.Primitive.U32>> BountyApprovals(CancellationToken token)
        {
            string parameters = BountiesStorage.BountyApprovalsParams();
            return await _client.GetStorageAsync<BaseVec<Ajuna.NetApi.Model.Types.Primitive.U32>>(parameters, token);
        }
    }
    
    public sealed class BountiesCalls
    {
        
        /// <summary>
        /// >> propose_bounty
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method ProposeBounty(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U128> value, BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8> description)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(value.Encode());
            byteArray.AddRange(description.Encode());
            return new Method(32, "Bounties", 0, "propose_bounty", byteArray.ToArray());
        }
        
        /// <summary>
        /// >> approve_bounty
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method ApproveBounty(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> bounty_id)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(bounty_id.Encode());
            return new Method(32, "Bounties", 1, "approve_bounty", byteArray.ToArray());
        }
        
        /// <summary>
        /// >> propose_curator
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method ProposeCurator(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> bounty_id, Ajuna.NetApi.Model.SpRuntime.EnumMultiAddress curator, BaseCom<Ajuna.NetApi.Model.Types.Primitive.U128> fee)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(bounty_id.Encode());
            byteArray.AddRange(curator.Encode());
            byteArray.AddRange(fee.Encode());
            return new Method(32, "Bounties", 2, "propose_curator", byteArray.ToArray());
        }
        
        /// <summary>
        /// >> unassign_curator
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method UnassignCurator(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> bounty_id)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(bounty_id.Encode());
            return new Method(32, "Bounties", 3, "unassign_curator", byteArray.ToArray());
        }
        
        /// <summary>
        /// >> accept_curator
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method AcceptCurator(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> bounty_id)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(bounty_id.Encode());
            return new Method(32, "Bounties", 4, "accept_curator", byteArray.ToArray());
        }
        
        /// <summary>
        /// >> award_bounty
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method AwardBounty(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> bounty_id, Ajuna.NetApi.Model.SpRuntime.EnumMultiAddress beneficiary)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(bounty_id.Encode());
            byteArray.AddRange(beneficiary.Encode());
            return new Method(32, "Bounties", 5, "award_bounty", byteArray.ToArray());
        }
        
        /// <summary>
        /// >> claim_bounty
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method ClaimBounty(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> bounty_id)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(bounty_id.Encode());
            return new Method(32, "Bounties", 6, "claim_bounty", byteArray.ToArray());
        }
        
        /// <summary>
        /// >> close_bounty
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method CloseBounty(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> bounty_id)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(bounty_id.Encode());
            return new Method(32, "Bounties", 7, "close_bounty", byteArray.ToArray());
        }
        
        /// <summary>
        /// >> extend_bounty_expiry
        /// Contains one variant per dispatchable that can be called by an extrinsic.
        /// </summary>
        public static Method ExtendBountyExpiry(BaseCom<Ajuna.NetApi.Model.Types.Primitive.U32> bounty_id, BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8> remark)
        {
            System.Collections.Generic.List<byte> byteArray = new List<byte>();
            byteArray.AddRange(bounty_id.Encode());
            byteArray.AddRange(remark.Encode());
            return new Method(32, "Bounties", 8, "extend_bounty_expiry", byteArray.ToArray());
        }
    }
    
    /// <summary>
    /// >> BountyProposed
    /// New bounty proposal. \[index\]
    /// </summary>
    public sealed class EventBountyProposed : BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32>
    {
    }
    
    /// <summary>
    /// >> BountyRejected
    /// A bounty proposal was rejected; funds were slashed. \[index, bond\]
    /// </summary>
    public sealed class EventBountyRejected : BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32, Ajuna.NetApi.Model.Types.Primitive.U128>
    {
    }
    
    /// <summary>
    /// >> BountyBecameActive
    /// A bounty proposal is funded and became active. \[index\]
    /// </summary>
    public sealed class EventBountyBecameActive : BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32>
    {
    }
    
    /// <summary>
    /// >> BountyAwarded
    /// A bounty is awarded to a beneficiary. \[index, beneficiary\]
    /// </summary>
    public sealed class EventBountyAwarded : BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32, Ajuna.NetApi.Model.SpCore.AccountId32>
    {
    }
    
    /// <summary>
    /// >> BountyClaimed
    /// A bounty is claimed by beneficiary. \[index, payout, beneficiary\]
    /// </summary>
    public sealed class EventBountyClaimed : BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32, Ajuna.NetApi.Model.Types.Primitive.U128, Ajuna.NetApi.Model.SpCore.AccountId32>
    {
    }
    
    /// <summary>
    /// >> BountyCanceled
    /// A bounty is cancelled. \[index\]
    /// </summary>
    public sealed class EventBountyCanceled : BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32>
    {
    }
    
    /// <summary>
    /// >> BountyExtended
    /// A bounty expiry is extended. \[index\]
    /// </summary>
    public sealed class EventBountyExtended : BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32>
    {
    }
    
    public enum BountiesErrors
    {
        
        /// <summary>
        /// >> InsufficientProposersBalance
        /// Proposer's balance is too low.
        /// </summary>
        InsufficientProposersBalance,
        
        /// <summary>
        /// >> InvalidIndex
        /// No proposal or bounty at that index.
        /// </summary>
        InvalidIndex,
        
        /// <summary>
        /// >> ReasonTooBig
        /// The reason given is just too big.
        /// </summary>
        ReasonTooBig,
        
        /// <summary>
        /// >> UnexpectedStatus
        /// The bounty status is unexpected.
        /// </summary>
        UnexpectedStatus,
        
        /// <summary>
        /// >> RequireCurator
        /// Require bounty curator.
        /// </summary>
        RequireCurator,
        
        /// <summary>
        /// >> InvalidValue
        /// Invalid bounty value.
        /// </summary>
        InvalidValue,
        
        /// <summary>
        /// >> InvalidFee
        /// Invalid bounty fee.
        /// </summary>
        InvalidFee,
        
        /// <summary>
        /// >> PendingPayout
        /// A bounty payout is pending.
        /// To cancel the bounty, you must unassign and slash the curator.
        /// </summary>
        PendingPayout,
        
        /// <summary>
        /// >> Premature
        /// The bounties cannot be claimed/closed because it's still in the countdown period.
        /// </summary>
        Premature,
    }
}
