USE SIGESA


CREATE TABLE TokenRecuperacion (
    TokenRecuperacionId INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId           INT NOT NULL,
    Token               NVARCHAR(128) NOT NULL,
    Expiracion          DATETIME2 NOT NULL,
    Usado               BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_TokenRecuperacion_Usuario
        FOREIGN KEY (UsuarioId) REFERENCES Usuario(UsuarioId)
        ON DELETE CASCADE
);

CREATE INDEX IX_TokenRecuperacion_Token
    ON TokenRecuperacion (Token)
    WHERE Usado = 0;



/* 
TOKEN DE SEGURIDAD DE DOS PASOS SIGESA   *crqv zqzq mkhy vyeu
Cuenta de Gmail
Correo:sigesa.eventos@gmail.com
Contrasena:Sigesa2026!Eventos  

/*

   