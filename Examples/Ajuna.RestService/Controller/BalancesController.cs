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
using Ajuna.NetApi.Model.PalletBalances;
using Ajuna.NetApi.Model.SpCore;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Ajuna.Infrastructure.RestService.Controller
{
    
    
    [ApiController()]
    [Route("[controller]")]
    public sealed class BalancesController : ControllerBase
    {
        
        private IBalancesStorage _balancesStorage;
        
        public BalancesController(IBalancesStorage balancesStorage)
        {
            _balancesStorage = balancesStorage;
        }
        
        /// <summary>
        /// >> TotalIssuance
        ///  The total units issued in the system.
        /// </summary>
        [HttpGet("TotalIssuance")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.Types.Primitive.U128), 200)]
        public IActionResult GetTotalIssuance()
        {
            return this.Ok(_balancesStorage.GetTotalIssuance());
        }
        
        /// <summary>
        /// >> Account
        ///  The balance of an account.
        /// 
        ///  NOTE: This is only used in the case that this pallet is used to store balances.
        /// </summary>
        [HttpGet("Account")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.PalletBalances.AccountData), 200)]
        public IActionResult GetAccount(string key)
        {
            return this.Ok(_balancesStorage.GetAccount(key));
        }
        
        /// <summary>
        /// >> Locks
        ///  Any liquidity locks on some account balances.
        ///  NOTE: Should only be accessed when setting, changing and freeing a lock.
        /// </summary>
        [HttpGet("Locks")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.FrameSupport.WeakBoundedVecT2), 200)]
        public IActionResult GetLocks(string key)
        {
            return this.Ok(_balancesStorage.GetLocks(key));
        }
        
        /// <summary>
        /// >> Reserves
        ///  Named reserves on some account balances.
        /// </summary>
        [HttpGet("Reserves")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.FrameSupport.BoundedVecT6), 200)]
        public IActionResult GetReserves(string key)
        {
            return this.Ok(_balancesStorage.GetReserves(key));
        }
        
        /// <summary>
        /// >> StorageVersion
        ///  Storage version of the pallet.
        /// 
        ///  This is set to v2.0.0 for new networks.
        /// </summary>
        [HttpGet("StorageVersion")]
        [ProducesResponseType(typeof(Ajuna.NetApi.Model.PalletBalances.EnumReleases), 200)]
        public IActionResult GetStorageVersion()
        {
            return this.Ok(_balancesStorage.GetStorageVersion());
        }
    }
}
