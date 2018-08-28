using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Msp;
using Protos;
using Thinktecture.HyperledgerFabric.Chaincode.Handler.Iterators;

namespace Thinktecture.HyperledgerFabric.Chaincode.Chaincode
{
    /// <summary>
    /// The ChaincodeStub is implemented by the <code>fabric-shim</code>
    /// library and passed to the <see cref="IChaincode"/> calls by the Hyperledger Fabric platform.
    /// The stub encapsulates the APIs between the chaincode implementation and the Fabric peer.
    /// </summary>
    public interface IChaincodeStub
    {
        ChaincodeEvent ChaincodeEvent { get; }

        /// <summary>
        /// <para>
        /// Returns a HEX-encoded string of SHA256 hash of the transaction's nonce, creator and epoch concatenated, as a
        /// unique representation of the specific transaction. This value can be used to prevent replay attacks in 
        /// chaincodes that need to authenticate an identity independent of the transaction's submitter.
        /// In a chaincode proposal, the submitter will have been authenticated by the peer such that the identity
        /// returned by <see cref="Creator"> can be trusted. But in some scenarios,
        /// the chaincode needs to authenticate an identity independent of the proposal submitter.
        /// </para>
        /// <para>
        /// For example, Alice is the administrator who installs and instantiates a chaincode that manages assets.
        /// During instantiate Alice assigns the initial owner of the asset to Bob. The chaincode has a function
        /// called <code>transfer()</code> that moves the asset to another identity by changing the asset's "owner"
        /// property to the identity receiving the asset. Naturally only Bob, the current owner, is supposed to be
        /// able to call that function. While the chaincode can rely on <see cref="Creator"/> to check the
        /// submitter's identity and compare that with the current owner, sometimes it's not always possible for the
        /// asset owner itself to submit the transaction. Let's suppose Bob hires a broker agency to handle his trades.
        /// The agency participates in the blockchain network and carry out trades
        /// on behalf of Bob. The chaincode must have a way to authenticate the transaction to ensure it has Bob's
        /// authorization to do the asset transfer. This can be achieved by asking Bob to sign the message, so that
        /// the chaincode can use Bob's certificate, which was obtained during the chaincode instantiate, to verify
        /// the signature and thus ensure the trade was authorized by Bob.
        /// </para>
        /// <para>
        /// Now, to prevent Bob's signature from being re-used in a malicious attack, we want to ensure the signature
        /// is unique. This is where the <code>binding</code> concept comes in. As explained above, the binding
        /// string uniquely represents the transaction where the trade proposal and Bob's authorization is
        /// submitted in. As long as Bob's signature is over the proposal payload and the binding string
        /// concatenated together, namely <code>sigma=Sign(BobSigningKey, tx.Payload||tx.Binding)</code>,
        /// it's guaranteed to be unique and can not be re-used in a different transaction for exploitation.
        /// </para>
        /// </summary>
        string Binding { get; }

        /// <summary>
        /// Returns the timestamp when the transaction was created. This
        /// is taken from the transaction {@link ChannelHeader}, therefore it will indicate the
        /// client's timestamp, and will have the same value across all endorsers.
        /// </summary>
        Timestamp TxTimestamp { get; }

        /// <summary>
        /// Returns a fully decoded object of the signed transaction proposal.
        /// </summary>
        DecodedSignedProposal DecodedSignedProposal { get; }

        /// <summary>
        /// Returns the transient map that can be used by the chaincode but not
        /// saved in the ledger, such as cryptographic information for encryption and decryption.
        /// </summary>
        MapField<string, ByteString> TransientMap { get; }

        /// <summary>
        /// Returns the channel ID for the proposal for chaincode to process.
        /// This would be the 'channel_id' of the transaction proposal (see ChannelHeader
        /// in protos/common/common.proto) except where the chaincode is calling another on
        /// a different channel.
        /// </summary>
        string ChannelId { get; }

        /// <summary>
        /// Returns the transaction ID for the current chaincode invocation request. The transaction
        /// ID uniquely identifies the transaction within the scope of the channel.
        /// </summary>
        string TxId { get; }

        /// <summary>
        /// Returns the arguments as list of strings from the chaincode invocation request.
        /// </summary>
        IList<string> Args { get; }

        /// <summary>
        /// This object contains the essential identity information of the chaincode invocation's submitter,
        /// including its organizational affiliation (mspid) and certificate (id_bytes).
        /// </summary>
        SerializedIdentity Creator { get; }

        /// <summary>
        /// Returns an object containing the chaincode function name to invoke, and the array
        /// of arguments to pass to the target function.
        /// </summary>
        /// <returns>An object containing the information about the function with its parameters being called.</returns>
        ChaincodeFunctionParameterInformation GetFunctionAndParameters();

