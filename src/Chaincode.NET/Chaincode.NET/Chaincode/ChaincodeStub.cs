using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Chaincode.NET.Extensions;
using Chaincode.NET.Handler;
using Chaincode.NET.Handler.Iterators;
using Common;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Msp;
using Protos;

namespace Chaincode.NET.Chaincode
{
    public class ChaincodeStubFactory : IChaincodeStubFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ChaincodeStubFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ChaincodeStub CreateChaincodeStub(
            IHandler handler,
            string channelId,
            string txId,
            ChaincodeInput chaincodeInput,
            SignedProposal signedProposal
        )
        {
            using (var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                return new ChaincodeStub(handler, channelId, txId, chaincodeInput, signedProposal,
                    scope.ServiceProvider.GetRequiredService<ILogger<ChaincodeStub>>());
            }
        }
    }

    public class ChaincodeStub
    {
        private const char EmptyKeySubstitute = '\x01';
        private const char MinUnicodeRuneValue = '\u0000';
        private const char CompositekeyNs = '\x00';
        private readonly string MaxUnicodeRuneValue = char.ConvertFromUtf32(0x10ffff);

        private readonly IHandler _handler;
        private readonly ChaincodeInput _chaincodeInput;
        private readonly ILogger<ChaincodeStub> _logger;
        private Proposal _proposal;

        // ReSharper disable MemberCanBePrivate.Global
        public ChaincodeEvent ChaincodeEvent { get; private set; }
        public string Binding { get; private set; }
        public Timestamp TxTimestamp { get; private set; }
        public DecodedSignedProposal DecodedSignedProposal { get; private set; }
        public MapField<string, ByteString> TransientMap { get; private set; }
        public string ChannelId { get; private set; }
        public string TxId { get; private set; }
        public IList<string> Args { get; private set; }

        public SerializedIdentity Creator { get; private set; }
        // ReSharper restore MemberCanBePrivate.Global

        public ChaincodeStub(
            IHandler handler,
            string channelId,
            string txId,
            ChaincodeInput chaincodeInput,
            SignedProposal signedProposal,
            ILogger<ChaincodeStub> logger
        )
        {
            _handler = handler;
            ChannelId = channelId;
            TxId = txId;
            _chaincodeInput = chaincodeInput;
            _logger = logger;

            Args = chaincodeInput.Args.Select(entry => entry.ToStringUtf8()).ToList();

            DecodedSignedProposal = ValidateSignedProposal(signedProposal);
        }

        public ChaincodeFunctionParameterInformation GetFunctionAndParameters()
        {
            if (Args.Count < 1)
            {
                return null;
            }

            var result = new ChaincodeFunctionParameterInformation
            {
                Function = Args.First().ToLower(),
                Parameters =
                    Args.Skip(1)
                        .ToList() // TODO: For usage later wrap this into a class and provide nice methods for access
            };

            return result;
        }

        private DecodedSignedProposal ValidateSignedProposal(SignedProposal signedProposal)
        {
            if (signedProposal == null)
            {
                return null;
            }

            var decodedSignedProposal = new DecodedSignedProposal()
            {
                Signature = signedProposal.Signature
            };

            try
            {
                _proposal = Proposal.Parser.ParseFrom(signedProposal.ProposalBytes);
                decodedSignedProposal.Proposal = new ChaincodeProposal();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed extracting proposal from signedProposal: {ex}");
            }

            if (_proposal.Header == null || _proposal.Header.Length == 0)
            {
                throw new Exception("Proposal header is empty");
            }

            if (_proposal.Payload == null || _proposal.Payload.Length == 0)
            {
                throw new Exception("Proposal payload is empty");
            }

            Header header;

            try
            {
                header = Header.Parser.ParseFrom(_proposal.Header);
                decodedSignedProposal.Proposal.Header = new ChaincodeProposalHeader();
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not extract the header from the proposal: {ex}");
            }

            SignatureHeader signatureHeader;

            try
            {
                signatureHeader = SignatureHeader.Parser.ParseFrom(header.SignatureHeader);
                decodedSignedProposal.Proposal.Header.SignatureHeader =
                    new ChaincodeProposalHeaderSignatureHeader {Nonce = signatureHeader.Nonce};
            }
            catch (Exception ex)
            {
                throw new Exception($"Decoding SignatureHeader failed: {ex}");
            }

            try
            {
                var creator = SerializedIdentity.Parser.ParseFrom(signatureHeader.Creator);
                decodedSignedProposal.Proposal.Header.SignatureHeader.Creator = creator;
                Creator = creator;
            }
            catch (Exception ex)
            {
                throw new Exception($"Decoding SerializedIdentity failed: {ex}");
            }

            try
            {
                var channelHeader = ChannelHeader.Parser.ParseFrom(header.ChannelHeader);
                decodedSignedProposal.Proposal.Header.ChannelHeader = channelHeader;

                TxTimestamp = channelHeader.Timestamp;
            }
            catch (Exception ex)
            {
                throw new Exception($"Decoding ChannelHeader failed: {ex}");
            }

            ChaincodeProposalPayload payload;

            try
            {
                payload = ChaincodeProposalPayload.Parser.ParseFrom(_proposal.Payload);
                decodedSignedProposal.Proposal.Payload = payload;
            }
            catch (Exception ex)
            {
                throw new Exception($"Decoding ChaincodeProposalPayload failed: {ex}");
            }

            TransientMap = payload.TransientMap;

            Binding = ComputeProposalBinding(decodedSignedProposal);

            return decodedSignedProposal;
        }

        private string ComputeProposalBinding(DecodedSignedProposal decodedSignedProposal)
        {
            var nonce = decodedSignedProposal.Proposal.Header.SignatureHeader.Nonce.ToByteArray();
            var creator = decodedSignedProposal.Proposal.Header.SignatureHeader.Creator.ToByteArray();
            var epoch = decodedSignedProposal.Proposal.Header.ChannelHeader.Epoch;

            var epochBuffer = BitConverter.GetBytes(epoch);

            var total = nonce.Concat(creator).Concat(epochBuffer);

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(total.ToArray()).ByteArrayToString();
            }
        }

        public Task<ByteString> GetState(string key)
        {
            _logger.LogInformation($"{nameof(GetState)} called with key: {key}");

            return _handler.HandleGetState(string.Empty, key, ChannelId, TxId);
        }

        public Task<ByteString> PutState(string key, ByteString value) =>
            _handler.HandlePutState(string.Empty, key, value, ChannelId, TxId);

        public Task<ByteString> DeleteState(string key) =>
            _handler.HandleDeleteState(string.Empty, key, ChannelId, TxId);

        public Task<StateQueryIterator> GetStateByRange(string startKey, string endKey)
        {
            if (string.IsNullOrEmpty(startKey))
            {
                startKey = EmptyKeySubstitute.ToString();
            }

            return _handler.HandleGetStateByRange(string.Empty, startKey, endKey, ChannelId, TxId);
        }

        public Task<StateQueryIterator> GetQueryResult(string query) =>
            _handler.HandleGetQueryResult(string.Empty, query, ChannelId, TxId);

        public Task<HistoryQueryIterator> GetHistoryForKey(string key) =>
            _handler.HandleGetHistoryForKey(key, ChannelId, TxId);

        public Task InvokeChaincode(string chaincodeName, IEnumerable<ByteString> args, string channel = "")
        {
            if (!string.IsNullOrEmpty(channel))
            {
                chaincodeName = $"{chaincodeName}/{channel}";
            }

            return _handler.HandleInvokeChaincode(chaincodeName, args, ChannelId, TxId);
        }

        public void SetEvent(string name, ByteString payload)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Event name must be a non-empty string");
            }

            var @event = new ChaincodeEvent()
            {
                EventName = name,
                Payload = payload
            };

            ChaincodeEvent = @event;
        }

        public string CreateCompositeKey(string objectType, IEnumerable<string> attributes)
        {
            ValidateCompositeKeyAttribute(objectType);

            var compositeKey = CompositekeyNs + objectType + MinUnicodeRuneValue;

            foreach (var attribute in attributes)
            {
                ValidateCompositeKeyAttribute(attribute);
                compositeKey += attribute + MinUnicodeRuneValue;
            }

            return compositeKey;
        }

        public (string ObjectType, IList<string> Attributes) SplitCompositeKey(string compositeKey)
        {
            if (string.IsNullOrEmpty(compositeKey) || compositeKey[0] != CompositekeyNs)
            {
                return (null, new string[] { });
            }

            var splitKey = compositeKey.Substring(1).Split(MinUnicodeRuneValue).ToList();
            string objectType = null;
            var attributes = new List<string>();

            if (splitKey.Count > 0 && !string.IsNullOrEmpty(splitKey[0]))
            {
                objectType = splitKey[0];
                splitKey.RemoveAt(splitKey.Count - 1);

                if (splitKey.Count > 1)
                {
                    splitKey.RemoveAt(0);
                    attributes = splitKey;
                }
            }

            return (objectType, attributes);
        }

        public Task<StateQueryIterator> GetStateByPartialCompositeKey(string objectType, IList<string> attributes)
        {
            var partialCompositeKey = CreateCompositeKey(objectType, attributes);

            return GetStateByRange(partialCompositeKey, partialCompositeKey + MaxUnicodeRuneValue);
        }

        public Task<ByteString> GetPrivateData(string collection, string key)
        {
            ValidateCollection(collection);
            return _handler.HandleGetState(collection, key, ChannelId, TxId);
        }


        public Task<ByteString> PutPrivateData(string collection, string key, ByteString value)
        {
            ValidateCollection(collection);
            return _handler.HandlePutState(collection, key, value, ChannelId, TxId);
        }

        public Task<ByteString> DeletePrivateData(string collection, string key)
        {
            ValidateCollection(collection);
            return _handler.HandleDeleteState(collection, key, ChannelId, TxId);
        }

        public Task<StateQueryIterator> GetPrivateDataByRange(string collection, string startKey, string endKey)
        {
            ValidateCollection(collection);
            
            if (string.IsNullOrEmpty(startKey))
            {
                startKey = EmptyKeySubstitute.ToString();
            }

            return _handler.HandleGetStateByRange(collection, startKey, endKey, ChannelId, TxId);
        }

        public Task<StateQueryIterator> GetPrivateDataByPartialCompositeKey(
            string collection,
            string objectType,
            IList<string> attributes
        )
        {
            ValidateCollection(collection);
            var partialCompositeKey = CreateCompositeKey(objectType, attributes);

            return GetPrivateDataByRange(collection, partialCompositeKey, partialCompositeKey + MaxUnicodeRuneValue);
        }

        public Task<StateQueryIterator> GetPrivateDataQueryResult(string collection, string query)
        {
            ValidateCollection(collection);
            return _handler.HandleGetQueryResult(collection, query, ChannelId, TxId);
        }

        private void ValidateCollection(string collection)
        {
            if (string.IsNullOrEmpty(collection))
            {
                throw new Exception("collection must be a valid string");
            }
        }

        private void ValidateCompositeKeyAttribute(string objectType)
        {
            if (string.IsNullOrEmpty(objectType))
            {
                throw new Exception("objectType or attribute not a non-zero length string");
            }
        }
    }

    public class ChaincodeProposalHeader
    {
        public ChaincodeProposalHeaderSignatureHeader SignatureHeader { get; set; }
        public ChannelHeader ChannelHeader { get; set; }
    }

    public class ChaincodeProposalHeaderSignatureHeader
    {
        public ByteString Nonce { get; set; }
        public SerializedIdentity Creator { get; set; }
    }

    public class ChaincodeProposal
    {
        public ChaincodeProposalHeader Header { get; set; }
        public ChaincodeProposalPayload Payload { get; set; }
    }

    public class DecodedSignedProposal
    {
        public ByteString Signature { get; set; }
        public ChaincodeProposal Proposal { get; set; }
    }
}
