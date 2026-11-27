using OnlineStore.Domain;

namespace OnlineStore.Tests;

public class OrderTests
{
    private static Customer CreateSampleCustomer() => new()
    {
        Name = "Кущов Максим Андрійович",
        Email = "maksym.kushchov@example.com",
        Address = new Address("Kyiv", "Khreshchatyk St", "22", "01001"),
        PhoneNumber = "+380501234567"
    };

    [Fact]
    public void Total_WithTwoDifferentProducts_CalculatesExpectedSum()
    {
        // Arrange
        var order = new Order(CreateSampleCustomer());
        var laptop = new Product("Laptop", 35000m);
        var mouse = new Product("Wireless Mouse", 1200m);

        // Act
        order.AddItem(laptop, 1);
        order.AddItem(mouse, 2);

        // Assert
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(37400m, order.Total);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_WithNonPositiveQuantity_ThrowsAndLeavesItemsUnchanged(int invalidQuantity)
    {
        // Arrange
        var order = new Order(CreateSampleCustomer());
        var mouse = new Product("Wireless Mouse", 1200m);
        order.AddItem(mouse, 1);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => order.AddItem(mouse, invalidQuantity));
        Assert.Single(order.Items);
        Assert.Equal(1200m, order.Total);
    }

    [Fact]
    public void Pay_WhenOrderIsCreated_TransitionsStatusToPaid()
    {
        // Arrange
        var order = new Order(CreateSampleCustomer());
        order.AddItem(new Product("Keyboard", 3200m), 1);
        IPaymentMethod paymentMethod = new CardPayment("4242");

        // Act
        order.Pay(paymentMethod);

        // Assert
        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void Pay_WhenOrderIsAlreadyPaid_ThrowsInvalidOperationExceptionAndKeepsStatusPaid()
    {
        // Arrange
        var order = new Order(CreateSampleCustomer());
        order.AddItem(new Product("Keyboard", 3200m), 1);
        IPaymentMethod paymentMethod = new CardPayment("4242");
        order.Pay(paymentMethod);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Pay(paymentMethod));
        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void Pay_WhenOrderIsCancelled_ThrowsInvalidOperationExceptionAndKeepsStatusCancelled()
    {
        // Arrange
        var order = new Order(CreateSampleCustomer());
        order.AddItem(new Product("Keyboard", 3200m), 1);
        order.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Pay(new BankTransferPayment()));
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void OrderPaid_WhenPaymentSucceeds_NotifiesBothSubscribers()
    {
        // Arrange
        var order = new Order(CreateSampleCustomer());
        order.AddItem(new Product("Laptop", 35000m), 1);

        var notifications = new List<string>();
        var auditLogs = new List<string>();
        var notificationService = new NotificationService(msg => notifications.Add(msg));
        var loggingService = new LoggingService(msg => auditLogs.Add(msg));

        order.OrderPaid += notificationService.Handle;
        order.OrderPaid += loggingService.Handle;

        // Act
        order.Pay(new CardPayment("4242"));

        // Assert
        Assert.Single(notifications);
        Assert.Single(auditLogs);
    }

    [Fact]
    public void OrderPaid_WhenPaymentFails_DoesNotInvokeSubscribers()
    {
        // Arrange
        var order = new Order(CreateSampleCustomer());
        order.AddItem(new Product("Mouse", 1200m), 1);
        order.Cancel();

        int eventCount = 0;
        order.OrderPaid += (_, _) => eventCount++;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Pay(new CardPayment("4242")));
        Assert.Equal(0, eventCount);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }
}
