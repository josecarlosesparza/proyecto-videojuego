# Escuadrón de robots y calamar cibernético

Proyecto: `C:\Users\jose\Proyecto videojuego`.

## Probar la campaña

Abre `Assets/NexoAvance/Scenes/MenuPrincipal.unity`, pulsa Play, Jugar, elige uno de los cuatro protagonistas y confirma. La selección y las capacidades del jugador se conservan.

También puedes abrir directamente `CallesBajoControl`, `FabricaAutomatizada` o `NucleoNexo` en esa carpeta y pulsar Play. Sin selección previa se utiliza Asalto.

- **Misión 1:** soldados naranjas al principio; cazadores azules desde el segundo encuentro; bombarderos amarillos hacia el final. El calamar está al final de la avenida. La arena se activa al llegar a X=172.
- **Misión 2:** naranjas y azules, artilleros verdes, presentación del incinerador en el tercer encuentro y del dron en el cuarto; después se combinan con bombarderos.
- **Misión 3:** mezcla de los seis tipos, con grupos terrestres, presión aérea y ataques de área.

Controles: A/D o flechas para moverse, espacio para saltar, J para disparar, G para granadas, Esc para pausa. Al morir, usa Reintentar; las fases y los refuerzos se restablecen según el punto de control existente.

## Ataques

| Tipo | Vida inicial | Ataque y daño | Misiones |
|---|---:|---|---|
| Soldado naranja original | 100 | Bala individual: 8 | 1, 2, 3 |
| Cazador azul | 80 | Ráfaga de cuatro balas alternando armas: 5 cada una | 1, 2, 3 |
| Artillero verde | 190 | Preparación de 1.3 s, ocho balas: 6 cada una, pausa | 2, 3 |
| Bombardero amarillo | 120 | Granada en arco, destino fijado al disparar: 20 por explosión | 1, 2, 3 |
| Incinerador rojo | 120 | Fuego cercano: 5 cada 0.5 s, durante 1.5 s; pausa de 2.1 s | 2, 3 |
| Dron eléctrico | 85 | Esfera: 12; velocidad del jugador al 80% durante 1 s | 2, 3 |

Las marcas amarillas/naranjas anuncian granadas, misiles y barridos. Las armas avisan antes de atacar. El fuego respeta paredes y la invulnerabilidad del jugador. Los nuevos robots usan las rutas de persecución existentes; el dron mantiene una altura alcanzable y sigue los desniveles del terreno. Las balas y los ataques de área se reutilizan mediante pools.

## Calamar: KRAKEN-01

Conserva su papel de jefe de la **misión 1**. El Supervisor de la misión 2 y NEXO de la misión 3 mantienen sus objetivos y funcionamiento.

- Vida inicial: 1200.
- Fase 1, vida >=65%: alterna ráfagas de cañones (10 por bala) y barridos bajos (20). Salta para esquivar el barrido anunciado en el suelo.
- Fase 2, vida <65% y >=30%: añade misiles (25) con una marca de impacto y pequeños grupos de apoyo. Máximo inicial: tres apoyos vivos; los pendientes esperan espacio.
- Fase 3, vida <30%: el núcleo azul se vuelve vulnerable, recibe el doble de daño y tiene un contorno luminoso. Su área vulnerable incluye la carcasa cercana, con radio 0.9, para que los cuatro protagonistas puedan alcanzarla con el salto normal. Los intervalos de ataque se reducen un 20%. Salta y dispara hacia la parte baja del contorno del núcleo.

Cada transición ocurre una vez por intento. La derrota detiene ataques y apariciones. La victoria elimina proyectiles y ataques de área peligrosos y detiene los refuerzos antes de mostrar la pantalla de victoria.

## Valores del Inspector

- `Assets/NexoAvance/Profiles/Robots/*.asset`: daño, vida, velocidad, distancia de ataque, alcance, duración del aviso, pausa, cantidad e intervalo de balas, salida de ambas armas; radio y tiempos de granada; duración y frecuencia del fuego; ralentización y altura del dron.
- Componente **EncounterDirector** de cada misión: `Encounters` (posición de activación, cantidad, intervalo, espera inicial y composición), `Max Active`, `Max Heavy Active`, `Max Boss Supports`, margen de cámara y distancia segura. Naranja y azul son los apoyos del calamar. No se sustituyen los encuentros por patrullas ni se reactivan al retroceder.
- Componente **OctopusBoss** de KRAKEN-01: daños, velocidad de bala, intervalos, duración de avisos, ancho de barrido, radio de misil, intervalo y cantidad de apoyos. La vida está en **CombatHealth / Maximum**. **BossCore / Multiplier** y `Radius` ajustan el punto débil.

