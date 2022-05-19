//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.NetApi.Model.FrameSupport;
using Ajuna.NetApi.Model.FrameSystem;
using Ajuna.NetApi.Model.PrimitiveTypes;
using Ajuna.NetApi.Model.SpCore;
using Ajuna.NetApi.Model.SpRuntime;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using Ajuna.RestService.Generated.Storage;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Ajuna.RestService.Generated.Controller
{
    
    
    /// <summary>
    /// SystemController controller to access storages.
    /// </summary>
    [ApiController()]
    [Route("[controller]")]
    public sealed class SystemController : ControllerBase
    {
        
        private ISystemStorage _systemStorage;
        
        /// <summary>
        /// SystemController constructor.
        /// </summary>
        public SystemController(ISystemStorage systemStorage)
        {
            _systemStorage = systemStorage;
        }
        
        /// <summary>
        /// >> Account
        ///  The full account information for a particular account ID.
        /// </summary>
        [HttpGet("Account")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.FrameSystem.AccountInfo), 200)]
        public IActionResult GetAccount(string key)
        {
            return this.Ok(_systemStorage.GetAccount(key));
        }
        
        /// <summary>
        /// >> ExtrinsicCount
        ///  Total extrinsics count for the current block.
        /// </summary>
        [HttpGet("ExtrinsicCount")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.U32), 200)]
        public IActionResult GetExtrinsicCount()
        {
            return this.Ok(_systemStorage.GetExtrinsicCount());
        }
        
        /// <summary>
        /// >> BlockWeight
        ///  The current weight for the block.
        /// </summary>
        [HttpGet("BlockWeight")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.FrameSupport.PerDispatchClassT1), 200)]
        public IActionResult GetBlockWeight()
        {
            return this.Ok(_systemStorage.GetBlockWeight());
        }
        
        /// <summary>
        /// >> AllExtrinsicsLen
        ///  Total length (in bytes) for all extrinsics put together, for the current block.
        /// </summary>
        [HttpGet("AllExtrinsicsLen")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.U32), 200)]
        public IActionResult GetAllExtrinsicsLen()
        {
            return this.Ok(_systemStorage.GetAllExtrinsicsLen());
        }
        
        /// <summary>
        /// >> BlockHash
        ///  Map of block numbers to block hashes.
        /// </summary>
        [HttpGet("BlockHash")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.PrimitiveTypes.H256), 200)]
        public IActionResult GetBlockHash(string key)
        {
            return this.Ok(_systemStorage.GetBlockHash(key));
        }
        
        /// <summary>
        /// >> ExtrinsicData
        ///  Extrinsics data for the current block (maps an extrinsic&#39;s index to its data).
        /// </summary>
        [HttpGet("ExtrinsicData")]
        [ProducesResponseType(typeof(BaseVec<Ajuna.NetApi.Model.Types.Primitive.U8>), 200)]
        public IActionResult GetExtrinsicData(string key)
        {
            return this.Ok(_systemStorage.GetExtrinsicData(key));
        }
        
        /// <summary>
        /// >> Number
        ///  The current block number being processed. Set by `execute_block`.
        /// </summary>
        [HttpGet("Number")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.U32), 200)]
        public IActionResult GetNumber()
        {
            return this.Ok(_systemStorage.GetNumber());
        }
        
        /// <summary>
        /// >> ParentHash
        ///  Hash of the previous block.
        /// </summary>
        [HttpGet("ParentHash")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.PrimitiveTypes.H256), 200)]
        public IActionResult GetParentHash()
        {
            return this.Ok(_systemStorage.GetParentHash());
        }
        
        /// <summary>
        /// >> Digest
        ///  Digest of the current block, also part of the block header.
        /// </summary>
        [HttpGet("Digest")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.SpRuntime.Digest), 200)]
        public IActionResult GetDigest()
        {
            return this.Ok(_systemStorage.GetDigest());
        }
        
        /// <summary>
        /// >> Events
        ///  Events deposited for the current block.
        /// 
        ///  NOTE: This storage item is explicitly unbounded since it is never intended to be read
        ///  from within the runtime.
        /// </summary>
        [HttpGet("Events")]
        [ProducesResponseType(typeof(BaseVec<Ajuna.NetApi.Model.FrameSystem.EventRecord>), 200)]
        public IActionResult GetEvents()
        {
            return this.Ok(_systemStorage.GetEvents());
        }
        
        /// <summary>
        /// >> EventCount
        ///  The number of events in the `Events&lt;T&gt;` list.
        /// </summary>
        [HttpGet("EventCount")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.U32), 200)]
        public IActionResult GetEventCount()
        {
            return this.Ok(_systemStorage.GetEventCount());
        }
        
        /// <summary>
        /// >> EventTopics
        ///  Mapping between a topic (represented by T::Hash) and a vector of indexes
        ///  of events in the `&lt;Events&lt;T&gt;&gt;` list.
        /// 
        ///  All topic vectors have deterministic storage locations depending on the topic. This
        ///  allows light-clients to leverage the changes trie storage tracking mechanism and
        ///  in case of changes fetch the list of events of interest.
        /// 
        ///  The value has the type `(T::BlockNumber, EventIndex)` because if we used only just
        ///  the `EventIndex` then in case if the topic has the same contents on the next block
        ///  no notification will be triggered thus the event might be lost.
        /// </summary>
        [HttpGet("EventTopics")]
        [ProducesResponseType(typeof(BaseVec<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32,Ajuna.NetApi.Model.Types.Primitive.U32>>), 200)]
        public IActionResult GetEventTopics(string key)
        {
            return this.Ok(_systemStorage.GetEventTopics(key));
        }
        
        /// <summary>
        /// >> LastRuntimeUpgrade
        ///  Stores the `spec_version` and `spec_name` of when the last runtime upgrade happened.
        /// </summary>
        [HttpGet("LastRuntimeUpgrade")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.FrameSystem.LastRuntimeUpgradeInfo), 200)]
        public IActionResult GetLastRuntimeUpgrade()
        {
            return this.Ok(_systemStorage.GetLastRuntimeUpgrade());
        }
        
        /// <summary>
        /// >> UpgradedToU32RefCount
        ///  True if we have upgraded so that `type RefCount` is `u32`. False (default) if not.
        /// </summary>
        [HttpGet("UpgradedToU32RefCount")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.Bool), 200)]
        public IActionResult GetUpgradedToU32RefCount()
        {
            return this.Ok(_systemStorage.GetUpgradedToU32RefCount());
        }
        
        /// <summary>
        /// >> UpgradedToTripleRefCount
        ///  True if we have upgraded so that AccountInfo contains three types of `RefCount`. False
        ///  (default) if not.
        /// </summary>
        [HttpGet("UpgradedToTripleRefCount")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.Bool), 200)]
        public IActionResult GetUpgradedToTripleRefCount()
        {
            return this.Ok(_systemStorage.GetUpgradedToTripleRefCount());
        }
        
        /// <summary>
        /// >> ExecutionPhase
        ///  The execution phase of the block.
        /// </summary>
        [HttpGet("ExecutionPhase")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.FrameSystem.EnumPhase), 200)]
        public IActionResult GetExecutionPhase()
        {
            return this.Ok(_systemStorage.GetExecutionPhase());
        }
    }
}