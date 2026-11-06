; Instalador limpio de Facturacion (instalacion nueva, generica).
; Requisito previo: compilar SistemaFacturacion en modo Release antes de correr este script
; (Visual Studio > Configuration Manager > Release > Build), para que exista
; SistemaFacturacion\bin\Release\SistemaFacturacion.exe
;
; Aislamiento de versiones: este instalador usa un AppId propio (distinto del de
; cualquier version anterior, p.ej. la de Rodolfo) y una carpeta + grupo de inicio
; que incluyen el numero de version. Asi, instalar esta version en una maquina que
; ya tiene otra version de Facturacion NO la toca: no comparte carpeta, no comparte
; registro de Windows, no la desinstala ni pisa su base de datos. Quedan como dos
; programas totalmente independientes en "Agregar o quitar programas".
; Si generas una build nueva mas adelante, solo cambia MyAppVersion (y si quieres
; que coexista con esta build, no reutilices el mismo AppId).
#define MyAppVersion "1.1.1"
[Setup]
AppId={{6493B464-AB51-49D0-9884-CBC9F6FBAAD7}
AppName=Facturacion
AppVersion={#MyAppVersion}
AppVerName=Facturacion {#MyAppVersion}
AppPublisher=SuSistema
DefaultDirName=C:\Facturacion-{#MyAppVersion}
DefaultGroupName=Facturacion {#MyAppVersion}
OutputDir=Output
OutputBaseFilename=facturacion-instalador-{#MyAppVersion}
Compression=lzma2/normal
SolidCompression=yes
PrivilegesRequired=admin
UsePreviousAppDir=no
CloseApplications=yes
RestartApplications=no
; La app compila AnyCPU/Prefer32Bit=false, o sea corre en 64 bits en Windows de 64 bits
; y necesita el Microsoft.ACE.OLEDB.16.0 de 64 bits. Sin estas dos lineas, Setup.exe corre
; como proceso de 32 bits y el chequeo de registro (mas abajo, en [Code]) mira la vista de
; registro de 32 bits (WOW6432Node) en vez de la de 64 bits: puede dar un falso "ya esta
; instalado" (si hay algun componente de 32 bits viejo) y entonces NUNCA instala el
; redistribuible de 64 bits que la app realmente necesita. Con esto, Setup.exe corre nativo
; en 64 bits y el chequeo/instalacion usan la vista de registro correcta.
; NOTA: esto asume que la maquina destino tiene Office/ACE de 64 bits (o nada instalado).
; Si el cliente tiene Office de 32 bits (comun en PCs de negocio mas viejas), este
; instalador de 64 bits va a fallar igual que le paso a Rodolfo - ver conversacion previa.
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile="D:\Proyecto\Proyecto SuSistema\Facturacion\Icono Aplicacion.ico"
UninstallDisplayIcon={app}\SistemaFacturacion.exe
[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
[Files]
; ConexionSQL.dat NO se copia desde bin\Release: ese archivo es el de esta maquina de
; desarrollo y trae hardcodeada la ruta local D:\Proyecto\...\BaseDatos\FACTURACION.accdb.
; Copiarlo tal cual rompe la conexion en cualquier otra maquina. Se genera en [Code]
; (ssPostInstall) apuntando a {app}\FACTURACION.accdb, solo si no existe ya uno (para no
; pisar la config real de un cliente en una reinstalacion/actualizacion).
; FACTURACION\* se excluye: es la carpeta de datos que la app escribe en tiempo de
; ejecucion (certificados .p12 de firma electronica, logos, etc. del cliente activo
; en esta maquina de dev - por ejemplo, el certificado real de Rodolfo). Un instalador
; generico para un cliente nuevo no debe llevar el certificado/logo de otro cliente.
Source: "D:\Proyecto\Proyecto SuSistema\Facturacion\SistemaFacturacion\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb,ConexionSQL.dat,FACTURACION\*"
Source: "D:\Proyecto\Proyecto SuSistema\Facturacion\Icono Aplicacion.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Proyecto\Proyecto SuSistema\Facturacion\BaseDatos\FACTURACION.accdb"; DestDir: "{app}"; Flags: onlyifdoesntexist uninsneveruninstall
; Redistribuible que registra el proveedor OLE DB Microsoft.ACE.OLEDB.16.0 usado para
; conectar con el .accdb. Sin esto, la app falla en maquinas limpias (sin Office/Access).
; No se copia a {app}: solo se extrae a una carpeta temporal y se instala si hace falta (ver [Code]).
Source: "Redist\AccessDatabaseEngine_X64.exe"; DestDir: "{tmp}"; Flags: dontcopy
[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el Escritorio"; GroupDescription: "Iconos adicionales:"
[Icons]
Name: "{group}\Facturacion"; Filename: "{app}\SistemaFacturacion.exe"; IconFilename: "{app}\Icono Aplicacion.ico"
Name: "{group}\Desinstalar Facturacion"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Facturacion"; Filename: "{app}\SistemaFacturacion.exe"; IconFilename: "{app}\Icono Aplicacion.ico"; Tasks: desktopicon
[Run]
Filename: "{app}\SistemaFacturacion.exe"; Description: "Abrir Facturacion"; Flags: postinstall nowait skipifsilent
[Code]
function AceOleDb16Registrado: Boolean;
begin
  // La app pide especificamente Microsoft.ACE.OLEDB.16.0 (ver ConexionSQL.dat /
  // ConexionBD.cs). No aceptar .12.0 como equivalente: si solo esta esa version
  // (p.ej. de un Office viejo de 32 bits), la app igual falla con el mismo error.
  Result := RegKeyExists(HKEY_CLASSES_ROOT, 'Microsoft.ACE.OLEDB.16.0');
end;

procedure CrearConexionSQLSiHaceFalta;
var
  DestFile: String;
  Cadena: String;
begin
  DestFile := ExpandConstant('{app}\ConexionSQL.dat');
  if not FileExists(DestFile) then
  begin
    Cadena := 'Provider=Microsoft.ACE.OLEDB.16.0;Data Source=' + ExpandConstant('{app}') + '\FACTURACION.accdb;';
    SaveStringToFile(DestFile, Cadena, False);
  end;
end;

procedure InstalarAccessDatabaseEngineSiHaceFalta;
var
  RedistPath: String;
  ResultCode: Integer;
begin
  if AceOleDb16Registrado then
    exit;

  ExtractTemporaryFile('AccessDatabaseEngine_X64.exe');
  RedistPath := ExpandConstant('{tmp}\AccessDatabaseEngine_X64.exe');
  if Exec(RedistPath, '/quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
  begin
    if ResultCode = 3010 then
      MsgBox('Se instalo Microsoft Access Database Engine, pero Windows pide reiniciar '
        + 'para terminar de registrarlo. Reinicia la PC antes de abrir Facturacion.',
        mbInformation, MB_OK)
    else if ResultCode <> 0 then
      MsgBox('La instalacion de Microsoft Access Database Engine devolvio un error (codigo '
        + IntToStr(ResultCode) + '). Facturacion podria no poder conectarse a la base de datos '
        + 'hasta instalarlo manualmente desde https://www.microsoft.com/download/details.aspx?id=54920',
        mbError, MB_OK);
  end
  else
    MsgBox('No se pudo instalar Microsoft Access Database Engine automaticamente. '
      + 'Instalalo manualmente desde https://www.microsoft.com/download/details.aspx?id=54920 '
      + 'antes de usar Facturacion.', mbError, MB_OK);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    InstalarAccessDatabaseEngineSiHaceFalta;
    CrearConexionSQLSiHaceFalta;
  end;
end;
