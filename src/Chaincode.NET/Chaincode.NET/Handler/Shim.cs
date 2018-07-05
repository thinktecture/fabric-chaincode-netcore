using System;
using System.Threading.Tasks;
using Chaincode.NET.Chaincode;
using Chaincode.NET.Messaging;
using Chaincode.NET.Settings;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Protos;

namespace Chaincode.NET.Handler
{
    public class Shim
    {
        private readonly IChaincode _chaincode;
        private readonly ChaincodeStubFactory _chaincodeStubFactory;
        private readonly ILogger<Shim> _logger;
        private readonly ILogger<Handler> _handlerLogger;
        private readonly ILogger<MessageQueue> _messageQueueLogger;
        private readonly ChaincodeSettings _chaincodeSettings;

        public Shim(
            IChaincode chaincode,
            IOptions<ChaincodeSettings> chaincodeSettings,
            ChaincodeStubFactory chaincodeStubFactory,
            ILogger<Shim> logger,
            ILogger<Handler> handlerLogger,
            ILogger<MessageQueue> messageQueueLogger
        )
        {
            _chaincode = chaincode;
            _chaincodeStubFactory = chaincodeStubFactory;
            _logger = logger;
            _handlerLogger = handlerLogger;
            _messageQueueLogger = messageQueueLogger;
            _chaincodeSettings = chaincodeSettings.Value;

            logger.LogInformation($"Instantiating shim with chaincode of type {chaincode.GetType().Name}");
            GrpcEnvironment.SetLogger(new ConsoleLogger());
        }

        public async Task<Handler> Start()
        {
            var url = ParseUrl(_chaincodeSettings.PeerAddress);

            // TODO: TLS Stuff?
            // TODO: Handler factory?
            // TODO: MessageQueue factory?

            var handler = new Handler(_chaincode, url.Host, url.Port, _chaincodeStubFactory, _handlerLogger, _messageQueueLogger);

            var chaincodeId = new ChaincodeID {Name = _chaincodeSettings.ChaincodeIdName};

            _logger.LogInformation("Registering with peer " +
                                   $"{url.Host}:{url.Port} as chaincode " +
                                   $"{_chaincodeSettings.ChaincodeIdName}");

            await handler.Chat(new ChaincodeMessage()
            {
                Type = ChaincodeMessage.Types.Type.Register,
                Payload = chaincodeId.ToByteString()
            });

            return handler;
        }

        private (string Host, int Port) ParseUrl(string peerAddress)
        {
            if (peerAddress.Contains("://"))
            {
                throw new Exception("Peer Address should not contain any protocol information.");
            }

            var split = peerAddress.Split(':');

            if (split.Length != 2)
            {
                throw new ArgumentException("Please provide peer address in the format of host:port");
            }

            return (split[0], int.Parse(split[1]));
        }

        public static Response Success(ByteString payload) => new Response()
        {
            Status = (int) ResponseCodes.Ok,
            Payload = payload
        };

        public static Response Error(string message) => new Response()
        {
            Status = (int) ResponseCodes.Error,
            Message = message
        };
    }
}
