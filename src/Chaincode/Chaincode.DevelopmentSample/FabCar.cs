using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Thinktecture.HyperledgerFabric.Chaincode.Contract;
using Thinktecture.HyperledgerFabric.Chaincode.Extensions;

namespace Thinktecture.HyperledgerFabric.Chaincode.DevelopmentSample
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

    public class FabCar : ContractBase
    {
        private readonly ILogger<FabCar> _logger;

        public async Task<ByteString> QueryCar(IContractContext context, string carNumber)
        {
            var carBytes = await context.Stub.GetState(carNumber);

            if (carBytes == null || carBytes.Length <= 0) throw new Exception($"Car {carNumber} does not exist.");

            _logger.LogInformation(carBytes.ToStringUtf8());

            return carBytes;
        }

        public async Task<ByteString> Init(IContractContext context)
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
                if (await context.Stub.PutStateJson($"CAR{index}", car))
                    _logger.LogInformation("Added car", car.ToString());
                else
                    _logger.LogError($"Error writing car {car} onto the ledger");
            }

            return ByteString.Empty;
        }

        public async Task<ByteString> CreateCar(
            IContractContext context,
            string carNumber,
            string make,
            string model,
            string color,
            string owner
        )
        {
            var car = new Car(make, model, color, owner);

            await context.Stub.PutState(carNumber, JsonConvert.SerializeObject(car));

            return ByteString.Empty;
        }

        public async Task<ByteString> QueryAllCars(IContractContext context)
        {
            var startKey = "CAR0";
            var endKey = "CAR999";

            var iterator = await context.Stub.GetStateByRange(startKey, endKey);

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

        public async Task<ByteString> ChangeCarOwner(IContractContext context, string carNumber, string owner)
        {
            var car = await context.Stub.GetStateJson<Car>(carNumber);
            car.Owner = owner;

            await context.Stub.PutStateJson(carNumber, car);

            return ByteString.Empty;
        }

        public FabCar(ILogger<FabCar> logger)
            : base("FabCar")
        {
            _logger = logger;
        }
    }

    public class CarQueryResult
    {
        public string Key { get; set; }
        public Car Record { get; set; }
    }
}
