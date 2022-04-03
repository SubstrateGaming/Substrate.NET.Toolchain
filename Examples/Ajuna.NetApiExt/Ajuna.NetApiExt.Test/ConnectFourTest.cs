using Ajuna.NetApi;
using Ajuna.NetApi.Model.Extrinsics;
using Ajuna.NetApi.Model.PalletConnectfour;
using Ajuna.NetApi.Model.Rpc;
using Ajuna.NetApi.Model.SpCore;
using Ajuna.NetApi.Model.Types;
using Ajuna.NetApi.Model.Types.Primitive;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using Schnorrkel.Keys;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ajuna.NetApiExt.Test
{
    public class ConnectFourTest
    {
        private const string WebSocketUrl = "ws://127.0.0.1:9944";

        private SubstrateClientExt _substrateClient;


        // Secret Key URI `//Alice` is account:
        // Secret seed:      0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a
        // Public key(hex):  0xd43593c715fdd31c61141abd04a99fd6822c8558854ccde39a5684e7a56da27d
        // Account ID:       0xd43593c715fdd31c61141abd04a99fd6822c8558854ccde39a5684e7a56da27d
        // SS58 Address:     5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY
        public MiniSecret MiniSecretAlice => new MiniSecret(Utils.HexToByteArray("0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a"), ExpandMode.Ed25519);
        public Account Alice => Account.Build(KeyType.Sr25519, MiniSecretAlice.ExpandToSecret().ToBytes(), MiniSecretAlice.GetPair().Public.Key);

        // Secret Key URI `//Bob` is account:
        // Secret seed:      0x398f0c28f98885e046333d4a41c19cee4c37368a9832c6502f6cfd182e2aef89
        // Public key(hex):  0x8eaf04151687736326c9fea17e25fc5287613693c912909cb226aa4794f26a48
        // Account ID:       0x8eaf04151687736326c9fea17e25fc5287613693c912909cb226aa4794f26a48
        // SS58 Address:     5FHneW46xGXgs5mUiveU4sbTyGBzmstUspZC92UhjJM694ty
        public MiniSecret MiniSecretBob => new MiniSecret(Utils.HexToByteArray("0x398f0c28f98885e046333d4a41c19cee4c37368a9832c6502f6cfd182e2aef89"), ExpandMode.Ed25519);
        public Account Bob => Account.Build(KeyType.Sr25519, MiniSecretBob.ExpandToSecret().ToBytes(), MiniSecretBob.GetPair().Public.Key);


        [SetUp]
        public void Setup()
        {
            var config = new LoggingConfiguration();

            // Targets where to log to: File and Console
            var console = new ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, console);

            // Apply config           
            LogManager.Configuration = config;

            _substrateClient = new SubstrateClientExt(new Uri(WebSocketUrl));
        }

        [TearDown]
        public void TearDown()
        {
            _substrateClient.Dispose();
        }

        /// <summary>
        /// Simple extrinsic tester
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="extrinsicUpdate"></param>
        static void ActionExtrinsicUpdate(string subscriptionId, ExtrinsicStatus extrinsicUpdate)
        {
            switch (extrinsicUpdate.ExtrinsicState)
            {
                case ExtrinsicState.None:
                    Assert.IsTrue(true);
                    Assert.IsTrue(extrinsicUpdate.InBlock.Value.Length > 0 || extrinsicUpdate.Finalized.Value.Length > 0);
                    break;
                case ExtrinsicState.Future:
                    Assert.IsTrue(false);
                    break;
                case ExtrinsicState.Ready:
                    Assert.IsTrue(true);
                    break;
                case ExtrinsicState.Dropped:
                    Assert.IsTrue(false);
                    break;
                case ExtrinsicState.Invalid:
                    Assert.IsTrue(false);
                    break;
            }
        }

        [Test]
        public async Task QueueTestAsync()
        {
            var extrinsic_wait = 5000;

            var cts = new CancellationTokenSource();
            await _substrateClient.ConnectAsync(cts.Token);

            //
            var emptyQueueMethod = NetApi.Model.PalletConnectFour.ConnectFourCalls.EmptyQueue();
            _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate, emptyQueueMethod, Alice, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
            Thread.Sleep(extrinsic_wait);

            // 
            var queueMethod = NetApi.Model.PalletConnectFour.ConnectFourCalls.Queue();
            _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate, queueMethod, Alice, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
            Thread.Sleep(extrinsic_wait);

            // 
            _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate, emptyQueueMethod, Alice, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
            Thread.Sleep(extrinsic_wait);

            Assert.IsTrue(true);
        }

        [Test]
        public async Task NewGameTestAsync()
        {
            var extrinsic_wait = 5000;

            var cts = new CancellationTokenSource();
            await _substrateClient.ConnectAsync(cts.Token);

            var accountId32Alice = new AccountId32();
            accountId32Alice.Create("0xd43593c715fdd31c61141abd04a99fd6822c8558854ccde39a5684e7a56da27d");

            var accountId32Bob = new AccountId32();
            accountId32Bob.Create("0x8eaf04151687736326c9fea17e25fc5287613693c912909cb226aa4794f26a48");

            // 
            var newGameMethod = NetApi.Model.PalletConnectFour.ConnectFourCalls.NewGame(accountId32Bob);
            _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate, newGameMethod, Alice, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
            Thread.Sleep(extrinsic_wait);


            var board_id_a = await _substrateClient.ConnectFourStorage.PlayerBoard(accountId32Alice, cts.Token);
            Assert.AreEqual("H256", board_id_a.GetType().Name);

            var board_id_b = await _substrateClient.ConnectFourStorage.PlayerBoard(accountId32Bob, cts.Token);
            Assert.AreEqual("H256", board_id_b.GetType().Name);

            Assert.AreEqual(board_id_a.Value.Bytes, board_id_b.Value.Bytes);

            var board1 = await _substrateClient.ConnectFourStorage.Boards(board_id_a, cts.Token);
            Assert.AreEqual("BoardStruct", board1.GetType().Name);

            var boardStruct1 = board1 as BoardStruct;

            Assert.AreEqual(board_id_a.Value.Bytes, boardStruct1.Id.Value.Bytes);
            Assert.AreEqual(Alice.Value, Utils.GetAddressFrom(boardStruct1.Red.Value.Bytes));
            Assert.AreEqual(Bob.Value, Utils.GetAddressFrom(boardStruct1.Blue.Value.Bytes));
            Assert.IsTrue(boardStruct1.LastTurn.Value > 0);
            //Assert.AreEqual(2, boardStruct.LastTurn.Value);
            Assert.IsTrue(boardStruct1.NextPlayer.Value > 0 && boardStruct1.NextPlayer.Value <= 2);
            //Assert.AreEqual(2, boardStruct.NextPlayer.Value);
            Assert.AreEqual(BoardState.Running, boardStruct1.BoardState.Value);

            var col0 = new U8();
            col0.Create(0);

            var col1 = new U8();
            col1.Create(1);

            var player1Start = false;
            if (boardStruct1.NextPlayer.Value == 1)
            {
                _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate,
                    NetApi.Model.PalletConnectFour.ConnectFourCalls.PlayTurn(col0), Alice, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
                Thread.Sleep(extrinsic_wait);
                player1Start = true;
            }

            _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate,
                NetApi.Model.PalletConnectFour.ConnectFourCalls.PlayTurn(col1), Bob, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
            Thread.Sleep(extrinsic_wait);

            _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate,
                NetApi.Model.PalletConnectFour.ConnectFourCalls.PlayTurn(col0), Alice, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
            Thread.Sleep(extrinsic_wait);

            _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate,
                NetApi.Model.PalletConnectFour.ConnectFourCalls.PlayTurn(col1), Bob, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
            Thread.Sleep(extrinsic_wait);

            _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate,
                NetApi.Model.PalletConnectFour.ConnectFourCalls.PlayTurn(col0), Alice, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
            Thread.Sleep(extrinsic_wait);

            _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate,
                NetApi.Model.PalletConnectFour.ConnectFourCalls.PlayTurn(col1), Bob, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
            Thread.Sleep(extrinsic_wait);

            _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate,
                NetApi.Model.PalletConnectFour.ConnectFourCalls.PlayTurn(col0), Alice, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
            Thread.Sleep(extrinsic_wait);

            var board2 = await _substrateClient.ConnectFourStorage.Boards(board_id_a, cts.Token);
            var boardStruct2 = board2 as BoardStruct;

            if (player1Start)
            {
                Assert.AreEqual("0x000001010101000000020202000000000000000000000000000000000000000000000000000000000000", Utils.Bytes2HexString(boardStruct2.Board.Bytes));
                Assert.AreEqual(BoardState.Finished, boardStruct2.BoardState.Value);
                Assert.AreEqual("AccountId32", boardStruct2.BoardState.Value2.GetType().Name);
                Assert.AreEqual(boardStruct1.Red.Value.Bytes, (boardStruct2.BoardState.Value2 as AccountId32).Value.Bytes);
            }
            else
            {
                Assert.AreEqual("0x000000010101000000020202000000000000000000000000000000000000000000000000000000000000", Utils.Bytes2HexString(boardStruct2.Board.Bytes));
                _ = await _substrateClient.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate,
                    NetApi.Model.PalletConnectFour.ConnectFourCalls.PlayTurn(col1), Bob, new ChargeAssetTxPayment(0, 64), (uint)extrinsic_wait, cts.Token);
                Thread.Sleep(extrinsic_wait);

                var board3 = await _substrateClient.ConnectFourStorage.Boards(board_id_a, cts.Token);
                var boardStruct3 = board3 as BoardStruct;

                Assert.AreEqual("0x000000010101000002020202000000000000000000000000000000000000000000000000000000000000", Utils.Bytes2HexString(boardStruct3.Board.Bytes));
                Assert.AreEqual(BoardState.Finished, boardStruct3.BoardState.Value);
                Assert.AreEqual("AccountId32", boardStruct3.BoardState.Value2.GetType().Name);
                Assert.AreEqual(boardStruct1.Blue.Value.Bytes, (boardStruct3.BoardState.Value2 as AccountId32).Value.Bytes);
            }
        }
    }
}