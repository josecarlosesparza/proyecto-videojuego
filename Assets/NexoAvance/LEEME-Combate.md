# NEXO — Disparos y salud

El tercer avance integra combate en PrimerAvance. El menú sigue siendo la primera pantalla: pulsa Jugar para entrar.

## Cómo probar
1. Abre Proyecto videojuego en Unity y pulsa Play desde MenuPrincipal.
2. Avanza hacia la derecha. La caja sirve como cobertura; salta las plataformas para acercarte al robot.
3. Mantén J para disparar en la dirección hacia la que mira el personaje.
4. Cuando el robot tiene línea de tiro, enciende una luz amarilla y después dispara. Puedes esquivar saltando o cubrirte.
5. Observa la barra SALUD en la esquina inferior derecha. Cada impacto enemigo resta 15 puntos.
6. Al perder toda la salud, elige Reintentar o Volver al menú. También puedes reiniciar con R.

## Valores iniciales
| Elemento | Valor |
|---|---|
| Salud del jugador | 100 |
| Salud del robot | 60 |
| Daño de cada bala del jugador | 10 |
| Daño de cada bala enemiga | 15 |
| Disparos del jugador | Hasta 4 por segundo |
| Munición | Ilimitada |
| Aviso del robot | 0.5 segundos |
| Espera entre ataques del robot | 1.5 segundos, más el aviso |

El robot es un centinela fijo para este primer encuentro. Detecta al jugador a corta distancia y a una altura compatible con su disparo horizontal. No dispara si una barrera corta la línea de tiro. Las balas de ambos bandos se detienen en superficies sólidas.

Al recibir daño, el personaje afectado destella en rojo. Al agotarse su salud, el robot desaparece y deja de disparar y de bloquear el paso. El objetivo cambia para indicar que puedes avanzar hasta la bandera. Llegar al final sin eliminarlo todavía no muestra el recorrido completado.

R y Reiniciar recorrido restauran la salud de ambos, reactivan al enemigo y eliminan las balas. La pausa detiene el movimiento de las balas y los ataques.

## Ajustar desde Unity
En PrimerAvance selecciona:
- Jugador · Resistencia > Combat Health: salud máxima.
- Jugador · Resistencia > Player Combat: daño, intervalo y velocidad de las balas.
- Combate > Robot centinela > Combat Health: salud del enemigo.
- Combate > Robot centinela > Robot Shooter: daño, alcance, aviso, intervalo y velocidad.
- Combate > Interfaz de combate > Salud del jugador: posición y apariencia de la barra.

Las opciones del menú y el movimiento anterior se conservan. El audio actual sigue siendo el del menú; este avance no añade sonidos de combate, granadas ni otros tipos de enemigos.

## Código
- CombatHealth.cs: salud, daño, destello y eventos de derrota.
- CombatBullet.cs: avance de las balas y comprobación del recorrido completo para evitar que atraviesen paredes.
- PlayerCombat.cs: tecla J, orientación y cadencia.
- RobotShooter.cs: detección, aviso y ataques del centinela.
- CombatSession.cs: barra de salud, derrota y reinicio.
- CombatBuilder.cs: integración en las escenas existentes.
- CombatSmokeTest.cs: pruebas automáticas del combate.

El menú NEXO > Instalar combate en PrimerAvance permite regenerar los objetos del combate. Reemplaza el grupo Combate, por lo que conviene guardar una copia de la escena antes de usarlo si ya lo personalizaste. No hace falta ejecutarlo para jugar: el avance se entrega integrado.

## Archivos
- NEXO-Combate.unitypackage: respaldo del contenido de NexoAvance.
- NEXO-Jugable/NEXO.exe: versión ejecutable; conserva toda su carpeta.
- Respaldo-antes-combate.zip: estado anterior de NexoAvance y ProjectSettings.
- pruebas-combate.txt y pruebas-menu.txt: resultados de validación.

Referencia: [Physics2D.Raycast de Unity](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics2D.Raycast.html).
