# NEXO Resistencia — Primer avance en Unity 2D

## Abrir el avance ya integrado
En Proyecto videojuego, abre Assets/NexoAvance/Scenes/PrimerAvance.unity. Presiona Play y haz clic dentro de Game para usar el teclado.

## Importar el respaldo en otro proyecto
1. En Unity abre **Assets > Import Package > Custom Package**.
2. Selecciona **NEXO-PrimerAvance.unitypackage** e importa su contenido.
3. Abre **Assets/NexoAvance/Scenes/PrimerAvance.unity**.
4. Presiona **Play** y haz clic dentro de la pestaña **Game**.
También puedes abrir NEXOPrimerAvance con Unity Hub como proyecto independiente (Unity 6000.6.2f1).

Si necesitas reconstruir el escenario, utiliza **NEXO > Crear escena del primer avance**. Guarda tus cambios antes. Esta acción regenera la escena PrimerAvance y su arte; úsala solo para volver al diseño inicial.

## Controles
- A/D o flechas izquierda/derecha: desplazarse.
- Espacio: un salto.
- Esc: pausar o continuar.
- R: regresar al punto inicial.
- Al perder el foco, el juego se pausa; pulsa Continuar para seguir.

## Qué incluye
Personaje original de la resistencia con ropa verde, pañuelo rojo y rifle visual; dos cuadros de reposo, seis de carrera y uno de salto. Movimiento lateral, cambio de orientación, salto único, gravedad y colisiones 2D. Suelo, caja y cuatro plataformas sólidas. Cámara lateral, pausa, reinicio y recuperación si se cae fuera del escenario. Una bandera marca el final del recorrido de prueba.

Los dibujos se generan con píxeles mediante código y se guardan como PNG editables. No se utilizan recursos de Metal Slug. La referencia sirve para el movimiento lateral de acción.

Este avance no incluye disparos funcionales, enemigos, daño, granadas ni las tres misiones completas. El rifle es parte del dibujo. El rótulo de recorrido completado demuestra llegar al extremo del escenario, no la victoria de la misión final.

## Cómo explicar el avance en clase
“Implementé la base de un videojuego de acción 2D en Unity. El jugador puede caminar, cambiar de orientación y saltar obstáculos. Utilicé Rigidbody2D para la gravedad y BoxCollider2D para evitar atravesar el suelo y las plataformas. Las animaciones cambian según el movimiento y si el personaje está en el aire. Esta escena es un tramo de entrenamiento inspirado en Calles bajo control, la primera misión de mi diseño.”

## Archivos principales
- ResistancePlayer.cs: teclado, velocidad, salto, detección de suelo, animación y reinicio.
- AvanceView.cs: cámara y controles visibles.
- Editor/BuildAvance.cs: genera el arte y la escena desde el menú NEXO.
- AvanceSmokeTest.cs: comprobaciones automáticas; solo se ejecutan con el argumento -nexoSmokeTest.
- Art/: sprites originales, material y material físico sin fricción.
- Scenes/PrimerAvance.unity: escena para abrir y ejecutar.

Selecciona Jugador · Resistencia en la jerarquía para ajustar Speed, Jump Speed y Acceleration. Valores iniciales: 6, 11 y 55. Las plataformas son objetos individuales que puedes mover desde Scene.

## Relación con el documento
Se conservan el concepto de resistencia contra NEXO, la vista 2D, los controles A/D o flechas y Espacio, el aspecto pixelado y las animaciones básicas.
Tu documento todavía menciona Godot y GDScript en Aspectos técnicos y Presupuesto. Como elegiste Unity, conviene actualizar esas menciones a Unity y C#.
El documento describe un prototipo posterior con disparos y robots; esta entrega cubre el avance de movimiento que solicitaste.

## Referencia técnica
Unity Rigidbody2D.linearVelocity:
https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/rigidbody2d/linearvelocity


## Validación realizada
Compilación de Windows correcta en Unity 6000.6.2f1. Pasaron las 11 comprobaciones automáticas de suelo, movimiento, caja, salto, ausencia de doble salto, plataformas, orientación, pausa, reinicio y recuperación tras caída. El detalle está en NEXO-pruebas.txt.

## Versión ejecutable
Abre NEXO-Jugable/NEXO.exe para jugar sin entrar en Unity. Conserva toda la carpeta NEXO-Jugable junto al ejecutable.
