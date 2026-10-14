# Encuentros por avance · NEXO

## Probar
Abre Assets/NexoAvance/Scenes/MenuPrincipal.unity y pulsa Play. Jugar permite elegir cualquiera de los cuatro protagonistas. Avanza hacia la derecha: los encuentros se activan una vez y entran escalonadamente. Retroceder no vuelve a activarlos. Las tres escenas reales de campaña contienen el sistema: CallesBajoControl, FabricaAutomatizada y NucleoNexo. También puedes abrir cualquiera directamente.

## Ajustar intensidad en el Inspector
En cada escena, selecciona ENCUENTROS · refuerzos por avance (Encounter Director).

| Campo | Función |
|---|---|
| Max Active | Máximo simultáneo de enemigos comunes. Los pendientes esperan; los jefes no consumen este cupo. |
| Encounters / Trigger X | Coordenada horizontal que activa el encuentro. |
| Count | Cantidad total del grupo. |
| Interval | Segundos entre refuerzos del mismo encuentro. Aumentarlo da más descanso. |
| Initial Delay | Espera inicial al entrar en la zona. |
| Allow Rear | Alterna una entrada por detrás cada tres refuerzos. |
| Composition | Tipos que se alternan dentro del grupo. |
| Camera Margin | Margen fuera del borde visible. |
| Safe Player Distance | Separación mínima al aparecer. |
| Safe Rear Fallback | Si no hay entrada frontal válida, intenta una trasera segura. Útil ante puertas y al final del nivel. |
| Normal / Heavy / Drone Speed | Velocidad de persecución según tipo. |
| Firing Distance | Distancia aproximada a la que dejan de acercarse para disparar. |

Configuración inicial: misión 1, 35 enemigos en 7 encuentros y máximo 9 activos; misión 2, 40 en 7 y máximo 11; misión 3, 51 en 8 y máximo 13. No se aumenta la vida ni el daño de los enemigos originales. Grupos iniciales de 3–4, grupos intensos de 6–8, con pausas por distancia y demora inicial.

Los prefabs están en Assets/NexoAvance/Prefabs/Refuerzos. Enemy Pursuit permite ajustar Separation y Resume Margin (margen para reanudar la persecución sin oscilar). Street Enemy expone Attack Interval, Warning Time, Attack Range y Damage. Melee admite ataques cercanos; los tipos actuales conservan sus ataques a distancia. La distancia de parada se escalona ligeramente entre miembros del grupo. Cada movimiento reserva su destino; los demás buscan otro punto o esperan a que haya espacio, también al saltar sobre vehículos.

## Integración
Se conservan los sprites originales, los cuatro protagonistas, sus estadísticas, las granadas, la metralleta tras dos bajas, la cámara, los objetivos y los jefes. Los grupos antiguos quedan desactivados y sus controladores no vuelven a activarlos cuando existe Encounter Director.

La navegación calcula superficies y conexiones sobre colliders reales, comprueba el espacio del cuerpo y permite saltos cortos entre puntos válidos. Las puertas y objetivos destruidos hacen recalcular las rutas. No hay patrullas ni límites de persecución ligados a la zona inicial. Las entradas sin ruta accesible esperan; no se consume el refuerzo.

Morir o pausar congela aparición, persecución y ataques. Reintentar limpia los refuerzos activos y rearma los encuentros del tramo del punto de control; los encuentros anteriores a ese punto se omiten. Reiniciar la misión completa restablece todos los encuentros. Las victorias existentes siguen dependiendo del jefe y de generadores/terminales, no de limpiar cada grupo. HasOutstanding incluye enemigos vivos y refuerzos pendientes de encuentros activados para futuros objetivos de eliminación.

No vuelvas a ejecutar Integrar encuentros por avance después de personalizar valores sin guardar una copia: ese comando regenera la configuración inicial. Las escenas previas están respaldadas en C:/Users/jose/Documents/Codex/2026-09-22/ho/outputs/Encuentros/Antes.

## Validación ejecutada · 14 de octubre de 2026
Compilación Windows de Unity 6000.6.2f1: Succeeded. La versión probada está en C:/Users/jose/Documents/Codex/2026-09-22/ho/outputs/Encuentros/NEXO/NEXO.exe.

- EncounterCheck: 99 comprobaciones correctas. Recorrió por posiciones de prueba todos los encuentros de las tres escenas y verificó cantidades, intervalos, límite y cola, no reactivación al retroceder, aparición fuera de cámara, distancia segura, rutas accesibles, separación, ausencia de cuerpos dentro de sólidos, persecución prolongada sobre obstáculos, visión del jugador, aviso de ataque, daño de balas, bloqueo por paredes, derrota y reinicio. Se probaron 35, 40 y 51 apariciones sin posiciones inválidas.
- HeroCheck: 189 comprobaciones correctas. Los cuatro protagonistas funcionaron en las tres misiones; selección por ratón, movimiento, salto, disparo hacia ambos lados, daño, muerte, reaparición, reinicio, transición 1→2→3 y victoria final contra NEXO. Objetivos y jefes se destruyeron mediante el mecanismo de daño desde la prueba para verificar las condiciones de victoria.
- Se revisaron capturas renderizadas por Unity de los grupos en las tres misiones. No se detectaron excepciones de ejecución ni referencias faltantes en las pruebas finales.

Estas pruebas se ejecutaron automáticamente en el ejecutable generado por Unity; no son una partida manual completa ni una evaluación humana del equilibrio de dificultad. Para aislar navegación, la prueba desactiva temporalmente el fuego y protege al jugador; los ataques y el daño se comprueban después de forma separada. La cantidad y ritmo iniciales quedan listos para ajustarse tras jugar.

Evidencia de entrega: validacion.txt, protagonistas-validacion.txt, compilacion.txt y las capturas mision-1-grupo.png, mision-2-grupo.png, mision-3-grupo.png en C:/Users/jose/Documents/Codex/2026-09-22/ho/outputs/Encuentros.
