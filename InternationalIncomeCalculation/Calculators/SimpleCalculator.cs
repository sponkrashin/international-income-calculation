namespace InternationalIncomeCalculation.Calculators;

using InternationalIncomeCalculation.Configuration;

public class SimpleCalculator
{
    private readonly FeesCalculator feesCalculator;

    public SimpleCalculator(FeesCalculator feesCalculator)
    {
        this.feesCalculator = feesCalculator;
    }

    public decimal Calculate(
        CommonConfiguration commonConfiguration,
        BankConfiguration bankConfiguration,
        BankTariffConfiguration tariffConfiguration)
    {
        return this.Calculate(
            commonConfiguration.TotalIncome,
            commonConfiguration,
            bankConfiguration,
            tariffConfiguration);
    }

    public decimal Calculate(
        decimal amount,
        CommonConfiguration commonConfiguration,
        BankConfiguration bankConfiguration,
        BankTariffConfiguration tariffConfiguration)
    {
        var exchangeRate = bankConfiguration.UseStockExchange
            ? commonConfiguration.StockExchangeRate * (1 - commonConfiguration.StockExchangeFeePercentage / 100)
            : tariffConfiguration.ExchangeRate;

        var totalAmountInRubles = amount * exchangeRate;

        var totalAmountInRublesForLimits = bankConfiguration.UseStockExchange
            ? commonConfiguration.TotalIncome * commonConfiguration.CentralBankExchangeRate
            : totalAmountInRubles;

        var fees = this.feesCalculator.Calculate(
            commonConfiguration.Incomes,
            exchangeRate,
            totalAmountInRublesForLimits,
            tariffConfiguration);

        return totalAmountInRubles - fees;
    }
}
