# Branch Miguel

Guía para crear la branch `Miguel`, trabajar en ella y llevar los cambios a `main`.

Los comandos funcionan en Git Bash y en PowerShell.

## 1. Obtener el proyecto

Si todavía no tienes el repositorio en tu PC:

```bash
git clone https://github.com/M1GUEs2000/SistemaFacturacionElectronica.git
cd SistemaFacturacionElectronica
```

Si ya lo tienes, entra a la carpeta y sigue con el paso 2.

> Un clon nuevo no trae los archivos ignorados, como `secrets.config`, las firmas `.p12` o las bases `.accdb`. Hay que copiarlos a mano. Para `secrets.config` usa como base `Facturacion.api/secrets.config.example`.

## 2. Crear la branch desde `main` actualizado

```bash
git switch main
git pull origin main
git switch -c Miguel
git push -u origin Miguel
```

Si la branch `Miguel` ya existe en GitHub, no la crees de nuevo:

```bash
git fetch origin
git switch Miguel
git pull origin Miguel
```

## 3. Guardar los cambios

Revisa qué cambió antes de agregarlo:

```bash
git status
git add ruta/del/archivo.cs
git commit -m "feat(modulo): descripcion corta del cambio"
git push
```

Antes de cada commit, confirma que `git status` no incluya datos de clientes, firmas electrónicas, bases de datos ni credenciales. El repositorio es público.

## 4. Traer lo último de `main` a `Miguel`

Hazlo antes de hacer el merge, para resolver los conflictos en tu branch y no en `main`:

```bash
git switch Miguel
git fetch origin
git merge origin/main
```

Si hay conflictos:

1. Abre los archivos que marca `git status` y deja el contenido correcto.
2. Quita las marcas `<<<<<<<`, `=======` y `>>>>>>>`.
3. Termina el merge:

```bash
git add ruta/del/archivo.cs
git commit
```

Si prefieres cancelar el merge y volver a como estabas:

```bash
git merge --abort
```

Cuando esté listo, compila, prueba y sube la branch:

```bash
git push
```

## 5. Hacer el merge a `main`

### Opción recomendada: Pull Request en GitHub

1. Abre <https://github.com/M1GUEs2000/SistemaFacturacionElectronica/compare/main...Miguel>.
2. Pulsa **Create pull request**.
3. Revisa la pestaña **Files changed**.
4. Pulsa **Merge pull request** y luego **Confirm merge**.

### Opción por consola

```bash
git switch main
git pull origin main
git merge --no-ff Miguel
git push origin main
```

## 6. Seguir trabajando después del merge

Actualiza tu copia y deja `Miguel` al día con `main`:

```bash
git switch main
git pull origin main
git switch Miguel
git merge main
git push
```

## Reglas

- No uses `git push --force` en `main`.
- No trabajes directamente en `main`: usa siempre la branch `Miguel`.
- No hagas el merge a `main` sin compilar y probar antes.
