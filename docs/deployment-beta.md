# Guía de Despliegue — Beta Interna ComercioFlow

**Versión:** 1.0  
**Etapa:** 36 — Beta Interna / Hardening MVP  
**Fecha:** 2026-05-27  

---

## Prerrequisitos

| Herramienta | Versión mínima | Verificar |
|-------------|----------------|-----------|
| .NET SDK | 10.0 | `dotnet --version` |
| Node.js | 22.x LTS | `node --version` |
| Docker Desktop | 4.x | `docker --version` |
| Git | 2.x | `git --version` |
| PowerShell | 5.1+ o pwsh 7+ | `$PSVersionTable` |

---

## 1. Clonar el Repositorio

```bash
git clone https://github.com/[org]/comercioflow.git
cd comercioflow
```

---

## 2. Infraestructura con Docker Compose

El archivo `docker-compose.yml` en la raíz levanta PostgreSQL y RabbitMQ.

### Iniciar servicios de infraestructura

```bash
docker compose up -d postgres rabbitmq
```

### Verificar que están corriendo

```bash
docker compose ps
```

Deberías ver `postgres` y `rabbitmq` en estado `running`.

### Puertos expuestos

| Servicio | Puerto local | Puerto interno |
|----------|-------------|----------------|
| PostgreSQL | `5432` | `5432` |
| RabbitMQ AMQP | `5672` | `5672` |
| RabbitMQ Management UI | `15672` | `15672` |

**RabbitMQ Management:** http://localhost:15672 (usuario: `guest`, contraseña: `guest`)

---

## 3. Variables de Entorno

### Backend API (`backend/src/Api`)

Crear el archivo `backend/src/Api/appsettings.Development.json` (ya existe como template):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=saascommerce_dev;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "GENERAR_SECRETO_ALEATORIO_MIN_32_CHARS",
    "Issuer": "SaasCommerce.Dev",
    "Audience": "SaasCommerce.Dev",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 30
  },
  "RabbitMq": {
    "UseInMemory": false,
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173"
    ]
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

> **Seguridad:** Generar el `Jwt:Secret` con al menos 32 caracteres aleatorios:
> ```powershell
> [System.Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
> ```

### Frontend (`frontend/`)

Crear `frontend/.env.local`:

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs/notifications
```

---

## 4. Migraciones de Base de Datos

Aplicar todas las migraciones antes del primer arranque:

```bash
cd backend
dotnet ef database update --project src/Modules/SaasCommerce.Modules.csproj --startup-project src/Api/SaasCommerce.Api.csproj
```

### Verificar migración exitosa

```bash
dotnet ef migrations list --project src/Modules/SaasCommerce.Modules.csproj --startup-project src/Api/SaasCommerce.Api.csproj
```

Debe mostrar todas las migraciones con estado `[in database]`.

---

## 5. Arranque de Servicios

### 5.1 Backend API

```bash
cd backend/src/Api
dotnet run --configuration Development
```

La API estará disponible en: `http://localhost:5000`  
Swagger UI: `http://localhost:5000/swagger`

### 5.2 Worker de Mensajería

En una segunda terminal:

```bash
cd backend/src/Worker
dotnet run --configuration Development
```

El worker procesa eventos de MassTransit (inventario, notificaciones, outbox).

### 5.3 Frontend

```bash
cd frontend
npm install
npm run dev
```

La aplicación estará disponible en: `http://localhost:5173`

---

## 6. Comandos de Verificación

### Health checks

```bash
# Liveness — debe retornar HTTP 200 con {"isSuccess":true,"data":"Live"}
curl http://localhost:5000/health/live

# Readiness — debe retornar HTTP 200 con status "Healthy"
curl http://localhost:5000/health/ready
```

Respuesta esperada de `/health/ready`:

```json
{
  "isSuccess": true,
  "data": {
    "status": "Healthy",
    "postgreSql": { "name": "PostgreSQL", "status": "Healthy" },
    "rabbitMq": { "name": "RabbitMQ", "status": "Healthy" },
    "outbox": { "name": "Outbox", "status": "Healthy" }
  }
}
```

### Ejecutar tests del backend

```bash
cd backend
dotnet test --no-restore -c Release
```

Todos los tests deben pasar (0 fallos).

### Linting del frontend

```bash
cd frontend
npm run lint
```

### Build de producción del frontend

```bash
cd frontend
npm run build
```

---

## 7. Configuración para Producción / Beta

Para entornos de producción o beta, usar variables de entorno del sistema operativo en lugar de archivos de configuración:

```bash
# Linux/macOS
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Host=prod-db;..."
export Jwt__Secret="..."

# Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ConnectionStrings__DefaultConnection = "Host=prod-db;..."
$env:Jwt__Secret = "..."
```

---

## 8. Análisis de Calidad con SonarQube

### Iniciar SonarQube con Docker

```bash
docker compose -f docker-compose.sonar.yml up -d
```

SonarQube UI: http://localhost:9000 (usuario: `admin`, contraseña: `admin`)

### Ejecutar análisis

```powershell
cd "D:\Dev\SAAS MVP\comercioflow"
powershell -ExecutionPolicy Bypass -File scripts/sonar.ps1
```

El análisis tarda ~5 minutos. Verificar Quality Gate en: http://localhost:9000/dashboard?id=SaasCommerce

**Criterios del Quality Gate:**
- `new_coverage ≥ 80%`
- `new_violations = 0`
- `new_duplicated_lines_density ≤ 3%`

---

## 9. Backup de PostgreSQL

### Backup manual

```bash
docker exec -t postgres pg_dump -U postgres saascommerce_dev > backup_$(date +%Y%m%d_%H%M%S).sql
```

### Restaurar backup

```bash
cat backup_20260601_080000.sql | docker exec -i postgres psql -U postgres saascommerce_dev
```

### Backup automático (cron Linux)

```bash
# Agregar a crontab: crontab -e
0 */6 * * * docker exec postgres pg_dump -U postgres saascommerce_dev > /backups/saascommerce_$(date +\%Y\%m\%d_\%H\%M\%S).sql
```

---

## 10. Troubleshooting

### Problema: API no conecta a PostgreSQL

1. Verificar que el contenedor está corriendo: `docker compose ps`
2. Revisar conexión: `docker exec -it postgres psql -U postgres -c "SELECT 1"`
3. Verificar string de conexión en `appsettings.Development.json`

### Problema: Worker no conecta a RabbitMQ

1. Verificar estado: `docker compose ps rabbitmq`
2. Acceder a Management UI: http://localhost:15672
3. Revisar configuración `RabbitMq:Host` y `RabbitMq:Port`

### Problema: Frontend no carga datos

1. Verificar que la API está corriendo en el puerto correcto
2. Revisar `VITE_API_BASE_URL` en `.env.local`
3. Verificar CORS: `Cors:AllowedOrigins` debe incluir `http://localhost:5173`

### Problema: Migraciones fallidas

```bash
# Ver migraciones pendientes
dotnet ef migrations list ...

# Revertir última migración
dotnet ef database update PreviousMigrationName ...

# Forzar estado
dotnet ef database update --force ...
```

---

## 11. Versión de la Aplicación

Verificar versión actual:

```bash
curl http://localhost:5000/api/version
```

Respuesta esperada:

```json
{
  "isSuccess": true,
  "data": {
    "name": "SaasCommerce RD",
    "api": "v1",
    "status": "BaseReady"
  }
}
```

---

*Última actualización: 2026-05-27*
