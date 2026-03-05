CREATE DATABASE BDCAFETERIA;
GO

USE BDCAFETERIA;
GO

CREATE TABLE Roles (
    RolId INT IDENTITY(1,1) PRIMARY KEY,
    RolNombre VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Usuarios (
    UsuarioId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Contrasena VARCHAR(50) NOT NULL,
    RolId INT NOT NULL,
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RolId) REFERENCES Roles(RolId)
);

CREATE TABLE Categorias (
    CategoriaId INT IDENTITY(1,1) PRIMARY KEY,
    NombreCategoria VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Tamanios (
    TamanioId INT IDENTITY(1,1) PRIMARY KEY,
    NombreTamanio VARCHAR(30) NOT NULL UNIQUE
);

CREATE TABLE MetodosPago (
    MetodoPagoId INT IDENTITY(1,1) PRIMARY KEY,
    NombreMetodoPago VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Productos (
    ProductoId INT IDENTITY(1,1) PRIMARY KEY,
    NombreProducto VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(200) NULL,
    CategoriaId INT NOT NULL,
    Imagen VARCHAR(100) NULL,
    Disponible BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Productos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(CategoriaId)
);

CREATE TABLE ProductosTamanios (
    ProductoTamanioId INT IDENTITY(1,1) PRIMARY KEY,
    ProductoId INT NOT NULL,
    TamanioId INT NOT NULL,
    Precio DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ProductosTamanios_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(ProductoId),
    CONSTRAINT FK_ProductosTamanios_Tamanios FOREIGN KEY (TamanioId) REFERENCES Tamanios(TamanioId),
    CONSTRAINT UQ_ProductosTamanios UNIQUE (ProductoId, TamanioId)
);

CREATE TABLE DireccionesEntrega (
    DireccionId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    NombreDireccion VARCHAR(50) NOT NULL,
    Calle VARCHAR(100) NOT NULL,
    Numero VARCHAR(20) NOT NULL,
    Distrito VARCHAR(100) NOT NULL,
    CodigoPostal VARCHAR(10) NOT NULL,
    referencias VARCHAR(200) NULL,
    CONSTRAINT FK_DireccionesEntrega_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId)
);

CREATE TABLE Pedidos (
    PedidoId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    FechaPedido DATETIME NOT NULL DEFAULT GETDATE(),
    Descuento DECIMAL(10,2) NOT NULL DEFAULT 0,
    Total DECIMAL(10,2) NOT NULL,
    MetodoPagoId INT NOT NULL,
    DireccionId INT NULL,
    TipoEntrega VARCHAR(20) NOT NULL,
    Estado VARCHAR(20) NOT NULL, -- Pendiente | Entregado
    CONSTRAINT FK_Pedidos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT FK_Pedidos_MetodosPago FOREIGN KEY (MetodoPagoId) REFERENCES MetodosPago(MetodoPagoId),
    CONSTRAINT FK_Pedidos_DireccionesEntrega FOREIGN KEY (DireccionId) REFERENCES DireccionesEntrega(DireccionId)
);

CREATE TABLE DetallePedido (
    DetalleId INT IDENTITY(1,1) PRIMARY KEY,
    PedidoId INT NOT NULL,
    ProductoTamanioId INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_DetallePedido_Pedidos FOREIGN KEY (PedidoId) REFERENCES Pedidos(PedidoId),
    CONSTRAINT FK_DetallePedido_ProductosTamanios FOREIGN KEY (ProductoTamanioId) REFERENCES ProductosTamanios(ProductoTamanioId)
);

CREATE TABLE Cupones (
    CuponId INT IDENTITY(1,1) PRIMARY KEY,
    NombreCupon VARCHAR(50) NOT NULL UNIQUE,
    TipoDescuento VARCHAR(20) NOT NULL, -- Porcentaje | Monto
    ValorDescuento DECIMAL(10,2) NOT NULL,
    FechaInicio DATETIME NOT NULL,
    FechaFin DATETIME NOT NULL,
    Activo BIT NOT NULL DEFAULT 1
);



-- Datos 
INSERT INTO Roles (RolNombre) VALUES ('Admin'), ('Cliente');

INSERT INTO Categorias (NombreCategoria) VALUES ('Bebidas Calientes'), ('Bebidas Fr as'), ('Frappe'), ('Reposter a'), ('S ndwiches');

INSERT INTO Tamanios (NombreTamanio) VALUES ('Peque o'), ('Mediano'), ('Grande');

INSERT INTO MetodosPago (NombreMetodoPago) VALUES ('Tarjeta'), ('Billetera digital');

INSERT INTO Usuarios (Nombre, Email, Contrasena, RolId) VALUES ('Admin', 'admin@gmail.com', 'admin123', 1);


INSERT INTO Productos (NombreProducto, Descripcion, CategoriaId, Disponible) VALUES
-- Bebidas Calientes (CategoriaId = 1)
('Caf  Americano', 'Caf  negro con agua caliente', 1, 1),
('Caf  Latte', 'Caf  con leche espumosa', 1, 1),
('Caf  Cappuccino', 'Caf  con leche y espuma de leche', 1, 1),
('Caf  Mocha', 'Caf  con chocolate y leche', 1, 1),
('Th  Tarragon', 'T  verde con menta', 1, 1),

-- Bebidas Fr as (CategoriaId = 2)
('Iced Latte', 'Caf  con leche helado', 2, 1),
('Cold Brew', 'Caf  preparado en fr o', 2, 1),
('Iced Americano', 'Caf  americano helado', 2, 1),

-- Frappe (CategoriaId = 3)
('Frappe de Caf ', 'Bebida helada con caf  y crema', 3, 1),
('Frappe de Caramel', 'Bebida helada con caramel y crema', 3, 1),
('Frappe de Chocolate', 'Bebida helada con chocolate y crema', 3, 1),

-- Reposter a (CategoriaId = 4)
('Croissant', 'Croissant de mantequilla', 4, 1),
('Muffin de Ar ndano', 'Muffin dulce con ar ndanos', 4, 1),
('Cookie de Chocolate', 'Galleta con chips de chocolate', 4, 1),

-- S ndwiches (CategoriaId = 5)
('S ndwich de Pollo', 'S ndwich con pollo a la parrilla', 5, 1),
('S ndwich de Jam n', 'S ndwich con jam n y queso', 5, 1);

INSERT INTO ProductosTamanios (ProductoId, TamanioId, Precio, Stock) VALUES
-- Caf  Americano
(1, 1, 5.50, 20),
(1, 2, 6.50, 15),
(1, 3, 7.50, 10),

-- Caf  Latte
(2, 1, 6.00, 20),
(2, 2, 7.00, 15),
(2, 3, 8.00, 10),

-- Caf  Cappuccino
(3, 1, 6.50, 18),
(3, 2, 7.50, 12),
(3, 3, 8.50, 8),

-- Caf  Mocha
(4, 1, 7.00, 15),
(4, 2, 8.00, 10),
(4, 3, 9.00, 8),

-- Th  Tarragon
(5, 1, 5.00, 25),
(5, 2, 6.00, 18),
(5, 3, 7.00, 12),

-- Iced Latte
(6, 1, 6.50, 20),
(6, 2, 7.50, 14),
(6, 3, 8.50, 10),

-- Cold Brew
(7, 1, 7.00, 18),
(7, 2, 8.00, 12),
(7, 3, 9.00, 8),

-- Iced Americano
(8, 1, 6.00, 22),
(8, 2, 7.00, 16),
(8, 3, 8.00, 10),

-- Frappe de Caf 
(9, 1, 7.50, 15),
(9, 2, 8.50, 10),
(9, 3, 9.50, 8),

-- Frappe de Caramel
(10, 1, 7.50, 15),
(10, 2, 8.50, 10),
(10, 3, 9.50, 8),

-- Frappe de Chocolate
(11, 1, 7.50, 14),
(11, 2, 8.50, 10),
(11, 3, 9.50, 7),

-- Croissant (solo tama o  nico, Peque o)
(12, 1, 4.00, 30),

-- Muffin de Ar ndano (solo tama o  nico, Peque o)
(13, 1, 3.50, 25),

-- Cookie de Chocolate (solo tama o  nico, Peque o)
(14, 1, 3.00, 30),

-- S ndwich de Pollo (solo tama o  nico, Peque o)
(15, 1, 8.00, 20),

-- S ndwich de Jam n (solo tama o  nico, Peque o)
(16, 1, 7.00, 20);

UPDATE Productos SET Imagen = '/images/productos/Bebidas_Calientes/Cafe_Americano.png' WHERE ProductoId = 1;
UPDATE Productos SET Imagen = '/images/productos/Bebidas_Calientes/Cafe_Latte.png' WHERE ProductoId = 2;
UPDATE Productos SET Imagen = '/images/productos/Bebidas_Calientes/Cafe_Cappuccino.png' WHERE ProductoId = 3;
UPDATE Productos SET Imagen = '/images/productos/Bebidas_Calientes/Cafe_Mocha.png' WHERE ProductoId = 4;
UPDATE Productos SET Imagen = '/images/productos/Bebidas_Calientes/Te_Tarragon.png' WHERE ProductoId = 5;
UPDATE Productos SET Imagen = '/images/productos/Bebidas_Frias/Iced_Latte.png' WHERE ProductoId = 6;
UPDATE Productos SET Imagen = '/images/productos/Bebidas_Frias/Cold_Brew.png' WHERE ProductoId = 7;
UPDATE Productos SET Imagen = '/images/productos/Bebidas_Frias/Iced_Americano.png' WHERE ProductoId = 8;
UPDATE Productos SET Imagen = '/images/productos/Frappe/Frappe_de_Cafe.png' WHERE ProductoId = 9;
UPDATE Productos SET Imagen = '/images/productos/Frappe/Frappe_de_Caramel.png' WHERE ProductoId = 10;
UPDATE Productos SET Imagen = '/images/productos/Frappe/Frappe_de_Chocolate.png' WHERE ProductoId = 11;
UPDATE Productos SET Imagen = '/images/productos/Reposteria/Croissant.png' WHERE ProductoId = 12;
UPDATE Productos SET Imagen = '/images/productos/Reposteria/Muffin_de_Arandano.png' WHERE ProductoId = 13;
UPDATE Productos SET Imagen = '/images/productos/Reposteria/Cookie_de_Chocolate.png' WHERE ProductoId = 14;
UPDATE Productos SET Imagen = '/images/productos/Sandwiches/Sandwich_de_Pollo.png' WHERE ProductoId = 15;
UPDATE Productos SET Imagen = '/images/productos/Sandwiches/Sandwich_de_Jamon.png' WHERE ProductoId = 16;