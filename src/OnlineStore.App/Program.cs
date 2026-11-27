using OnlineStore.Domain;

Console.WriteLine("Modern Programming Course");
Console.WriteLine("Student: Кущов Максим Андрійович");
Console.WriteLine("Group: д4тк");
Console.WriteLine("Variant: 1");
Console.WriteLine("Domain: Online Store");
Console.WriteLine(".NET: 10");
Console.WriteLine(new string('-', 40));

var laptop = new Product("Laptop", 35000m);
var mouse = new Product("Wireless Mouse", 1200m);
var keyboard = new Product("Mechanical Keyboard", 3400m);

var customer1 = new Customer("Кущов Максим Андрійович", "maksym.kushchov@example.com");
var customer2 = new Customer("Олена Коваленко", "olena.kovalenko@example.com");

// Order 1: CardPayment via IPaymentMethod
var order1 = new Order(customer1);
order1.AddItem(laptop, 1);
order1.AddItem(mouse, 2);
order1.AddItem(keyboard, 1);
order1.RemoveItem(mouse.Id);

Console.WriteLine($"Order 1 ({order1.Id}) for {order1.Customer.Name}");
Console.WriteLine($"Items count: {order1.Items.Count}, Total: {order1.Total:F2} UAH, Status: {order1.Status}");

IPaymentMethod cardPayment = new CardPayment("4242");
order1.Pay(cardPayment);
Console.WriteLine($"Paid via {cardPayment.Name}: {cardPayment.LastTransactionDetails}");
Console.WriteLine($"Order 1 Status after payment: {order1.Status}");

// Verify invariant: cannot pay twice or modify paid order
try
{
    order1.Pay(cardPayment);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Expected guard (re-pay): {ex.Message}");
}

try
{
    order1.AddItem(mouse, 1);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Expected guard (modify paid): {ex.Message}");
}

Console.WriteLine(new string('-', 40));

// Order 2: BankTransferPayment via IPaymentMethod
var order2 = new Order(customer2);
order2.AddItem(keyboard, 2);

Console.WriteLine($"Order 2 ({order2.Id}) for {order2.Customer.Name}");
Console.WriteLine($"Items count: {order2.Items.Count}, Total: {order2.Total:F2} UAH, Status: {order2.Status}");

IPaymentMethod bankTransfer = new BankTransferPayment("UA213223130000026007233566001");
order2.Pay(bankTransfer);
Console.WriteLine($"Paid via {bankTransfer.Name}: {bankTransfer.LastTransactionDetails}");
Console.WriteLine($"Order 2 Status after payment: {order2.Status}");

Console.WriteLine(new string('-', 40));

// Order 3: Cancel and verify cannot pay cancelled order
var order3 = new Order(customer1);
order3.AddItem(mouse, 1);
order3.Cancel();
Console.WriteLine($"Order 3 Status after cancel: {order3.Status}");

try
{
    order3.Pay(cardPayment);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Expected guard (pay cancelled): {ex.Message}");
}
