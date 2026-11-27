# План мініпроєкту: Модуль заявок на повернення товарів (`OnlineStore Returns Module`)

## 1. Назва та мета MVP
- **Назва мініпроєкту:** Модуль заявок на повернення товарів (`OnlineStore Returns Module`)
- **Предметна область:** Інтернет-магазин (`OnlineStore`, Варіант 1)
- **Мета MVP:** Реалізувати мінімально життєздатний модуль керування заявками на повернення товарів поверх наявного домену `OnlineStore`, який дає змогу створювати заявки на повернення для оплачених замовлень, переглядати їх через REST API, схвалювати або відхиляти заявки з дотриманням бізнес-правил, зберігати дані в SQLite через EF Core та перевіряти логіку автоматизованими тестами й CI.
- **Обмеження MVP:** Без графічного інтерфейсу користувача, авторизації, інтеграції з платіжними шлюзами для реального повернення коштів та зовнішнього хостингу.

## 2. Учасник та розподіл ролей
- **Кущов Максим Андрійович** (група **д4тк**, індивідуальне виконання):
  - проєктування схеми бази даних та міграцій EF Core SQLite;
  - реалізація доменних сутностей, бізнес-правил і Minimal API маршрутів;
  - розробка модульних тестів xUnit та налаштування GitHub Actions CI.

## 3. Бізнес-правила предметної області
1. **Заборона порожньої заявки:** Не можна схвалити заявку (`Approve()`), якщо вона не містить жодної позиції `ReturnItem` (`Items.Count == 0`).
2. **Незмінність остаточного статусу:** Не можна повторно змінювати статус заявки (`Approve()` або `Reject()`), якщо вона вже перебуває у стані `Approved` або `Rejected` (`InvalidOperationException` / `409 Conflict`).
3. **Коректна кількість товару:** Кількість товару для повернення в кожній позиції `ReturnItem` має бути додатною (`Quantity > 0`, інакше `ArgumentOutOfRangeException` / `400 Bad Request`).
4. **Перевірка вихідного замовлення:** Заявку на повернення можна створити лише для існуючого замовлення (`404` / `400`), яке перебуває у статусі `Paid` (оплачене замовлення).
5. **Обов'язкова причина повернення:** При створенні заявки `Reason` не може бути порожнім рядком (`ArgumentException`).

## 4. Модель даних та схема зв'язків

### ER / UML схема (текстова)
```text
+--------------------+          1       N   +----------------------+
|     OrderRow       |----------------------|     ReturnRequest    |
| (існуюче замовл.)  |                      |----------------------|
+--------------------+                      | Id : Guid (PK)       |
                                            | OrderId : Guid (FK)  |
                                            | Reason : string      |
                                            | Status : ReturnStatus|
                                            | CreatedAt : DateTime |
                                            +----------------------+
                                                       | 1
                                                       |
                                                       | N
                                            +----------------------+
                                            |      ReturnItem      |
                                            |----------------------|
                                            | Id : Guid (PK)       |
                                            | ReturnRequestId (FK) |
                                            | ProductName : string |
                                            | Quantity : int       |
                                            | UnitPrice : decimal  |
                                            +----------------------+
```

### Статуси заявки (`ReturnStatus`)
- `Pending` — заявка створена та очікує на розгляд.
- `Approved` — заявка схвалена (остаточний статус).
- `Rejected` — заявка відхилена (остаточний статус).

### Persistence-моделі (EF Core SQLite)
Для збереження інкапсуляції доменних сутностей (`ReturnRequest`, `ReturnItem`) використовуються окремі таблиці та моделі:
- `ReturnRequestRow` (`Id`, `OrderId`, `Reason`, `Status`, `CreatedAt`, `List<ReturnItemRow> Items`)
- `ReturnItemRow` (`Id`, `ReturnRequestRowId`, `ProductName`, `Quantity`, `UnitPrice`)

## 5. Специфікація API-маршрутів (`/returns`)

| HTTP Method | Маршрут | Призначення | Request DTO | Response DTO | Коди статусів |
|---|---|---|---|---|---|
| `GET` | `/returns` | Отримати список усіх заявок на повернення (опційний фільтр `?status=Pending`) | — | `IReadOnlyList<ReturnRequestResponse>` | `200 OK` |
| `GET` | `/returns/{id:guid}` | Отримати одну заявку на повернення за ідентифікатором | — | `ReturnRequestResponse` | `200 OK`, `404 Not Found` |
| `POST` | `/returns` | Створити нову заявку на повернення для оплаченого замовлення | `CreateReturnRequest(Guid OrderId, string Reason, IReadOnlyList<CreateReturnItemRequest> Items)` | `ReturnRequestResponse` | `201 Created`, `400 Bad Request`, `404 Not Found`, `409 Conflict` |
| `POST` | `/returns/{id:guid}/approve` | Схвалити заявку на повернення (лише з `Pending` за наявності позицій) | — | `ReturnRequestResponse` | `200 OK`, `404 Not Found`, `409 Conflict` |
| `POST` | `/returns/{id:guid}/reject` | Відхилити заявку на повернення (лише зі статусу `Pending`) | `RejectReturnRequest(string? Note)` (опційно) | `ReturnRequestResponse` | `200 OK`, `404 Not Found`, `409 Conflict` |

## 6. План реалізації (Git Issues)
1. **Issue #14:** [Design return entities, persistence models and EF Core SQLite migration](https://github.com/Kusich23/modern-programming-labs/issues/14)
2. **Issue #15:** [Implement return domain rules and Minimal API endpoints (/returns)](https://github.com/Kusich23/modern-programming-labs/issues/15)
3. **Issue #16:** [Add xUnit tests for return workflow and configure GitHub Actions CI](https://github.com/Kusich23/modern-programming-labs/issues/16)
