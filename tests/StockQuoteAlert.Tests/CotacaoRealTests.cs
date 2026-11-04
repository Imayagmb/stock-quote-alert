using StockQuoteAlert;

public class CotacaoRealTests
{
    [Fact]
    public async Task CotacaoReal_AcimaDaVenda_DisparaAlertaDeVenda()
    {
        decimal price = await new QuoteService().GetPriceAsync("VALE3");
        var monitor = new AlertMonitor(price - 1m, price - 2m);

        var status = monitor.Update(price);

        Assert.Equal(PriceStatus.AboveSell, status);
        Assert.True(monitor.ShouldSendAlert(status));
    }

    [Fact]
    public async Task CotacaoReal_AbaixoDaCompra_DisparaAlertaDeCompra()
    {
        decimal price = await new QuoteService().GetPriceAsync("VALE3");
        var monitor = new AlertMonitor(price + 2m, price + 1m);

        var status = monitor.Update(price);

        Assert.Equal(PriceStatus.BelowBuy, status);
        Assert.True(monitor.ShouldSendAlert(status));
    }

    [Fact]
    public async Task CotacaoReal_DentroDaFaixa_NaoDisparaAlerta()
    {
        decimal price = await new QuoteService().GetPriceAsync("VALE3");
        var monitor = new AlertMonitor(price + 1m, price - 1m);

        var status = monitor.Update(price);

        Assert.Equal(PriceStatus.InRange, status);
        Assert.False(monitor.ShouldSendAlert(status));
    }
}