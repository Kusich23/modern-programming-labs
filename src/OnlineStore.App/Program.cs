using OnlineStore.Domain;

Console.WriteLine("Modern Programming Course");
Console.WriteLine("Student: Кущов Максим Андрійович");
Console.WriteLine("Group: д4тк");
Console.WriteLine("Variant: 1");
Console.WriteLine("Domain: Online Store");
Console.WriteLine(".NET: 10");
Console.WriteLine(new string('-', 50));

var kyivAddress = new Address("Kyiv", "Khreshchatyk St", "22", "01001");
var lvivAddress = new Address("Lviv", "Svobody Ave", "15", "79000");

var customerWithPhone = new Customer
{
    Name = "Кущов Максим Андрійович",
    Email = "maksym.kushchov@example.com",
    Address = kyivAddress,
    PhoneNumber = "+380501234567"
};

var customerWithoutPhone = new Customer
{
    Name = "Олена Коваленко",
    Email = "olena.kovalenko@example.com",
    Address = lvivAddress,
    PhoneNumber = null
};

Console.WriteLine($"Customer 1: {customerWithPhone.Name}, Phone: {customerWithPhone.PhoneNumber ?? "Not provided"}, City: {customerWithPhone.Address.City}");
Console.WriteLine($"Customer 2: {customerWithoutPhone.Name}, Phone: {customerWithoutPhone.PhoneNumber ?? "Not provided"}, City: {customerWithoutPhone.Address.City}");
Console.WriteLine(new string('-', 50));

var usbCable = new Product("USB-C Cable", 350m);
var mouse = new Product("Wireless Mouse", 1200m);
var keyboard = new Product("Mechanical Keyboard", 2500m);
var laptop = new Product("Laptop", 35000m);

IPaymentMethod cardPayment = new CardPayment("4242");
IPaymentMethod bankTransfer = new BankTransferPayment("UA213223130000026007233566001");

// 1. Small order (< 500 UAH)
var smallOrder = new Order(customerWithoutPhone);
smallOrder.AddItem(usbCable, 1);
PrintOrderSummary("Small Order (Created)", smallOrder);

// 2. Medium order (< 5000 UAH, Paid, >= 2000 UAH -> Free Delivery)
var mediumFreeOrder = new Order(customerWithPhone);
mediumFreeOrder.AddItem(keyboard, 1);
mediumFreeOrder.Pay(cardPayment);
PrintOrderSummary("Medium Order (Paid >= 2000)", mediumFreeOrder);

// 3. Medium order (Paid < 2000 UAH -> Standard Delivery)
var mediumStandardOrder = new Order(customerWithoutPhone);
mediumStandardOrder.AddItem(mouse, 1);
mediumStandardOrder.Pay(bankTransfer);
PrintOrderSummary("Medium Order (Paid < 2000)", mediumStandardOrder);

// 4. Large order (>= 5000 UAH, Paid -> Free Delivery)
var largeOrder = new Order(customerWithPhone);
largeOrder.AddItem(laptop, 1);
largeOrder.Pay(cardPayment);
PrintOrderSummary("Large Order (Paid)", largeOrder);

// 5. Cancelled order
var cancelledOrder = new Order(customerWithoutPhone);
cancelledOrder.AddItem(mouse, 1);
cancelledOrder.Cancel();
PrintOrderSummary("Cancelled Order", cancelledOrder);

static void PrintOrderSummary(string label, Order order)
{
    Console.WriteLine($"[{label}] Total: {order.Total:F2} UAH | Category: {order.GetOrderCategory()} | Delivery: {order.GetDeliveryType()}");
    Console.WriteLine($"  Status ({order.Status}): {order.GetStatusDescription()}");
}
