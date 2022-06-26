using InternationalIncomeCalculation.Calculators;
using InternationalIncomeCalculation.Configuration;

using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var commonConfiguration = configuration.GetSection("Common").Get<CommonConfiguration>();
var banksConfiguration = configuration.GetSection("Banks").Get<IReadOnlyCollection<BankConfiguration>>();

var stockExchangeConvertedAmount = commonConfiguration.TotalIncome * commonConfiguration.StockExchangeRate;

var feesCalculator = new FeesCalculator();
var simpleCalculator = new SimpleCalculator(feesCalculator);
var withTransfersCalculator = new WithTransfersCalculator(simpleCalculator, feesCalculator);

var banksToReceivePayments = banksConfiguration
    .Where(x => x.CanReceiveInternationalPayments)
    .ToArray();

foreach (var bank in banksConfiguration)
{
    foreach (var tariff in bank.Tariffs)
    {
        Console.WriteLine($"{bank.Name} (тариф {tariff.Name})");

        var receivedAmount = simpleCalculator.Calculate(commonConfiguration, bank, tariff);
        Console.WriteLine($"{receivedAmount:N2}");

        var differenceWithStockExchange = (stockExchangeConvertedAmount - receivedAmount) / stockExchangeConvertedAmount;
        Console.WriteLine($"{differenceWithStockExchange:P2}");

        Console.WriteLine();
    }
}

foreach (var bank in banksToReceivePayments)
{
    var banksToTransfer = banksConfiguration
        .Except(new[] { bank })
        .ToArray();

    foreach (var tariff in bank.Tariffs)
    {
        Console.WriteLine($"{bank.Name} (тариф {tariff.Name}, с переводами)");

        var calculationResult = withTransfersCalculator.Calculate(
            commonConfiguration,
            bank,
            tariff,
            banksToTransfer);

        var receivedAmount = calculationResult.ReceivedAmount;
        Console.WriteLine($"{receivedAmount:N2}");

        var differenceWithStockExchange = (stockExchangeConvertedAmount - receivedAmount) / stockExchangeConvertedAmount;
        Console.WriteLine($"{differenceWithStockExchange:P2}");

        foreach (var resultPart in calculationResult.Parts)
        {
            Console.WriteLine($"{resultPart.AmountToTransfer:N2} => {resultPart.Bank.Name} (тариф {resultPart.Tariff.Name})");
        }

        Console.WriteLine();
    }
}

Console.ReadKey();
