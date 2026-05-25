-- ================================================================
-- SIGESA — Datos iniciales (seed)
-- Ejecutar DESPUÉS de BD_Creacion_Inicial.sql
-- Versión 1.3
-- ================================================================

USE SIGESA;
GO

-- ================================================================
-- 1. Roles
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Rol WHERE Nombre = 'Administrador')
    INSERT INTO Rol (Nombre, Descripcion, Activo)
    VALUES ('Administrador', 'Acceso total a todos los modulos del sistema.', 1);
GO

IF NOT EXISTS (SELECT 1 FROM Rol WHERE Nombre = 'Asistente')
    INSERT INTO Rol (Nombre, Descripcion, Activo)
    VALUES ('Asistente', 'Acceso de lectura y registro de pagos. No puede crear ni modificar reservas o cotizaciones.', 1);
GO

-- ================================================================
-- 2. Tipos de pago
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM TipoPago WHERE Nombre = 'Anticipo')
    INSERT INTO TipoPago (Nombre, Descripcion, Activo)
    VALUES ('Anticipo', 'Pago inicial para reservar la fecha del evento.', 1);
GO

IF NOT EXISTS (SELECT 1 FROM TipoPago WHERE Nombre = 'Abono')
    INSERT INTO TipoPago (Nombre, Descripcion, Activo)
    VALUES ('Abono', 'Pago parcial durante el proceso.', 1);
GO

IF NOT EXISTS (SELECT 1 FROM TipoPago WHERE Nombre = 'Saldo')
    INSERT INTO TipoPago (Nombre, Descripcion, Activo)
    VALUES ('Saldo', 'Pago final que cancela el saldo pendiente del evento.', 1);
GO

-- ================================================================
-- 3. Metodos de pago
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM MetodoPago WHERE Nombre = 'SINPE')
    INSERT INTO MetodoPago (Nombre, Descripcion, Activo)
    VALUES ('SINPE', 'SINPE Movil', 1);
GO

IF NOT EXISTS (SELECT 1 FROM MetodoPago WHERE Nombre = 'Transferencia')
    INSERT INTO MetodoPago (Nombre, Descripcion, Activo)
    VALUES ('Transferencia', 'Transferencia bancaria', 1);
GO

IF NOT EXISTS (SELECT 1 FROM MetodoPago WHERE Nombre = 'Efectivo')
    INSERT INTO MetodoPago (Nombre, Descripcion, Activo)
    VALUES ('Efectivo', 'Pago en efectivo', 1);
GO

-- ================================================================
-- 4. Paquetes
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Paquete WHERE Nombre = 'Basico')
    INSERT INTO Paquete (Nombre, Descripcion, PrecioBase, MaxPersonas, DuracionHoras, Activo)
    VALUES ('Basico', 'Incluye alquiler del salon, menaje completo y alimentacion para 50 personas. Duracion 4 horas.', 400000, 50, 4, 1);
GO

IF NOT EXISTS (SELECT 1 FROM Paquete WHERE Nombre = 'Estandar')
    INSERT INTO Paquete (Nombre, Descripcion, PrecioBase, MaxPersonas, DuracionHoras, Activo)
    VALUES ('Estandar', 'Incluye alquiler del salon, menaje completo, alimentacion para 80 personas y decoracion total del salon. Duracion 4 horas.', 730000, 80, 4, 1);
GO

IF NOT EXISTS (SELECT 1 FROM Paquete WHERE Nombre = 'Premium')
    INSERT INTO Paquete (Nombre, Descripcion, PrecioBase, MaxPersonas, DuracionHoras, Activo)
    VALUES ('Premium', 'Incluye alquiler del salon, menaje completo, alimentacion para 100 personas, decoracion total del salon y animador con musica. Duracion 4 horas.', 970000, 100, 4, 1);
GO

-- ================================================================
-- 5. Servicios adicionales
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Catering fuera del salon (por persona)')
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo) VALUES ('Catering fuera del salon (por persona)', 'Alimentacion', 10000, 1);
IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Torta personalizada')
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo) VALUES ('Torta personalizada', 'Alimentacion', 25000, 1);
IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Servicio de DJ')
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo) VALUES ('Servicio de DJ', 'Entretenimiento', 80000, 1);
IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Animacion infantil')
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo) VALUES ('Animacion infantil', 'Entretenimiento', 60000, 1);
IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Fotografia del evento')
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo) VALUES ('Fotografia del evento', 'Fotografia', 120000, 1);
IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Video del evento')
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo) VALUES ('Video del evento', 'Fotografia', 150000, 1);
IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Decoracion floral')
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo) VALUES ('Decoracion floral', 'Decoracion', 45000, 1);
IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Decoracion tematica')
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo) VALUES ('Decoracion tematica', 'Decoracion', 90000, 1);
IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Servicio de meseros')
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo) VALUES ('Servicio de meseros', 'Personal', 40000, 1);
GO

-- ================================================================
-- Verificacion
-- ================================================================
PRINT '--- Roles ---';
SELECT RolId, Nombre, Activo FROM Rol;
PRINT '--- TipoPago ---';
SELECT TipoPagoId, Nombre, Activo FROM TipoPago;
PRINT '--- MetodoPago ---';
SELECT MetodoPagoId, Nombre, Activo FROM MetodoPago;
PRINT '--- Paquetes ---';
SELECT PaqueteId, Nombre, PrecioBase FROM Paquete;
PRINT '--- Servicios ---';
SELECT ServicioId, Nombre, Categoria FROM Servicio ORDER BY Categoria;
GO
