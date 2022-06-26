namespace InternationalIncomeCalculation.Configuration;

public class CommonConfiguration
{
    public decimal[] Incomes { get; set; } = Array.Empty<decimal>();

    public decimal StockExchangeRate { get; set; } = 0;

    public decimal StockExchangeFeePercentage { get; set; }

    public decimal CentralBankExchangeRate { get; set; }

    public decimal TotalIncome => this.Incomes.Sum();
}
