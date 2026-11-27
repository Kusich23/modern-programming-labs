namespace OnlineStore.Domain;

/// <summary>
/// Provides lambda-based discount policies and order predicates for the store domain.
/// </summary>
public static class DiscountPolicies
{
    public static Func<decimal, decimal> NoDiscount { get; } = total => total;

    public static Func<decimal, decimal> TenPercentDiscount { get; } = total => total * 0.90m;

    public static Func<decimal, decimal> TwentyPercentDiscount { get; } = total => total * 0.80m;

    public static Predicate<Order> LargeOrder { get; } = order => order.Total >= 5000m;
}
