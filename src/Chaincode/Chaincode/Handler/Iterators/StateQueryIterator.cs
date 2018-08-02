using Protos;
using Queryresult;

namespace Thinktecture.HyperledgerFabric.Chaincode.Handler.Iterators
{
    public class StateQueryIterator : CommonIterator<KV>
    {
        public StateQueryIterator(IHandler handler, string channelId, string txId, QueryResponse response)
            : base(handler, channelId, txId, response, IteratorType.Query)
        {
        }

        protected override KV GetResultFromBytes(QueryResultBytes bytes)
        {
            return KV.Parser.ParseFrom(bytes.ResultBytes);
        }
    }
}
