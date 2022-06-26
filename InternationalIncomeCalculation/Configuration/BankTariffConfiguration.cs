namespace InternationalIncomeCalculation.Configuration;

public class BankTariffConfiguration
{
    public string Name { get; set; } = string.Empty;

    public decimal MaintenancePrice { get; set; }

    public decimal PaymentOrderPrice { get; set; }

    public decimal ExchangeRate { get; set; }

    public decimal? CurrencyControlFeePercentagePrice { get; set; }

    public decimal? CurrencyControlFeeMin { get; set; }

    public decimal? CurrencyControlFeeMax { get; set; }

    public TariffThresholdConfiguration[] TransferThresholds { get; set; } = Array.Empty<TariffThresholdConfiguration>();

    public (decimal Min, decimal Max) CurrencyControlFeeRange => (this.CurrencyControlFeeMin ?? 0, this.CurrencyControlFeeMax ?? decimal.MaxValue);
}
