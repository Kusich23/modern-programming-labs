using OnlineStore.Domain;

Console.WriteLine("Modern Programming Course");
Console.WriteLine("Student: Кущов Максим Андрійович");
Console.WriteLine("Group: д4тк");
Console.WriteLine("Variant: 1");
Console.WriteLine("Domain: Online Store");
Console.WriteLine(".NET: 10");
Console.WriteLine(new string('=', 60));

var customer = new Customer
{
    Name = "Кущов Максим Андрійович",
    Email = "maksym.kushchov@example.com",
    Address = new Address("Kyiv", "Khreshchatyk St", "22", "01001"),
    PhoneNumber = "+380501234567"
};

var laptop = new Product("Laptop Pro 15", 35000m);
var mouse = new Product("Wireless Mouse", 1200m);

int orderPaidTriggerCount = 0;
Action<string> logger = message => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
var notificationService = new NotificationService(logger);

var order = new Order(customer);
order.OrderPaid += notificationService.Handle;
order.OrderPaid += (_, _) => orderPaidTriggerCount++;

// 1. Valid operation
Console.WriteLine("Operation: AddItem(laptop, 1) + AddItem(mouse, 2)");
order.AddItem(laptop, 1, logger);
order.AddItem(mouse, 2, logger);
Console.WriteLine($"Order status: {order.Status}, Order items count: {order.Items.Count}, Total: {order.Total:F2} UAH\n");

// 2. Invalid operation #1: negative quantity (ArgumentOutOfRangeException)
Console.WriteLine("Operation: AddItem(mouse, quantity = -2)");
try
{
    order.AddItem(mouse, -2, logger);
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Error ({ex.GetType().Name}): {ex.Message}");
}
Console.WriteLine($"Order status: {order.Status}, Order items count: {order.Items.Count}\n");

// 3. Invalid operation #2: exceeding max quantity per product (DomainRuleViolationException)
Console.WriteLine($"Operation: AddItem(mouse, quantity = 150)");
try
{
    order.AddItem(mouse, 150, logger);
}
catch (DomainRuleViolationException ex)
{
    Console.WriteLine($"Error ({ex.GetType().Name} [{ex.RuleCode}]): {ex.Message}");
}
Console.WriteLine($"Order status: {order.Status}, Order items count: {order.Items.Count}\n");

// Valid payment
Console.WriteLine("Operation: Pay(CardPayment)");
order.Pay(new CardPayment("4242"), logger);
Console.WriteLine($"Order status: {order.Status}, OrderPaid events fired: {orderPaidTriggerCount}\n");

// 4. Invalid operation #3: re-paying an already Paid order (InvalidOperationException)
Console.WriteLine("Operation: Pay(CardPayment) again on Paid order");
try
{
    order.Pay(new CardPayment("4242"), logger);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Error ({ex.GetType().Name}): {ex.Message}");
}
Console.WriteLine($"Order status: {order.Status}, OrderPaid events fired: {orderPaidTriggerCount}\n");

// 5. Invalid operation #4: modifying items of a Paid order (InvalidOperationException)
Console.WriteLine("Operation: AddItem(mouse, 1) on Paid order");
try
{
    order.AddItem(mouse, 1, logger);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Error ({ex.GetType().Name}): {ex.Message}");
}
Console.WriteLine($"Order status: {order.Status}, Order items count: {order.Items.Count}\n");

// 6. Invalid operation #5: paying a Cancelled order (state & OrderPaid unchanged)
var cancelledOrder = new Order(customer);
int cancelledOrderPaidCount = 0;
cancelledOrder.OrderPaid += (_, _) => cancelledOrderPaidCount++;
cancelledOrder.AddItem(mouse, 1);
cancelledOrder.Cancel(logger);

Console.WriteLine("Operation: Pay(BankTransferPayment) on Cancelled order");
try
{
    cancelledOrder.Pay(new BankTransferPayment(), logger);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Error ({ex.GetType().Name}): {ex.Message}");
}
Console.WriteLine($"Cancelled order status: {cancelledOrder.Status}, OrderPaid events fired: {cancelledOrderPaidCount}\n");

// 7. int.TryParse validation for simulated user quantity inputs
string[] rawQuantityInputs = ["3", "abc", "-5"];
Console.WriteLine("Validating simulated user quantity inputs with int.TryParse:");
foreach (string rawInput in rawQuantityInputs)
{
    if (!int.TryParse(rawInput, out int parsedQuantity))
    {
        Console.WriteLine($"  Input '{rawInput}' -> Invalid integer format (TryParse returned false).");
        continue;
    }

    try
    {
        var tempOrder = new Order(customer);
        tempOrder.AddItem(mouse, parsedQuantity);
        Console.WriteLine($"  Input '{rawInput}' -> Added {parsedQuantity}x '{mouse.Name}' (Total: {tempOrder.Total:F2} UAH).");
    }
    catch (ArgumentOutOfRangeException ex)
    {
        Console.WriteLine($"  Input '{rawInput}' -> Validation error: {ex.Message}");
    }
}
