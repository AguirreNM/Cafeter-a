# ☕ Cafetería Starbucks — Sistema de Gestión Web

Sistema de gestión completo para cafetería desarrollado con **ASP.NET Core 8.0 MVC**, **Entity Framework Core** y **ASP.NET Core Identity**. Permite la administración de productos, pedidos, usuarios y reportes, así como una experiencia de compra completa para clientes.

---

## 📋 Tabla de Contenidos

- [Descripción General](#descripción-general)
- [Características Principales](#características-principales)
- [Tecnologías Utilizadas](#tecnologías-utilizadas)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Requisitos del Sistema](#requisitos-del-sistema)
- [Instalación y Configuración](#instalación-y-configuración)
- [Configuración de Base de Datos](#configuración-de-base-de-datos)
- [Cómo Ejecutar el Proyecto](#cómo-ejecutar-el-proyecto)
- [Credenciales de Prueba](#credenciales-de-prueba)
- [Módulos del Sistema](#módulos-del-sistema)
- [Módulo Administrativo](#módulo-administrativo)
- [Paleta de Colores](#paleta-de-colores)
- [Autor](#autor)

---

## 📖 Descripción General

**PryCafeteria** es una aplicación web MVC orientada a la gestión integral de una cafetería. Cuenta con dos roles principales:

- **Administrador:** Gestiona productos, categorías, tamaños, precios, stock, cupones, métodos de pago, usuarios, pedidos y reportes exportables a Excel.
- **Cliente:** Navega el catálogo, agrega productos al carrito, aplica cupones de descuento, gestiona direcciones, elige método de pago y realiza pedidos.

El sistema aplica migraciones y crea el usuario administrador automáticamente al ejecutarse por primera vez, sin necesidad de scripts SQL adicionales.

---

## ✨ Características Principales

- 🔐 Autenticación y autorización con ASP.NET Core Identity (roles: Admin / Cliente)
- 🛒 Carrito de compras persistente en sesión con modificación en tiempo real
- 📦 Gestión completa de productos con imágenes, tamaños y stock por variante
- 🏷️ Sistema de cupones de descuento (porcentaje o monto fijo) aplicables en el carrito
- 📊 Dashboard administrativo con métricas, alertas de stock bajo y pedidos del día
- 📈 Reportes exportables a Excel (ventas, productos más vendidos, clientes, cupones, stock)
- 🖼️ Subida y redimensionado automático de imágenes con ImageSharp
- 🛡️ Bloqueo de cuenta tras 5 intentos fallidos de login (15 minutos)
- ⚡ Caché en memoria para métricas del dashboard (2 minutos)
- 📱 Diseño responsive con Bootstrap 5 y paleta corporativa Starbucks
- 🚨 Páginas de error personalizadas (404, 500)
- 📋 Historial de movimientos de stock (entradas, salidas, ajustes)

---

## 🛠️ Tecnologías Utilizadas

| Tecnología | Versión | Uso |
|---|---|---|
| ASP.NET Core MVC | 8.0 | Framework principal |
| Entity Framework Core | 8.0 | ORM y migraciones |
| ASP.NET Core Identity | 8.0 | Autenticación y roles |
| SQL Server / Azure SQL | 2019+ | Base de datos relacional |
| Bootstrap | 5 | UI responsive y componentes |
| jQuery | 3.x | Validaciones y scripting cliente |
| ClosedXML | 0.102.2 | Exportación de reportes a Excel |
| SixLabors.ImageSharp | 3.1.12 | Procesamiento y redimensionado de imágenes |
| ASP.NET Core Session | 8.0 | Carrito de compras en sesión |
| IMemoryCache | 8.0 | Caché de métricas del dashboard |

---

## 📁 Estructura del Proyecto

```
Starbucks/
├── PryCafeteria/
│   └── PryCafeteria/
│       ├── Controllers/
│       │   ├── CuentasController.cs           # HU01-HU02: Login, registro, logout
│       │   ├── DashboardController.cs          # HU20: Panel administrativo con caché
│       │   ├── UsuariosController.cs           # HU03: Gestión de usuarios (CRUD + roles)
│       │   ├── ProductosController.cs          # HU04: CRUD productos + imágenes
│       │   ├── CategoriasController.cs         # HU05: CRUD categorías
│       │   ├── TamaniosController.cs           # HU08: CRUD tamaños
│       │   ├── ProductosTamaniosController.cs  # HU08-HU09: Precios y stock por variante
│       │   ├── CuponesController.cs            # Gestión de cupones de descuento
│       │   ├── MetodosPagosController.cs       # Métodos de pago disponibles
│       │   ├── CarritoController.cs            # HU11-HU13-HU19: Carrito + cupones
│       │   ├── CatalogoController.cs           # Catálogo de productos para clientes
│       │   ├── PedidosController.cs            # Gestión de pedidos
│       │   ├── DireccionesController.cs        # Direcciones de entrega del cliente
│       │   ├── PerfilController.cs             # Perfil y datos del cliente
│       │   ├── ReportesController.cs           # HU20-E6: Reportes + exportación Excel
│       │   ├── HomeController.cs               # Landing page
│       │   └── ErrorController.cs             # Manejo de errores 404/500
│       ├── Models/
│       │   ├── ApplicationUser.cs              # Usuario extendido (Nombre, Apellido, etc.)
│       │   ├── BdcafeteriaContext.cs           # DbContext de Entity Framework Core
│       │   ├── CustomClaimsPrincipalFactory.cs # Claims con nombre del usuario
│       │   ├── Producto.cs                     # Modelo de producto
│       │   ├── Categoria.cs                    # Modelo de categoría
│       │   ├── Tamanio.cs                      # Modelo de tamaño
│       │   ├── ProductosTamanio.cs             # Precio y stock por variante
│       │   ├── Pedido.cs                       # Modelo de pedido
│       │   ├── DetallePedido.cs                # Ítem de pedido
│       │   ├── Cupone.cs                       # Cupón de descuento
│       │   ├── MetodosPago.cs                  # Método de pago
│       │   ├── DireccionesEntrega.cs           # Dirección de entrega
│       │   ├── StockMovimiento.cs              # Historial de movimientos de stock
│       │   └── ViewModels/
│       │       ├── LoginViewModel.cs
│       │       ├── RegistroViewModel.cs
│       │       ├── UsuarioViewModel.cs
│       │       ├── CarritoViewModel.cs
│       │       └── PedidoViewModel.cs
│       ├── Services/
│       │   ├── CarritoService.cs              # Lógica del carrito en sesión
│       │   ├── ExcelService.cs               # Generación de reportes Excel con ClosedXML
│       │   ├── IImagenService.cs              # Interfaz del servicio de imágenes
│       │   └── ImagenService.cs              # Validación + redimensionado con ImageSharp
│       ├── Filters/
│       │   └── StockBajoFilter.cs            # Filtro global: badge de stock bajo en sidebar
│       ├── Migrations/                        # Migraciones de Entity Framework Core
│       ├── Views/
│       │   ├── Cuentas/                       # Login y registro
│       │   ├── Dashboard/                     # Panel admin con métricas
│       │   ├── Usuarios/                      # Gestión de usuarios
│       │   ├── Productos/                     # CRUD productos + imágenes
│       │   ├── Categorias/                    # CRUD categorías
│       │   ├── Tamanios/                      # CRUD tamaños
│       │   ├── ProductosTamanios/             # Precios y stock por variante
│       │   ├── Cupones/                       # Gestión de cupones
│       │   ├── MetodosPagos/                  # Métodos de pago
│       │   ├── Carrito/                       # Carrito de compras del cliente
│       │   ├── Catalogo/                      # Catálogo de productos
│       │   ├── Pedidos/                       # Historial y gestión de pedidos
│       │   ├── Direcciones/                   # Direcciones del cliente
│       │   ├── Perfil/                        # Perfil del usuario
│       │   ├── Reportes/                      # Reportes administrativos
│       │   └── Shared/
│       │       ├── _Layout.cshtml             # Layout admin (sidebar + navbar)
│       │       ├── _LayoutCliente.cshtml      # Layout cliente (navbar + carrito)
│       │       ├── Error404.cshtml
│       │       └── Error500.cshtml
│       ├── wwwroot/
│       │   ├── css/                           # site.css, dashboard.css, login-style.css
│       │   ├── js/                            # site.js, login-script.js
│       │   ├── images/productos/              # Imágenes de productos subidas
│       │   └── lib/                           # Bootstrap, jQuery, validaciones
│       ├── appsettings.json                   # Configuración de la app (editar Server aquí)
│       ├── appsettings.Development.json.example  # Plantilla de configuración local
│       └── Program.cs                         # Startup, DI, seeding inicial
├── BDCAFETERIA.sql                            # Script de base de datos (referencia)
├── MIGRACIONES_FASE_AB.sql                   # Script de migraciones fase A-B
├── .gitignore
└── README.md
```

---

## ⚙️ Requisitos del Sistema

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2019+ **o** SQL Server Express (gratuito) **o** Azure SQL
- Visual Studio 2022 (recomendado) **o** VS Code con extensión C#
- (Opcional) SQL Server Management Studio (SSMS) para administrar la base de datos

---

## 🔧 Instalación y Configuración

### 1. Clonar el repositorio

```bash
git clone https://github.com/AguirreNM/Cafeteria.git
cd Cafeteria
```

### 2. Configurar la cadena de conexión

Edita `PryCafeteria/PryCafeteria/appsettings.json` con los datos de tu servidor SQL:

**Opción A — SQL Server local con autenticación de Windows (recomendado):**
```json
{
  "ConnectionStrings": {
    "BDCAFETERIAConn": "Server=localhost\\SQLEXPRESS;Initial Catalog=BDCAFETERIA;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
  }
}
```

**Opción B — SQL Server con usuario y contraseña:**
```json
{
  "ConnectionStrings": {
    "BDCAFETERIAConn": "Server=localhost\\SQLEXPRESS;Initial Catalog=BDCAFETERIA;User ID=sa;Password=TuPassword;Encrypt=True;TrustServerCertificate=True;"
  }
}
```

**Nombres de servidor más comunes:**

| Instalación | Server name |
|---|---|
| SQL Server Express (estándar) | `localhost\SQLEXPRESS` |
| SQL Server Developer/Standard | `localhost` |
| SQL LocalDB (Visual Studio) | `(localdb)\MSSQLLocalDB` |
| Azure SQL | `tu-servidor.database.windows.net` |

> **Tip:** Si no sabes el nombre de tu servidor, conéctate a SSMS y el nombre que aparece en "Server name" es el que debes usar, o ejecuta `SELECT @@SERVERNAME` en una consulta.

> **Configuración local:** También puedes copiar `appsettings.Development.json.example` como `appsettings.Development.json` y colocar ahí tu conexión local. Este archivo está en `.gitignore` y nunca se sube al repositorio.

---

## 🗄️ Configuración de Base de Datos

El sistema **aplica las migraciones automáticamente** al iniciar. No necesitas ejecutar scripts SQL ni comandos adicionales.

Al ejecutarse por primera vez, el sistema:
1. Crea la base de datos `BDCAFETERIA` con todas sus tablas
2. Crea los roles `Admin` y `Cliente`
3. Crea el usuario administrador por defecto

**Si prefieres aplicar las migraciones manualmente:**
```bash
cd PryCafeteria/PryCafeteria
dotnet ef database update
```

**Modelo de base de datos:**

| Tabla | Descripción |
|---|---|
| **__EFMigrationsHistory** | Historial de migraciones aplicadas por Entity Framework Core |
| **AspNetUsers** | Usuarios con Nombre, Apellido, FechaRegistro (extendidos con Identity) |
| **AspNetRoles** | Roles: Admin, Cliente |
| **AspNetRoleClaims** | Claims asociados a roles de Identity |
| **AspNetUserClaims** | Claims asociados a usuarios individuales |
| **AspNetUserLogins** | Proveedores de login externos vinculados a usuarios |
| **AspNetUserRoles** | Relación muchos a muchos entre usuarios y roles |
| **AspNetUserTokens** | Tokens de seguridad generados para usuarios (ej. reset de contraseña) |
| **Categorias** | Categorías de productos (ej. Bebidas Calientes, Frías) |
| **Cupones** | Descuentos por porcentaje o monto fijo |
| **DetallePedido** | Ítems individuales de cada pedido |
| **DireccionesEntrega** | Direcciones registradas por los clientes |
| **MetodosPago** | Métodos de pago disponibles |
| **Pedidos** | Órdenes de compra con estado, dirección y método de pago |
| **Productos** | Catálogo de productos con imagen, descripción y disponibilidad |
| **ProductosTamanios** | Precio y stock por combinación producto + tamaño |
| **StockMovimientos** | Historial de entradas, salidas y ajustes de stock |
| **Tamanios** | Tamaños disponibles (Pequeño, Mediano, Grande) |

---

## ▶️ Cómo Ejecutar el Proyecto

**Opción A — Visual Studio 2022:**
1. Abre `PryCafeteria/PryCafeteria.sln`
2. Configura la cadena de conexión en `appsettings.json`
3. Presiona `F5` o el botón ▶ **Run**

**Opción B — Terminal / CLI:**
```bash
cd PryCafeteria/PryCafeteria
dotnet run
```

La app estará disponible en: `https://localhost:7238` *(el puerto puede variar, revisa la consola)*

---

## 🔑 Credenciales de Prueba

El sistema crea automáticamente el siguiente usuario administrador al ejecutarse por primera vez:

| Campo | Valor |
|---|---|
| **Email** | `admin@gmail.com` |
| **Contraseña** | `admin123` |
| **Rol** | Admin |

> Para crear clientes de prueba, usa el formulario de Registro desde la pantalla de login o créalos desde el panel de administración de Usuarios.

---

## 🧩 Módulos del Sistema

### 👤 Autenticación (HU01 – HU02)
- Registro de nuevos clientes con validaciones de formato y email único
- Login con cookie de 14 días y opción "Recordarme"
- Logout seguro
- Bloqueo automático tras 5 intentos fallidos (15 minutos)
- Redirección post-login según rol: Admin → Dashboard, Cliente → Home

### 🛍️ Catálogo de Productos (Cliente)
- Visualización de productos disponibles por categoría
- Selección de tamaño y cantidad
- Agregado al carrito desde el catálogo

### 🛒 Carrito de Compras (HU11 – HU12 – HU13 – HU19)
- Agregar productos con tamaño y cantidad
- Modificar cantidades o eliminar ítems en el carrito
- Visualización de subtotal, descuento y total en tiempo real
- Aplicación de cupones de descuento (porcentaje o monto fijo)
- Persistencia en sesión (hasta 60 minutos de inactividad)

### 📦 Pedidos
- Proceso de checkout: selección de dirección, método de pago y confirmación
- Historial de pedidos para clientes
- Gestión de estados de pedido para administradores
- Vista de detalle de cada pedido

### 👤 Perfil y Direcciones del Cliente
- Edición de datos personales (nombre, apellido, contraseña)
- Gestión de múltiples direcciones de entrega (agregar, editar, eliminar)

---

## 🛡️ Módulo Administrativo

> Accesible únicamente para usuarios con rol **Admin**

### 📊 Dashboard (HU20)
- Tarjetas de resumen: total de productos, categorías, cupones activos y ítems con stock crítico
- Pedidos del día y pedidos pendientes de atención
- Ingresos del día (pedidos en estado "Entregado")
- Listado de productos con stock bajo o agotado
- Últimos productos agregados al catálogo
- Caché de métricas con actualización cada 2 minutos

### 📦 Gestión de Productos (HU04)
- CRUD completo con validación de nombre duplicado
- Subida de imagen con redimensionado automático (ImageSharp)
- Control de disponibilidad del producto
- Bloqueo de eliminación si el producto tiene ventas registradas

### 🏷️ Categorías (HU05)
- CRUD completo con validación de nombre duplicado
- Bloqueo de eliminación si tiene productos asociados

### 📐 Tamaños y Precios (HU08)
- CRUD de tamaños (Pequeño, Mediano, Grande, etc.)
- Asignación de precio y stock por combinación producto + tamaño
- Validación de precio mayor a 0 y stock no negativo
- Bloqueo de eliminación si tiene ventas registradas

### 📦 Control de Stock (HU09)
- Estados visuales: ✅ En stock / ⚠️ Bajo stock (≤ 5 unidades) / ❌ Agotado
- Alerta visual en el sidebar del panel de administración
- Historial de movimientos de stock (entradas, salidas, ajustes)

### 🏷️ Cupones de Descuento
- Creación de cupones con código único
- Tipos: descuento por porcentaje o monto fijo
- Fecha de vencimiento y límite de usos
- Validación al aplicar en el carrito

### 💳 Métodos de Pago
- CRUD de métodos de pago disponibles para checkout

### 👥 Gestión de Usuarios (HU03)
- Listado con tabs: Administradores / Clientes
- Búsqueda por nombre o email
- Crear, editar y cambiar rol de usuarios
- Fecha de registro y total de pedidos por usuario
- Bloqueo de eliminación si el usuario tiene pedidos asociados

### 📈 Reportes y Exportación Excel
- Reporte de ventas por período
- Productos más vendidos
- Clientes más frecuentes
- Uso y efectividad de cupones
- Stock actual de todos los productos
- Exportación de cualquier reporte a archivo `.xlsx` con ClosedXML

---

## 🎨 Paleta de Colores

| Color | Hex | Uso |
|---|---|---|
| Verde Starbucks | `#00704A` | Color principal, botones, navbar |
| Verde Oscuro | `#005238` | Hover, sombras, sidebar |
| Crema / Beige | `#D4AF77` | Acentos, bordes, íconos secundarios |
| Fondo Claro | `#F0EBE0` | Fondo general de la app |
| Blanco | `#FFFFFF` | Tarjetas, modales, formularios |

---

## ❗ Solución de Problemas Comunes

### "A network-related error occurred"
SQL Server no encontrado. Verifica que el servicio SQL Server esté corriendo (Servicios de Windows) y que el nombre del servidor en `appsettings.json` sea correcto.

### "Cannot open database BDCAFETERIA"
La base de datos aún no existe. Ejecuta el proyecto una vez y se creará automáticamente, o corre `dotnet ef database update`.

### "Login failed for user"
La cadena de conexión usa autenticación de usuario/contraseña pero las credenciales son incorrectas. Cambia a `Integrated Security=True` para usar autenticación de Windows.

### Error al ejecutar `dotnet ef`
Instala las herramientas globales de EF:
```bash
dotnet tool install --global dotnet-ef --version 8.*
```

---

## 👨‍💻 Autor

Desarrollado por **AguirreNM**

- GitHub: [@AguirreNM](https://github.com/AguirreNM/Cafeteria)

---

## 📄 Licencia

Este proyecto es de uso académico / personal. Sin licencia de distribución pública definida.
