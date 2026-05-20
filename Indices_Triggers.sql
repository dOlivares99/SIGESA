

-- ================================================================
-- ÍNDICES
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evento_ClienteId')
    CREATE INDEX IX_Evento_ClienteId   ON Evento(ClienteId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evento_FechaEvento')
    CREATE INDEX IX_Evento_FechaEvento ON Evento(FechaEvento);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Evento_Estado')
    CREATE INDEX IX_Evento_Estado      ON Evento(Estado);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Cotizacion_EventoId')
    CREATE INDEX IX_Cotizacion_EventoId ON Cotizacion(EventoId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Pago_EventoId')
    CREATE INDEX IX_Pago_EventoId      ON Pago(EventoId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Auditoria_UsuarioId')
    CREATE INDEX IX_Auditoria_UsuarioId ON Auditoria(UsuarioId);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Auditoria_FechaHora')
    CREATE INDEX IX_Auditoria_FechaHora ON Auditoria(FechaHora);
PRINT 'Índices creados.';
GO

-- ================================================================
-- TRIGGER: Recalcular MontoPagado y EstadoPago al insertar pago
-- ================================================================
IF OBJECT_ID('TR_Pago_ActualizarEvento','TR') IS NOT NULL
    DROP TRIGGER TR_Pago_ActualizarEvento;
GO
CREATE TRIGGER TR_Pago_ActualizarEvento
ON Pago AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE E
    SET
        E.MontoPagado = (SELECT ISNULL(SUM(P.Monto),0) FROM Pago P WHERE P.EventoId = E.EventoId),
        E.EstadoPago  = CASE
            WHEN (SELECT ISNULL(SUM(P.Monto),0) FROM Pago P WHERE P.EventoId = E.EventoId) = 0
                THEN 'Pendiente'
            WHEN (SELECT ISNULL(SUM(P.Monto),0) FROM Pago P WHERE P.EventoId = E.EventoId) >= E.MontoTotal
                THEN 'Saldado'
            ELSE 'Parcial'
        END
    FROM Evento E
    INNER JOIN inserted I ON E.EventoId = I.EventoId;
END;
GO
PRINT 'Trigger TR_Pago_ActualizarEvento creado.';

-- ================================================================
-- TRIGGER: Bitácora automática en cambios de estado de Evento
-- ================================================================
IF OBJECT_ID('TR_Evento_Auditoria','TR') IS NOT NULL
    DROP TRIGGER TR_Evento_Auditoria;
GO
CREATE TRIGGER TR_Evento_Auditoria
ON Evento AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Auditoria (UsuarioId, Tabla, Accion, RegistroId, ValorAnterior, ValorNuevo)
    SELECT I.UsuarioCreacion, 'Evento', 'UPDATE', I.EventoId, D.Estado, I.Estado
    FROM inserted I
    INNER JOIN deleted D ON I.EventoId = D.EventoId
    WHERE I.Estado <> D.Estado;
END;
GO
PRINT 'Trigger TR_Evento_Auditoria creado.';
