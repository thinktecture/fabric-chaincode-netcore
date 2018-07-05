using System;
using System.Threading.Tasks;
using Protos;

namespace Chaincode.NET.Handler.Iterators
{
    public abstract class CommonIterator<T>
    {
        private readonly IHandler _handler;
        private readonly string _channelId;
        private readonly string _txId;
        private QueryResponse _response;
        private readonly IteratorType _type;
        private int _currentLocation;

        // TODO: Change Node style events into more suitable C# stuff? 
        public event Action<QueryResult<T>> Data;
        public event Action End;
        public event Action<Exception> Error;

        protected CommonIterator(
            IHandler handler,
            string channelId,
            string txId,
            QueryResponse response,
            IteratorType type
        )
        {
            _handler = handler;
            _channelId = channelId;
            _txId = txId;
            _response = response;
            _type = type;
        }

        private void OnData(QueryResult<T> obj) => Data?.Invoke(obj);
        private void OnEnd() => End?.Invoke();
        private void OnError(Exception exception) => Error?.Invoke(exception);

        public Task<QueryResponse> Close() => _handler.HandleQueryCloseState(_response.Id, _channelId, _txId);

        protected abstract T GetResultFromBytes(QueryResultBytes bytes);

        private QueryResult<T> CreateAndEmitResult()
        {
            var result = new QueryResult<T>()
            {
                Value = GetResultFromBytes(_response.Results[_currentLocation])
            };

            _currentLocation++;

            result.Done = !(_currentLocation < _response.Results.Count || _response.HasMore);

            OnData(result);

            return result;
        }

        public async Task<QueryResult<T>> Next()
        {
            if (_currentLocation < _response.Results.Count)
            {
                return CreateAndEmitResult();
            }

            if (!_response.HasMore)
            {
                OnEnd();
                return new QueryResult<T>() {Done = true};
            }

            try
            {
                var response = await _handler.HandleQueryStateNext(_response.Id, _channelId, _txId);
                _currentLocation = 0;
                _response = response;
                return CreateAndEmitResult();
            }
            catch (Exception ex)
            {
                // Only throw if we don't have a handle for the error event, otherwise emit an error
                if (Error == null)
                {
                    throw;
                }

                OnError(ex);
                return null;
            }
        }
    }
}
