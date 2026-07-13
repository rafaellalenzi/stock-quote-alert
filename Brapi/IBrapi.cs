public interface IBrapi
{
    public Task <BrapiResponse?> GetStockQuoteAsync(string symbol);
}