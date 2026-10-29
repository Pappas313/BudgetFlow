# 💰 BudgetFlow — Personal Finance & Budget Management System

### Full-Stack Financial Management Application | ASP.NET Core | React | SQL Server | SSIS | Tableau

BudgetFlow is a full-stack personal finance and budgeting application designed to help users manage their income, expenses, bank accounts, recurring bills, monthly budgets, and savings goals through a centralized financial management platform.

The project combines modern web application development with database management, ETL processing, financial reporting, and business intelligence.

Built using **ASP.NET Core Web API, C#, React, TypeScript, Entity Framework Core, and Microsoft SQL Server**, BudgetFlow follows Clean Architecture principles to ensure scalability, maintainability, security, and separation of concerns.

The application also incorporates **SQL Server Integration Services (SSIS)** for financial data extraction and CSV report generation, enabling interactive financial analytics through **Tableau dashboards**.

---

## 🎯 Project Objectives

The main objectives of BudgetFlow are to:

- Develop a scalable personal finance management platform.
- Implement secure RESTful APIs using ASP.NET Core.
- Manage financial transactions using Microsoft SQL Server.
- Track income, expenses, and account balances.
- Create and monitor monthly budgets.
- Manage recurring bills and payment schedules.
- Monitor savings goals and financial progress.
- Generate financial reports using SSIS.
- Export structured financial datasets to CSV.
- Visualize financial performance through Tableau.
- Automate development and reporting tasks using PowerShell.
- Apply Clean Architecture, SOLID principles, and CQRS.
- Demonstrate modern full-stack development practices.

---

## 🛠️ Technology Stack

### Backend Development

| Technology | Purpose |
|---|---|
| C# | Backend programming language |
| .NET 10 | Application framework |
| ASP.NET Core Web API | RESTful API development |
| Entity Framework Core | Object-relational mapping |
| SQL Server | Relational database |
| ASP.NET Core Identity | User management |
| JWT | Authentication and authorization |
| MediatR | CQRS implementation |
| FluentValidation | Request validation |
| Serilog | Structured application logging |
| OpenAPI / Swagger | API documentation |

### Frontend Development

| Technology | Purpose |
|---|---|
| React | Frontend framework |
| TypeScript | Type-safe JavaScript development |
| HTML5 | Application structure |
| CSS3 | Styling |
| Bootstrap 5 | Responsive UI components |
| React Router | Client-side navigation |
| Axios | REST API communication |
| TanStack Query | Server-state management |
| Recharts | Financial data visualization |
| React Hook Form | Form management |
| Zod | Form validation |

### Data Engineering & Business Intelligence

| Technology | Purpose |
|---|---|
| SQL Server | Financial data storage |
| T-SQL | Database queries and reporting |
| SSIS | ETL processing |
| CSV | Financial data export |
| Tableau | Interactive financial analytics |
| PowerShell | Task automation |
| Puppet | Optional configuration management |

### Development & Testing

- Visual Studio 2022
- GitHub Copilot
- SQL Server Management Studio
- Git and GitHub
- xUnit
- Moq
- GitHub Actions

---

## ✨ Key Features

### 🔐 Authentication & Authorization

Secure user authentication using ASP.NET Core Identity and JWT.

Features include user registration, login, token-based authentication, protected API endpoints, and user-specific financial data access.

### 🏦 Bank Account Management

Manage multiple financial accounts from a centralized interface.

Users can create bank accounts, update account details, monitor balances, and review account-specific transactions.

### 💳 Income & Expense Tracking

Record and categorize financial transactions.

Features include income tracking, expense management, transaction history, category assignment, account transfers, and filtering by date or category.

### 📅 Bills & Recurring Payments

Manage financial obligations and recurring payments.

Users can track rent, utilities, subscriptions, insurance, and other bills while monitoring due dates, payment history, and overdue obligations.

### 📊 Monthly Budget Management

Create category-based monthly budgets and monitor spending performance.

The application calculates:

**Remaining Budget**

Remaining Budget = Allocated Budget − Actual Expenses

**Budget Utilization**

Budget Utilization (%) = (Actual Expenses / Allocated Budget) × 100

**Budget Variance**

Budget Variance = Allocated Budget − Actual Expenses

These calculations help users identify overspending and improve financial planning.

### 🎯 Savings Goals

Create financial savings targets and monitor progress.

Users can define target amounts, record savings contributions, calculate completion percentages, and track remaining amounts.

### 📈 Financial Dashboard

A centralized dashboard provides an overview of financial performance.

Dashboard components include:

- Total account balances.
- Monthly income.
- Monthly expenses.
- Net cash flow.
- Spending distribution by category.
- Budget utilization.
- Upcoming bills.
- Recent transactions.
- Savings goal progress.
- Monthly financial trends.

Financial charts are developed using React and Recharts.

### 📁 Financial Reporting with SSIS

SQL Server Integration Services is used to extract, transform, and export financial information.

The ETL workflow consists of:

