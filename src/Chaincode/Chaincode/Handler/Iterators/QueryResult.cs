namespace Thinktecture.HyperledgerFabric.Chaincode.Handler.Iterators
{
    public class QueryResult<T>
    {
        public T Value { get; set; }
        public bool Done { get; set; }
    }
}