Para probar un robot aislado sin alterar la campaña, duplica una escena y cambia el primer encuentro de su EncounterDirector a cantidad 1 y composición del tipo deseado. En misiones 2 y 3 ya están referenciados los seis prefabs. Coloca el disparador cerca del inicio y pulsa Play. Los nuevos prefabs están en `Assets/NexoAvance/Prefabs/RobotSquad`.

El menú del editor `NEXO > Integrar escuadrón y calamar` reconstruye la integración y restaura sus valores iniciales; **no lo uses después de personalizar perfiles o encuentros** salvo que quieras regenerarlos. `NEXO > Compilar escuadrón y calamar` genera el ejecutable de esta entrega.

## Recursos originales y animaciones pendientes

`Assets/NexoAvance/Art/RobotSquad` contiene los dos originales, los recortes transparentes y una extracción del naranja de la hoja nueva solo como referencia. El soldado del juego conserva su sprite naranja anterior.

La extracción modifica únicamente la máscara alfa y el encuadre; los píxeles RGB conservados proceden del archivo original. Importación Sprite, filtro Point, sin compresión ni mipmaps y proporciones intactas.

Hay **una pose estática por robot y una del calamar**. Los campos `Idle`, `Run`, `Jump`, `Shoot`, `Hurt` y `Death` de RobotProfile quedan preparados pero vacíos. No hay animaciones nuevas completas ni deformación del sprite para simularlas.

Para animarlas hacen falta hojas PNG transparentes por robot con el mismo tamaño de lienzo, escala y pivote de pies en todos los fotogramas: reposo (4–6), locomoción (6–8), salto o vuelo (4–6), preparación/disparo (3–6), daño (2–3) y muerte (6–8). El dron necesita ciclos de rotores/carga; el incinerador, encendido y apagado; el artillero, giro del cañón.

Para el calamar hacen falta capas o fotogramas separados de tentáculos, cañones, lanzamisiles, apertura del núcleo, armadura dañada y destrucción. La versión actual utiliza avisos y áreas de ataque; **no muestra pérdida de armadura animada** porque esa imagen no existe.

## Evidencia de validación

Los informes y capturas de esta carpeta documentan las pruebas automáticas ejecutadas en el jugador de Unity. Las pruebas usan escenas reales, física y componentes del juego, junto con colocación programática del jugador y daño controlado para alcanzar estados concretos. No equivalen a una partida manual completa ni a una calibración final de dificultad.

Resultados de esta entrega: **354 comprobaciones correctas**, sin fallos:

- `validacion.txt`: 58 comprobaciones de los seis robots, sus ataques, persecución de dron/incinerador, explosión única, ralentización, fases, núcleo, apoyos, derrota, reinicio y victoria del calamar.
- `encuentros-validacion.txt`: 99 comprobaciones sobre las tres misiones. Se verificaron 35/40/51 apariciones, todas fuera de cámara y en rutas válidas, separación, paredes, cola de refuerzos y persecución larga sobre obstáculos.
- `protagonistas-validacion.txt`: 189 comprobaciones de los cuatro protagonistas en las tres misiones: selección, movimiento, salto, disparos en ambos sentidos, daño, derrota/reaparición, reinicio, objetivos, victorias y transiciones.
- `alcance-jefe.txt`: 8 comprobaciones adicionales después de ajustar el núcleo. Los cuatro héroes lo alcanzan disparando durante un salto real; el barrido causa 20 al estar en el suelo y se esquiva saltando; el misil causa 25 una sola vez.
- `pixeles.txt`: comparación RGB exacta de los siete recortes y verificación SHA256 de los dos archivos originales.
- Compilación de Unity 6000.6.2f1: `Succeeded`. No se detectaron excepciones ni referencias faltantes en los registros de estas ejecuciones. Hay advertencias de API obsoleta en herramientas de comprobación anteriores del proyecto.

No se realizó una partida manual completa sin ayudas ni una medición de dificultad final. Los nuevos diseños siguen siendo poses estáticas; los avisos y efectos de ataque sí funcionan.

La copia anterior de las tres escenas se conserva en `Antes` y los scripts previos en `work/robots/antes` dentro de la carpeta de trabajo de Codex.