        /// <summary>
        /// Retrieves the current value of the state variable <paramref name="key"/>
        /// </summary>
        /// <param name="key">State variable key to retrieve from the state store.</param>
        /// <returns>Current value of the state variable.</returns>
        Task<ByteString> GetState(string key);

        /// <summary>
        /// Writes the state variable <paramref name="key"/> of value <paramref name="value"/>
        /// to the state store. If the variable already exists, the value will be overwritten.
        /// </summary>
        /// <param name="key">State variable key to set the value for.</param>
        /// <param name="value">State variable value</param>
        /// <returns>Returns a completed task, if the peer has successfully handled the state update request,
        /// otherwise the task will throw.</returns>
        Task<ByteString> PutState(string key, ByteString value);

        /// <summary>
        /// Deletes the state variable <paramref name="key"/> from the state store.
        /// </summary>
        /// <param name="key">State variable key to delete from the state store.</param>
        /// <returns>Returns a completed task, if the peer has successfully handled the state delete request,
        /// otherwise the task will throw.</returns>
        Task<ByteString> DeleteState(string key);

        /// <summary>
        /// <para>Returns a range iterator over a set of keys in the
        /// ledger. The iterator can be used to iterate over all keys
        /// between the <paramref cref="startKey" /> (inclusive) and <paramref cref="endKey" /> (exclusive).
        /// The keys are returned by the iterator in lexical order. Note
        /// that <paramref cref="startKey" /> and <paramref cref="endKey" /> can be empty string, which implies 
        /// unbounded range query on start or end.</para>
        /// <para>Call <see cref="StateQueryIterator.Close()"/> object when done. The query is re-executed
        /// during validation phase to ensure result set has not changed since
        /// transaction endorsement (phantom reads detected).</para>
        /// </summary>
        /// <param name="startKey">State variable key as the start of the key range (inclusive).</param>
        /// <param name="endKey">State variable key as the end of the key range (exclusive).</param>
        /// <returns>A <see cref="StateQueryIterator"/></returns>
        Task<StateQueryIterator> GetStateByRange(string startKey, string endKey);

        /// <summary>
        /// <para>Performs a "rich" query against a state database. It is
        /// only supported for state databases that support rich query,
        /// e.g. CouchDB. The query string is in the native syntax
        /// of the underlying state database. An <see cref="StateQueryIterator"/> is returned
        /// which can be used to iterate (next) over the query result set.</para>
        /// <para>The query is NOT re-executed during validation phase, phantom reads are
        /// not detected. That is, other committed transactions may have added,
        /// updated, or removed keys that impact the result set, and this would not
        /// be detected at validation/commit time. Applications susceptible to this
        /// should therefore not use GetQueryResult as part of transactions that update
        /// ledger, and should limit use to read-only chaincode operations.</para>
        /// </summary>
        /// <param name="query">Query string native to the underlying state database.</param>
        /// <returns>A <see cref="StateQueryIterator"/></returns>
        Task<StateQueryIterator> GetQueryResult(string query);

        /// <summary>
        /// <para>Returns a history of key values across time.
        /// For each historic key update, the historic value and associated
        /// transaction id and timestamp are returned. The timestamp is the
        /// timestamp provided by the client in the proposal header.
        /// This method requires peer configuration
        /// <code>core.ledger.history.enableHistoryDatabase</code> to be true.</para>
        /// <para>The query is NOT re-executed during validation phase, phantom reads are
        /// not detected. That is, other committed transactions may have updated
        /// the key concurrently, impacting the result set, and this would not be
        /// detected at validation/commit time. Applications susceptible to this
        /// should therefore not use GetHistoryForKey as part of transactions that
        /// update ledger, and should limit use to read-only chaincode operations.</para>
        /// </summary>
        /// <param name="key">The state variable key.</param>
        /// <returns>A <see cref="HistoryQueryIterator"/>.</returns>
        Task<HistoryQueryIterator> GetHistoryForKey(string key);

