# Protagonistas seleccionables

## Uso

Abre `Assets/NexoAvance/Scenes/MenuPrincipal.unity`, activa Play, pulsa Jugar, selecciona una tarjeta y confirma Jugar. Volver al menú e iniciar otra partida abre de nuevo la selección. La campaña continúa por CallesBajoControl, FabricaAutomatizada y NucleoNexo mediante sus botones de victoria y conserva la elección. Abrir directamente una misión en una sesión nueva del editor usa Asalto.

## Recursos originales

`Assets/NexoAvance/Art/Protagonistas/Original.png` conserva el archivo original. Asalto.png, Exploradora.png, Artillero.png e Ingeniera.png son recortes de sus píxeles con máscara alfa binaria, sin generación ni redibujo. Se eliminaron fondo, sombras y fragmentos de otros personajes. Los colores RGB de los píxeles conservados coinciden con el original. No se ha reducido la resolución de los archivos.

Importación: Sprite, Point, sin mipmaps ni compresión, sin cambio de dimensiones, 370 píxeles/unidad. Pivotes independientes sobre la línea de las botas. El escalado del objeto visual es uniforme (1,1,1). El collider común permanece de 0.68 × 1.7 unidades.

## Integración

HeroRoster.asset centraliza nombres, sprites, salidas de proyectiles y espacios para animaciones. HeroSelection guarda la elección durante la sesión y entre escenas; se restablece a Asalto al entrar en Play. No crea un jugador persistente adicional. HeroAppearance aplica el diseño al jugador que ya usa cada escena. ResistancePlayer lo instala también en escenas antiguas abiertas directamente. Las tres escenas de campaña tienen el componente y el diseño predeterminado guardados.

El menú crea la pantalla de selección dentro de su Canvas existente. La selección cambia solo el diseño. La mejora de metralleta conserva su daño y ráfagas, pero no dibuja el rectángulo de arma antiguo encima del arma original. Las balas salen de puntos diferentes según el sprite y se reflejan al mirar a la izquierda. Una comprobación de colisión evita que la boca del arma permita disparar a través de una pared.

## Animaciones pendientes

Solo existe una pose por personaje. Todos los estados usan esa pose; no hay carrera, salto, disparo, daño ni muerte animados terminados. La inversión horizontal permite mirar a ambos lados sin deformar el dibujo.

Para animaciones completas hacen falta, por cada personaje: reposo (4 cuadros), carrera (8), salto (3: despegue, aire y caída), disparo (3), daño (2) y muerte (6). Son cantidades recomendadas; los arrays aceptan otras. Deben conservar el diseño, transparencia, escala de 370 px/unidad y pivote de los pies; una orientación derecha basta si se acepta invertirla a la izquierda. Se recomienda un lienzo común de 640 × 800 px por cuadro, con margen para cabello y arma. Los estados están preparados en HeroFrames y HeroMotion dentro del catálogo.

## Validación

`validacion.txt` recogerá los resultados de la ejecución de Unity. La prueba usa eventos de clic de la interfaz, las tres escenas reales, movimiento y salto del controlador, proyectiles con colisión, muerte/reaparición, reinicio y las transiciones de campaña. Para aislar estas comprobaciones desactiva el fuego automático de enemigos y completa los objetivos y jefes mediante daño de prueba. No equivale a jugar manualmente toda la campaña ni a comprobar su dificultad.

Los respaldos de las escenas están en `Antes` y los scripts previos en `work/heroes/antes` dentro de la carpeta de trabajo de Codex.

Resultado final: compilación correcta y 185 comprobaciones aprobadas en la versión final de Unity. Se revisaron la pantalla de selección y capturas de los cuatro personajes en las tres escenas, verificando escala, transparencia y apoyo de los pies. Además se probó con clics reales dentro del editor: Jugar → Ingeniera → Jugar, con aparición correcta en la misión 1. Unity quedó fuera de Play, en MenuPrincipal. No se hizo una partida manual completa con cada personaje; los objetivos y jefes se completaron con daño de prueba durante las comprobaciones de transición.
También pasó la revisión completa de la misión 1 con el nuevo Asalto: 20 comprobaciones adicionales, incluido recorrido con movimiento y saltos reales hasta el extremo final, seguimiento continuo de cámara, granadas y combate. Resultado total: 205 comprobaciones aprobadas. Los resultados del recorrido están en validacion-recorrido.txt. Ambas ejecuciones terminaron con código 0, sin excepciones de juego ni referencias faltantes detectadas. El registro del reproductor contiene avisos de servicios de telemetría de Unity sin conexión, ajenos al funcionamiento local.
