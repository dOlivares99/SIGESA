-- ================================================================
-- SIGESA — Sistema de Gestión de Sala de Eventos
-- Script de creación de base de datos
-- SQL Server 2017 / Azure SQL Database
-- Versión 1.1
-- ================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SIGESA')
BEGIN
    CREATE DATABASE SIGESA
    COLLATE Latin1_General_CI_AI;
    PRINT 'Base de datos SIGESA creada correctamente.';
END
GO

USE SIGESA;
GO

-- ================================================================
-- 1. TABLA: Usuario
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuario')
BEGIN
    CREATE TABLE Usuario (
        UsuarioId       INT IDENTITY(1,1)   NOT NULL,
        Nombre          NVARCHAR(100)       NOT NULL,
        Email           NVARCHAR(150)       NOT NULL,
        PasswordHash    NVARCHAR(256)       NOT NULL,
        Rol             NVARCHAR(20)        NOT NULL
            CONSTRAINT CK_Usuario_Rol CHECK (Rol IN ('Administrador', 'Asistente')),
        Activo          BIT                 NOT NULL    DEFAULT 1,
        FechaCreacion   DATETIME            NOT NULL    DEFAULT GETDATE(),
        CONSTRAINT PK_Usuario PRIMARY KEY (UsuarioId),
        CONSTRAINT UQ_Usuario_Email UNIQUE (Email)
    );
    PRINT 'Tabla Usuario creada.';
END
GO

-- ================================================================
-- 2. TABLA: Cliente
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cliente')
BEGIN
    CREATE TABLE Cliente (
        ClienteId       INT IDENTITY(1,1)   NOT NULL,
        Nombre          NVARCHAR(150)       NOT NULL,
        Documento       NVARCHAR(20)        NOT NULL,
        Telefono        NVARCHAR(20)        NOT NULL,
        Email           NVARCHAR(150)           NULL,
        FechaRegistro   DATETIME            NOT NULL    DEFAULT GETDATE(),
        CONSTRAINT PK_Cliente PRIMARY KEY (ClienteId),
        CONSTRAINT UQ_Cliente_Documento UNIQUE (Documento)
    );
    PRINT 'Tabla Cliente creada.';
END
GO

-- ================================================================
-- 3. TABLA: Paquete
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Paquete')
BEGIN
    CREATE TABLE Paquete (
        PaqueteId       INT IDENTITY(1,1)   NOT NULL,
        Nombre          NVARCHAR(100)       NOT NULL,
        Descripcion     NVARCHAR(500)           NULL,
        PrecioBase      DECIMAL(10,2)       NOT NULL    DEFAULT 0,
        MaxPersonas     INT                 NOT NULL    DEFAULT 50,
        DuracionHoras   INT                 NOT NULL    DEFAULT 4,
        Activo          BIT                 NOT NULL    DEFAULT 1,
        FechaCreacion   DATETIME            NOT NULL    DEFAULT GETDATE(),
        CONSTRAINT PK_Paquete PRIMARY KEY (PaqueteId),
        CONSTRAINT UQ_Paquete_Nombre UNIQUE (Nombre),
        CONSTRAINT CK_Paquete_Precio CHECK (PrecioBase >= 0)
    );
    PRINT 'Tabla Paquete creada.';
END
GO

-- ================================================================
-- 4. TABLA: Servicio
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Servicio')
BEGIN
    CREATE TABLE Servicio (
        ServicioId      INT IDENTITY(1,1)   NOT NULL,
        Nombre          NVARCHAR(100)       NOT NULL,
        Categoria       NVARCHAR(80)        NOT NULL,
        PrecioUnitario  DECIMAL(10,2)       NOT NULL    DEFAULT 0,
        Activo          BIT                 NOT NULL    DEFAULT 1,
        FechaCreacion   DATETIME            NOT NULL    DEFAULT GETDATE(),
        CONSTRAINT PK_Servicio PRIMARY KEY (ServicioId),
        CONSTRAINT UQ_Servicio_Nombre UNIQUE (Nombre),
        CONSTRAINT CK_Servicio_Precio CHECK (PrecioUnitario >= 0)
    );
    PRINT 'Tabla Servicio creada.';
END
GO

-- ================================================================
-- 5. TABLA: Evento
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Evento')
BEGIN
    CREATE TABLE Evento (
        EventoId        INT IDENTITY(1,1)   NOT NULL,
        ClienteId       INT                 NOT NULL,
        PaqueteId       INT                 NOT NULL,
        TipoEvento      NVARCHAR(80)        NOT NULL,
        FechaEvento     DATE                NOT NULL,
        NumPersonas     INT                 NOT NULL    DEFAULT 1,
        Estado          NVARCHAR(20)        NOT NULL    DEFAULT 'Pendiente'
            CONSTRAINT CK_Evento_Estado CHECK (Estado IN (
                'Pendiente','Confirmada','EnProceso','Realizada','Cancelada')),
        MontoTotal      DECIMAL(10,2)       NOT NULL    DEFAULT 0,
        MontoPagado     DECIMAL(10,2)       NOT NULL    DEFAULT 0,
        SaldoPendiente  AS (MontoTotal - MontoPagado),
        EstadoPago      NVARCHAR(20)        NOT NULL    DEFAULT 'Pendiente'
            CONSTRAINT CK_Evento_EstadoPago CHECK (EstadoPago IN (
                'Pendiente','Parcial','Saldado')),
        Notas           NVARCHAR(1000)          NULL,
        FechaCreacion   DATETIME            NOT NULL    DEFAULT GETDATE(),
        UsuarioCreacion INT                 NOT NULL,
        CONSTRAINT PK_Evento PRIMARY KEY (EventoId),
        CONSTRAINT FK_Evento_Cliente  FOREIGN KEY (ClienteId)  REFERENCES Cliente(ClienteId),
        CONSTRAINT FK_Evento_Paquete  FOREIGN KEY (PaqueteId)  REFERENCES Paquete(PaqueteId),
        CONSTRAINT FK_Evento_Usuario  FOREIGN KEY (UsuarioCreacion) REFERENCES Usuario(UsuarioId),
        CONSTRAINT CK_Evento_Personas CHECK (NumPersonas > 0),
        CONSTRAINT CK_Evento_Monto    CHECK (MontoTotal >= 0),
        CONSTRAINT CK_Evento_Pagado   CHECK (MontoPagado >= 0)
    );
    PRINT 'Tabla Evento creada.';
