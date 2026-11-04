using StockQuoteAlert;

public class AlertMonitorTests
{
    [Fact]
    public void PrecoDentroDaFaixa_NaoEnviaAlerta()
    {
        var monitor = new AlertMonitor(22.67m, 22.59m);
        var status = monitor.Update(22.61m);

        Assert.Equal(PriceStatus.InRange, status);
        Assert.False(monitor.ShouldSendAlert(status));
    }

    [Fact]
    public void PrecoIgualAoLimite_NaoDisparaAlerta()
    {
        var monitor = new AlertMonitor(22.67m, 22.59m);

        Assert.Equal(PriceStatus.InRange, monitor.Update(22.67m));
        Assert.Equal(PriceStatus.InRange, monitor.Update(22.59m));
    }

    [Fact]
    public void AcimaDaVenda_EnviaUmaVezSo()
    {
        var monitor = new AlertMonitor(22.67m, 22.59m);

        var primeira = monitor.Update(22.70m);
        Assert.True(monitor.ShouldSendAlert(primeira));
        monitor.MarkAlertSent(primeira);

        var segunda = monitor.Update(22.72m);
        Assert.False(monitor.ShouldSendAlert(segunda));
    }

    [Fact]
    public void VoltaParaFaixa_LiberaNovoAlerta()
    {
        var monitor = new AlertMonitor(22.67m, 22.59m);

        var status = monitor.Update(22.70m);
        monitor.MarkAlertSent(status);
        monitor.Update(22.61m);

        var deNovo = monitor.Update(22.70m);
        Assert.True(monitor.ShouldSendAlert(deNovo));
    }

    [Fact]
    public void EnvioFalhou_TentaDeNovoNaProximaConsulta()
    {
        var monitor = new AlertMonitor(22.67m, 22.59m);

        monitor.Update(22.70m);
        var proxima = monitor.Update(22.70m);

        Assert.True(monitor.ShouldSendAlert(proxima));
    }
}