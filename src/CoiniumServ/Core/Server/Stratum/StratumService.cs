﻿/*
 *   Coinium - Crypto Currency Pool Software - https://github.com/CoiniumServ/CoiniumServ
 *   Copyright (C) 2013 - 2014, Coinium Project - http://www.coinium.org
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using AustinHarris.JsonRpc;
using Coinium.Core.Mining;
using Coinium.Core.RPC.Sockets;
using Coinium.Core.Server.Stratum.Responses;

namespace Coinium.Core.Server.Stratum
{
    /// <summary>
    /// Stratum protocol implementation.
    /// </summary>
    public class StratumService : JsonRpcService
    {
        /// <summary>
        /// Subscribes a Miner to allow it to recieve work to begin hashing and submitting shares.
        /// </summary>
        /// <param name="signature">Miner Connection</param>
        [JsonRpcMethod("mining.subscribe")]
        public SubscribeResponse SubscribeMiner(string signature)
        {
            var context = (SocketsRpcContext)JsonRpcContext.Current().Value;
            var miner = (StratumMiner)(context.Miner);

            var response = new SubscribeResponse
            {
                ExtraNonce1 =  ExtraNonce.Instance.NextExtraNonce().ToString("x8"), // Hex-encoded, per-connection unique string which will be used for coinbase serialization later. (http://mining.bitcoin.cz/stratum-mining)
                ExtraNonce2Size = ExtraNonce.ExpectedExtraNonce2Size // Represents expected length of extranonce2 which will be generated by the miner. (http://mining.bitcoin.cz/stratum-mining)
            };

            miner.Subscribe();

            return response;
        }

        /// <summary>
        /// Authorise a miner based on their username and password
        /// </summary>
        /// <param name="user">Worker Username (e.g. "coinium.1").</param>
        /// <param name="password">Worker Password (e.g. "x").</param>
        [JsonRpcMethod("mining.authorize")]
        public bool AuthorizeMiner(string user, string password)
        {
            var context = (SocketsRpcContext)JsonRpcContext.Current().Value;
            var miner = (StratumMiner)(context.Miner);

            return miner.Authenticate(user, password);
        }

        /// <summary>
        /// Allows a miner to submit the work they have done 
        /// </summary>
        /// <param name="user">Worker Username.</param>
        /// <param name="jobId">Job ID(Should be unique per Job to ensure that share diff is recorded properly) </param>
        /// <param name="extranNonce2">Hex-encoded big-endian extranonce2, length depends on extranonce2_size from mining.notify</param>
        /// <param name="nTime"> UNIX timestamp (32bit integer, big-endian, hex-encoded), must be >= ntime provided by mining,notify and <= current time'</param>
        /// <param name="nonce"> 32bit integer hex-encoded, big-endian </param>
        [JsonRpcMethod("mining.submit")]
        public bool SubmitMiner(string user, string jobId, string extranNonce2, string nTime, string nonce)
        {
            var context = (SocketsRpcContext)JsonRpcContext.Current().Value;
            var miner = (StratumMiner)(context.Miner);

            return MiningManager.Instance.ProcessShare(miner, jobId, extranNonce2, nTime, nonce);             
        }
    }
}