        /// <summary>
        /// <para>Locally calls the specified chaincode <see cref="IChaincode.Invoke"/> using the
        /// same transaction context; that is, chaincode calling chaincode doesn't
        /// create a new transaction message.</para>
        /// <para>If the called chaincode is on the same channel, it simply adds the called
        /// chaincode read set and write set to the calling transaction.</para>
        /// <para>If the called chaincode is on a different channel,
        /// only the Response is returned to the calling chaincode; any <see cref="PutState"/> calls
        /// from the called chaincode will not have any effect on the ledger; that is,
        /// the called chaincode on a different channel will not have its read set
        /// and write set applied to the transaction. Only the calling chaincode's
        /// read set and write set will be applied to the transaction. Effectively
        /// the called chaincode on a different channel is a `Query`, which does not
        /// participate in state validation checks in subsequent commit phase.</para>
        /// <para>If <paramref name="channel"/> is empty, the caller's channel is assumed.</para>
        /// </summary>
        /// <param name="chaincodeName">Name of the chaincode to call.</param>
        /// <param name="args">List of arguments to pass to the called chaincode.</param>
        /// <param name="channel">Name of the channel where the target chaincode is active.</param>
        /// <returns>Returns a <see cref="Response"/> returned by the called chaincode.</returns>
        Task<Response> InvokeChaincode(string chaincodeName, IEnumerable<ByteString> args, string channel = "");

        /// <summary>
        /// Allows the chaincode to propose an event on the transaction proposal. When the transaction
        /// is included in a block and the block is successfully committed to the ledger, the block event
        /// will be delivered to the current event listeners that have been registered with the peer's
        /// event producer. Note that the block event gets delivered to the listeners regardless of the
        /// status of the included transactions (can be either valid or invalid), so client applications
        /// are responsible for checking the validity code on each transaction. Consult each SDK's documentation
        /// for details.
        /// </summary>
        /// <param name="name">Name of the event</param>
        /// <param name="payload">A payload can be used to include data about the event</param>
        void SetEvent(string name, ByteString payload);

        /// <summary>
        /// <para>Creates a composite key by combining the objectType string and the given <paramref name="attributes"/>
        /// to form a composite key. The objectType and attributes are expected to have only valid utf8 strings and should not contain
        /// U+0000 (nil byte) and U+10FFFF (biggest and unallocated code point). The resulting composite key can be
        /// used as the key in <see cref="PutState"/>.</para>
        /// <para>Hyperledger Fabric uses a simple key/value model for saving chaincode states. In some use case
        /// scenarios, it is necessary to keep track of multiple attributes. Furthermore, it may be necessary to make the various
        /// attributes searchable. Composite keys can be used to address these requirements. Similar to using composite
        /// keys in a relational database table, here you would treat the searchable attributes as key columns that
        /// make up the composite key. Values for the attributes become part of the key, thus they are searchable with
        /// functions like <see cref="GetStateByRange" /> and <see cref="GetStateByPartialCompositeKey"/>.</para>
        /// </summary>
        /// <param name="objectType">A string used as the prefix of the resulting key.</param>
        /// <param name="attributes">List of attribute values to concatenate into the key.</param>
        /// <returns>A composite key with the <paramref name="objectType"/> and the array of <paramref name="attributes"/>
        /// joined together with special delimiters that will not be confused with values of the attributes</returns>
        string CreateCompositeKey(string objectType, IEnumerable<string> attributes);

        /// <summary>
        /// Splits the specified key into attributes on which the composite key was formed.
        /// Composite keys found during range queries or partial composite key queries can
        /// therefore be split into their original composite parts, essentially recovering
        /// the values of the attributes.
        /// </summary>
        /// <param name="compositeKey">The composite key to split</param>
        /// <returns>The split composite key</returns>
        (string ObjectType, IList<string> Attributes) SplitCompositeKey(string compositeKey);

        /// <summary>
        /// <para>Queries the state in the ledger based on a given partial composite key. This function returns an iterator
        /// which can be used to iterate over all composite keys whose prefix matches the given partial composite key.
        /// The <paramref name="objectType"/> and <paramref cref="attributes" />are expected to have only valid utf8 strings and should not contain
        /// U+0000 (nil byte) and U+10FFFF (biggest and unallocated code point).</para>
        /// <para> See related functions <see cref="SplitCompositeKey"/> and <see cref="CreateCompositeKey"/>.</para>
        /// <para>Call <see cref="StateQueryIterator.Close"/> when done.</para>
        /// <para>The query is re-executed during validation phase to ensure result set has not changed since transaction
        /// endorsement (phantom reads detected).</para>
        /// </summary>
        /// <param name="objectType">A string used as the prefix of the resulting key.</param>
        /// <param name="attributes">List of attribute values to concatenate into the partial composite key.</param>
        /// <returns>A <see cref="StateQueryIterator"/>.</returns>
        Task<StateQueryIterator> GetStateByPartialCompositeKey(string objectType, IList<string> attributes);

        /// <summary>
        /// getPrivateData returns the value of the specified `key` from the specified
        /// <paramref name="collection"/>. Note that <see cref="GetPrivateData"> doesn't read data from the
        /// private writeset, which has not been committed to the <paramref name="collection"/>. In
        /// other words, <see cref="GetPrivateData"> doesn't consider data modified by <see cref="PutPrivateData"/>
        /// that has not been committed.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="key">Private data variable key to retrieve from the state store.</param>
        /// <returns></returns>
        Task<ByteString> GetPrivateData(string collection, string key);

