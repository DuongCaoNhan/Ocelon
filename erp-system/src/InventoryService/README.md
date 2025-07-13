# Inventory Service

## Overview

The Inventory Service manages stock, warehousing, and supply chain operations for the ERP system. Built with .NET 10 and Domain-Driven Design architecture.

## Features

- **Product Management** - Create and manage product catalog
- **Stock Management** - Track inventory levels and stock movements
- **Warehouse Management** - Multi-location inventory tracking
- **Supply Chain** - Supplier and purchase order management

## Technology Stack

- .NET 10
- Entity Framework Core
- MediatR
- FluentValidation
- Serilog
- Application Insights

## API Endpoints

### Products
- `GET /api/v1/products` - Get all products
- `GET /api/v1/products/{id}` - Get product by ID
- `POST /api/v1/products` - Create new product
- `PUT /api/v1/products/{id}` - Update product
- `DELETE /api/v1/products/{id}` - Delete product

### Stock
- `GET /api/v1/stock` - Get stock levels
- `GET /api/v1/stock/{productId}` - Get stock for product
- `POST /api/v1/stock/movements` - Record stock movement

### Warehouses
- `GET /api/v1/warehouses` - Get all warehouses
- `POST /api/v1/warehouses` - Create new warehouse

## Configuration

Similar to HR Service with environment variables for database, Azure services, and monitoring.

## Local Development

```bash
cd src/InventoryService
dotnet restore
dotnet run --project InventoryService.API
```

## Docker

```bash
docker build -f src/InventoryService/Dockerfile -t inventoryservice:latest .
```

## Database

Uses SQL Server for relational data with tables for:
- Products
- Stock
- Warehouses
- StockMovements
- Suppliers

## Security & Monitoring

Same patterns as HR Service with JWT authentication, structured logging, and health checks.