1. Extracting financial data from SQL Server.
2. Reading financial reporting views.
3. Transforming and validating financial records.
4. Generating structured CSV reports.
5. Logging export execution results.
6. Preparing datasets for Tableau.

Generated reports include:

- `monthly_income_expenses.csv`
- `category_expenses.csv`
- `budget_variance.csv`
- `account_balances.csv`
- `upcoming_bills.csv`
- `savings_progress.csv`

### 📉 Tableau Financial Analytics

Tableau is used to create interactive dashboards from the exported financial datasets.

Planned visualizations include:

- Income vs. Expenses.
- Monthly Cash Flow.
- Expenses by Category.
- Budget vs. Actual Spending.
- Account Balance Distribution.
- Savings Goal Progress.
- Upcoming Bills.
- Financial KPI Indicators.

Interactive filters support analysis by month, year, category, account, and currency.

---

## 🏗️ System Architecture

BudgetFlow follows **Clean Architecture** with four primary backend layers.

```text
                         USER
                          |
                          v
                 React + TypeScript
                    Frontend
                          |
                          | REST API / HTTPS
                          v
                  ASP.NET Core API
                   Presentation Layer
                          |
                          v
                   Application Layer
                    CQRS / MediatR
                          |
                          v
                     Domain Layer
                Entities / Business Rules
                          ^
                          |
                  Infrastructure Layer
                 EF Core / SQL Server
                          |
                          v
                    SQL Server DB
                          |
                          v
                     SSIS ETL
                          |
                          v
                     CSV Reports
                          |
                          v
                  Tableau Dashboard
```

### Architecture Layers

**Domain Layer**

Contains financial entities, domain services, business rules, enumerations, and financial calculations.

**Application Layer**

Implements application use cases, CQRS commands and queries, DTOs, validation, and service abstractions.

**Infrastructure Layer**

Handles SQL Server persistence, Entity Framework Core, authentication infrastructure, repositories, and external service integrations.

**Presentation Layer**

Exposes RESTful API endpoints and manages HTTP requests, authorization, validation responses, and exception handling.

---

## 📂 Project Structure

```text
BudgetFlow/
│
├── src/
│   ├── BudgetFlow.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Extensions/
│   │   └── Program.cs
│   │
│   ├── BudgetFlow.Application/
│   │   ├── Features/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   ├── Behaviors/
│   │   └── Abstractions/
│   │
│   ├── BudgetFlow.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── ValueObjects/
│   │   ├── Services/
│   │   └── Exceptions/
│   │
│   └── BudgetFlow.Infrastructure/
│       ├── Persistence/
│       ├── Configurations/
│       ├── Repositories/
│       ├── Identity/
│       └── Services/
│
├── frontend/
│   └── budgetflow-web/
│       ├── src/
│       │   ├── api/
│       │   ├── components/
│       │   ├── features/
│       │   ├── hooks/
│       │   ├── routes/
│       │   └── services/
│       └── package.json
│
├── tests/
│   ├── BudgetFlow.UnitTests/
│   └── BudgetFlow.IntegrationTests/
│
├── database/
│   ├── scripts/
│   └── seed/
│
├── etl/
│   ├── ssis/
│   └── exports/
│
├── automation/
│   ├── powershell/
│   └── puppet/
│
├── docs/
│
├── BudgetFlow.sln
└── README.md
```

---

## 🗄️ Database Design

BudgetFlow uses Microsoft SQL Server as its primary database.

**Database Name:** `BudgetFlowDB`

### Main Database Tables

| Table | Description |
|---|---|
| AspNetUsers | Application users |
| BankAccounts | Bank account information |
| FinancialTransactions | Income, expenses, and transfers |
| TransactionCategories | Financial categories |
| Bills | Recurring financial obligations |
| BillPayments | Bill payment history |
| MonthlyBudgets | Monthly budget allocations |
| SavingsGoals | Savings targets |
| SavingsContributions | Savings contribution history |
| AuditLogs | Application audit events |
| ReportExports | Financial report export history |

Entity Framework Core is used to manage database relationships, schema migrations, and data access.

---

## 🔗 REST API Endpoints

### Authentication

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register a user |
| POST | `/api/auth/login` | Authenticate a user |
| POST | `/api/auth/refresh` | Refresh authentication |
| POST | `/api/auth/logout` | Logout |
| GET | `/api/auth/me` | Retrieve current user |

### Bank Accounts

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/accounts` | Retrieve accounts |
| GET | `/api/accounts/{id}` | Retrieve account details |
| POST | `/api/accounts` | Create an account |
| PUT | `/api/accounts/{id}` | Update an account |
| DELETE | `/api/accounts/{id}` | Archive an account |

### Transactions

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/transactions` | Retrieve transactions |
| POST | `/api/transactions` | Create a transaction |
| PUT | `/api/transactions/{id}` | Update a transaction |
| DELETE | `/api/transactions/{id}` | Delete a transaction |
| POST | `/api/transactions/transfer` | Transfer funds |

