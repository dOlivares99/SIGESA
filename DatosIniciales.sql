-- ================================================================
-- SIGESA — Datos iniciales (seed)
-- Ejecutar DESPUÉS de haber creado todas las tablas
-- ================================================================

USE SIGESA;
GO

-- ================================================================
-- 1. Usuario administrador
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'admin@sigesa.com')
BEGIN
    INSERT INTO Usuario (Nombre, Email, PasswordHash, Rol, Activo)
    VALUES (
        'Administrador SIGESA',
        'admin@sigesa.com',
        'HASH_GENERADO_POR_APLICACION',
        'Administrador',
        1
    );
    PRINT 'Usuario administrador insertado.';
END
ELSE
    PRINT 'Usuario administrador ya existe, se omite.';
GO

-- ================================================================
-- 2. Paquetes
-- Básico:   50 personas x 8.000 = 400.000
-- Estándar: 80 personas x 8.000 = 640.000 + decoración 90.000 = 730.000
-- Premium: 100 personas x 8.000 = 800.000 + decoración 90.000
--          + animación 80.000 = 970.000
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Paquete WHERE Nombre = 'Básico')
BEGIN
    INSERT INTO Paquete (Nombre, Descripcion, PrecioBase, MaxPersonas, DuracionHoras, Activo)
    VALUES (
        'Básico',
        'Incluye alquiler del salón, menaje completo y alimentación para 50 personas. Duración 4 horas.',
        400000, 50, 4, 1
    );
    PRINT 'Paquete Básico insertado.';
END
ELSE
    PRINT 'Paquete Básico ya existe, se omite.';
GO

IF NOT EXISTS (SELECT 1 FROM Paquete WHERE Nombre = 'Estándar')
BEGIN
    INSERT INTO Paquete (Nombre, Descripcion, PrecioBase, MaxPersonas, DuracionHoras, Activo)
    VALUES (
        'Estándar',
        'Incluye alquiler del salón, menaje completo, alimentación para 80 personas y decoración total del salón. Duración 4 horas.',
        730000, 80, 4, 1
    );
    PRINT 'Paquete Estándar insertado.';
END
ELSE
    PRINT 'Paquete Estándar ya existe, se omite.';
GO

IF NOT EXISTS (SELECT 1 FROM Paquete WHERE Nombre = 'Premium')
BEGIN
    INSERT INTO Paquete (Nombre, Descripcion, PrecioBase, MaxPersonas, DuracionHoras, Activo)
    VALUES (
        'Premium',
        'Incluye alquiler del salón, menaje completo, alimentación para 100 personas, decoración total del salón y animador con música. Duración 4 horas.',
        970000, 100, 4, 1
    );
    PRINT 'Paquete Premium insertado.';
END
ELSE
    PRINT 'Paquete Premium ya existe, se omite.';
GO

-- ================================================================
-- 3. Servicios adicionales
-- ================================================================
IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Catering fuera del salón (por persona)')
BEGIN
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo)
    VALUES ('Catering fuera del salón (por persona)', 'Alimentación', 10000, 1);
    PRINT 'Servicio Catering fuera del salón insertado.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Torta personalizada')
BEGIN
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo)
    VALUES ('Torta personalizada', 'Alimentación', 25000, 1);
    PRINT 'Servicio Torta personalizada insertado.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Servicio de DJ')
BEGIN
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo)
    VALUES ('Servicio de DJ', 'Entretenimiento', 80000, 1);
    PRINT 'Servicio DJ insertado.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Animación infantil')
BEGIN
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo)
    VALUES ('Animación infantil', 'Entretenimiento', 60000, 1);
    PRINT 'Servicio Animación infantil insertado.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Fotografía del evento')
BEGIN
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo)
    VALUES ('Fotografía del evento', 'Fotografía', 120000, 1);
    PRINT 'Servicio Fotografía insertado.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Video del evento')
BEGIN
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo)
    VALUES ('Video del evento', 'Fotografía', 150000, 1);
    PRINT 'Servicio Video insertado.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Decoración floral')
BEGIN
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo)
    VALUES ('Decoración floral', 'Decoración', 45000, 1);
    PRINT 'Servicio Decoración floral insertado.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Decoración temática')
BEGIN
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo)
    VALUES ('Decoración temática', 'Decoración', 90000, 1);
    PRINT 'Servicio Decoración temática insertado.';
END
GO

IF NOT EXISTS (SELECT 1 FROM Servicio WHERE Nombre = 'Servicio de meseros')
BEGIN
    INSERT INTO Servicio (Nombre, Categoria, PrecioUnitario, Activo)
    VALUES ('Servicio de meseros', 'Personal', 40000, 1);
    PRINT 'Servicio Meseros insertado.';
END
GO

-- ======================================================================
-- 4. Verificación final del seed (SE EJECUTA HASTA EL FINAL X SEPARADO)
-- ======================================================================
PRINT '--- Usuarios ---';
SELECT UsuarioId, Nombre, Email, Rol, Activo FROM Usuario;

PRINT '--- Paquetes ---';
SELECT PaqueteId, Nombre, PrecioBase, MaxPersonas, DuracionHoras, Activo
FROM Paquete ORDER BY PrecioBase;

PRINT '--- Servicios adicionales ---';
SELECT ServicioId, Nombre, Categoria, PrecioUnitario, Activo
FROM Servicio ORDER BY Categoria, Nombre;

GO