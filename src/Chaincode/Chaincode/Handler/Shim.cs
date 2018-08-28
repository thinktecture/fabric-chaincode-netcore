using System;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Protos;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Settings;
using Thinktecture.IO;

namespace Thinktecture.HyperledgerFabric.Chaincode.Handler
{
    /// <summary>
    /// The shim class provides the service to register the chaincode with the target peer, and
    /// listen for incoming requests from the peer to dispatch to the chaincode in order to process
    /// transaction proposals or execute queries.
    /// </summary>
    public class Shim
    {
        private readonly ChaincodeSettings _chaincodeSettings;
        private readonly IHandlerFactory _handlerFactory;
        private readonly IFile _file;
        private readonly ILogger<Shim> _logger;

        public Shim(
            IOptions<ChaincodeSettings> chaincodeSettings,
            ILogger<Shim> logger,
            IHandlerFactory handlerFactory,
            IFile file
        )
        {
            _logger = logger;
            _handlerFactory = handlerFactory;
            _file = file;
            _chaincodeSettings = chaincodeSettings.Value;

            if (_chaincodeSettings.LogGrpc) GrpcEnvironment.SetLogger(new ConsoleLogger());
        }

        /// <summary>
        /// Call this method to start the chaincode process. After constructing a chaincode object,
        /// pass the object to this function which will initiate a request to register the chaincode
        /// with the target peer. The address of the target peer must be provided via a environment 
        /// variable <code>CORE_PEER_ADDRESS</code> or program argument <code>--core-peer-address</code>.
        /// </summary>
        /// <returns></returns>
        public async Task<IHandler> Start()
        {
            var url = ParseUrl(_chaincodeSettings.PeerAddress);

            var handler = _handlerFactory.Create(url.Host, url.Port, CreateChannelCredentials());

            var chaincodeId = new ChaincodeID {Name = _chaincodeSettings.ChaincodeIdName};

            _logger.LogInformation("Registering with peer " +
                                   $"{url.Host}:{url.Port} as chaincode " +
                                   $"{_chaincodeSettings.ChaincodeIdName}");

            await handler.Chat(new ChaincodeMessage
            {
                Type = ChaincodeMessage.Types.Type.Register,
                Payload = chaincodeId.ToByteString()
            });

            return handler;
        }

        private ChannelCredentials CreateChannelCredentials()
        {
            if (string.IsNullOrEmpty(_chaincodeSettings.PeerTlsRootCertificateFilePath) &&
                string.IsNullOrEmpty(_chaincodeSettings.TlsClientCertFilePath) &&
                string.IsNullOrEmpty(_chaincodeSettings.TlsClientKeyFilePath))
            {
                return ChannelCredentials.Insecure;
            }

            if (!_file.Exists(_chaincodeSettings.PeerTlsRootCertificateFilePath))
            {
                throw new FileNotFoundException(
                    "Could not locate file for environment variable CORE_PEER_TLS_ROOTCERT_FILE",
                    _chaincodeSettings.PeerTlsRootCertificateFilePath);
            }

            if (!_file.Exists(_chaincodeSettings.TlsClientKeyFilePath))
            {
                throw new FileNotFoundException(
                    "Could not locate file for environment variable CORE_TLS_CLIENT_KEY_PATH",
                    _chaincodeSettings.TlsClientKeyFilePath);
            }

            if (!_file.Exists(_chaincodeSettings.TlsClientCertFilePath))
            {
                throw new FileNotFoundException(
                    "Could not locate file for environment variable CORE_TLS_CLIENT_CERT_PATH",
                    _chaincodeSettings.TlsClientCertFilePath);
            }

            return new SslCredentials(
                _file.ReadAllText(_chaincodeSettings.PeerTlsRootCertificateFilePath),
                new KeyCertificatePair(
                    _file.ReadAllText(_chaincodeSettings.TlsClientCertFilePath),
                    _file.ReadAllText(_chaincodeSettings.TlsClientKeyFilePath)
                )
            );
        }

        private (string Host, int Port) ParseUrl(string peerAddress)
        {
            if (peerAddress.Contains("://"))
                throw new Exception("Peer Address should not contain any protocol information.");

            var split = peerAddress.Split(':');

            if (split.Length != 2)
                throw new ArgumentException("Please provide peer address in the format of host:port");

            return (split[0], int.Parse(split[1]));
        }

        /// <summary>
        /// Returns an empty standard response object with status code 200.
        /// </summary>
        /// <returns>The response to be sent back to HLF.</returns>
        public static Response Success()
        {
            return Success(ByteString.Empty);
        }

        /// <summary>
        /// Returns a standard response object with status code 200 and a <paramref name="payload"/>
        /// </summary>
        /// <param name="payload">The payload to be send with the response, can be any content the chaincode wish
        /// to return to the client.</param>
        /// <returns>The response to be sent back to HLF.</returns>
        public static Response Success(ByteString payload)
        {
            return new Response
            {
                Status = (int) ResponseCodes.Ok,
                Payload = payload
            };
        }

        /// <summary>
        /// Returns a standard response object with status code 500 and a message.
        /// </summary>
        /// <param name="message">The message to be sent back to the client.</param>
        /// <returns>The response to be sent back to HLF.</returns>
        public static Response Error(string message)
        {
            return new Response
            {
                Status = (int) ResponseCodes.Error,
                Message = message
            };
        }

        /// <summary>
        /// Returns a standard response object with status code 500 and an exception.
        /// </summary>
        /// <param name="exception">The exception to be sent back to the client.</param>
        /// <returns>The response to be sent back to HLF.</returns>
        public static Response Error(Exception exception)
        {
            return Error(exception.ToString());
        }
    }
}
