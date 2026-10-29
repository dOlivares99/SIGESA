-- ================================================================
-- Fix: agrega la columna Activo (BIT NOT NULL DEFAULT 1) a toda
-- tabla del modelo que no la tenga.
--
-- Contexto: Db.sql define Activo dentro de cada CREATE TABLE, que
-- solo corre IF NOT EXISTS la tabla. En bases creadas antes de
-- agregar Activo (p. ej. Azure), las tablas ya existian, el CREATE
-- se salto y Activo nunca se agrego. Solo Cliente tenia ALTER de
-- respaldo; este script cubre las 7 tablas.
--
-- Idempotente: solo altera la tabla a la que le falte la columna.
-- ================================================================
DECLARE @t SYSNAME, @sql NVARCHAR(MAX);
DECLARE c CURSOR FOR
    SELECT name FROM (VALUES
        ('Rol'),('TipoPago'),('MetodoPago'),
        ('Usuario'),('Cliente'),('Paquete'),('Servicio')) AS v(name);
OPEN c; FETCH NEXT FROM c INTO @t;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID(@t) AND name = 'Activo')
    BEGIN
        SET @sql = 'ALTER TABLE ' + QUOTENAME(@t) + ' ADD Activo BIT NOT NULL DEFAULT 1;';
        EXEC sp_executesql @sql;
        PRINT 'Activo agregado a ' + @t;
    END
    FETCH NEXT FROM c INTO @t;
END
CLOSE c; DEALLOCATE c;
