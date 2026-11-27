# Modern Programming — OnlineStore

- **ПІБ:** Кущов Максим Андрійович
- **Група:** д4тк
- **Номер варіанта:** 1
- **Предметна область:** Інтернет-магазин (`OnlineStore`)
- **Версія .NET:** .NET 10

## Опис предметної області та модуля повернень

Проєкт реалізує інформаційну систему інтернет-магазину з каталогом товарів, клієнтами, замовленнями (`Order`, `OrderItem`) та модулем обробки заявок на повернення товарів (`ReturnRequest`, `ReturnItem`). Доменна модель інкапсулює колекції й гарантує бізнес-правила: додатні ціни та кількості, заборону зміни або повторної оплати вже оплачених/скасованих замовлень, а також заборону схвалення порожньої заявки на повернення чи повторної зміни її остаточного статусу (`Approved` / `Rejected`). Вебшар реалізований на ASP.NET Core Minimal API зі збереженням даних у SQLite через Entity Framework Core.

## Структура рішення (`ModernProgramming.slnx`)

- `src/OnlineStore.Domain/` — бібліотека предметної області (сутності `Product`, `Customer`, `Order`, `OrderItem`, `ReturnRequest`, `ReturnItem`, value object `Address`, інтерфейси `IPaymentMethod`, `INotificationService`, сервіси, винятки).
- `src/OnlineStore.App/` — консольний застосунок для демонстрації ООП, LINQ, подій, конфігурації JSON, асинхронного HTTP-клієнта та DI.
- `src/OnlineStore.Api/` — ASP.NET Core Minimal API з OpenAPI (`/openapi/v1.json`), EF Core SQLite (`OnlineStoreDbContext`, міграції) та `.http`-файлом запитів.
- `tests/OnlineStore.Tests/` — модульні тести xUnit (`OrderTests`, `OrderApplicationServiceTests`, `ReturnRequestTests`).
- `docs/project-plan.md` — специфікація мініпроєкту, схема даних та API-контракти.
- `.github/workflows/dotnet.yml` — GitHub Actions CI для автоматичної збірки та тестування.

## Вимоги та залежності

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- `Microsoft.EntityFrameworkCore.Sqlite` та `Microsoft.EntityFrameworkCore.Design` (`10.*`)
- `Microsoft.AspNetCore.OpenApi`
- Інструмент `dotnet-ef` (`10.*`) для запуску міграцій

## Локальний запуск, міграції та тестування

1. Відновити залежності та зібрати рішення:
   ```bash
   dotnet restore ModernProgramming.slnx
   dotnet build ModernProgramming.slnx --configuration Release
   ```

2. Застосувати міграції EF Core до локальної бази SQLite (`onlinestore.db`):
   ```bash
   dotnet ef database update --project src/OnlineStore.Api
   ```

3. Запустити веб-API:
   ```bash
   dotnet run --project src/OnlineStore.Api
   ```

4. Запустити модульні тести:
   ```bash
   dotnet test ModernProgramming.slnx --configuration Release
   ```

## Приклади HTTP-запитів

### Замовлення (`/orders`)
- Отримати список замовлень (опційно з фільтром `?status=Paid`):
  ```http
  GET /orders?status=Paid
  ```
- Створити нове замовлення (`201 Created`):
  ```http
  POST /orders
  Content-Type: application/json

  {
    "customerId": "11111111-1111-1111-1111-111111111111",
    "items": [
      { "productId": "22222222-2222-2222-2222-222222222222", "quantity": 1 }
    ]
  }
  ```
- Оплатити замовлення (`200 OK`, або `409 Conflict` при повторній оплаті):
  ```http
  POST /orders/{id}/pay
  ```

### Заявки на повернення (`/returns`)
- Отримати всі заявки або заявку за ID:
  ```http
  GET /returns
  GET /returns/{id}
  ```
- Створити заявку на повернення для оплаченого замовлення (`201 Created`, `400 Bad Request`, `404 Not Found`, `409 Conflict`):
  ```http
  POST /returns
  Content-Type: application/json

  {
    "orderId": "{paidOrderId}",
    "reason": "Defective scroll wheel",
    "items": [
      { "productName": "Wireless Mouse", "quantity": 1, "unitPrice": 1200 }
    ]
  }
  ```
- Схвалити або відхилити заявку (`200 OK`, `404 Not Found`, `409 Conflict` для порожньої чи фіналізованої заявки):
  ```http
  POST /returns/{id}/approve
  POST /returns/{id}/reject
  ```

Додаткові приклади запитів доступні у файлі [OnlineStore.Api.http](src/OnlineStore.Api/OnlineStore.Api.http).
