-- ============================================================
-- MIGRACIONES MANUALES PARA EL PROYECTO PryCafeteria
-- Ejecutar en orden sobre la base de datos BDCAFETERIA
-- Compatible con SQL Server
-- ============================================================

USE BDCAFETERIA;
GO

-- ──────────────────────────────────────────────────────────────────
-- MIGRACIÓN 1: Agregar FechaCreacion a tabla Categorias
-- HU-Categorias E6: fecha visible en el listado
-- ──────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Categorias' AND COLUMN_NAME = 'FechaCreacion'
)
BEGIN
    ALTER TABLE Categorias
    ADD FechaCreacion DATETIME NOT NULL DEFAULT GETDATE();

    PRINT '✅ Columna FechaCreacion agregada a Categorias';
END
ELSE
    PRINT '⚠️  FechaCreacion ya existe en Categorias — omitido';
GO

-- ──────────────────────────────────────────────────────────────────
-- MIGRACIÓN 2: Crear tabla StockMovimientos
-- HU-Stock E9: auditoría de movimientos de stock
-- ──────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'StockMovimientos'
)
BEGIN
    CREATE TABLE StockMovimientos (
        StockMovimientoId INT IDENTITY(1,1) PRIMARY KEY,
        ProductoTamanioId INT NOT NULL,
        Tipo              VARCHAR(20) NOT NULL,   -- Ingreso | Venta | Ajuste
        Cantidad          INT NOT NULL,
        StockResultante   INT NOT NULL,
        Fecha             DATETIME NOT NULL DEFAULT GETDATE(),
        UsuarioId         NVARCHAR(450) NULL,

        CONSTRAINT FK_StockMovimientos_ProductosTamanios
            FOREIGN KEY (ProductoTamanioId)
            REFERENCES ProductosTamanios(ProductoTamanioId)
            ON DELETE NO ACTION,

        CONSTRAINT FK_StockMovimientos_AspNetUsers
            FOREIGN KEY (UsuarioId)
            REFERENCES AspNetUsers(Id)
            ON DELETE SET NULL
    );

    -- Índice para consultas frecuentes por variante y fecha
    CREATE INDEX IX_StockMovimientos_ProductoTamanioId_Fecha
        ON StockMovimientos(ProductoTamanioId, Fecha DESC);

    PRINT '✅ Tabla StockMovimientos creada correctamente';
END
ELSE
    PRINT '⚠️  StockMovimientos ya existe — omitido';
GO

-- ──────────────────────────────────────────────────────────────────
-- MIGRACIÓN 3: CHECK constraint Stock >= 0 en ProductosTamanios
-- HU-Stock E8: impedir valores negativos de stock
-- ──────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_NAME = 'ProductosTamanios'
    AND CONSTRAINT_NAME = 'CK_ProductosTamanios_StockPositivo'
)
BEGIN
    ALTER TABLE ProductosTamanios
    ADD CONSTRAINT CK_ProductosTamanios_StockPositivo CHECK (Stock >= 0);

    PRINT '✅ CHECK constraint Stock >= 0 aplicado a ProductosTamanios';
END
ELSE
    PRINT '⚠️  CK_ProductosTamanios_StockPositivo ya existe — omitido';
GO

-- ──────────────────────────────────────────────────────────────────
-- MIGRACIÓN 4: Índices para optimización de consultas del Dashboard
-- HU-Dashboard E15: consultas optimizadas
-- ──────────────────────────────────────────────────────────────────

-- Índice en Pedidos.FechaPedido (filtro más frecuente del dashboard)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Pedidos_FechaPedido' AND object_id = OBJECT_ID('Pedidos')
)
BEGIN
    CREATE INDEX IX_Pedidos_FechaPedido ON Pedidos(FechaPedido DESC);
    PRINT '✅ Índice IX_Pedidos_FechaPedido creado';
END
GO

-- Índice en Pedidos.Estado (filtro de pendientes)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Pedidos_Estado' AND object_id = OBJECT_ID('Pedidos')
)
BEGIN
    CREATE INDEX IX_Pedidos_Estado ON Pedidos(Estado);
    PRINT '✅ Índice IX_Pedidos_Estado creado';
END
GO

-- Índice en ProductosTamanios.Stock (consulta de stock bajo / agotado)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ProductosTamanios_Stock' AND object_id = OBJECT_ID('ProductosTamanios')
)
BEGIN
    CREATE INDEX IX_ProductosTamanios_Stock ON ProductosTamanios(Stock);
    PRINT '✅ Índice IX_ProductosTamanios_Stock creado';
END
GO

-- ──────────────────────────────────────────────────────────────────
-- MIGRACIÓN 5: Trigger para descontar stock al confirmar pedido
-- HU-Stock E10: descuento automático por venta
-- ──────────────────────────────────────────────────────────────────
IF EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TR_DescontarStock')
    DROP TRIGGER TR_DescontarStock;
GO

CREATE TRIGGER TR_DescontarStock
ON DetallePedido
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Descontar stock para cada detalle insertado
    UPDATE pt
    SET pt.Stock = pt.Stock - i.Cantidad
    FROM ProductosTamanios pt
    INNER JOIN inserted i ON pt.ProductoTamanioId = i.ProductoTamanioId;

    -- Registrar movimiento de tipo "Venta" en StockMovimientos
    INSERT INTO StockMovimientos (ProductoTamanioId, Tipo, Cantidad, StockResultante, Fecha, UsuarioId)
    SELECT
        i.ProductoTamanioId,
        'Venta',
        -i.Cantidad,                                    -- negativo = salida
        pt.Stock,                                        -- stock ya actualizado
        GETDATE(),
        NULL                                             -- movimiento automático del sistema
    FROM inserted i
    INNER JOIN ProductosTamanios pt ON pt.ProductoTamanioId = i.ProductoTamanioId;
END;
GO

PRINT '✅ Trigger TR_DescontarStock creado/reemplazado correctamente';
GO

-- ──────────────────────────────────────────────────────────────────
-- VERIFICACIÓN FINAL
-- ──────────────────────────────────────────────────────────────────
SELECT
    'Categorias.FechaCreacion' AS Objeto,
    CASE WHEN EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME='Categorias' AND COLUMN_NAME='FechaCreacion'
    ) THEN '✅ OK' ELSE '❌ FALTA' END AS Estado
UNION ALL
SELECT 'StockMovimientos (tabla)',
    CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='StockMovimientos')
    THEN '✅ OK' ELSE '❌ FALTA' END
UNION ALL
SELECT 'CK_ProductosTamanios_StockPositivo',
    CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_NAME='CK_ProductosTamanios_StockPositivo')
    THEN '✅ OK' ELSE '❌ FALTA' END
UNION ALL
SELECT 'TR_DescontarStock (trigger)',
    CASE WHEN EXISTS (SELECT 1 FROM sys.triggers WHERE name='TR_DescontarStock')
    THEN '✅ OK' ELSE '❌ FALTA' END;
GO
