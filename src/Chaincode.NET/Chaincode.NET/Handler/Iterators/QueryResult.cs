namespace Chaincode.NET.Handler.Iterators
{
    public class QueryResult<T>
    {
        public T Value { get; set; }
        public bool Done { get; set; }
    }
}