### Financial Management

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/bills` | Retrieve bills |
| POST | `/api/bills` | Create a bill |
| GET | `/api/budgets` | Retrieve budgets |
| POST | `/api/budgets` | Create a budget |
| GET | `/api/savings-goals` | Retrieve savings goals |
| POST | `/api/savings-goals` | Create a savings goal |
| GET | `/api/dashboard/summary` | Retrieve financial overview |

---

## 🚀 Getting Started

### Prerequisites

Install the following software:

- Visual Studio 2022 with compatible .NET 10 tooling.
- .NET 10 SDK.
- SQL Server Developer or Express Edition.
- SQL Server Management Studio.
- Node.js and npm.
- Git.
- GitHub Copilot (optional for running the application).

For reporting functionality:

- SQL Server Integration Services with compatible development tools.
- Tableau Desktop or a compatible Tableau environment.

### 1. Clone the Repository

```bash
git clone <repository-url>
cd BudgetFlow
```

Replace `<repository-url>` with the actual GitHub repository URL.

### 2. Configure SQL Server

Configure the development connection string in:

`src/BudgetFlow.API/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BudgetFlowDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Restore .NET Dependencies

```bash
dotnet restore
```

### 4. Apply Database Migrations

```bash
dotnet ef database update --project src/BudgetFlow.Infrastructure --startup-project src/BudgetFlow.API
```

### 5. Run the Backend

```bash
dotnet run --project src/BudgetFlow.API
```

The API runs on the local address displayed in the terminal.

### 6. Install Frontend Dependencies

```bash
cd frontend/budgetflow-web
npm install
```

### 7. Start React

```bash
npm run dev
```

The Vite development server commonly runs at:

`http://localhost:5173`

Configure the frontend API base URL to match the backend's local HTTPS address.

---

## 🧪 Testing

BudgetFlow uses xUnit and Moq for automated testing.

The testing strategy covers:

- Financial calculations.
- Domain validation.
- Authentication and authorization.
- Bank account operations.
- Income and expense transactions.
- Account transfers.
- Budget calculations.
- Savings goals.
- Database persistence.
- API integration.

Run backend tests using:

```bash
dotnet test
```

Integration tests may require a running SQL Server instance and dedicated test configuration.

---

## ⚙️ Automation

PowerShell scripts support development and reporting operations.

Planned scripts include:

```text
automation/powershell/
├── setup-environment.ps1
├── build-solution.ps1
├── update-database.ps1
├── seed-database.ps1
├── run-tests.ps1
├── export-reports.ps1
├── backup-database.ps1
└── health-check.ps1
```

An optional Puppet configuration demonstrates infrastructure configuration management for compatible deployment environments.

---

## 🔒 Security Considerations

BudgetFlow is designed with financial data protection in mind.

Security measures include:

- ASP.NET Core Identity authentication.
- JWT-based authorization.
- User-specific resource ownership.
- Secure password hashing.
- Input validation.
- HTTPS communication.
- Centralized exception handling.
- Secure application configuration.
- Database integrity constraints.
- Financial transaction consistency.
- Audit logging.

Production credentials, access tokens, database backups, and private financial exports must not be committed to the repository.

Public demonstrations should use synthetic financial data.

---

## 🔮 Future Enhancements

Potential future improvements include:

- Open Banking API integration.
- Automated bank transaction synchronization.
- Financial forecasting.
- AI-assisted expense categorization.
- Budget recommendations.
- Email and push notifications.
- Multi-currency conversion.
- PDF financial statements.
- Advanced financial analytics.
- Mobile application development.
- Automated recurring transactions.
- Scheduled SSIS reporting.
- Additional dashboard visualizations.

---

## 🎓 Skills Demonstrated

This project demonstrates practical experience in:

**Backend Engineering:** C#, ASP.NET Core, REST APIs, Clean Architecture, CQRS, Entity Framework Core, and SQL Server.

**Frontend Engineering:** React, TypeScript, JavaScript, HTML5, CSS3, Bootstrap, and responsive UI development.

**Data Engineering:** SQL Server, T-SQL, SSIS, ETL pipelines, and CSV reporting.

**Business Intelligence:** Tableau, financial KPIs, dashboard development, and financial data visualization.

**Software Engineering:** SOLID principles, dependency injection, authentication, authorization, automated testing, Git, and CI/CD.

**Automation:** PowerShell scripting and optional Puppet configuration management.

---

## 📌 Project Status

**Status:** Under Development

The project is being developed incrementally using Visual Studio 2022 and GitHub Copilot.

The planned development phases include:

1. Solution architecture and project configuration.
2. SQL Server database and Entity Framework Core.
3. Authentication and REST API development.
4. Financial management modules.
5. React frontend and dashboards.
6. SQL Server reporting and SSIS integration.
7. Tableau financial analytics.
8. Automation, testing, and documentation.

---

## 📄 License

License information will be added when a license is selected.

---

**BudgetFlow — A Full-Stack Personal Finance, Budget Management & Financial Analytics Platform.**
