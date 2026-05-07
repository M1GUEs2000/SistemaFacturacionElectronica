# CLAUDE.md

> ⚠️ **El contexto principal vive en la bóveda — leer PRIMERO antes de tocar código.**

## Bóveda (leer en este orden)

| Archivo | Qué contiene |
|---|---|
| `d:\Obsidian\Bovedá\proyectos\sistema-facturacion\README.md` | Estado actual, decisiones clave |
| `d:\Obsidian\Bovedá\proyectos\sistema-facturacion\arquitectura-sistema-facturacion.md` | Módulos y nodos ✅/⚠️ |
| `d:\Obsidian\Bovedá\proyectos\sistema-facturacion\tareas.md` | Tareas pendientes (P-XXX / CU-XXXXX) |

**Flujo:** README → arquitectura → nodo del módulo (si ✅ confiar en él, no leer código). Si ⚠️ → leer código y documentar nodo al terminar.

Post-commit: GitHub Actions lista nodos desactualizados en el commit — actualizar antes de cerrar sesión.

## Stack

Sistema de facturación electrónica para el SRI de Ecuador. WinForms + ASP.NET MVC 5 WebAPI. Base de datos Microsoft Access por defecto; también soporta SQL Server (variable `DB_PROVIDER=SQL`).

| Proyecto | Rol |
|---|---|
| `SistemaFacturacion` | WinForms (~61 forms) |
| `LogicaNegocios` | Toda la lógica: Manejadores (CRUD por entidad), Procesos (flujos XML→firma→SRI→PDF), AppServices |
| `AccesoDatos` | OleDb/SqlClient dual vía `ConexionBD` |
| `Facturacion.api` | API REST — controllers espejo de los Manejadores |
| `Firmador.xml` / `GenerarXml` | Firma XADES y generación XML SRI |

**`AppServices`** (`LogicaNegocios/Services/AppServices.cs`) es el composition root único — contiene todas las instancias de Manejadores y Procesos, se construye una vez en `Program.Main` (desktop) o por request vía `AppServicesFactory` (API).

`AppServices.TarifaIva` es la única fuente de verdad para IVA — no crear campos locales en formularios.

## Reglas de código

- **Catch vacío prohibido** — siempre `_log.CrearLog(...)` con contexto + `throw`
- **SQL siempre parametrizado** — `?` (OleDb) o `@param` (SqlClient), nunca concatenar input
- **Credenciales en `secrets.config`** o variable de entorno, nunca hardcodeadas

## Docs locales

| Archivo | Contenido |
|---|---|
| [`funcionamientosri.md`](funcionamientosri.md) | Flujo SRI completo |
| [`ride.md`](ride.md) | Generación PDF (RIDE) |
