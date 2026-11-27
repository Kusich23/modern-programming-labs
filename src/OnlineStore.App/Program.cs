using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineStore.Domain;

Console.WriteLine("Modern Programming Course");
Console.WriteLine("Student: Кущов Максим Андрійович");
Console.WriteLine("Group: д4тк");
Console.WriteLine("Variant: 1");
Console.WriteLine("Domain: Online Store");
Console.WriteLine(".NET: 10");
Console.WriteLine(new string('=', 60));

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<INotificationService>(_ =>
    new ConsoleNotificationService(message => Console.WriteLine($"[ConsoleNotification] {message}")));
builder.Services.AddTransient<OrderApplicationService>();

using IHost host = builder.Build();

var orderService = host.Services.GetRequiredService<OrderApplicationService>();

var customer = new Customer
{
    Name = "Кущов Максим Андрійович",
    Email = "maksym.kushchov@example.com",
    Address = new Address("Kyiv", "Khreshchatyk St", "22", "01001"),
    PhoneNumber = "+380501234567"
};

var laptop = new Product("Laptop Pro 15", 35000m);
var mouse = new Product("Wireless Mouse", 1200m);

// 1. Successful payment via DI-configured OrderApplicationService + ConsoleNotificationService
Console.WriteLine("Scenario 1: Successful payment with ConsoleNotificationService");
var order1 = new Order(customer);
order1.AddItem(laptop, 1);
order1.AddItem(mouse, 1);
orderService.ProcessPayment(order1, new CardPayment("4242"));

// 2. Cancelled order payment attempt (LogWarning + InvalidOperationException)
Console.WriteLine("\nScenario 2: Payment attempt on cancelled order");
var cancelledOrder = new Order(customer);
cancelledOrder.AddItem(mouse, 1);
cancelledOrder.Cancel();

try
{
    orderService.ProcessPayment(cancelledOrder, new BankTransferPayment());
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Caught expected InvalidOperationException: {ex.Message}");
}

// 3. Swap INotificationService to InMemoryNotificationService without changing OrderApplicationService
Console.WriteLine("\nScenario 3: Swapping INotificationService to InMemoryNotificationService (OCP & DIP)");
HostApplicationBuilder inMemoryBuilder = Host.CreateApplicationBuilder(args);
inMemoryBuilder.Services.AddSingleton<InMemoryNotificationService>();
inMemoryBuilder.Services.AddSingleton<INotificationService>(sp =>
    sp.GetRequiredService<InMemoryNotificationService>());
inMemoryBuilder.Services.AddTransient<OrderApplicationService>();

using IHost inMemoryHost = inMemoryBuilder.Build();
var inMemoryOrderService = inMemoryHost.Services.GetRequiredService<OrderApplicationService>();
var inMemoryNotifications = inMemoryHost.Services.GetRequiredService<InMemoryNotificationService>();

var order2 = new Order(customer);
order2.AddItem(mouse, 2);
inMemoryOrderService.ProcessPayment(order2, new BankTransferPayment());

Console.WriteLine($"InMemoryNotificationService captured {inMemoryNotifications.Messages.Count} message(s):");
foreach (string msg in inMemoryNotifications.Messages)
{
    Console.WriteLine($"  - {msg}");
}
