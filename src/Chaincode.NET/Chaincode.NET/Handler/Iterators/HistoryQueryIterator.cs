using Protos;
using Queryresult;

namespace Chaincode.NET.Handler.Iterators
{
    public class HistoryQueryIterator : CommonIterator<KeyModification>
    {
        public HistoryQueryIterator(IHandler handler, string channelId, string txId, QueryResponse response)
            : base(handler, channelId, txId, response, IteratorType.History)
        {
        }

        protected override KeyModification GetResultFromBytes(QueryResultBytes bytes) =>
            KeyModification.Parser.ParseFrom(bytes.ResultBytes);
    }
}