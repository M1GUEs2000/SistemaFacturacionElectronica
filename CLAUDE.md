# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A multi-project C# invoicing system (Sistema de Facturación) for Ecuador's SRI electronic invoicing. Covers the full invoice lifecycle: creation, XML generation, digital signing (XADES), SRI authorization, and PDF generation.

## Build & Run

Open `SistemaFacturacion.sln` in Visual Studio 2022 and build (F6 or `msbuild SistemaFacturacion.sln`).

- **Desktop app**: Set `SistemaFacturacion` as startup project, run with F5. Launches `frmLogin`.
- **Web API**: Set `Facturacion.api` as startup project; runs on IIS Express (port 49388 / SSL 44374).
- **No test projects** exist in this solution.

## Database configuration

Default is Microsoft Access. Connection string is read from `ConexionSQL.dat` in the app's base directory. Switch to SQL Server via environment variables:

```
DB_PROVIDER=SQL
DB_SERVER=localhost
DB_DATABASE=...
DB_USER=...
DB_PASSWORD=...
DB_TRUST_SERVER_CERTIFICATE=True   # optional, defaults to True
```

The Access `.accdb` file is at `BaseDatos/FACTURACION.accdb`.

## Project structure

| Project | Role |
|---|---|
| `SistemaFacturacion` | WinForms desktop UI (~61 forms) |
| `LogicaNegocios` | All business logic — Manejadores, Procesos, AppServices |
| `AccesoDatos` | Data access; dual OleDb/SqlClient via `ConexionBD` |
| `AccesoDatosWeb` | .NET Standard 2.0 data access for the web layer |
| `Facturacion.api` | ASP.NET MVC 5 / WebAPI — REST endpoints for the same domain |
| `Firmador.xml` | XADES digital signature of XML invoices (uses BouncyCastle + MITyC) |
| `GenerarXml` | SRI-compliant XML document generation |

## Architecture

**Dependency direction**: UI → `AppServices` → Manejadores/Procesos → `IConexionBD`

`AppServices` (`LogicaNegocios/Services/AppServices.cs`) is the single composition root. It owns every manager and process instance and is passed by constructor throughout the app. The desktop and API layers each construct it independently via `AppServicesFactory` (API) or `Program.Main` (desktop).

**Managers** (`LogicaNegocios/*Manejador.cs`): CRUD + business rules per domain entity (Cliente, Empresa, Facturacion, Producto, Proveedor, Retencion, NotasCredito, FormaPago, Parametros, Login, Log).

**Processes** (`LogicaNegocios/Procesos/`): Orchestrate multi-step workflows — XML generation → signing → SRI submission → authorization polling → PDF output:
- `ProcesosFacturacion` — invoice workflow
- `ProcesosNotaCredito` — credit note workflow
- `ProcesosRetenciones` — withholding workflow
- `ProcesosGenerales` — shared helpers
- `ProcesosLote` — batch processing
- `FacturacionQueueAsync` — async queue for background processing

**File outputs** are organized under `FACTURACION/` beside the executable:
```
FACTURACION/
├── GENERAL/FIRMAELECTRONICA/   # .p12 certificate
├── GENERAL/LOGOFACTURA/        # logo image
├── FACTURAS/XML|XMLFIRMADOS|XMLAUTORIZADOS|PDF|PDFPREVIEW
├── NOTASCREDITO/...
└── RETENCIONES/...
```

## IVA — tarifa universal

`AppServices.TarifaIva` (`decimal`) es la única fuente de verdad para la tarifa de IVA vigente. Se carga una sola vez con `AppServices.CargarTarifaIva(nombreEmpresa)` al iniciar la app. **No crear campos locales `_tarifaIva` en los formularios** — usar siempre `_services.TarifaIva`.

### Cómo fluye el valor en `frmProductos`
- La BD almacena `VALOR` **sin IVA**.
- Al seleccionar un producto en la grid, `VALOR` se carga en `txtValorSinIVA` y se llama `RecalcularConIVADesdeSin()` para calcular `txtValor` (precio con IVA).
- Al guardar (insertar/actualizar) se envía `txtValorSinIVA` a la BD.

---

## API layer (`Facturacion.api`)

Controllers mirror the desktop managers: `FacturasController`, `ClientesController`, `ProductosController`, `RetencionesController`, `NotasCreditoController`, `EmpresasController`, `LoginController`. Models/DTOs live in `Models/`, transformation in `Mappers/`, service interfaces in `Servicios/`. `AppServicesFactory` constructs `AppServices` on each request.

## Cerebro del proyecto

