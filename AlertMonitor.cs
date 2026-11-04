namespace StockQuoteAlert;

public enum PriceStatus
{
    InRange,
    AboveSell,
    BelowBuy
}

public class AlertMonitor
{
    private readonly decimal _sellPrice;
    private readonly decimal _buyPrice;
    private bool _sellAlertSent;
    private bool _buyAlertSent;

    public AlertMonitor(decimal sellPrice, decimal buyPrice)
    {
        _sellPrice = sellPrice;
        _buyPrice = buyPrice;
    }

    public PriceStatus Update(decimal price)
    {
        if (price > _sellPrice)
        {
            _buyAlertSent = false;
            return PriceStatus.AboveSell;
        }

        if (price < _buyPrice)
        {
            _sellAlertSent = false;
            return PriceStatus.BelowBuy;
        }

        _sellAlertSent = false;
        _buyAlertSent = false;
        return PriceStatus.InRange;
    }

    public bool ShouldSendAlert(PriceStatus status)
    {
        return (status == PriceStatus.AboveSell && !_sellAlertSent)
            || (status == PriceStatus.BelowBuy && !_buyAlertSent);
    }

    public void MarkAlertSent(PriceStatus status)
    {
        if (status == PriceStatus.AboveSell) _sellAlertSent = true;
        if (status == PriceStatus.BelowBuy) _buyAlertSent = true;
    }
}