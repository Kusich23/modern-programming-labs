using OnlineStore.Domain;

namespace OnlineStore.Api.Dtos;

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);

public sealed record CreateOrderRequest(Guid CustomerId, IReadOnlyList<CreateOrderItemRequest> Items);

public sealed record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal);

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string Status,
    decimal Total,
    IReadOnlyList<OrderItemResponse> Items)
{
    public static OrderResponse FromOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        IReadOnlyList<OrderItemResponse> items = order.Items
            .Select(i => new OrderItemResponse(
                i.Product.Id,
                i.Product.Name,
                i.UnitPrice,
                i.Quantity,
                i.Subtotal))
            .ToList();

        return new OrderResponse(
            order.Id,
            order.Customer.Id,
            order.Customer.Name,
            order.Status.ToString(),
            order.Total,
            items);
    }
}