> ⚠️ **OBLIGATORIO leer el vault PRIMERO.** Todo lo que no sea código técnico vive ahí: estado actual, pendientes, vulnerabilidades, decisiones de arquitectura, análisis y contexto de negocio. **No re-derivar esa información leyendo código — es un gasto de tokens innecesario.**

### Archivos de entrada obligatorios (leer en este orden)

| Archivo | Qué contiene |
|---|---|
| `d:\Obsidian\Bovedá\proyectos\sistema-facturacion\README.md` | Estado actual, decisiones clave, stack |
| `d:\Obsidian\Bovedá\proyectos\sistema-facturacion\arquitectura-sistema-facturacion.md` | Mapa de módulos y qué nodos están ✅ vs ⚠️ |
| `d:\Obsidian\Bovedá\proyectos\sistema-facturacion\tareas.md` | Tareas pendientes (P-XXX / CU-XXXXX) |
| `d:\Obsidian\Bovedá\CLAUDE.md` | Convenciones del vault — leer solo si hay dudas de estructura |

**Flujo de sesión:**
1. Leer `README.md` → estado actual
2. Leer `arquitectura-sistema-facturacion.md` → qué nodos están ✅ completo vs ⚠️ pendiente
3. Ir al nodo `nodos/[modulo].md` si está ✅ — confiar en él, no leer código fuente
4. Si el nodo está ⚠️ → leer código fuente y documentar el nodo al terminar
5. Al cerrar sesión → actualizar `## 📌 Estado actual` en README

### Post-commit — mantener nodos sincronizados

Después de cada `git push`, GitHub Actions postea un comentario en el commit listando qué nodos del vault pueden estar desactualizados. **Revisar ese comentario y actualizar los nodos afectados en Obsidian antes de cerrar la sesión.** Ver `.github/node-map.yml` para el mapeo completo de archivos → nodos.

## Reglas de código (OBLIGATORIAS)

### Manejo de errores — nunca catch vacío

Todo `catch` debe registrar el error con contexto suficiente para diagnosticarlo:

```csharp
// ❌ NUNCA
catch (Exception) { }
catch (Exception ex) { }

// ✅ SIEMPRE — mínimo log con contexto
catch (Exception ex)
{
    _log.CrearLog($"Error en [NombreMetodo]: {ex.Message}", usuario, ip, sql);
    throw; // o manejar según corresponda
}
```

- Si el método tiene acceso a `_log`: usar `_log.CrearLog()`
- Si es en `Global.asax` o capa API: escribir a `App_Data/error.log` con `DateTime` + mensaje + `ex.ToString()`
- Si es un error esperado y recuperable: loguear como advertencia y continuar
- **Nunca** silenciar una excepción con `catch {}` vacío — si no sabés qué hacer con el error, al menos hacé `throw`

---

### Seguridad — directivas por defecto

Aplicar en cada línea de código nueva, no como revisión posterior:

**SQL — siempre parametrizado:**
```csharp
// ❌ NUNCA concatenar input del usuario
string sql = "SELECT * FROM TABLA WHERE CAMPO = '" + valor + "'";

// ✅ SIEMPRE parámetros
string sql = "SELECT * FROM TABLA WHERE CAMPO = ?";  // OleDb
string sql = "SELECT * FROM TABLA WHERE CAMPO = @val"; // SqlClient
```

**Input externo — siempre validar en el boundary:**
- Nombres de archivo: siempre `Path.GetFileName()` + verificar que el path resuelto esté dentro del directorio esperado
- Datos de usuario: validar longitud y formato antes de usar

**Credenciales — nunca en código:**
- Toda credencial va en `secrets.config` (API) o variable de entorno — nunca hardcodeada ni en `Web.config` base
- Nunca devolver campos de contraseña en respuestas API

**Nota:** el agente `/cyber-neo` hace auditoría completa. Estas directivas son para prevenir vulnerabilidades en código nuevo antes de que lleguen a auditoría.

---

## Documentación del proyecto

| Archivo | Contenido |
|---------|-----------|
| [`funcionamientosri.md`](funcionamientosri.md) | Flujo completo SRI: ambientes, clave de acceso, estados, SOAP, tablas, archivos generados |
| [`ride.md`](ride.md) | Generación de PDF (RIDE): estructura, helpers, IVA codes, rutas de salida |

---

## Key dependencies

- **BouncyCastle + MITyC**: XADES XML digital signatures
- **IKVM**: Java-interop bridge (used by signing libraries)
- **iTextSharp**: PDF generation
- **Microsoft ReportViewer 15.0**: RDLC reports in WinForms
- **EntityFramework**: used in `AccesoDatosWeb`
- **Azure.Identity / MSAL**: optional cloud identity (not core to the invoicing flow)