END
GO

-- ================================================================
-- 6. TABLA: EventoServicio
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EventoServicio')
BEGIN
    CREATE TABLE EventoServicio (
        EventoServicioId    INT IDENTITY(1,1)   NOT NULL,
        EventoId            INT                 NOT NULL,
        ServicioId          INT                 NOT NULL,
        Cantidad            INT                 NOT NULL    DEFAULT 1,
        PrecioAcordado      DECIMAL(10,2)       NOT NULL    DEFAULT 0,
        CONSTRAINT PK_EventoServicio PRIMARY KEY (EventoServicioId),
        CONSTRAINT FK_EventoServicio_Evento   FOREIGN KEY (EventoId)   REFERENCES Evento(EventoId),
        CONSTRAINT FK_EventoServicio_Servicio FOREIGN KEY (ServicioId) REFERENCES Servicio(ServicioId),
        CONSTRAINT CK_EventoServicio_Cantidad CHECK (Cantidad > 0),
        CONSTRAINT CK_EventoServicio_Precio   CHECK (PrecioAcordado >= 0)
    );
    PRINT 'Tabla EventoServicio creada.';
END
GO

-- ================================================================
-- 7. TABLA: Cotizacion
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cotizacion')
BEGIN
    CREATE TABLE Cotizacion (
        CotizacionId        INT IDENTITY(1,1)   NOT NULL,
        EventoId            INT                 NOT NULL,
        Total               DECIMAL(10,2)       NOT NULL    DEFAULT 0,
        Estado              NVARCHAR(20)        NOT NULL    DEFAULT 'Borrador'
            CONSTRAINT CK_Cotizacion_Estado CHECK (Estado IN (
                'Borrador','Enviada','Aceptada','Rechazada')),
        MotivoRechazo       NVARCHAR(500)           NULL,
        FechaCreacion       DATETIME            NOT NULL    DEFAULT GETDATE(),
        FechaVencimiento    DATETIME                NULL,
        RutaPDF             NVARCHAR(500)           NULL,
        UsuarioCreacion     INT                 NOT NULL,
        CONSTRAINT PK_Cotizacion PRIMARY KEY (CotizacionId),
        CONSTRAINT FK_Cotizacion_Evento  FOREIGN KEY (EventoId)         REFERENCES Evento(EventoId),
        CONSTRAINT FK_Cotizacion_Usuario FOREIGN KEY (UsuarioCreacion)  REFERENCES Usuario(UsuarioId),
        CONSTRAINT CK_Cotizacion_Total CHECK (Total >= 0)
    );
    PRINT 'Tabla Cotizacion creada.';
END
GO

-- ================================================================
-- 8. TABLA: Pago
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pago')
BEGIN
    CREATE TABLE Pago (
        PagoId          INT IDENTITY(1,1)   NOT NULL,
        EventoId        INT                 NOT NULL,
        TipoPago        NVARCHAR(20)        NOT NULL
            CONSTRAINT CK_Pago_Tipo CHECK (TipoPago IN ('Anticipo','Saldo','Abono')),
        Monto           DECIMAL(10,2)       NOT NULL,
        MetodoPago      NVARCHAR(30)        NOT NULL
            CONSTRAINT CK_Pago_Metodo CHECK (MetodoPago IN ('SINPE','Transferencia','Efectivo')),
        FechaPago       DATETIME            NOT NULL    DEFAULT GETDATE(),
        UrlComprobante  NVARCHAR(500)           NULL,
        Observacion     NVARCHAR(300)           NULL,
        UsuarioId       INT                 NOT NULL,
        CONSTRAINT PK_Pago PRIMARY KEY (PagoId),
        CONSTRAINT FK_Pago_Evento  FOREIGN KEY (EventoId)  REFERENCES Evento(EventoId),
        CONSTRAINT FK_Pago_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(UsuarioId),
        CONSTRAINT CK_Pago_Monto CHECK (Monto > 0)
    );
    PRINT 'Tabla Pago creada.';
END
GO

-- ================================================================
-- 9. TABLA: Auditoria
-- ================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Auditoria')
BEGIN
    CREATE TABLE Auditoria (
        AuditoriaId     INT IDENTITY(1,1)   NOT NULL,
        UsuarioId       INT                     NULL,
        Tabla           NVARCHAR(60)        NOT NULL,
        Accion          NVARCHAR(30)        NOT NULL
            CONSTRAINT CK_Auditoria_Accion CHECK (Accion IN (
                'INSERT','UPDATE','DELETE','LOGIN','LOGOUT','ACCESS')),
        RegistroId      INT                     NULL,
        ValorAnterior   NVARCHAR(MAX)           NULL,
        ValorNuevo      NVARCHAR(MAX)           NULL,
        FechaHora       DATETIME            NOT NULL    DEFAULT GETDATE(),
        DireccionIP     NVARCHAR(45)            NULL,
        CONSTRAINT PK_Auditoria PRIMARY KEY (AuditoriaId),
        CONSTRAINT FK_Auditoria_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(UsuarioId)
    );
    PRINT 'Tabla Auditoria creada.';
END
GO


