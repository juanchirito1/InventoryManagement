# 📦 Warehouse & Inventory Management API (Sistema de Gestión de Inventario y Ventas)

API RESTful empresarial desarrollada con **C#**, **.NET 10**, **ASP.NET Core** y **Entity Framework Core**, siguiendo los principios de **Clean Architecture (Arquitectura Limpia)** y **Domain-Driven Design (DDD)**.

Este proyecto fue diseñado para demostrar habilidades clave de backend en entornos profesionales: consistencia transaccional, control de inventario (Kardex), validación de reglas de negocio y documentación interactiva.

---

## 🏛️ Arquitectura del Proyecto

El sistema está desacoplado en capas independientes:

```
InventoryManagement/
├── src/
│   ├── InventoryManagement.Domain/          # Núcleo puro: Entidades, Enums, Reglas e Invariantes del Negocio
│   ├── InventoryManagement.Application/     # Casos de uso: DTOs, Interfaces de Servicios y Lógica de Aplicación
│   ├── InventoryManagement.Infrastructure/  # Persistencia: DbContext, EF Core, SQL Server, Servicios Concretos
│   └── InventoryManagement.Api/             # Capa Web: Controladores REST, Middleware de Errores, Swagger/OpenAPI
└── InventoryManagement.slnx                 # Archivo de solución de .NET
```

---

## 🚀 Características Principales

1. **Control de Stock y Kardex Automatizado:**
   - Toda entrada o salida de inventario genera un registro de auditoría (`StockMovement`) con tipo de movimiento, motivo y documento de referencia.
   - Protección contra sobreventa: la entidad `Product` valida invariantes y dispara `InsufficientStockException` si se intenta vender más de lo disponible.

2. **Transacciones ACID en Órdenes de Compra:**
   - Creación atómica de pedidos: descuenta stock de múltiples productos y registra la orden dentro de una transacción (`Database.BeginTransactionAsync`). Si algún producto falla, todo se revierte (*rollback*).
   - Cancelación de pedidos con restitución automática de inventario.

3. **Manejo Global de Excepciones:**
   - `ExceptionHandlingMiddleware` captura excepciones de dominio y las transforma en respuestas HTTP 400 uniformes con mensajes claros para el cliente.

4. **Dashboard y Reportes Operativos:**
   - Endpoint analítico (`/api/dashboard/summary`) que calcula en tiempo real: productos con stock crítico/bajo, ingresos totales, órdenes completadas y últimos movimientos.

5. **Sembrado Automático de Datos (Data Seeding):**
   - Al iniciar, la aplicación auto-genera la base de datos en SQL Server LocalDB e inserta categorías, productos y clientes de prueba listos para interactuar.

---

## 🛠️ Tecnologías Empleadas

- **Lenguaje:** C# 13 / .NET 10
- **Framework:** ASP.NET Core Web API
- **ORM:** Entity Framework Core (Code-First)
- **Base de Datos:** Microsoft SQL Server (LocalDB `MSSQLLocalDB`)
- **Documentación:** Swagger / OpenAPI UI

---

## 💻 Cómo Ejecutar el Proyecto

### Opción 1: Desde Visual Studio
1. Abre el archivo de solución `InventoryManagement.slnx` en Visual Studio.
2. Asegúrate de que `InventoryManagement.Api` esté seleccionado como el proyecto de inicio (*Startup Project*).
3. Presiona `F5` o el botón de ejecución verde.
4. El navegador se abrirá en `http://localhost:<puerto>/` mostrando la documentación interactiva de Swagger.

### Opción 2: Desde la Terminal (.NET CLI)
```bash
# Navegar a la carpeta de la API
cd src/InventoryManagement.Api

# Ejecutar el servidor
dotnet run
```
Abre en tu navegador `http://localhost:5000` o la URL mostrada en consola.

---

## 📋 Endpoints Disponibles

| Módulo | Método | Endpoint | Descripción |
|---|---|---|---|
| **Dashboard** | `GET` | `/api/dashboard/summary` | Métricas generales, alertas de stock y últimos movimientos |
| **Productos** | `GET` | `/api/products` | Lista de productos con filtros de búsqueda y stock bajo |
| **Productos** | `GET` | `/api/products/{id}` | Detalle de un producto específico |
| **Productos** | `POST` | `/api/products` | Registrar nuevo producto con stock inicial |
| **Productos** | `POST` | `/api/products/{id}/adjust-stock` | Ajuste manual de inventario (Entrada/Salida/Kardex) |
| **Productos** | `GET` | `/api/products/movements` | Historial de movimientos de inventario |
| **Categorías**| `GET` | `/api/categories` | Lista de categorías de almacén |
| **Categorías**| `POST` | `/api/categories` | Registrar nueva categoría |
| **Clientes**  | `GET` | `/api/customers` | Lista de clientes registrados |
| **Clientes**  | `POST` | `/api/customers` | Registrar nuevo cliente |
| **Órdenes**   | `GET` | `/api/orders` | Historial de órdenes de venta |
| **Órdenes**   | `POST` | `/api/orders` | Procesar orden (descuenta stock transaccional) |
| **Órdenes**   | `POST` | `/api/orders/{id}/cancel` | Cancelar orden y reponer inventario |
