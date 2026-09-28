# Compilar desde código

Necesitas un SDK de .NET con soporte para `net6.0`, el juego con MelonLoader inicializado y ModData instalado. Los proyectos usan las referencias de tu instalación; no incluyen DLL del juego.

Desde la raíz del repositorio, en PowerShell:

```powershell
./scripts/build.ps1 -GameDir 'C:\Program Files (x86)\Steam\steamapps\common\TheLongDark'
```

El script compila los tres proyectos en `artifacts/Tipi`, `artifacts/TipiCuringRack` y `artifacts/TipiWorkbench`. No instala ni modifica el juego. Para instalar, copia únicamente las DLL de los mods junto con los recursos de `release/Mods`.

`modcomponent/Editor` conserva el constructor histórico de recursos. El `.modcomponent` distribuido en `release/Mods` es el paquete usado con esta versión e incluye el catálogo y los bundles necesarios. El script de .NET no reconstruye esos recursos de Unity.
