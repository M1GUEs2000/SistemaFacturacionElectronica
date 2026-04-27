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

Todo el conocimiento, análisis, vulnerabilidades y decisiones de este proyecto viven en el vault de Obsidian:

**Vault:** `d:\Obsidian\Bovedá\`

Este CLAUDE.md solo contiene contexto técnico de código (build, arquitectura, dependencias). Para análisis, contexto y conocimiento del proyecto → ir al vault.

### Cómo navegar el vault

El vault tiene su propio `CLAUDE.md` que explica su estructura completa — leerlo antes de buscar cualquier nodo. Contiene: carpetas del vault, convenciones de nombres, y la estrategia de navegación de dos niveles (mapa de proyecto → nodos de detalle en `research/`).

**NUNCA asumir la carpeta de un nodo** — listar el vault completo primero si no se sabe dónde está.

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
