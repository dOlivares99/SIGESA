# SIGESA — Sistema de Gestión de Sala de Eventos Mónica

Sistema web administrativo desarrollado para la gestión integral de eventos, cotizaciones, pagos y clientes de **Sala de Eventos Mónica**. Proyecto universitario desarrollado como parte del curso de Ingeniería de Software en la **Universidad Fidélitas**, Costa Rica.

---

## 🛠️ Tecnologías utilizadas

| Capa | Tecnología |
|---|---|
| Frontend | ASP.NET Core MVC, Razor Views, Bootstrap 5, Tabler Icons |
| Backend API | ASP.NET Core Web API |
| ORM | Entity Framework Core |
| Base de datos | SQL Server 2022 / Azure SQL Database |
| Autenticación | Sesiones ASP.NET Core |
| Estilos | CSS personalizado + Bootstrap 5 |
| Control de versiones | Git / GitHub |
| Hospedaje | Azure for Students |

---

## 🏗️ Arquitectura

El proyecto sigue una **arquitectura de 4 capas**:
La capa WEB se comunica con la API a través de la interfaz `IRestProvider`, que encapsula todas las llamadas HTTP (GET, POST, PUT, PATCH, DELETE).

---

## 📦 Módulos implementados

### Sprint 1
- **Autenticación** — Login y logout con sesiones y hash seguro de contraseñas

### Sprint 2
- **Dashboard** — Panel principal con resumen del sistema
- **Usuarios** — Gestión de usuarios con roles (Administrador / Asistente)
- **Clientes** — Registro, edición e historial de eventos por cliente
- **Paquetes** — Catálogo de paquetes con activación/desactivación
- **Servicios** — Catálogo de servicios adicionales agrupados por categoría

### Próximos sprints
- Reservas y eventos
- Cotizaciones con generación de PDF
- Registro y control de pagos
- Reportes y auditoría

---

## ⚙️ Configuración del proyecto

### Requisitos previos
- .NET 8 SDK
- SQL Server 2022 o Azure SQL Database
- Visual Studio 2022 o VS Code

### Pasos para correr el proyecto

1. Clona el repositorio:
```bash
   git clone https://github.com/tu-usuario/SIGESA.git
```

2. Ejecuta el script de base de datos ubicado en `/Database/SIGESA_Script.sql` sobre tu instancia de SQL Server.

3. Ejecuta el script de datos iniciales `/Database/SIGESA_Seed.sql` para crear roles y usuario administrador.

4. Configura la cadena de conexión en `API/appsettings.json`:
```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=TU_SERVIDOR;Database=SIGESA;..."
     }
   }
```

5. Configura la URL de la API en `WEB/appsettings.json`:
```json
   {
     "ApiSettings": {
       "BaseUrl": "http://localhost:5119/api"
     }
   }
```

6. Corre ambos proyectos (API y WEB) simultáneamente:
```bash
   # Terminal 1
   cd API && dotnet run

   # Terminal 2
   cd WEB && dotnet run
```

7. Accede al sistema en `http://localhost:PUERTO` con las credenciales:
   - **Correo:** `admin@sigesa.com`
   - **Contraseña:** `Admin1234`

---

## 👥 Equipo de desarrollo

Proyecto desarrollado por estudiantes de Ingeniería en Sistemas de la Universidad Fidélitas como parte del Trabajo Comunal Universitario (TCU) en beneficio de Sala de Eventos Mónica.

---

## 📄 Licencia

Proyecto académico — Universidad Fidélitas, Costa Rica. Todos los derechos reservados.
