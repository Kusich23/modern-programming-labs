using OnlineStore.Domain;

Console.WriteLine("Modern Programming Course");
Console.WriteLine("Student: Кущов Максим Андрійович");
Console.WriteLine("Group: д4тк");
Console.WriteLine("Variant: 1");
Console.WriteLine("Domain: Online Store");
Console.WriteLine(".NET: 10");
Console.WriteLine(new string('=', 60));

// 1. Action<string> logger with timestamp
Action<string> logger = message => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");

var customer = new Customer
{
    Name = "Кущов Максим Андрійович",
    Email = "maksym.kushchov@example.com",
    Address = new Address("Kyiv", "Khreshchatyk St", "22", "01001"),
    PhoneNumber = "+380501234567"
};

var laptop = new Product("Laptop Pro 15", 35000m);
var mouse = new Product("Wireless Mouse", 1200m);
var keyboard = new Product("Mechanical Keyboard", 3200m);

// 2. Domain operations with Action<string> logger + Extension methods
var order = new Order(customer);
order.AddItem(laptop, 1, logger);
order.AddItem(mouse, 1, logger);
order.AddItem(keyboard, 1, logger);
order.RemoveItem(mouse.Id, logger);

Console.WriteLine($"\nSummary: {order.GetSummary()}");
Console.WriteLine($"Extension IsLargeOrder(): {order.IsLargeOrder()}");
Console.WriteLine($"Predicate<Order> LargeOrder(order): {DiscountPolicies.LargeOrder(order)}");

// 3. Apply all three Func<decimal, decimal> discount policies
Console.WriteLine("\nDiscount policies preview:");
Console.WriteLine($"  NoDiscount:          {DiscountPolicies.NoDiscount(order.Total):F2} UAH");
Console.WriteLine($"  TenPercentDiscount:  {DiscountPolicies.TenPercentDiscount(order.Total):F2} UAH");
Console.WriteLine($"  TwentyPercentDiscount: {DiscountPolicies.TwentyPercentDiscount(order.Total):F2} UAH");

// 4. Subscribe NotificationService and LoggingService to OrderPaid event via +=
var notificationService = new NotificationService(logger);
var loggingService = new LoggingService(logger);

order.OrderPaid += notificationService.Handle;
order.OrderPaid += loggingService.Handle;

Console.WriteLine("\nPaying order with 10% discount policy:");
IPaymentMethod cardPayment = new CardPayment("4242");
order.Pay(cardPayment, logger, DiscountPolicies.TenPercentDiscount);
Console.WriteLine($"Summary after Pay: {order.GetSummary()}");

// 5. Demonstrate Cancel with logger on a second order
Console.WriteLine("\nCancelling second order:");
var cancelledOrder = new Order(customer);
cancelledOrder.AddItem(mouse, 2, logger);
cancelledOrder.Cancel(logger);
Console.WriteLine($"Summary after Cancel: {cancelledOrder.GetSummary()} | IsLargeOrder: {cancelledOrder.IsLargeOrder()}");
