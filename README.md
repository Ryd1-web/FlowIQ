# 🧾 FlowIQ — MSME Cashflow Assistant

**A production-grade financial management application built for Nigerian Micro, Small & Medium Enterprises (MSMEs).**

FlowIQ helps small business owners track income, manage expenses, monitor cashflow health, and get AI-powered predictions — all from a mobile-friendly interface designed with simple language.

---

## 🏗️ Architecture

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│   Angular PWA    │────▶│  .NET 8 Web API  │────▶│ Python FastAPI   │
│   (Frontend)     │     │   (Backend)      │     │  (AI Service)    │
│   Port: 4200     │     │   Port: 5146     │     │  Port: 8000      │
└──────────────────┘     └──────────────────┘     └──────────────────┘
                               │
                               ▼
                         ┌──────────────┐
                         │  PostgreSQL  │
                         │  Port: 5432  │
                         └──────────────┘
```

### Integration Rules
- **Angular → .NET → FastAPI** (frontend never calls AI service directly)
- AI failures are graceful — core app continues working if AI service is down
- Backend returns standardized `ApiResponse<T>` wrappers

---

## 📁 Project Structure

```
FlowIQ/
├── backend/                        # .NET 8 Web API (Clean Architecture)
│   ├── src/
│   │   ├── FlowIQ.Domain/         # Entities, Enums, Repository Interfaces
│   │   ├── FlowIQ.Application/    # DTOs, Services, Validators, Exceptions
│   │   ├── FlowIQ.Infrastructure/ # EF Core, Repositories, JWT, AI Client
│   │   └── FlowIQ.API/           # Controllers, Middleware, Config
│   └── tests/
│       └── FlowIQ.Tests/         # xUnit + Moq + FluentAssertions
├── frontend/
│   └── flowiq-app/               # Angular 19+ PWA with Material Design
│       └── src/app/
│           ├── pages/            # Auth, Dashboard, Income, Expense, History
│           ├── services/         # API services + offline storage
│           ├── guards/           # Auth guard
│           ├── interceptors/     # JWT interceptor
│           └── models/           # TypeScript interfaces
├── ai-service/                   # Python FastAPI AI Microservice
│   ├── app/
│   │   ├── models.py            # Pydantic request/response models
│   │   ├── prediction_service.py # Cashflow prediction (polynomial regression)
│   │   └── anomaly_service.py   # Anomaly detection (IQR + Z-score)
│   ├── tests/
│   └── main.py                  # FastAPI app entry point
└── README.md
```

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) & npm
- [Python 3.11+](https://www.python.org/downloads/)
- [PostgreSQL 15+](https://www.postgresql.org/download/)

### 1️⃣ Database Setup

```bash
# Create PostgreSQL database
psql -U postgres -c "CREATE DATABASE flowiq;"
```

Update the connection string in `backend/src/FlowIQ.API/appsettings.json` if needed:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=flowiq;Username=postgres;Password=postgres"
}
```

### 2️⃣ Backend (.NET 8)

```bash
cd backend

# Restore packages
dotnet restore

# Run tests (16 tests)
dotnet test

# Run the API (starts on port 5146)
dotnet run --project src/FlowIQ.API
```

The API auto-seeds sample data on first run:
- **Test User**: +2348012345678 (OTP: always `123456` in dev)
- **Business**: Wale's Provisions Store
- **Data**: 30 days of random income/expense transactions

**Swagger UI**: http://localhost:5146/swagger

### 3️⃣ Frontend (Angular)

```bash
cd frontend/flowiq-app

# Install dependencies
npm install

# Start dev server (port 4200)
npx ng serve

# Production build
npx ng build
```

### 4️⃣ AI Service (Python)

```bash
cd ai-service

# Create virtual environment
python -m venv venv
venv\Scripts\activate  # Windows
# source venv/bin/activate  # macOS/Linux

# Install dependencies
pip install -r requirements.txt

# Run tests
pytest tests/ -v

# Start server (port 8000)
uvicorn main:app --host 0.0.0.0 --port 8000 --reload
```