        /// <summary>
        /// <see cref="PutPrivateData"/> puts the specified <paramref name="key"/> and <paramref cref="value" /> into the transaction's
        /// private writeset. Note that only hash of the private writeset goes into the
        /// transaction proposal response (which is sent to the client who issued the
        /// transaction) and the actual private writeset gets temporarily stored in a
        /// transient store. PutPrivateData doesn't effect the <paramref name="collection"/> until the
        /// transaction is validated and successfully committed. Simple keys must not be
        /// an empty string and must not start with null character (0x00), in order to
        /// avoid range query collisions with composite keys, which internally get
        /// prefixed with 0x00 as composite key namespace.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="key">Private data variable key to set the value for.</param>
        /// <param name="value">Private data variable value.</param>
        /// <returns></returns>
        Task<ByteString> PutPrivateData(string collection, string key, ByteString value);

        /// <summary>
        /// <see cref="DeletePrivateData"/> records the specified <paramref name="key"/> to be deleted in the private writeset of
        /// the transaction. Note that only hash of the private writeset goes into the
        /// transaction proposal response (which is sent to the client who issued the
        /// transaction) and the actual private writeset gets temporarily stored in a
        /// transient store. The <paramref cref="key"/> and its value will be deleted from the collection
        /// when the transaction is validated and successfully committed.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="key">Private data variable key to delete from the state store.</param>
        /// <returns></returns>
        Task<ByteString> DeletePrivateData(string collection, string key);

        /// <summary>
        /// <see cref="GetPrivateDataByRange"/> returns a range iterator over a set of keys in a
        /// given private <paramref name="collection"/>. The iterator can be used to iterate over all keys
        /// between the <paramref cref="startKey" /> (inclusive) and <paramref cref="endKey" /> (exclusive).
        /// The keys are returned by the iterator in lexical order. Note
        /// that <paramref cref="startKey" /> and <paramref cref="endKey" /> can be empty string, which implies 
        /// unbounded range  query on start or end.
        /// Call <see cref="StateQueryIterator.Close"/> when done.
        /// The query is re-executed during validation phase to ensure result set
        /// has not changed since transaction endorsement (phantom reads detected).
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="startKey">Private data variable key as the start of the key range (inclusive)</param>
        /// <param name="endKey">Private data variable key as the end of the key range (exclusive)</param>
        /// <returns></returns>
        Task<StateQueryIterator> GetPrivateDataByRange(string collection, string startKey, string endKey);

        /// <summary>
        /// <see cref="GetPrivateDataByPartialCompositeKey"/> queries the state in a given private
        /// <paramref cref="collection" /> based on a given partial composite key. This function returns
        /// an iterator which can be used to iterate over all composite keys whose prefix
        /// matches the given partial composite key. The <paramref name="objectType"/> and <paramref name="attributes"/> are
        /// expected to have only valid utf8 strings and should not contain
        /// U+0000 (nil byte) and U+10FFFF (biggest and unallocated code point).
        /// See related functions <see cref="SplitCompositeKey"/> and <see cref="CreateCompositeKey"/>.
        /// Call Close() on the returned StateQueryIteratorInterface object when done.
        /// The query is re-executed during validation phase to ensure result set
        /// has not changed since transaction endorsement (phantom reads detected).
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="objectType">A string used as the prefix of the resulting key.</param>
        /// <param name="attributes">List of attribute values to concatenate into the partial composite key.</param>
        /// <returns></returns>
        Task<StateQueryIterator> GetPrivateDataByPartialCompositeKey(
            string collection,
            string objectType,
            IList<string> attributes
        );

        /// <summary>
        /// <see cref="GetPrivateDataQueryResult"/> performs a "rich" query against a given private
        /// <paramref name="collection"/>. It is only supported for state databases that support rich query,
        /// e.g.CouchDB. The query string is in the native syntax
        /// of the underlying state database. An iterator is returned
        /// which can be used to iterate (next) over the query result set.
        /// The query is NOT re-executed during validation phase, phantom reads are
        /// not detected. That is, other committed transactions may have added,
        /// updated, or removed keys that impact the result set, and this would not
        /// be detected at validation/commit time. Applications susceptible to this
        /// should therefore not use <see cref="GetQueryResult"/> as part of transactions that update
        /// ledger, and should limit use to read-only chaincode operations.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="query">The query to be performed.</param>
        /// <returns>A <see cref="StateQueryIterator"/></returns>
        Task<StateQueryIterator> GetPrivateDataQueryResult(string collection, string query);
    }
}
