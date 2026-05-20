
-- ================================================================
-- STORED PROCEDURES
-- ================================================================

-- SP: Verificar disponibilidad de fecha
IF OBJECT_ID('SP_VerificarDisponibilidad','P') IS NOT NULL DROP PROCEDURE SP_VerificarDisponibilidad;
GO
CREATE PROCEDURE SP_VerificarDisponibilidad
    @FechaEvento     DATE,
    @EventoIdExcluir INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Disponible BIT = 1;
    IF EXISTS (
        SELECT 1 FROM Evento
        WHERE FechaEvento = @FechaEvento
          AND Estado NOT IN ('Cancelada')
          AND (@EventoIdExcluir IS NULL OR EventoId <> @EventoIdExcluir)
    ) SET @Disponible = 0;
    SELECT @Disponible AS Disponible;
END;
GO
PRINT 'SP SP_VerificarDisponibilidad creado.';

-- SP: Dashboard principal
IF OBJECT_ID('SP_Dashboard','P') IS NOT NULL DROP PROCEDURE SP_Dashboard;
GO
CREATE PROCEDURE SP_Dashboard
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Mes  INT = MONTH(GETDATE());
    DECLARE @Anio INT = YEAR(GETDATE());

    SELECT COUNT(*) AS EventosMes
    FROM Evento
    WHERE MONTH(FechaEvento)=@Mes AND YEAR(FechaEvento)=@Anio AND Estado NOT IN ('Cancelada');

    SELECT ISNULL(SUM(P.Monto),0) AS IngresosMes
    FROM Pago P
    INNER JOIN Evento E ON P.EventoId = E.EventoId
    WHERE MONTH(P.FechaPago)=@Mes AND YEAR(P.FechaPago)=@Anio;

    SELECT COUNT(DISTINCT FechaEvento) AS DiasBloqueados,
           DAY(EOMONTH(GETDATE()))     AS DiasMes
    FROM Evento
    WHERE MONTH(FechaEvento)=@Mes AND YEAR(FechaEvento)=@Anio AND Estado NOT IN ('Cancelada');

    SELECT TOP 5
        E.EventoId, C.Nombre AS Cliente, E.TipoEvento,
        E.FechaEvento, E.Estado, E.SaldoPendiente
    FROM Evento E
    INNER JOIN Cliente C ON E.ClienteId = C.ClienteId
    WHERE E.FechaEvento >= CAST(GETDATE() AS DATE)
      AND E.Estado NOT IN ('Cancelada','Realizada')
    ORDER BY E.FechaEvento ASC;

    SELECT E.EventoId, C.Nombre AS Cliente, E.FechaEvento, E.SaldoPendiente
    FROM Evento E
    INNER JOIN Cliente C ON E.ClienteId = C.ClienteId
    WHERE E.FechaEvento BETWEEN CAST(GETDATE() AS DATE) AND DATEADD(DAY,7,CAST(GETDATE() AS DATE))
      AND E.EstadoPago <> 'Saldado'
      AND E.Estado NOT IN ('Cancelada');
END;
GO
PRINT 'SP SP_Dashboard creado.';

-- SP: Reporte de ingresos por período
IF OBJECT_ID('SP_ReporteIngresos','P') IS NOT NULL DROP PROCEDURE SP_ReporteIngresos;
GO
CREATE PROCEDURE SP_ReporteIngresos
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT E.TipoEvento, COUNT(E.EventoId) AS CantidadEventos,
           ISNULL(SUM(P.Monto),0) AS TotalIngresado
    FROM Evento E
    LEFT JOIN Pago P ON E.EventoId = P.EventoId
                     AND CAST(P.FechaPago AS DATE) BETWEEN @FechaInicio AND @FechaFin
    WHERE E.FechaEvento BETWEEN @FechaInicio AND @FechaFin AND E.Estado NOT IN ('Cancelada')
    GROUP BY E.TipoEvento ORDER BY TotalIngresado DESC;

    SELECT P.PagoId, C.Nombre AS Cliente, E.TipoEvento, E.FechaEvento,
           P.TipoPago, P.MetodoPago, P.Monto, P.FechaPago
    FROM Pago P
    INNER JOIN Evento E  ON P.EventoId  = E.EventoId
    INNER JOIN Cliente C ON E.ClienteId = C.ClienteId
    WHERE CAST(P.FechaPago AS DATE) BETWEEN @FechaInicio AND @FechaFin
    ORDER BY P.FechaPago DESC;
END;
GO
PRINT 'SP SP_ReporteIngresos creado.';

-- SP: Reporte de ocupación mensual
IF OBJECT_ID('SP_ReporteOcupacion','P') IS NOT NULL DROP PROCEDURE SP_ReporteOcupacion;
GO
CREATE PROCEDURE SP_ReporteOcupacion
    @Anio INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @Anio IS NULL SET @Anio = YEAR(GETDATE());
    SELECT MONTH(FechaEvento) AS Mes,
           DATENAME(MONTH, DATEFROMPARTS(@Anio, MONTH(FechaEvento), 1)) AS NombreMes,
           COUNT(EventoId) AS TotalEventos,
           COUNT(DISTINCT FechaEvento) AS DiasBloqueados
    FROM Evento
    WHERE YEAR(FechaEvento)=@Anio AND Estado NOT IN ('Cancelada')
    GROUP BY MONTH(FechaEvento)
    ORDER BY MONTH(FechaEvento);
END;
GO
PRINT 'SP SP_ReporteOcupacion creado.';

-- SP: Reporte de servicios más contratados
IF OBJECT_ID('SP_ReporteServicios','P') IS NOT NULL DROP PROCEDURE SP_ReporteServicios;
GO
CREATE PROCEDURE SP_ReporteServicios
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT S.Nombre AS Servicio, S.Categoria,
           COUNT(ES.EventoServicioId)                      AS VecesContratado,
           ISNULL(SUM(ES.Cantidad),0)                      AS UnidadesTotales,
           ISNULL(SUM(ES.PrecioAcordado * ES.Cantidad),0)  AS IngresoGenerado
    FROM EventoServicio ES
    INNER JOIN Servicio S ON ES.ServicioId = S.ServicioId
    INNER JOIN Evento E   ON ES.EventoId   = E.EventoId
    WHERE E.FechaEvento BETWEEN @FechaInicio AND @FechaFin
      AND E.Estado NOT IN ('Cancelada')
    GROUP BY S.Nombre, S.Categoria
    ORDER BY VecesContratado DESC;
END;
GO
PRINT 'SP SP_ReporteServicios creado.';

