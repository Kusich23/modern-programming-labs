using OnlineStore.Domain;

namespace OnlineStore.Tests;

public class ReturnRequestTests
{
    [Fact]
    public void Approve_WhenRequestHasItemsAndIsPending_TransitionsToApproved()
    {
        // Arrange
        var request = new ReturnRequest(Guid.NewGuid(), "Defective item");
        request.AddItem("Wireless Mouse", 2, 1200m);

        // Act
        request.Approve();

        // Assert
        Assert.Equal(ReturnStatus.Approved, request.Status);
        Assert.Equal(2400m, request.TotalRefundAmount);
    }

    [Fact]
    public void Approve_WhenRequestHasNoItems_ThrowsDomainRuleViolationAndKeepsStatusPending()
    {
        // Arrange
        var request = new ReturnRequest(Guid.NewGuid(), "Changed mind");

        // Act & Assert
        var ex = Assert.Throws<DomainRuleViolationException>(() => request.Approve());
        Assert.Equal("EMPTY_RETURN_REQUEST", ex.RuleCode);
        Assert.Equal(ReturnStatus.Pending, request.Status);
    }

    [Fact]
    public void Reject_WhenRequestIsAlreadyApproved_ThrowsInvalidOperationAndKeepsStatusApproved()
    {
        // Arrange
        var request = new ReturnRequest(Guid.NewGuid(), "Damaged box");
        request.AddItem("Keyboard", 1, 3200m);
        request.Approve();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => request.Reject());
        Assert.Equal(ReturnStatus.Approved, request.Status);
    }

    [Fact]
    public void Approve_WhenRequestIsAlreadyRejected_ThrowsInvalidOperationAndKeepsStatusRejected()
    {
        // Arrange
        var request = new ReturnRequest(Guid.NewGuid(), "Expired warranty");
        request.AddItem("Laptop", 1, 35000m);
        request.Reject();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => request.Approve());
        Assert.Equal(ReturnStatus.Rejected, request.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void AddItem_WithNonPositiveQuantity_ThrowsArgumentOutOfRangeAndKeepsItemsEmpty(int invalidQuantity)
    {
        // Arrange
        var request = new ReturnRequest(Guid.NewGuid(), "Defective item");

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            request.AddItem("Wireless Mouse", invalidQuantity, 1200m));
        Assert.Empty(request.Items);
    }
}
