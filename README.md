# Burebista Traditional Tipi · The Long Dark

Un tipi portátil con piel cosida, palos con materiales del juego y una fogata interior. Este repositorio reúne el mod principal y dos addons opcionales: un bastidor de curado y un banco de trabajo exterior con brasero y cocina.

| Componente | Versión | Descarga | Guía |
| --- | --- | --- | --- |
| Tipi tradicional | **1.2.3** | [ZIP del tipi](downloads/BurebistaTraditionalTipi-v1.2.3.zip?raw=true) | Instrucciones abajo |
| Bastidor de curado | **0.1.1** | [ZIP del bastidor](downloads/TipiCuringRack-v0.1.1.zip?raw=true) | [Uso del bastidor](addons/TipiCuringRack/README.md) |
| Banco y cocina | **0.3.4** | [ZIP del banco](downloads/TipiWorkbench-v0.3.4.zip?raw=true) | [Uso del banco](addons/TipiWorkbench/README.md) |

**[Descargar el tipi con los dos addons](downloads/BurebistaTipi-Pack-1.2.3-Rack-0.1.1-Workbench-0.3.4.zip?raw=true)**

Los ZIP contienen una carpeta `Mods` lista para copiar. Los addons aparecen al desplegar el tipi cuando sus DLL están instaladas; no tienen una receta de construcción independiente en estas versiones.

## Requisitos e instalación

- The Long Dark para Windows con MelonLoader y sus ensamblados IL2CPP generados.
- ModComponent y sus dependencias correspondientes a tu instalación, para el objeto empaquetado y su receta.
- ModData, para el guardado del tipi.
- Los addons requieren el mod principal. No funcionan de forma independiente.

1. Cierra el juego.
2. Descarga el paquete conjunto, o el tipi y los addons que quieras instalar.
3. Copia el **contenido** de `Mods` en `TheLongDark/Mods`, conservando las subcarpetas y sustituyendo las versiones anteriores.
4. Deja una sola copia de cada DLL. Conserva las dependencias ya instaladas.
5. Si usabas el complemento antiguo de prueba `BurebistaCampPersistence.dll`, retíralo: el guardado ya está integrado en el tipi.

El paquete completo instala:

```text
TheLongDark/Mods/
├── BurebistaTraditionalTipi.dll
├── BurebistaTraditionalTipi.modcomponent
├── BurebistaTraditionalTipi/
│   ├── tipi_hide.png
│   └── tipi_wolf.png
├── BurebistaTipiCuringRack.dll       (opcional)
└── BurebistaTipiWorkbench.dll        (opcional)
```

Las dependencias y las DLL del juego no se redistribuyen. Para actualizar solo un addon basta sustituir su DLL; no hace falta reconstruir el tipi.

## Fabricar el tipi

Busca **Tipi tradicional empaquetado** en un banco de trabajo. La receta incluida requiere luz y 720 minutos de fabricación:

| Material | Cantidad |
| --- | ---: |
| Palos | 20 |
| Pieles de lobo curadas | 6 |
| Tripas curadas | 4 |
| Piedras | 10 |

Necesitas un cuchillo; la receta admite también sus variantes indicadas en [la receta](modcomponent/blueprints/burebistapackedtipi.json). Lleva el tipi empaquetado en el inventario para desplegarlo.

## Teclas

| Tecla | Acción |
| --- | --- |
| **F3** | Desplegar el tipi empaquetado |
| **F4** | Girar el tipi desplegado |
| **E** | Abrir o cerrar la puerta cerca del tipi |
| **F2** | Recoger el tipi |
| **F9** | Ocultar o mostrar las instrucciones del tipi |
| **G** | Colocar materiales frescos cercanos en el bastidor, con el addon instalado |
| **B** | Abrir fabricación junto al banco exterior |
| **N** | Encender o apagar las brasas de fabricación del banco |
| **C** | Acceder al fuego o al punto de cocina del brasero |

`Ctrl+F6` entrega un tipi empaquetado para pruebas. No es necesario para el uso normal.

## Recogida, cocina y guardado

Con Workbench **0.3.4**, la recogida se bloquea mientras ardan la fogata interior, la cocina del brasero o las brasas de fabricación. Retira las ollas, sartenes y comida antes de recoger. **N solo controla las brasas de fabricación**, no apaga el fuego nativo de cocina.

Al recoger correctamente el tipi, el addon retira también el punto de cocina invisible del brasero y registra su eliminación mediante el sistema nativo del juego para el siguiente guardado normal. No limpia automáticamente puntos abandonados por versiones anteriores.

El tipi guarda posición, orientación y puerta por zona y partida mediante ModData. El combustible de las fogatas usa el guardado del juego. El bastidor mantiene los materiales como objetos del mundo: al cargar puede ser necesario volver a colocarlos con G. No se garantiza su curado mientras la zona está descargada.

## Estado y problemas

La colocación central de la olla de Workbench 0.3.2 fue confirmada en partida por el usuario. Las compilaciones de los cambios posteriores se comprobaron; la protección y limpieza de 0.3.4 todavía requieren una prueba en partida de recoger, guardar y volver a cargar. El bastidor conserva su estado de beta.

Si aparece un fallo, indica las versiones, los pasos para reproducirlo y adjunta `MelonLoader/Latest.log` en [Issues](https://github.com/florynstefanache-source/BurebistaTraditionalTipi/issues).

## Código y versiones

- [Código del tipi](src) y proyecto en la raíz.
- [Código del bastidor](addons/TipiCuringRack/Source).
- [Código del banco](addons/TipiWorkbench/Source).
- [Compilar desde código](docs/BUILD.md).
- [Historial de cambios](CHANGELOG.md) · [Descargas y comprobaciones SHA-256](downloads/README.md).

Los materiales nativos se obtienen del juego durante la ejecución. La piel cosida original se conserva. Proyecto no oficial, sin afiliación con Hinterland. Se mantiene la [licencia del repositorio](LICENSE).