**API Docs**: http://localhost:8000/docs

---

## 🔑 API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register with Nigerian phone number |
| POST | `/api/auth/verify-otp` | Verify OTP (always `123456` in dev) |

### Business
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/business` | Get user's businesses |
| POST | `/api/business` | Create a new business |

### Income
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/income/business/{businessId}` | Get incomes for business |
| POST | `/api/income` | Record new income |

### Expenses
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/expense/business/{businessId}` | Get expenses for business |
| POST | `/api/expense` | Record new expense |

### Dashboard
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/dashboard/summary/{businessId}` | Cashflow summary |
| GET | `/api/dashboard/health/{businessId}` | Cashflow health status |
| GET | `/api/dashboard/trends/{businessId}` | Cashflow trends |

### AI Service
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/predict/cashflow` | Predict future cashflow |
| POST | `/detect/anomaly` | Detect unusual transactions |
| GET | `/health` | Service health check |

---

## 💡 Key Features

### Cashflow Health Engine
Automatically classifies business health based on income vs expenses:
- **🟢 Healthy**: Net cashflow positive, expenses < 80% of income
- **🟡 Warning**: Expenses between 80-100% of income
- **🔴 Critical**: Expenses exceed income (negative cashflow)

### AI Predictions
- **Polynomial Regression** for trend forecasting (7-90 day predictions)
- **7-day rolling averages** for noise reduction
- Recommendations in **simple, non-technical language**

### Anomaly Detection
- **IQR method** identifies statistical outliers
- **Z-score severity** classification (low, medium, high)
- Alerts in ₦ Naira with contextual explanations

### Offline-First (PWA)
- Service Worker for offline access
- IndexedDB caching via `idb` library
- Dashboard data cached for offline viewing
- Transactions queued when offline

### Expense Categories
Rent, Utilities, Salaries, Inventory, Transport, Marketing, Equipment, Maintenance, Insurance, Taxes, Communication, Supplies, Other

---

## 🛡️ Security

- **JWT Bearer Authentication** (HMAC-SHA256, 24hr expiry)
- **Phone + OTP** auth flow (Nigerian format: `+234` or `0` prefix)
- **Ownership validation** on all business/income/expense endpoints
- **CORS** restricted to frontend origin
- **Input validation** via FluentValidation with Nigerian-specific rules

---

## 🧪 Testing

### Backend (xUnit)
```bash
cd backend
dotnet test --verbosity normal
```
16 unit tests covering:
- `CashflowServiceTests` — Health status calculation, date range aggregation
- `IncomeServiceTests` — CRUD operations, business ownership validation
- `ExpenseServiceTests` — Category filtering, amount validation

### AI Service (pytest)
```bash
cd ai-service
pytest tests/ -v
```
8 tests covering:
- Cashflow prediction with various data sizes
- Critical/Warning/Healthy status prediction
- Anomaly detection with outliers
- Severity ranking validation

---

## 🛠️ Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| Frontend | Angular 19+, Material 3, PWA | Mobile-first SPA |
| Backend | .NET 8, EF Core 8, FluentValidation | REST API, Clean Architecture |
| AI Service | FastAPI, Scikit-learn, Pandas | ML Predictions |
| Database | PostgreSQL 15+ | Persistent storage |
| Auth | JWT Bearer + OTP | Phone-based authentication |
| Offline | Service Worker + IndexedDB | PWA offline support |

---

## 📝 Environment Configuration

### Backend (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=flowiq;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "Secret": "FlowIQ-Super-Secret-Key-For-Nigerian-MSME-App-2024!",
    "Issuer": "FlowIQ",
    "Audience": "FlowIQ-Users",
    "ExpiryInHours": 24
  },
  "AIService": {
    "BaseUrl": "http://localhost:8000",
    "ConfidenceThreshold": 0.6
  }
}
```

### Frontend (`environment.ts`)
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5146/api'
};
```

---

## 📄 License

MIT License — Built with ❤️ for Nigerian MSMEs.
