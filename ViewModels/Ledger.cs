namespace phoneCaseReworked.ViewModels
{
    public class LedgerTransactionViewModel
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RemainingBalance { get; set; }
        public string TransactionType { get; set; }
        public List<int> PurchaseIds { get; set; } = new List<int>();
    }
}
