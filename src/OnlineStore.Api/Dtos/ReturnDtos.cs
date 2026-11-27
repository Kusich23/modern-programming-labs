using OnlineStore.Api.Persistence;

namespace OnlineStore.Api.Dtos;

public sealed record CreateReturnItemRequest(string ProductName, int Quantity, decimal UnitPrice);

public sealed record CreateReturnRequest(
    Guid OrderId,
    string Reason,
    IReadOnlyList<CreateReturnItemRequest> Items);

public sealed record ReturnItemResponse(
    Guid Id,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);

public sealed record ReturnRequestResponse(
    Guid Id,
    Guid OrderId,
    string Reason,
    string Status,
    DateTime CreatedAt,
    decimal TotalRefundAmount,
    IReadOnlyList<ReturnItemResponse> Items)
{
    public static ReturnRequestResponse FromRow(ReturnRequestRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        IReadOnlyList<ReturnItemResponse> items = row.Items
            .Select(i => new ReturnItemResponse(
                i.Id,
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity))
            .ToList();

        decimal total = items.Sum(i => i.Subtotal);
        return new ReturnRequestResponse(
            row.Id,
            row.OrderId,
            row.Reason,
            row.Status,
            row.CreatedAt,
            total,
            items);
    }
}
