# 📋 Changelog — PryCafeteria (Cafetería Starbucks)

Todos los cambios importantes del proyecto se documentan aquí.  
Formato basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

---

## [Unreleased]

---

## [0.3.0] — 2026-03-05 — Fase C: Carrito, Pedidos y Reportes

### Añadido
- **HU11** — Carrito de compras: agregar productos con tamaño y cantidad
- **HU12** — Modificar y eliminar ítems del carrito
- **HU13** — Vista del carrito con subtotal, descuento y total
- **HU19** — Aplicación de cupones de descuento en el carrito (porcentaje / monto fijo)
- **Catálogo de productos** para clientes con filtro por categoría
- **Módulo de Pedidos** — checkout con selección de dirección y método de pago
- **Módulo de Direcciones** — gestión de múltiples direcciones de entrega por cliente
- **Módulo de Perfil** — edición de datos personales del cliente
- **ReportesController** con exportación a Excel (ventas, productos, clientes, cupones, stock)
- **ExcelService** con ClosedXML para generación de reportes `.xlsx`
- **StockMovimientos** — historial de entradas, salidas y ajustes de stock
- Migración `AgregarStockMovimientos` y `AddStockMovimientoReferencia`
- Dos layouts diferenciados: `_Layout.cshtml` (admin) y `_LayoutCliente.cshtml` (cliente)
- Páginas de error personalizadas: `Error404.cshtml`, `Error500.cshtml`

### Mejorado
- Dashboard ahora incluye métricas del día y caché de 2 minutos (IMemoryCache)
- `StockBajoFilter` como filtro global para mostrar badge de stock crítico en sidebar
- Redirección post-login diferenciada por rol (Admin → Dashboard, Cliente → Home)

---

## [0.2.0] — 2026-03-02 — Fase B: Gestión Avanzada y Dashboard

### Añadido
- **HU20** — Dashboard administrativo con tarjetas de métricas
- **HU08** — Gestión de tamaños (`TamaniosController`)
- **HU08** — Precios y stock por variante producto+tamaño (`ProductosTamaniosController`)
- **HU09** — Control de stock con estados: En Stock / Bajo Stock / Agotado
- **Cupones** — CRUD completo de cupones de descuento (`CuponesController`)
- **Métodos de Pago** — CRUD de métodos de pago (`MetodosPagosController`)
- **ImagenService** — Subida y redimensionado de imágenes de productos (SixLabors.ImageSharp)
- Paquete **ClosedXML** 0.102.2 añadido al proyecto
- Paquete **SixLabors.ImageSharp** 3.1.12 añadido al proyecto
- Migración `MejorasModelo` y `AgregarFechaCreacionCategoria`

### Mejorado
- `ProductosController` con soporte para subida de imágenes
- Validaciones mejoradas en formularios de productos y categorías

---

## [0.1.0] — 2026-02-10 — Fase A: Base del Sistema (MVP)

### Añadido
- **HU01** — Registro de nuevos clientes con validaciones
- **HU02** — Login / Logout con ASP.NET Core Identity
  - Bloqueo tras 5 intentos fallidos (15 minutos)
  - Cookie "Recordarme" de 14 días
- **HU03** — Gestión de usuarios (CRUD + cambio de roles)
- **HU04** — Gestión de productos (CRUD completo)
- **HU05** — Gestión de categorías (CRUD completo)
- Configuración inicial del proyecto ASP.NET Core 8.0 MVC
- Integración de Entity Framework Core 8.0 con SQL Server
- Integración de ASP.NET Core Identity con `ApplicationUser` extendido
- `CustomClaimsPrincipalFactory` para claims con nombre del usuario
- Migración inicial `InitialWithIdentity`
- Migración `AgregarFechaRegistro`
- Seeding automático: roles Admin/Cliente y usuario administrador por defecto
- Estructura base de layouts, controladores y vistas
- Paleta de colores Starbucks en `site.css` y `dashboard.css`
