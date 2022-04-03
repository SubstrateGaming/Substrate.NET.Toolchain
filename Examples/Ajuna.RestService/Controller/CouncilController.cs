//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ajuna.Infrastructure.Storages;
using Ajuna.NetApi.Model.FrameSupport;
using Ajuna.NetApi.Model.NodeRuntime;
using Ajuna.NetApi.Model.PalletCollective;
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
    /// CouncilController controller to access storages.
    /// </summary>
    [ApiController()]
    [Route("[controller]")]
    public sealed class CouncilController : ControllerBase
    {
        
        private ICouncilStorage _councilStorage;
        
        /// <summary>
        /// CouncilController constructor.
        /// </summary>
        public CouncilController(ICouncilStorage councilStorage)
        {
            _councilStorage = councilStorage;
        }
        
        /// <summary>
        /// >> Proposals
        ///  The hashes of the active proposals.
        /// </summary>
        [HttpGet("Proposals")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.FrameSupport.BoundedVecT7), 200)]
        public IActionResult GetProposals()
        {
            return this.Ok(_councilStorage.GetProposals());
        }
        
        /// <summary>
        /// >> ProposalOf
        ///  Actual proposal for a given hash, if it&#39;s current.
        /// </summary>
        [HttpGet("ProposalOf")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.NodeRuntime.EnumNodeCall), 200)]
        public IActionResult GetProposalOf(string key)
        {
            return this.Ok(_councilStorage.GetProposalOf(key));
        }
        
        /// <summary>
        /// >> Voting
        ///  Votes on a given proposal, if it is ongoing.
        /// </summary>
        [HttpGet("Voting")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.PalletCollective.Votes), 200)]
        public IActionResult GetVoting(string key)
        {
            return this.Ok(_councilStorage.GetVoting(key));
        }
        
        /// <summary>
        /// >> ProposalCount
        ///  Proposals so far.
        /// </summary>
        [HttpGet("ProposalCount")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.U32), 200)]
        public IActionResult GetProposalCount()
        {
            return this.Ok(_councilStorage.GetProposalCount());
        }
        
        /// <summary>
        /// >> Members
        ///  The current members of the collective. This is stored sorted (just by value).
        /// </summary>
        [HttpGet("Members")]
        [ProducesResponseType(typeof(BaseVec<Ajuna.NetApi.Model.SpCore.AccountId32>), 200)]
        public IActionResult GetMembers()
        {
            return this.Ok(_councilStorage.GetMembers());
        }
        
        /// <summary>
        /// >> Prime
        ///  The prime member that helps determine the default vote behavior in case of absentations.
        /// </summary>
        [HttpGet("Prime")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.SpCore.AccountId32), 200)]
        public IActionResult GetPrime()
        {
            return this.Ok(_councilStorage.GetPrime());
        }
    }
}
