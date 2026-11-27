using Microsoft.Extensions.Logging;

namespace OnlineStore.Domain;

/// <summary>
/// Application service coordinating order payment with notifications and structured logging.
/// </summary>
public sealed class OrderApplicationService(
    INotificationService notifications,
    ILogger<OrderApplicationService> logger)
{
    private readonly INotificationService _notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
    private readonly ILogger<OrderApplicationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Processes payment for the specified order and sends a confirmation notification on success.
    /// </summary>
    public void ProcessPayment(Order order, IPaymentMethod paymentMethod)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(paymentMethod);

        _logger.LogInformation(
            "Starting payment processing for order {OrderId} (Total: {Total}, Method: {PaymentMethod})",
            order.Id,
            order.Total,
            paymentMethod.Name);

        if (order.Status == OrderStatus.Cancelled)
        {
            _logger.LogWarning(
                "Attempted to pay cancelled order {OrderId} for customer {CustomerEmail}",
                order.Id,
                order.Customer.Email);
        }

        order.Pay(paymentMethod);

        string confirmation = $"Order {order.Id} paid ({order.Total:F2} UAH) via {paymentMethod.Name}.";
        _notifications.Notify(confirmation);

        _logger.LogInformation(
            "Payment completed and notification sent for order {OrderId}",
            order.Id);
    }
}
