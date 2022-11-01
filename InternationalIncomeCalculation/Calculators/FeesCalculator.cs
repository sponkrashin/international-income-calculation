namespace InternationalIncomeCalculation.Calculators;

using InternationalIncomeCalculation.Configuration;

public class FeesCalculator
{
    public decimal Calculate(IReadOnlyCollection<decimal> incomes, decimal exchangeRate, decimal amountInRubles, BankTariffConfiguration tariffConfiguration)
    {
        return
            this.CalculateCurrencyControlFee(incomes, exchangeRate, tariffConfiguration) +
            this.CalculateTransferFee(amountInRubles, tariffConfiguration) +
            this.CalculateMaintenanceFee(tariffConfiguration);
    }

    public decimal CalculateForIncomeInRubles(decimal amountInRubles, BankTariffConfiguration tariffConfiguration)
    {
        return this.Calculate(Array.Empty<decimal>(), 0, amountInRubles, tariffConfiguration);
    }

    private decimal CalculateCurrencyControlFee(IReadOnlyCollection<decimal> incomes, decimal exchangeRate, BankTariffConfiguration tariffConfiguration)
    {
        var feeMin = tariffConfiguration.CurrencyControlFeeRange.Min;
        var feeMax = tariffConfiguration.CurrencyControlFeeRange.Max;
        var feePercentage = tariffConfiguration.CurrencyControlFeePercentagePrice;
        var feeFixPrice = tariffConfiguration.CurrencyControlFixPrice;

        var currencyControlFee = incomes
            .Select(x =>
            {
                if (feeFixPrice != null)
                {
                    return feeFixPrice.Value * (tariffConfiguration.CurrencyControlFixPriceInCurrency ? exchangeRate : 1);
                }

                return feePercentage == null
                    ? 0
                    : Math.Min(Math.Max(x * feePercentage.Value / 100 * exchangeRate, feeMin), feeMax);
            })
            .Sum();

        return currencyControlFee;
    }

    private decimal CalculateTransferFee(decimal amountInRubles, BankTariffConfiguration tariffConfiguration)
    {
        var totalTransferFee = tariffConfiguration.TransferThresholds
            .Aggregate(
                new
                {
                    RemainingAmount = amountInRubles,
                    AccumulatedTransferFee = 0m
                },
                (acc, threshold) =>
                {
                    if (acc.RemainingAmount == 0)
                    {
                        return acc;
                    }

                    var transferAmount = Math.Min(acc.RemainingAmount, threshold.Amount);
                    var transferFee = transferAmount * threshold.PercentagePrice / 100 + threshold.FixPrice;

                    return new
                    {
                        RemainingAmount = acc.RemainingAmount - transferAmount,
                        AccumulatedTransferFee = acc.AccumulatedTransferFee + transferFee
                    };
                },
                acc => acc.AccumulatedTransferFee);

        return totalTransferFee;
    }

    private decimal CalculateMaintenanceFee(BankTariffConfiguration tariffConfiguration)
    {
        return tariffConfiguration.MaintenancePrice;
    }
}
