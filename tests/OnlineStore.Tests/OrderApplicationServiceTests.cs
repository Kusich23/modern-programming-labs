using Microsoft.Extensions.Logging.Abstractions;
using OnlineStore.Domain;

namespace OnlineStore.Tests;

public class OrderApplicationServiceTests
{
    private static Customer CreateSampleCustomer() => new()
    {
        Name = "Кущов Максим Андрійович",
        Email = "maksym.kushchov@example.com",
        Address = new Address("Kyiv", "Khreshchatyk St", "22", "01001"),
        PhoneNumber = "+380501234567"
    };

    [Fact]
    public void ProcessPayment_WhenOrderIsValid_SendsNotificationAndMarksOrderPaid()
    {
        // Arrange
        var notifications = new InMemoryNotificationService();
        var service = new OrderApplicationService(notifications, NullLogger<OrderApplicationService>.Instance);
        var order = new Order(CreateSampleCustomer());
        order.AddItem(new Product("Laptop", 35000m), 1);

        // Act
        service.ProcessPayment(order, new CardPayment("4242"));

        // Assert
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Single(notifications.Messages);
        Assert.Contains(order.Id.ToString(), notifications.Messages[0]);
    }

    [Fact]
    public void ProcessPayment_WhenOrderIsCancelled_ThrowsAndDoesNotSendNotification()
    {
        // Arrange
        var notifications = new InMemoryNotificationService();
        var service = new OrderApplicationService(notifications, NullLogger<OrderApplicationService>.Instance);
        var order = new Order(CreateSampleCustomer());
        order.AddItem(new Product("Mouse", 1200m), 1);
        order.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.ProcessPayment(order, new BankTransferPayment()));
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Empty(notifications.Messages);
    }
}
