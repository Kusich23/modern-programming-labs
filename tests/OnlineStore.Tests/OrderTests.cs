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
}
