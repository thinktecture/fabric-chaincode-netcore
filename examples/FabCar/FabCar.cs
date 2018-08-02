using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Protos;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;
using Thinktecture.HyperledgerFabric.Chaincode.Handler;

namespace FabCar
{
    public class Car
    {
        public Car()
        {
        }

        public Car(string make, string model, string color, string owner)
        {
            Make = make;
            Model = model;
            Color = color;
            Owner = owner;
        }

        public string Make { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string Owner { get; set; }
        public string DocType { get; set; } = "car";

        public override string ToString()
        {
            return $"{Make} {Model} in {Color} with owner {Owner}";
        }
    }

    public class FabCar : IChaincode
    {
        private readonly ChaincodeInvocationMap _invocationMap;
        private readonly ILogger<FabCar> _logger;

        public FabCar(ILogger<FabCar> logger)
        {
            _logger = logger;
            _invocationMap = new LoggingChaincodeInvocationMap(logger)
            {
                {nameof(QueryCar), QueryCar},
                {nameof(InitLedger), InitLedger},
                {nameof(CreateCar), CreateCar},
                {nameof(QueryAllCars), QueryAllCars},
                {nameof(ChangeCarOwner), ChangeCarOwner}
            };
        }

        public async Task<Response> Init(IChaincodeStub stub)
        {
            _logger.LogInformation("========== Instantiated FabCar Chaincode ==========");
            return Shim.Success();
        }

        public Task<Response> Invoke(IChaincodeStub stub)
        {
            _logger.LogInformation("========== Invoking ChainCode FabCar Chaincode ==========");
            return _invocationMap.Invoke(stub);
        }

        private async Task<ByteString> QueryCar(IChaincodeStub stub, Parameters args)
        {
            args.AssertCount(1);

            var carNumber = args.Get<string>(0);

            var carBytes = await stub.GetState(carNumber);

            if (carBytes == null || carBytes.Length <= 0) throw new Exception($"Car {carNumber} does not exist.");

            _logger.LogInformation(carBytes.ToStringUtf8());

            return carBytes;
        }

        private async Task<ByteString> InitLedger(IChaincodeStub stub, Parameters args)
        {
            var cars = new List<Car>
            {
                new Car("Toyota", "Prius", "blue", "Tomoko"),
                new Car("Ford", "Mustang", "red", "Brad"),
                new Car("Hyundai", "Tucson", "green", "Jin Soo"),
                new Car("Volkswagen", "Passat", "yellow", "Max"),
                new Car("Tesla", "S", "black", "Michael"),
                new Car("Peugeot", "205", "purpe", "Michel"),
                new Car("Chery", "522L", "white", "Aarav"),
                new Car("Fiat", "Punto", "violet", "Pari"),
                new Car("Tata", "Nano", "indigo", "Valeria"),
                new Car("Holden", "Barina", "brown", "Shotaro")
            };

            for (var index = 0; index < cars.Count; index++)
            {
                var car = cars[index];
                if (await stub.PutStateJson($"CAR{index}", car))
                    _logger.LogInformation("Added car", car.ToString());
                else
                    _logger.LogError($"Error writing car {car} onto the ledger");
            }

            return ByteString.Empty;
        }

        private async Task<ByteString> CreateCar(IChaincodeStub stub, Parameters args)
        {
            args.AssertCount(5);

            var car = new Car(args.Get<string>(1), args.Get<string>(2), args.Get<string>(3), args.Get<string>(4));

            await stub.PutState(args.Get<string>(0), JsonConvert.SerializeObject(car));

            return ByteString.Empty;
        }

        private async Task<ByteString> QueryAllCars(IChaincodeStub stub, Parameters args)
        {
            var startKey = "CAR0";
            var endKey = "CAR999";

            var iterator = await stub.GetStateByRange(startKey, endKey);

            var result = new List<CarQueryResult>();

            while (true)
            {
                var iterationResult = await iterator.Next();

                if (iterationResult.Value != null && iterationResult.Value.Value.Length > 0)
                {
                    var queryResult = new CarQueryResult
                    {
                        Key = iterationResult.Value.Key
                    };

                    try
                    {
                        queryResult.Record =
                            JsonConvert.DeserializeObject<Car>(iterationResult.Value.Value.ToStringUtf8());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            $"An error occured while trying to deserialize {iterationResult.Value.Value.ToStringUtf8()}");
                    }

                    result.Add(queryResult);
                }

                if (iterationResult.Done)
                {
                    _logger.LogInformation("End of data");
                    await iterator.Close();
                    _logger.LogInformation("Result", result);
                    return JsonConvert.SerializeObject(result).ToByteString();
                }
            }
        }

        private async Task<ByteString> ChangeCarOwner(IChaincodeStub stub, Parameters args)
        {
            args.AssertCount(2);

            var car = await stub.GetStateJson<Car>(args.Get<string>(0));
            car.Owner = args.Get<string>(1);

            await stub.PutStateJson(args.Get<string>(0), car);

            return ByteString.Empty;
        }
    }

    public class CarQueryResult
    {
        public string Key { get; set; }
        public Car Record { get; set; }
    }
}
