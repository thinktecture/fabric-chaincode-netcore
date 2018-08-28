using Protos;
using Queryresult;

namespace Thinktecture.HyperledgerFabric.Chaincode.Handler.Iterators
{
    /// <inheritdoc />
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
