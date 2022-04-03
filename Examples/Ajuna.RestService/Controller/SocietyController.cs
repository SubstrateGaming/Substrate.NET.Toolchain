//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.Infrastructure.Storages;
using Ajuna.NetApi.Model.PalletSociety;
using Ajuna.NetApi.Model.PrimitiveTypes;
using Ajuna.NetApi.Model.SpCore;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Ajuna.Infrastructure.RestService.Controller
{
    
    
    /// <summary>
    /// SocietyController controller to access storages.
    /// </summary>
    [ApiController()]
    [Route("[controller]")]
    public sealed class SocietyController : ControllerBase
    {
        
        private ISocietyStorage _societyStorage;
        
        /// <summary>
        /// SocietyController constructor.
        /// </summary>
        public SocietyController(ISocietyStorage societyStorage)
        {
            _societyStorage = societyStorage;
        }
        
        /// <summary>
        /// >> Founder
        ///  The first member.
        /// </summary>
        [HttpGet("Founder")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.SpCore.AccountId32), 200)]
        public IActionResult GetFounder()
        {
            return this.Ok(_societyStorage.GetFounder());
        }
        
        /// <summary>
        /// >> Rules
        ///  A hash of the rules of this society concerning membership. Can only be set once and
        ///  only by the founder.
        /// </summary>
        [HttpGet("Rules")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.PrimitiveTypes.H256), 200)]
        public IActionResult GetRules()
        {
            return this.Ok(_societyStorage.GetRules());
        }
        
        /// <summary>
        /// >> Candidates
        ///  The current set of candidates; bidders that are attempting to become members.
        /// </summary>
        [HttpGet("Candidates")]
        [ProducesResponseType(typeof(BaseVec<Ajuna.NetApi.Model.PalletSociety.Bid>), 200)]
        public IActionResult GetCandidates()
        {
            return this.Ok(_societyStorage.GetCandidates());
        }
        
        /// <summary>
        /// >> SuspendedCandidates
        ///  The set of suspended candidates.
        /// </summary>
        [HttpGet("SuspendedCandidates")]
        [ProducesResponseType(typeof(BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U128,Ajuna.NetApi.Model.PalletSociety.EnumBidKind>), 200)]
        public IActionResult GetSuspendedCandidates(string key)
        {
            return this.Ok(_societyStorage.GetSuspendedCandidates(key));
        }
        
        /// <summary>
        /// >> Pot
        ///  Amount of our account balance that is specifically for the next round&#39;s bid(s).
        /// </summary>
        [HttpGet("Pot")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.U128), 200)]
        public IActionResult GetPot()
        {
            return this.Ok(_societyStorage.GetPot());
        }
        
        /// <summary>
        /// >> Head
        ///  The most primary from the most recently approved members.
        /// </summary>
        [HttpGet("Head")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.SpCore.AccountId32), 200)]
        public IActionResult GetHead()
        {
            return this.Ok(_societyStorage.GetHead());
        }
        
        /// <summary>
        /// >> Members
        ///  The current set of members, ordered.
        /// </summary>
        [HttpGet("Members")]
        [ProducesResponseType(typeof(BaseVec<Ajuna.NetApi.Model.SpCore.AccountId32>), 200)]
        public IActionResult GetMembers()
        {
            return this.Ok(_societyStorage.GetMembers());
        }
        
        /// <summary>
        /// >> SuspendedMembers
        ///  The set of suspended members.
        /// </summary>
        [HttpGet("SuspendedMembers")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.Bool), 200)]
        public IActionResult GetSuspendedMembers(string key)
        {
            return this.Ok(_societyStorage.GetSuspendedMembers(key));
        }
        
        /// <summary>
        /// >> Bids
        ///  The current bids, stored ordered by the value of the bid.
        /// </summary>
        [HttpGet("Bids")]
        [ProducesResponseType(typeof(BaseVec<Ajuna.NetApi.Model.PalletSociety.Bid>), 200)]
        public IActionResult GetBids()
        {
            return this.Ok(_societyStorage.GetBids());
        }
        
        /// <summary>
        /// >> Vouching
        ///  Members currently vouching or banned from vouching again
        /// </summary>
        [HttpGet("Vouching")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.PalletSociety.EnumVouchingStatus), 200)]
        public IActionResult GetVouching(string key)
        {
            return this.Ok(_societyStorage.GetVouching(key));
        }
        
        /// <summary>
        /// >> Payouts
        ///  Pending payouts; ordered by block number, with the amount that should be paid out.
        /// </summary>
        [HttpGet("Payouts")]
        [ProducesResponseType(typeof(BaseVec<BaseTuple<Ajuna.NetApi.Model.Types.Primitive.U32,Ajuna.NetApi.Model.Types.Primitive.U128>>), 200)]
        public IActionResult GetPayouts(string key)
        {
            return this.Ok(_societyStorage.GetPayouts(key));
        }
        
        /// <summary>
        /// >> Strikes
        ///  The ongoing number of losing votes cast by the member.
        /// </summary>
        [HttpGet("Strikes")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.U32), 200)]
        public IActionResult GetStrikes(string key)
        {
            return this.Ok(_societyStorage.GetStrikes(key));
        }
        
        /// <summary>
        /// >> Votes
        ///  Double map from Candidate -&gt; Voter -&gt; (Maybe) Vote.
        /// </summary>
        [HttpGet("Votes")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.PalletSociety.EnumVote), 200)]
        public IActionResult GetVotes(string key)
        {
            return this.Ok(_societyStorage.GetVotes(key));
        }
        
        /// <summary>
        /// >> Defender
        ///  The defending member currently being challenged.
        /// </summary>
        [HttpGet("Defender")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.SpCore.AccountId32), 200)]
        public IActionResult GetDefender()
        {
            return this.Ok(_societyStorage.GetDefender());
        }
        
        /// <summary>
        /// >> DefenderVotes
        ///  Votes for the defender.
        /// </summary>
        [HttpGet("DefenderVotes")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.PalletSociety.EnumVote), 200)]
        public IActionResult GetDefenderVotes(string key)
        {
            return this.Ok(_societyStorage.GetDefenderVotes(key));
        }
        
        /// <summary>
        /// >> MaxMembers
        ///  The max number of members for the society at one time.
        /// </summary>
        [HttpGet("MaxMembers")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.U32), 200)]
        public IActionResult GetMaxMembers()
        {
            return this.Ok(_societyStorage.GetMaxMembers());
        }
    }
}
