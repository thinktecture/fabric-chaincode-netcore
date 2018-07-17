using Protos;
using Queryresult;

namespace Chaincode.NET.Handler.Iterators
{
    public class StateQueryIterator : CommonIterator<KV>
    {
        public StateQueryIterator(IHandler handler, string channelId, string txId, QueryResponse response)
            : base(handler, channelId, txId, response, IteratorType.Query)
        {
        }

        protected override KV GetResultFromBytes(QueryResultBytes bytes) => KV.Parser.ParseFrom(bytes.ResultBytes);
    }
}