# Accounting Service

## Overview

The Accounting Service manages financial operations, invoicing, and reporting for the ERP system. Built with .NET 10 and Domain-Driven Design architecture.

## Features

- **General Ledger** - Chart of accounts and journal entries
- **Accounts Payable** - Vendor bill management and payments
- **Accounts Receivable** - Customer invoicing and collections
- **Financial Reporting** - Balance sheet, P&L, and custom reports

## Technology Stack

- .NET 10
- Entity Framework Core
- MediatR
- FluentValidation
- Serilog
- Application Insights

## API Endpoints

### Accounts
- `GET /api/v1/accounts` - Get chart of accounts
- `POST /api/v1/accounts` - Create new account
- `PUT /api/v1/accounts/{id}` - Update account

### Journal Entries
- `GET /api/v1/journal-entries` - Get journal entries
- `POST /api/v1/journal-entries` - Create journal entry

### Invoices
- `GET /api/v1/invoices` - Get invoices
- `POST /api/v1/invoices` - Create invoice
- `PUT /api/v1/invoices/{id}/status` - Update invoice status

### Reports
- `GET /api/v1/reports/balance-sheet` - Generate balance sheet
- `GET /api/v1/reports/profit-loss` - Generate P&L statement

## Configuration

Similar to HR Service with environment variables for database, Azure services, and monitoring.

## Local Development

```bash
cd src/AccountingService
dotnet restore
dotnet run --project AccountingService.API
```

## Docker

```bash
docker build -f src/AccountingService/Dockerfile -t accountingservice:latest .
```

## Database

Uses SQL Server for financial data with tables for:
- Accounts
- JournalEntries
- Invoices
- Payments
- CustomerVendors

## Security & Monitoring

Same patterns as HR Service with JWT authentication, structured logging, and health checks.

## Compliance

- SOX compliance considerations
- Audit trail for all financial transactions
- Data retention policies
