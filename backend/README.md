# 💰 Smart Expense Tracker API

A production-ready RESTful API for personal finance management.
Built with **ASP.NET Core 8** · **Clean Architecture** · **SQL Server** · **Docker**

---

## 🚀 Features

| Feature | Details |
|---------|---------|
| 🔐 Auth | JWT + Refresh Token + Token Blacklist + Password Reset |
| 💸 Expenses | CRUD + Soft Delete + Restore + Search + Filter + Sort + Pagination |
| 📊 Budgets | Monthly budgets per category with real-time tracking |
| 🧠 Insights | Daily averages, category breakdowns, monthly trends |
| 📈 Reports | Monthly + Yearly + Excel Export |
| 🚨 Alerts | Smart alerts, no duplicates, pagination |
| 🔁 Recurring | Auto-process daily via Background Job |
| 👤 Profile | View + Update + Change Password + Currency |
| 🛡️ Admin | Users CRUD + Role + Suspend + Expenses + Reports + Audit Log |
| ⚡ Performance | DB-level filtering, no N+1 queries |
| 🔒 Security | Rate Limiting + HTTPS + CORS + Input Validation |

---

## 🏗️ Architecture

```
Domain/          → Entities, Constants, Exceptions (no dependencies)
Application/     → Business Logic, Services, DTOs, Interfaces
Infrastructure/  → EF Core, Repositories, JWT, Email, Export, Background Jobs
API/             → Controllers, Middleware, Swagger
```

---

## ⚙️ Tech Stack

`ASP.NET Core 8` · `Entity Framework Core` · `SQL Server` · `JWT` · `AutoMapper` · `FluentValidation` · `Serilog` · `ClosedXML` · `Swagger` · `Docker`

---

## 🐳 Quick Start (Docker)

```bash
# 1. Clone
git clone https://github.com/HadiAljaami/ExpenseTracker.git
cd ExpenseTracker

# 2. Configure environment
cp .env.example .env
# Edit .env with your values

# 3. Run
docker-compose up -d

# API ready at: http://localhost:8080
# Swagger UI:   http://localhost:8080
```

---

## 💻 Local Development

```bash
dotnet restore
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API
dotnet ef database update --project Infrastructure --startup-project API
dotnet run --project API
# Swagger: http://localhost:5000
```

---

## 🔑 Default Admin Credentials

```
Email:    admin@expensetracker.com
Password: Admin@123456
```
⚠️ **Change these immediately in production via `.env` file!**

---

## 📋 API Reference (v1)

### Auth `/api/v1/auth`
| Method | Endpoint | Auth |
|--------|----------|------|
| POST | `/register` | ❌ |
| POST | `/login` | ❌ |
| POST | `/refresh-token` | ❌ |
| POST | `/logout` | ✅ |
| POST | `/forgot-password` | ❌ |
| POST | `/reset-password` | ❌ |

### Expenses `/api/v1/expenses`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List with full filtering |
| GET | `/{id}` | Get by ID |
| POST | `/` | Create |
| PUT | `/{id}` | Update |
| DELETE | `/{id}` | Soft delete |
| GET | `/deleted` | View deleted |
| PATCH | `/{id}/restore` | Restore |

### Admin `/api/v1/admin` (Role: Admin)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/users` | All users (paginated + filtered) |
| GET | `/users/{id}` | User details |
| POST | `/users` | Create user |
| PATCH | `/users/{id}/role` | Change role |
| PATCH | `/users/{id}/suspend` | Suspend/unsuspend |
| DELETE | `/users/{id}` | Delete user |
| GET | `/expenses` | All expenses (filtered) |
| DELETE | `/expenses/{id}` | Delete expense |
| GET | `/stats` | System statistics |
| GET | `/stats/monthly` | Monthly growth |
| GET | `/reports` | Full system report |
| GET | `/audit-logs` | Admin audit trail |

### Other Endpoints
- `GET /api/v1/budgets` · `GET /api/v1/insights` · `GET /api/v1/reports/monthly`
- `GET /api/v1/reports/yearly` · `GET /api/v1/reports/export/excel`
- `GET /api/v1/alerts` · `GET /api/v1/users/me` · `GET /api/v1/categories`
- `GET /api/v1/recurring-expenses` · `GET /health`

---

## 🗄️ Default Categories

🍔 Food · 🚗 Transport · 💡 Bills · 🎬 Entertainment · 🏥 Healthcare · 🛍️ Shopping · 📚 Education · 📦 Other

---

## 📄 License
MIT License
