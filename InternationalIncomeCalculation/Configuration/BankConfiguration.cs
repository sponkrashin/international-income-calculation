namespace InternationalIncomeCalculation.Configuration;

public class BankConfiguration
{
    public string Name { get; set; } = string.Empty;
    
    public bool CanReceiveInternationalPayments { get; set; }
    
    public bool UseStockExchange { get; set; }
    
    public BankTariffConfiguration[] Tariffs { get; set; } = Array.Empty<BankTariffConfiguration>();
}
