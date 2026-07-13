public interface IBrapi
{
    public Task <ApiResponse?> GetStockQuoteAsync(string symbol);
}