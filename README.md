# 📦 Warehouse & Inventory Management API (Sistema de Gestión de Inventario y Ventas)

[![Build & Test CI](https://github.com/tu-usuario/InventoryManagement/actions/workflows/ci.yml/badge.svg)](https://github.com/tu-usuario/InventoryManagement/actions)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4)
![C#](https://img.shields.io/badge/C%23-13.0-239120)
![EF Core](https://img.shields.io/badge/Entity%20Framework-Core-68217a)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-CC292B)
![xUnit](https://img.shields.io/badge/Unit%20Tests-xUnit%20%2B%20FluentAssertions-brightgreen)

API RESTful empresarial de alto rendimiento desarrollada con **C#**, **.NET 10**, **ASP.NET Core** y **Entity Framework Core**, diseñada siguiendo los principios de **Clean Architecture (Arquitectura Limpia)**, **Domain-Driven Design (DDD)** y asegurada con **JWT (JSON Web Tokens)**.

---

## 🏛️ Arquitectura del Proyecto

El sistema está desacoplado en capas independientes para garantizar alta mantenibilidad, escalabilidad y facilidad de pruebas:

```
InventoryManagement/
├── .github/workflows/ci.yml                 # Pipeline de Integración Continua (GitHub Actions)
├── src/
│   ├── InventoryManagement.Domain/          # Núcleo puro: Entidades, Enums, Reglas e Invariantes del Negocio
│   ├── InventoryManagement.Application/     # Casos de uso: DTOs, Interfaces de Servicios y Lógica de Negocio
│   ├── InventoryManagement.Infrastructure/  # Persistencia: DbContext, EF Core, SQL Server, Auth, Servicios
│   └── InventoryManagement.Api/             # Capa Web: Controladores REST, JWT, Middleware y Swagger UI
├── tests/
│   └── InventoryManagement.UnitTests/       # Pruebas Unitarias Automatizadas (xUnit + FluentAssertions + Moq)
└── InventoryManagement.slnx                 # Archivo de solución moderna de .NET
```

---

## 🚀 Características Principales

1. **Control de Stock y Kardex Automatizado:**
   - Toda entrada o salida de inventario genera un registro de auditoría (`StockMovement`) con tipo de movimiento, motivo y documento de referencia.
   - Protección contra sobreventa: la entidad `Product` valida invariantes y dispara `InsufficientStockException` si se intenta vender más de lo disponible.

2. **Transacciones ACID en Órdenes de Venta:**
   - Creación atómica de pedidos: descuenta stock de múltiples productos y registra la orden dentro de una transacción (`Database.BeginTransactionAsync`). Si algún producto falla, todo se revierte (*rollback*).
   - Cancelación de pedidos con restitución automática de inventario.

3. **Seguridad y Control de Acceso basado en Roles (RBAC con JWT):**
   - Hashing criptográfico seguro con `HMACSHA512` y salting único por usuario.
   - Roles definidos:
     - **Admin:** Control total (creación/ajuste de stock, gestión de categorías, registro de usuarios).
     - **Seller:** Operaciones de venta (creación de órdenes, consulta de stock, registro de clientes).
   - Swagger interactivo con soporte de botón `Authorize 🔓` para probar tokens Bearer.

4. **Suite de Pruebas Unitarias Automatizadas:**
   - Pruebas con `xUnit` y `FluentAssertions` que validan el comportamiento del dominio, reglas de cálculo, excepciones de stock insuficiente y seguridad de contraseñas.

5. **Manejo Global de Excepciones:**
   - `ExceptionHandlingMiddleware` captura excepciones de dominio y las transforma en respuestas HTTP 400 uniformes con mensajes claros para el cliente.

6. **Dashboard y Reportes Operativos:**
   - Endpoint analítico (`/api/dashboard/summary`) que calcula en tiempo real: productos con stock crítico/bajo, ingresos totales, órdenes completadas y últimos movimientos.

---

## 🔑 Credenciales de Prueba (Cargadas Automáticamente)

Al iniciar la aplicación por primera vez, la base de datos se inicializa automáticamente con los siguientes usuarios de demostración:

| Rol | Usuario | Contraseña | Permisos |
|---|---|---|---|
| **Administrador** | `admin` | `Admin123!` | Acceso total a endpoints de administración y almacén |
| **Vendedor** | `vendedor` | `Vendedor123!` | Creación de órdenes y consultas |

---

## 🧪 Ejecución de Pruebas Unitarias

Para ejecutar la suite de pruebas unitarias automatizadas:

```bash
dotnet test
```

---

## 💻 Cómo Ejecutar el Proyecto

### Opción 1: Desde Visual Studio
1. Abre el archivo de solución `InventoryManagement.slnx`.
2. Asegúrate de que `InventoryManagement.Api` esté seleccionado como el proyecto de inicio (*Startup Project*).
3. Presiona `F5`.
4. El navegador se abrirá en `http://localhost:<puerto>/` mostrando la documentación interactiva de Swagger.
5. Inicia sesión en `/api/auth/login`, copia el token JWT devuelto, haz clic en el botón superior **Authorize** de Swagger y escribe: `Bearer <tu_token>`.

### Opción 2: Desde la Terminal (.NET CLI)
```powershell
# Ejecutar la API
dotnet run --project src/InventoryManagement.Api/InventoryManagement.Api.csproj
```

---

## 📋 Resumen de Endpoints Principales

| Módulo | Método | Endpoint | Rol Requerido | Descripción |
|---|---|---|---|---|
| **Auth** | `POST` | `/api/auth/login` | Público | Autenticación y obtención de token JWT |
| **Auth** | `POST` | `/api/auth/register` | Admin | Registro de nuevos usuarios con rol |
| **Dashboard** | `GET` | `/api/dashboard/summary` | Público | Métricas de ingresos, alertas de stock bajo y movimientos |
| **Productos** | `GET` | `/api/products` | Público | Lista de productos con filtros de búsqueda y alertas |
| **Productos** | `POST` | `/api/products` | Admin | Registrar nuevo producto con stock inicial |
| **Productos** | `POST` | `/api/products/{id}/adjust-stock` | Admin | Ajuste manual de inventario (Entrada/Salida/Kardex) |
| **Productos** | `GET` | `/api/products/movements` | Autenticado | Historial de movimientos de inventario |
| **Categorías**| `GET` | `/api/categories` | Público | Lista de categorías de almacén |
| **Categorías**| `POST` | `/api/categories` | Admin | Registrar nueva categoría |
| **Órdenes**   | `POST` | `/api/orders` | Autenticado | Procesar orden (descuenta stock transaccional) |
| **Órdenes**   | `POST` | `/api/orders/{id}/cancel` | Autenticado | Cancelar orden y reponer inventario |
