namespace InternationalIncomeCalculation.Calculators;

using InternationalIncomeCalculation.Configuration;

public class WithTransfersCalculator
{
    private const decimal TransferCalculationStep = 100;
    private const decimal TransferCalculationStepInRubles = 5000;

    private readonly SimpleCalculator simpleCalculator;
    private readonly FeesCalculator feesCalculator;

    public WithTransfersCalculator(
        SimpleCalculator simpleCalculator,
        FeesCalculator feesCalculator)
    {
        this.simpleCalculator = simpleCalculator;
        this.feesCalculator = feesCalculator;
    }

    public CalculationResult Calculate(
        CommonConfiguration commonConfiguration,
        BankConfiguration mainBankConfiguration,
        BankTariffConfiguration mainBankTariffConfiguration,
        params BankConfiguration[] banksToTransferConfigurations)
    {
        var bestResult = new CalculationResult
        {
            ReceivedAmount = 0,
            Parts = Array.Empty<CalculationResultPart>()
        };

        var amountInMainBank = commonConfiguration.TotalIncome;

        do
        {
            var receivedInMainBank = this.simpleCalculator.Calculate(
                amountInMainBank,
                commonConfiguration,
                mainBankConfiguration,
                mainBankTariffConfiguration);

            var amountToTransfer = commonConfiguration.TotalIncome - amountInMainBank;
            var amountToTransferInRubles = amountToTransfer * mainBankTariffConfiguration.ExchangeRate;

            var transferResult = this.CalculateBestTransferToBanks(
                amountToTransferInRubles,
                mainBankTariffConfiguration,
                banksToTransferConfigurations);

            var temporaryResult = new CalculationResult
            {
                ReceivedAmount = receivedInMainBank + transferResult.ReceivedAmount,
                Parts = transferResult.Parts
            };

            if (bestResult.ReceivedAmount < temporaryResult.ReceivedAmount)
            {
                bestResult = temporaryResult;
            }

            if (banksToTransferConfigurations.Length == 0 || amountInMainBank == 0)
            {
                break;
            }

            amountInMainBank = Math.Max(amountInMainBank - TransferCalculationStep, 0);
        } while (true);

        return bestResult;
    }

    private CalculationResult CalculateBestTransferToBanks(
        decimal amountInRubles,
        BankTariffConfiguration mainBankTariffConfiguration,
        params BankConfiguration[] banksToTransferConfigurations)
    {
        var bestResult = new CalculationResult
        {
            ReceivedAmount = 0,
            Parts = Array.Empty<CalculationResultPart>()
        };

        if (banksToTransferConfigurations.Length == 0)
        {
            return bestResult;
        }

        var amountInRublesInFirstBank = amountInRubles;

        do
        {
            var bestTransferToFirstBank = banksToTransferConfigurations[0].Tariffs
                .Select(tariff => new
                {
                    Tariff = tariff,
                    // ReSharper disable once AccessToModifiedClosure
                    TotalFee = this.feesCalculator.CalculateForIncomeInRubles(amountInRublesInFirstBank, tariff)
                })
                .MinBy(x => x.TotalFee);

            var bestTransferToOtherBanks = this.CalculateBestTransferToBanks(
                amountInRubles - amountInRublesInFirstBank,
                mainBankTariffConfiguration,
                banksToTransferConfigurations.Skip(1).ToArray());

            var receivedInFirstBank = Math.Max(amountInRublesInFirstBank - bestTransferToFirstBank!.TotalFee - mainBankTariffConfiguration.PaymentOrderPrice, 0);

            var firstBankResultParts = receivedInFirstBank != 0
                ? new[]
                {
                    new CalculationResultPart
                    {
                        Bank = banksToTransferConfigurations[0],
                        Tariff = bestTransferToFirstBank.Tariff,
                        AmountToTransfer = amountInRublesInFirstBank,
                        AmountToReceive = receivedInFirstBank
                    }
                }
                : Array.Empty<CalculationResultPart>();

            var temporaryResult = new CalculationResult
            {
                ReceivedAmount = receivedInFirstBank + bestTransferToOtherBanks.ReceivedAmount,
                Parts = Array.Empty<CalculationResultPart>()
                    .Union(firstBankResultParts)
                    .Union(bestTransferToOtherBanks.Parts)
                    .ToArray()
            };

            if (bestResult.ReceivedAmount < temporaryResult.ReceivedAmount)
            {
                bestResult = temporaryResult;
            }

            if (banksToTransferConfigurations.Length == 1 || amountInRublesInFirstBank == 0)
            {
                break;
            }

            amountInRublesInFirstBank = Math.Max(amountInRublesInFirstBank - TransferCalculationStepInRubles, 0);
        } while (true);

        return bestResult;
    }
}

public class CalculationResult
{
    public decimal ReceivedAmount { get; set; }

    public IReadOnlyCollection<CalculationResultPart> Parts { get; set; } = Array.Empty<CalculationResultPart>();
}

public class CalculationResultPart
{
    public BankConfiguration Bank { get; set; } = null!;

    public BankTariffConfiguration Tariff { get; set; } = null!;

    public decimal AmountToTransfer { get; set; }

    public decimal AmountToReceive { get; set; }
}
