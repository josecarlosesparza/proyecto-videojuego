# Calles Bajo Control — copia diurna

Escena de prueba: `Assets/NexoAvance/Scenes/CallesBajoControl_Dia.unity`.
En Unity abre esa escena y pulsa Play. El menú de campaña conserva su nivel original. El ejecutable de esta carpeta arranca directamente en la copia diurna.

## Construcción y recorrido

Se parte de la escena existente, conservando jugador, robots, jefe, vehículos, barricadas, suministro de granadas y puntos de control. El suelo original tiene su superficie en Y = -1 y sigue siendo continuo. No hay pozos ni saltos obligatorios entre precipicios.

La cámara conserva el adelanto de 3 unidades, suavizado de 0.18 s y límites X = 8 y 191. El personaje conserva velocidad 6 y salto 11; no se modificaron las reglas de combate ni de movimiento.

Orden fijo, en coordenadas aproximadas del recorrido:

| Zona | X | Contenido |
|---|---|---|
| Calle | 0–46 | Casas de cuatro diseños, alturas distintas, tejas, chimeneas, contraventanas e hiedra |
| Destrucción | 46–94 | Tres módulos de ruinas con muros abiertos, pisos, vigas y escaleras expuestas |
| Espacio abierto | 94–140 | Árboles de dos diseños, arbustos y fragmentos de muros |
| Industria | 140–202 | Taller abandonado, persianas, tuberías y estructura metálica abierta alrededor del jefe |

Se añadieron plataformas en X = 23, 68, 99, 124 y 157. Sus alturas son de 0.4 a 0.6 unidades. Tienen superficie clara y colisión; los escombros y arbustos del fondo no tienen colisión. La última plataforma evita el punto de reaparición X = 154.

## Profundidad

Los fondos siguen una fracción del desplazamiento de la cámara: nubes 0.92, colinas 0.76, pueblo lejano 0.50 y edificios 0.12. Por eso las nubes se desplazan muy lentamente por la pantalla y la arquitectura lo hace más deprisa. El suelo no usa parallax. El cálculo se ejecuta después del seguimiento de cámara y usa una posición de referencia fija para evitar deriva al retroceder o reaparecer.

Los fondos usan órdenes de dibujo negativos. El terreno, vehículos, personajes y proyectiles conservan sus órdenes actuales. La decoración no pasa por delante de los enemigos. La arquitectura usa colores apagados para separar las siluetas naranja y verde de los combatientes.

## Piezas reutilizables

Diez prefabs en `Assets/NexoAvance/Prefabs/EscenarioDia`: Casa0–3, Ruina0–2, Arbol0–1 y Taller. Son combinaciones de piezas simples usando el sprite Pixel y el material del juego. Se pueden colocar, escalar y editar en el editor. La distribución es determinista, sin generación aleatoria durante la partida.

El generador está en `Editor/DaylightLevelBuilder.cs`; el parallax en `Scripts/DaylightParallax.cs`. El comando NEXO → Crear copia diurna de Calles Bajo Control vuelve a generar la copia: úsalo solo si quieres descartar cambios manuales hechos en ella.

## Recursos para una siguiente mejora artística

Esta es una primera versión modular con geometría sencilla. La referencia se usó para orientar color, vegetación y arquitectura, no como un fondo único.

Para aumentar el detalle convendría crear un atlas pixel art con: cuatro fachadas de yeso/piedra, tejas y contraventanas; tres módulos de muros rotos y vigas; dos árboles y varios arbustos/hiedras; taller, tubos y cerchas oxidadas; baldosas de tierra/piedra con bordes transitables. También siluetas independientes de montañas y nubes. Cada pieza debería mantener una densidad de píxel similar a la del personaje y fondo transparente cuando corresponda.

Las pruebas automáticas de recorrido desactivan ataques y colliders de enemigos para medir el terreno; después verifican por separado el daño de un disparo y el sprite del fusilero. El tiempo de ese recorrido no representa la duración de una partida con combate.

## Validación final
16 comprobaciones aprobadas. Recorrido físico de 175 unidades completado; suelo continuo, cinco plataformas, puntos de reaparición libres, cuatro zonas, parallax, extremos de cámara, suministro de granadas y daño de disparo verificados. Capturas: zona-1.png a zona-4.png. Registro: validacion.txt. El archivo original de CallesBajoControl conserva su hash SHA-256.

