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
        var totalAmountInRubles = bankConfiguration.UseStockExchange
            ? amount * commonConfiguration.StockExchangeRate * (1 - commonConfiguration.StockExchangeFeePercentage / 100)
            : amount * tariffConfiguration.ExchangeRate;

        var totalAmountInRublesForLimits = bankConfiguration.UseStockExchange
            ? commonConfiguration.TotalIncome * commonConfiguration.CentralBankExchangeRate
            : totalAmountInRubles;

        var fees = this.feesCalculator.Calculate(commonConfiguration.Incomes, totalAmountInRublesForLimits, tariffConfiguration);
        return totalAmountInRubles - fees;
    }
}
