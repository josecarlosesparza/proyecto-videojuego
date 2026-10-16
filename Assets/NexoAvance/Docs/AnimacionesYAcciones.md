# Animaciones y acciones de NEXO

Implementación compartida en CallesBajoControl, FabricaAutomatizada y NucleoNexo. Los escenarios, la selección de protagonista y los colliders principales se conservan.

## Controles

- A/D o flechas: movimiento y orientación.
- Espacio: salto. Una nueva pulsación permite el segundo impulso; mantener la tecla no lo repite.
- J: disparo; mantener para continuar disparando según la cadencia del arma.
- G: granada, cuando hay existencias.
- K: golpe cuerpo a cuerpo en el suelo.
- Escape: pausa; R: reiniciar el encuentro.

Nueva partida desde MenuPrincipal permite elegir protagonista. También se pueden abrir directamente las tres escenas: Asalto es el predeterminado si no hubo selección.

## Recursos reales y límites gráficos

Los originales aprobados permanecen intactos. Se generaron nuevos fotogramas con la herramienta integrada de generación de imágenes, usando los diseños originales como referencia. Las hojas originales generadas y los PNG extraídos con transparencia están en `Assets/NexoAvance/Art/Acciones`. Los recortes usan máscaras de los componentes alfa, sin redibujar mediante código. Importación Point, sin compresión ni mipmaps.

| Actor | Recursos asignados |
|---|---|
| Asalto, Exploradora, Artillero e Ingeniera, cada uno | 6 de carrera; 3 poses de salto; 3 del golpe; 2 de reposo; 1 de disparo; 1 de lanzamiento; 1 de daño; 1 de derrota. |
| Cada uno de los seis robots | 6 de locomoción; preparación, disparo, recuperación y daño; 2 de destrucción. |
| Calamar | 6 fotogramas con distintas posiciones de tentáculos; preparación, cañones, barrido, golpe al suelo, misiles y derrota. |

Es una primera versión de animación por poses clave. No equivale a una biblioteca completa de movimientos con muchos fotogramas intermedios:

- El disparo corriendo conserva el ciclo de piernas y emite el proyectil; faltan 6 fotogramas combinados de retroceso del torso por protagonista.
- En el aire se usan las poses de ascenso/caída y el proyectil; faltan 2–3 fotogramas específicos de retroceso aéreo por protagonista.
- El doble salto reutiliza la pose recogida y añade un anillo de impulso. El aterrizaje reutiliza la pose agachada.
- Para suavizar granada, daño y muerte hacen falta 2–4 intermedios por acción y protagonista. Los estados actuales ya aceptan esas secuencias.
- Los robots tienen preparación/disparo/recuperación como poses clave. Faltan ciclos detallados de giro de cañones y carga, y más intermedios de retroceso. Los rotores tienen ciclo de vuelo, pero las poses especiales de combate no tienen un ciclo independiente de rotores.
- Los tentáculos del calamar cambian de posición mediante imágenes completas, no mediante recortes superpuestos. Sus ataques tienen poses clave; faltan intermedios específicos y desplazamiento del extremo del tentáculo hasta cada punto marcado. La zona anunciada y el efecto de impacto indican el área efectiva de daño. La exposición del núcleo conserva el indicador y la vulnerabilidad existentes; no se ha creado una apertura mecánica nueva de la coraza.

Formato para ampliar: protagonistas y robots en lienzos RGBA de 600 × 560 (800 de ancho en poses extendidas), misma escala que su secuencia y línea de apoyo a 30 píxeles del borde inferior. Calamar en 600 × 760 (800 de ancho para el golpe extendido), con pivotes por fotograma alineados al núcleo. Mantener el tamaño relativo de cabeza/torso y no redimensionar cada pose según su altura agachada. Para un rig articulado posterior se necesita un cuerpo reconstruido sin tentáculos y piezas completas, incluyendo las partes ocultas; el rig opcional está desactivado mientras esos recursos no existan.

## Ajustes del Inspector

- `ResistancePlayer`: `jumpSpeed`, `doubleJumpSpeed`, `allowDoubleJump`.
- `PlayerActions`: `meleeKey`, `preparation`, `impactDuration`, `recovery`, `cooldown`, `meleeSize`, `meleeOffset`, `meleeDamage`. Daño 0 significa dos balas normales, inicialmente 20.
- `HeroRoster`: secuencias de cada protagonista y `framesPerSecond`; marcadores de salida por sprite en `spriteMuzzles`.
- `Profiles/Robots`: fotogramas, `animationFps` y los parámetros de combate ya existentes. La animación no dispara por su cuenta.
- `BossAnimationRig`: secuencias y velocidad de reproducción. `OctopusBoss` conserva daño, cadencia, avisos y fases.

## Arquitectura

`HeroAppearance` selecciona la apariencia después del movimiento, sin modificar la escala ni el collider. `PlayerActions` controla preparación, impacto y recuperación. Cada golpe mantiene un conjunto de víctimas ya dañadas y comprueba obstáculos delante del personaje. El daño pasa por `CombatHealth`; balas y granadas siguen teniendo un solo punto de creación.

`RobotAnimation` sigue el estado del arsenal y de la persecución. `BossAnimationRig` sigue los avisos y los momentos efectivos de ataque. Ninguno de los dos produce daño adicional. Morir cancela nuevas acciones y los reinicios limpian el estado del combate.

## Validación

Las pruebas se ejecutan en una compilación de desarrollo creada por Unity, mediante `-actionCheck` y `-robotCheck`. Se producen capturas desde la cámara real del juego. Esto no equivale a una partida manual completa con teclado por las tres misiones. Consultar los informes finales en `outputs/Acciones/validacion.txt` y `outputs/Acciones/Robots/validacion.txt`; un informe con ERROR requiere revisar sus líneas antes de considerar aprobada esa prueba.

### Estado de la revisión final — 16 de octubre de 2026

La versión final, incluidos los diez recortes recuperados desde sus hojas originales generadas, se importó y compiló correctamente en Unity. El lienzo se amplió sin estirar el dibujo. Los 156 PNG finales pasaron la comprobación de transparencia y márgenes.

Sobre esa compilación se ejecutaron otra vez 252 comprobaciones de protagonistas y 80 de robots/jefe: 332 aprobadas, ambos procesos terminaron con código 0. No se encontraron excepciones, referencias faltantes ni errores de compilación en los registros de esta ejecución. Se revisaron capturas reales de derrota y golpe con los recortes corregidos, además de los PNG completos del Incinerador y el calamar.

La prueba cubrió los cuatro protagonistas en las tres escenas, salto y doble salto, disparo, golpe y bloqueo por muro, granadas, daño, derrota y reaparición, ataques y destrucción de robots, fases del calamar, núcleo, apoyos pendientes y transición a fábrica. No se realizó una partida manual completa con teclado por las tres misiones. Los límites gráficos descritos arriba siguen pendientes.
