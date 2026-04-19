# 💰 Smart Expense Tracker API

A production-ready RESTful API for personal finance management — built with **ASP.NET Core 8** using **Clean Architecture**.

---

## 🚀 Features

- 🔐 **JWT Authentication** — Register, Login, Logout with Token Blacklist
- 💸 **Expense Management** — Full CRUD, Soft Delete, Restore, Search, Filter, Sort, Pagination
- 📊 **Budget System** — Set monthly budgets per category with real-time tracking
- 🧠 **Insights Engine** — Daily averages, category breakdowns & monthly trends
- 📈 **Reports** — Monthly & Yearly reports with weekly breakdowns
- 📤 **Export** — Export expenses to Excel
- 🚨 **Smart Alerts** — Auto-triggered alerts with pagination, no duplicates
- 🔁 **Recurring Expenses** — Auto-process monthly/weekly/yearly expenses
- 👤 **User Profile** — View, update, change password, delete account
- 🛡️ **Admin Panel** — Manage users & view system statistics
- ⚡ **Rate Limiting** — Global + per-endpoint protection
- 🗑️ **Soft Delete** — Recoverable expense deletion

---

## 🏗️ Architecture

```
ExpenseTracker/
├── Domain/          → Entities only, zero dependencies
├── Application/     → Business logic, Services, DTOs, Interfaces
├── Infrastructure/  → EF Core, Repositories, JWT
└── API/             → Controllers, Middleware, Swagger
```

**Patterns:** Clean Architecture · Repository Pattern · Service Layer · Dependency Injection

---

## ⚙️ Tech Stack

| Technology | Purpose |
|-----------|---------|
| ASP.NET Core 8 | Web API |
| Entity Framework Core 8 | ORM |
| SQL Server | Database |
| JWT Bearer | Authentication |
| AutoMapper | Object mapping |
| FluentValidation | Input validation |
| Serilog | Structured logging |
| ClosedXML | Excel export |
| Swagger | API documentation |
| Rate Limiter | Request throttling |

---

## 📦 Getting Started

```bash
# 1. Clone
git clone https://github.com/HadiAljaami/ExpenseTracker.git
cd ExpenseTracker

# 2. Restore
dotnet restore

# 3. Update connection string in API/appsettings.json

# 4. Migrate
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API
dotnet ef database update --project Infrastructure --startup-project API

# 5. Run
dotnet run --project API
```

Swagger UI: **http://localhost:5000**

---

## 📋 API Endpoints

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register |
| POST | `/api/auth/login` | Login |
| POST | `/api/auth/logout` | Logout (revoke token) |

### Expenses
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/expenses?search=&categoryId=&fromDate=&sortBy=` | List with full filtering |
| GET | `/api/expenses/{id}` | Get by ID |
| POST | `/api/expenses` | Create |
| PUT | `/api/expenses/{id}` | Update |
| DELETE | `/api/expenses/{id}` | Soft delete |
| GET | `/api/expenses/deleted` | View deleted |
| PATCH | `/api/expenses/{id}/restore` | Restore deleted |

### Budgets
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/budgets` | List budgets |
| POST | `/api/budgets` | Create |
| PUT | `/api/budgets/{id}` | Update |
| DELETE | `/api/budgets/{id}` | Delete |

### Insights & Reports
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/insights` | Financial insights |
| GET | `/api/reports/monthly` | Monthly report |
| GET | `/api/reports/yearly` | Yearly report |
| GET | `/api/reports/export/excel` | Export to Excel |

### Alerts
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/alerts` | Paged alerts |
| GET | `/api/alerts?unreadOnly=true` | Unread only |
| PATCH | `/api/alerts/{id}/read` | Mark as read |
| PATCH | `/api/alerts/read-all` | Mark all as read |

### User Profile
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users/me` | Get profile |
| PUT | `/api/users/me` | Update profile + currency |
| PUT | `/api/users/me/change-password` | Change password |
| DELETE | `/api/users/me` | Delete account |

### Recurring Expenses
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/recurring-expenses` | List all |
| POST | `/api/recurring-expenses` | Create |
| PATCH | `/api/recurring-expenses/{id}/toggle` | Toggle active |
| DELETE | `/api/recurring-expenses/{id}` | Delete |

### Admin (Role: Admin)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/admin/users` | All users |
| GET | `/api/admin/users/{id}` | User details |
| DELETE | `/api/admin/users/{id}` | Delete user |
| GET | `/api/admin/stats` | System statistics |

---

## 🗄️ Default Categories

🍔 Food & Dining · 🚗 Transport · 💡 Bills · 🎬 Entertainment · 🏥 Healthcare · 🛍️ Shopping · 📚 Education · 📦 Other

---

## 📄 License
MIT License
