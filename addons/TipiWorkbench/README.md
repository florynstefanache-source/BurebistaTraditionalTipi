# Tipi Workbench · v0.3.4

Banco exterior opcional para **BurebistaTraditionalTipi**, con materiales del juego, herramientas decorativas y un brasero con un punto central para cocinar con olla o sartén.

**[Descargar](../../downloads/TipiWorkbench-v0.3.4.zip?raw=true)** · [Instalación del tipi y requisitos](../../README.md)

Cierra el juego y sustituye `Mods/BurebistaTipiWorkbench.dll` en `TheLongDark/Mods`. Las texturas auxiliares están integradas en la DLL. Conserva el tipi y sus dependencias; la protección de recogida está preparada para el tipi **1.2.3**.

## Uso y combustible

El banco aparece automáticamente junto al tipi desplegado. Acércate a menos de **2,5 m** del banco.

| Tecla | Función |
| --- | --- |
| **B** | Abrir el menú de fabricación |
| **N** | Encender o apagar las brasas de fabricación |
| **C** | Abrir la interacción del fuego o del punto central de cocina |

Encender las brasas de fabricación consume **1 carbón** (`GEAR_Coal`) o **3 carboncillos** (`GEAR_Charcoal`). Duran cinco minutos de tiempo activo e incluyen una sesión de fabricación sin segundo cobro mientras estén encendidas. Abrir el menú por sí solo no consume combustible. Las siguientes sesiones vuelven a aplicar el coste.

La cocina utiliza un fuego nativo con su propio combustible y las interacciones normales del juego. **N no apaga ese fuego.** Pulsa C y usa el punto de cocina para colocar la olla o sartén; se alinea en el centro del enrejado. Las herramientas de adorno no se pueden recoger.

El brasero no añade recetas de forja ni una temperatura de forjado. La fabricación del banco y el fuego de cocina son sistemas distintos.

## Recoger el tipi sin dejar una cocina invisible

- Apaga o deja agotarse la fogata interior y el fuego de cocina; apaga también las brasas de fabricación.
- Retira primero la olla, sartén y comida del punto de cocina.
- Pulsa la tecla habitual de recogida del tipi (**F2**).

La versión 0.3.4 bloquea la recogida si hay fuego o quedan objetos en los puntos de cocina comprobados. Tras una recogida correcta elimina el punto nativo del brasero, aunque su estructura sea invisible, y registra su retirada para el siguiente guardado normal. No elimina la cocina por un simple cambio de zona ni limpia puntos huérfanos de versiones antiguas.

Compilación correcta. La olla centrada se confirmó en partida en 0.3.2; falta validar en partida la recogida, el guardado y la carga con 0.3.4.
