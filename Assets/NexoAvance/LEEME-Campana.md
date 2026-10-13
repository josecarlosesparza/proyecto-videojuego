# NEXO — Misiones 2 y 3

## Cómo continuar la campaña
Abre el proyecto habitual en Unity y pulsa Play → Jugar.
1. Completa Calles Bajo Control y selecciona CONTINUAR A LA FÁBRICA.
2. Completa Fábrica automatizada y selecciona CONTINUAR AL NÚCLEO.
3. Destruye NEXO para completar la campaña.

La metralleta y las granadas restantes se conservan al continuar entre misiones. La salud comienza en 100 al entrar a una misión. No hay guardado permanente de campaña: volver al menú y pulsar Jugar inicia la primera misión.

## Misión 2: Fábrica automatizada
Planta industrial con plataformas, cintas de ensamblaje animadas y nueve enemigos: patrulleros, drones y torretas de ráfagas. No hay segundas oleadas.
- Destruye el generador A (300 de salud) y el generador B (350).
- El generador B está sobre una pasarela: sube por la plataforma de acceso y vuelve a saltar para alcanzarlo.
- Derrota al supervisor de ensamblaje (700 de salud).
- Cruza la salida iluminada para completar la misión.
Las cintas se animan visualmente; no arrastran al jugador.

## Misión 3: El núcleo de NEXO
Centro de datos con servidores, patrulleros, drones, torretas y dos puertas de seguridad.
- Destruye cada terminal (180 y 220 de salud) para abrir su puerta.
- Avanza hasta el núcleo central (1800 de salud).
- El núcleo tiene tres fases de ataque según su salud. Dispara proyectiles bajos y dirigidos, y añade abanicos y ráfagas más largas.
- Durante el aviso luminoso de carga activa un escudo breve. Ataca al desaparecer el aviso.
- Destruir el núcleo completa la campaña.

## Controles y suministros
A/D o flechas: movimiento. Espacio: salto. J: disparar. G: granada. Esc: pausa. R: reintento.
Las cajas entregan tres granadas por contacto; los botiquines recuperan 35 de salud, sin exceder 100. Reintentar mantiene objetivos destruidos, enemigos eliminados, arma y granadas restantes; reinicia al jefe y restaura la salud. Repetir misión comienza esa escena desde cero.

Se conserva lo pedido durante el desarrollo: Unity 2D, salud inferior izquierda, G para granadas y recorrido de la primera misión sin muros de combate. Las puertas nuevas son dispositivos de seguridad ligados a terminales, conforme al documento. Los valores no especificados en las secciones de misión se ajustaron al combate existente.

## Archivos de Unity
- Assets/NexoAvance/Scenes/FabricaAutomatizada.unity
- Assets/NexoAvance/Scenes/NucleoNexo.unity
- CampaignMission.cs: objetivos, progreso, derrota y victoria.
- CampaignObjective.cs: generadores y terminales destructibles.
- MachineBoss.cs: supervisor y núcleo de tres fases.
- CampaignExit.cs: continuación desde la primera misión.
- CampaignPickup.cs: botiquines y granadas.
- CampaignBuilder.cs: generador de las escenas.

La versión independiente está en NEXO-Jugable/NEXO.exe; conserva toda la carpeta. El respaldo previo está en work/antes-campana dentro de la carpeta de trabajo de Codex. Las pruebas automáticas aceleran el recorrido para verificar las condiciones; sus tiempos no representan la duración de una partida normal.

## Validación
28 comprobaciones de integración aprobadas: transiciones, equipamiento, suministros, daño, saltos hasta el generador elevado, objetivos, reintentos, jefes, fases del núcleo, victoria y retorno al menú. Registro: validacion.txt.

