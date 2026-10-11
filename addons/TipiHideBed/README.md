# Cama de piel del tipi · v0.1.1

**[Descargar addon](https://github.com/florynstefanache-source/BurebistaTraditionalTipi/raw/refs/heads/main/downloads/BurebistaTipiHideBed-v0.1.1.zip)**

Requiere BurebistaTraditionalTipi, MelonLoader y ModData. Compatible con el bastidor y el banco exterior; no los sustituye.

## Instalación y uso

1. Cierra el juego y copia `Mods/BurebistaTipiHideBed.dll` del ZIP a `TheLongDark/Mods`.
2. Conserva una sola copia del addon y los otros dos addons que ya tengas.
3. Monta el tipi o acércate a uno ya desplegado en tu partida guardada.
4. Lleva una piel **curada** en la mochila, cierra el inventario y espera unos segundos cerca del tipi.
5. Apunta a la cama y pulsa **E** para abrir el descanso normal del juego.

No requiere códigos ni una receta adicional. La cama aparece al fondo del tipi.

| Piel curada | Bonus de calor al dormir |
| --- | ---: |
| Ciervo | +5 °C |
| Lobo | +7 °C |
| Alce | +10 °C |
| Oso | +12 °C |

Utiliza automáticamente **una piel de la especie más cálida disponible**. Si después llevas una mejor, la sustituye automáticamente mientras estás cerca del tipi, fuera de menús y sin dormir, y **devuelve la piel anterior a la mochila** conservando su condición.

Orden: **ciervo → lobo → alce → oso**. No baja de categoría ni consume otra piel de la misma especie. Las pieles frescas no sirven. Oso es la mejora final.

El estado se guarda por partida mediante ModData. La cama se conserva al recoger y volver a desplegar el tipi y al cargar la partida, sin consumir otra piel. No se recoge por separado ni se deteriora al descansar.

## Validación

El usuario confirmó en partida que la versión **0.1.1 funciona** tras corregir la detección de cercanía y la búsqueda de pieles curadas. Compilación sin errores ni advertencias; pruebas de 80 combinaciones de inventario y reconocimiento de nombres nativos, Data y Clone. La confirmación no cubre por separado cada especie ni todos los ciclos de mejora y recarga.

## Código

- [Fuentes](Source)
- [Pruebas](Tests)

Compila `Source/TipiHideBed.csproj` con .NET SDK y `GameDir` apuntando a tu instalación. No se redistribuyen bibliotecas ni materiales del juego.
