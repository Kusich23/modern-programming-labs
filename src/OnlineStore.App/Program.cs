using OnlineStore.App;
using OnlineStore.Domain;

Console.WriteLine("Modern Programming Course");
Console.WriteLine("Student: Кущов Максим Андрійович");
Console.WriteLine("Group: д4тк");
Console.WriteLine("Variant: 1");
Console.WriteLine("Domain: Online Store");
Console.WriteLine(".NET: 10");
Console.WriteLine(new string('=', 60));

SeededStoreData data = StoreDataSeeder.CreateSampleData();
IReadOnlyCollection<Order> orders = data.Orders.GetAll();
IReadOnlyCollection<Customer> customers = data.Customers.GetAll();

Console.WriteLine($"Seeded: {data.Products.GetAll().Count} products, {customers.Count} customers, {orders.Count} orders.");
Console.WriteLine(new string('=', 60));

// 1. All orders with Total > 1000
var ordersOver1000 = orders
    .Where(o => o.Total > 1000m)
    .ToList();
Console.WriteLine($"1. Orders with Total > 1000 ({ordersOver1000.Count}):");
foreach (Order order in ordersOver1000)
{
    Console.WriteLine($"   - {order.Id}: {order.Customer.Name} | {order.Total:F2} UAH | {order.Status}");
}

// 2. Top-5 most expensive orders
var top5Orders = orders
    .OrderByDescending(o => o.Total)
    .Take(5)
    .ToList();
Console.WriteLine("\n2. Top-5 most expensive orders:");
foreach (Order order in top5Orders)
{
    Console.WriteLine($"   - {order.Total:F2} UAH ({order.Customer.Name}, {order.Status})");
}

// 3. Total sales amount (Sum)
decimal totalSales = orders.Sum(o => o.Total);
Console.WriteLine($"\n3. Total sales amount (all orders): {totalSales:F2} UAH");

// 4. Average order value (Average)
decimal averageOrderValue = orders.Average(o => o.Total);
Console.WriteLine($"\n4. Average order value: {averageOrderValue:F2} UAH");

// 5. Orders count per customer via GroupBy
var ordersPerCustomer = orders
    .GroupBy(o => o.Customer)
    .Select(g => new { CustomerName = g.Key.Name, Count = g.Count() })
    .OrderByDescending(x => x.Count)
    .ToList();
Console.WriteLine("\n5. Orders count per customer:");
foreach (var entry in ordersPerCustomer)
{
    Console.WriteLine($"   - {entry.CustomerName}: {entry.Count} orders");
}

// 6. All products from all orders via SelectMany
var orderedProducts = orders
    .SelectMany(o => o.Items)
    .Select(i => i.Product)
    .DistinctBy(p => p.Id)
    .ToList();
Console.WriteLine($"\n6. Distinct products across all orders ({orderedProducts.Count}):");
foreach (Product product in orderedProducts)
{
    Console.WriteLine($"   - {product.Name} ({product.Price:F2} UAH)");
}

// 7. Top-3 products by sold quantity
var top3ProductsByQty = orders
    .SelectMany(o => o.Items)
    .GroupBy(i => i.Product)
    .Select(g => new { ProductName = g.Key.Name, TotalQuantity = g.Sum(i => i.Quantity) })
    .OrderByDescending(x => x.TotalQuantity)
    .Take(3)
    .ToList();
Console.WriteLine("\n7. Top-3 products by sold quantity:");
foreach (var item in top3ProductsByQty)
{
    Console.WriteLine($"   - {item.ProductName}: {item.TotalQuantity} pcs");
}

// 8. Customers who spent more than 5000
var highSpendingCustomers = orders
    .GroupBy(o => o.Customer)
    .Select(g => new { Customer = g.Key, TotalSpent = g.Sum(o => o.Total) })
    .Where(x => x.TotalSpent > 5000m)
    .OrderByDescending(x => x.TotalSpent)
    .ToList();
Console.WriteLine("\n8. Customers who spent > 5000 UAH:");
foreach (var entry in highSpendingCustomers)
{
    Console.WriteLine($"   - {entry.Customer.Name}: {entry.TotalSpent:F2} UAH");
}

// 9. Check via Any if there is an order > 10000
bool hasOrderOver10000 = orders.Any(o => o.Total > 10000m);
Console.WriteLine($"\n9. Any order with Total > 10000: {hasOrderOver10000}");

// 10. Dictionary<Guid, decimal> with total spending per customer (key = Customer.Id)
Dictionary<Guid, decimal> spendingByCustomerId = orders
    .GroupBy(o => o.Customer.Id)
    .ToDictionary(g => g.Key, g => g.Sum(o => o.Total));
Console.WriteLine("\n10. Total spending dictionary (CustomerId -> TotalSpent):");
foreach ((Guid customerId, decimal spent) in spendingByCustomerId)
{
    string name = customers.First(c => c.Id == customerId).Name;
    Console.WriteLine($"   - {customerId} ({name}): {spent:F2} UAH");
}
