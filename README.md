# 💰 Smart Expense Tracker API

A production-ready RESTful API for personal finance management — built with **ASP.NET Core 8** using **Clean Architecture**.

---

## 🚀 Features

- 🔐 **JWT Authentication** — Register & Login with secure token-based auth
- 💸 **Expense Management** — Full CRUD with filtering, sorting & pagination
- 📊 **Budget System** — Set monthly budgets per category and track usage in real time
- 🧠 **Insights Engine** — Analyzes spending behavior with daily averages, category breakdowns & monthly trends
- 🚨 **Smart Alerts** — Auto-triggered alerts for budget warnings, exceeded limits & spending spikes
- 📈 **Monthly Reports** — Full reports with weekly breakdown and category analysis

---

## 🏗️ Architecture

```
ExpenseTracker/
├── Domain/          → Entities only, zero dependencies
├── Application/     → Business logic, Services, DTOs, Interfaces
├── Infrastructure/  → EF Core, Repositories, JWT
└── API/             → Controllers, Middleware, Swagger
```

**Patterns used:** Clean Architecture · Repository Pattern · Service Layer · Dependency Injection

---

## ⚙️ Tech Stack

| Technology | Purpose |
|-----------|---------|
| ASP.NET Core 8 | Web API framework |
| Entity Framework Core | ORM |
| SQL Server | Database |
| JWT Bearer | Authentication |
| AutoMapper | Object mapping |
| FluentValidation | Input validation |
| Serilog | Structured logging |
| Swagger / Swashbuckle | API documentation |

---

## 📦 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or Express)
- `dotnet-ef` tool

```bash
dotnet tool install --global dotnet-ef
```

### Setup

```bash
# 1. Clone the repository
git clone https://github.com/your-username/ExpenseTracker.git
cd ExpenseTracker

# 2. Restore packages
dotnet restore

# 3. Update connection string in API/appsettings.json
# "DefaultConnection": "Server=localhost;Database=ExpenseTrackerDb;..."

# 4. Apply migrations
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API
dotnet ef database update --project Infrastructure --startup-project API

# 5. Run
dotnet run --project API
```

Swagger UI opens at: **http://localhost:5000**

---

## 🔑 Authentication

All endpoints (except `/api/auth/*`) require a Bearer token.

```
Authorization: Bearer <your_token>
```

In Swagger, click **Authorize** and paste: `Bearer <token>`

---

## 📋 API Endpoints

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Create account |
| POST | `/api/auth/login` | Login & get token |

### Expenses
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/expenses` | List with filter/sort/pagination |
| GET | `/api/expenses/{id}` | Get by ID |
| POST | `/api/expenses` | Create expense |
| PUT | `/api/expenses/{id}` | Update expense |
| DELETE | `/api/expenses/{id}` | Delete expense |

### Budgets
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/budgets` | Get monthly budgets |
| POST | `/api/budgets` | Create budget |
| DELETE | `/api/budgets/{id}` | Delete budget |

### Insights & Reports
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/insights` | Financial insights summary |
| GET | `/api/reports/monthly` | Full monthly report |

### Alerts
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/alerts` | Unread alerts |
| PATCH | `/api/alerts/{id}/read` | Mark as read |

### Categories
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/categories` | All categories |

---

## 🗄️ Default Categories (Seeded)

| # | Category | Icon |
|---|----------|------|
| 1 | Food & Dining | 🍔 |
| 2 | Transport | 🚗 |
| 3 | Bills & Utilities | 💡 |
| 4 | Entertainment | 🎬 |
| 5 | Healthcare | 🏥 |
| 6 | Shopping | 🛍️ |
| 7 | Education | 📚 |
| 8 | Other | 📦 |

---

## 📄 License

MIT License — feel free to use this project for learning or your portfolio.
