using OnlineStore.Domain;

namespace OnlineStore.App;

/// <summary>
/// Holds seeded in-memory repositories for products, customers, and orders.
/// </summary>
public sealed record SeededStoreData(
    InMemoryRepository<Product> Products,
    InMemoryRepository<Customer> Customers,
    InMemoryRepository<Order> Orders);

/// <summary>
/// Generates realistic test data for the online store domain.
/// </summary>
public static class StoreDataSeeder
{
    public static SeededStoreData CreateSampleData()
    {
        var productsRepo = new InMemoryRepository<Product>();
        var customersRepo = new InMemoryRepository<Customer>();
        var ordersRepo = new InMemoryRepository<Order>();

        var laptop = new Product("Laptop Pro 15", 35000m);
        var monitor = new Product("4K Monitor 27\"", 12500m);
        var keyboard = new Product("Mechanical Keyboard", 3200m);
        var mouse = new Product("Wireless Mouse", 1200m);
        var headphones = new Product("Studio Headphones", 4800m);
        var ssd = new Product("NVMe SSD 1TB", 3600m);
        var usbCable = new Product("USB-C Cable", 350m);
        var mousePad = new Product("Desk Mat XL", 650m);

        Product[] products = [laptop, monitor, keyboard, mouse, headphones, ssd, usbCable, mousePad];
        foreach (Product product in products)
        {
            productsRepo.Add(product);
        }

        var c1 = new Customer
        {
            Name = "Кущов Максим Андрійович",
            Email = "maksym.kushchov@example.com",
            Address = new Address("Kyiv", "Khreshchatyk St", "22", "01001"),
            PhoneNumber = "+380501234567"
        };
        var c2 = new Customer
        {
            Name = "Олена Коваленко",
            Email = "olena.kovalenko@example.com",
            Address = new Address("Lviv", "Svobody Ave", "15", "79000"),
            PhoneNumber = "+380672345678"
        };
        var c3 = new Customer
        {
            Name = "Іван Петренко",
            Email = "ivan.petrenko@example.com",
            Address = new Address("Odesa", "Deribasivska St", "8", "65000"),
            PhoneNumber = null
        };
        var c4 = new Customer
        {
            Name = "Марія Шевченко",
            Email = "mariia.shevchenko@example.com",
            Address = new Address("Kharkiv", "Sumska St", "41", "61000"),
            PhoneNumber = "+380933456789"
        };
        var c5 = new Customer
        {
            Name = "Дмитро Бондар",
            Email = "dmytro.bondar@example.com",
            Address = new Address("Dnipro", "Yavornytskoho Ave", "101", "49000"),
            PhoneNumber = null
        };
        var c6 = new Customer
        {
            Name = "Софія Мельник",
            Email = "sofiia.melnyk@example.com",
            Address = new Address("Kyiv", "Saksahanskoho St", "14", "01033"),
            PhoneNumber = "+380994567890"
        };

        Customer[] customers = [c1, c2, c3, c4, c5, c6];
        foreach (Customer customer in customers)
        {
            customersRepo.Add(customer);
        }

        IPaymentMethod card = new CardPayment("4242");
        IPaymentMethod bank = new BankTransferPayment("UA213223130000026007233566001");

        // 20 orders with varied products, quantities, totals (<500, >1000, >10000) and statuses
        Order[] orders =
        [
            CreateOrder(c1, [(laptop, 1), (mouse, 1)], payWith: card),
            CreateOrder(c1, [(keyboard, 1), (mousePad, 2)], payWith: bank),
            CreateOrder(c1, [(usbCable, 2)], payWith: card),
            CreateOrder(c1, [(ssd, 2)], payWith: null),

            CreateOrder(c2, [(monitor, 1), (keyboard, 1)], payWith: bank),
            CreateOrder(c2, [(headphones, 1), (usbCable, 3)], payWith: card),
            CreateOrder(c2, [(mouse, 1)], payWith: null, cancel: true),

            CreateOrder(c3, [(laptop, 1), (monitor, 1)], payWith: bank),
            CreateOrder(c3, [(usbCable, 1)], payWith: card),
            CreateOrder(c3, [(ssd, 1), (mousePad, 1)], payWith: card),

            CreateOrder(c4, [(headphones, 2), (mouse, 2)], payWith: card),
            CreateOrder(c4, [(keyboard, 1)], payWith: bank),
            CreateOrder(c4, [(mousePad, 1)], payWith: null),

            CreateOrder(c5, [(monitor, 2)], payWith: bank),
            CreateOrder(c5, [(usbCable, 4), (mousePad, 2)], payWith: card),
            CreateOrder(c5, [(ssd, 1)], payWith: null, cancel: true),

            CreateOrder(c6, [(laptop, 1), (headphones, 1), (ssd, 1)], payWith: card),
            CreateOrder(c6, [(mouse, 3), (usbCable, 2)], payWith: bank),
            CreateOrder(c6, [(keyboard, 2)], payWith: card),
            CreateOrder(c6, [(usbCable, 1)], payWith: null)
        ];

        foreach (Order order in orders)
        {
            ordersRepo.Add(order);
        }

        return new SeededStoreData(productsRepo, customersRepo, ordersRepo);
    }

    private static Order CreateOrder(
        Customer customer,
        (Product Product, int Quantity)[] items,
        IPaymentMethod? payWith,
        bool cancel = false)
    {
        var order = new Order(customer);
        foreach ((Product product, int quantity) in items)
        {
            order.AddItem(product, quantity);
        }

        if (payWith is not null)
        {
            order.Pay(payWith);
        }
        else if (cancel)
        {
            order.Cancel();
        }

        return order;
    }
}
