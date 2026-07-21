CREATE TABLE Contrato
(
    ContratoId INT IDENTITY(1,1) PRIMARY KEY,

    CotizacionId INT NOT NULL UNIQUE,

    NumeroContrato VARCHAR(30) NOT NULL,

    FechaContrato DATETIME NOT NULL
        CONSTRAINT DF_Contrato_FechaContrato DEFAULT(GETDATE()),

    Estado VARCHAR(20) NOT NULL
        CONSTRAINT DF_Contrato_Estado DEFAULT('Pendiente'),

    Observaciones VARCHAR(1000) NULL,

    RutaPDF VARCHAR(500) NULL,

    UsuarioCreacion INT NOT NULL,

    CONSTRAINT UQ_Contrato_NumeroContrato
        UNIQUE (NumeroContrato),

    CONSTRAINT FK_Contrato_Cotizacion
        FOREIGN KEY (CotizacionId)
        REFERENCES Cotizacion(CotizacionId),

    CONSTRAINT FK_Contrato_Usuario
        FOREIGN KEY (UsuarioCreacion)
        REFERENCES Usuario(UsuarioId)
);