# NEXO — Menú principal

Este avance añade la pantalla de inicio al proyecto Unity y la conecta con el recorrido de movimiento de PrimerAvance.

## Opciones del menú

| Opción | Función |
|---|---|
| Jugar | Abre el recorrido de Calles bajo control. |
| Controles | Explica las teclas que ya funcionan. |
| Opciones | Ajusta el volumen del menú y la pantalla completa; permite restablecer los valores iniciales. |
| Créditos | Muestra la autoría y los datos del proyecto académico. |
| Salir | Pide confirmación antes de cerrar la aplicación. |

No todos los videojuegos necesitan las mismas opciones. Estas cinco cubren el inicio, la ayuda, los ajustes, la autoría y la salida de este prototipo. No se incluye “Continuar” porque el juego todavía no guarda una campaña.

## Probar en Unity

1. Abre **Proyecto videojuego**.
2. Abre **Assets > NexoAvance > Scenes > MenuPrincipal**.
3. Presiona **Play** y haz clic en la pestaña **Game**.
4. Elige **Jugar** para entrar al avance anterior.
5. Durante la partida, pulsa **Esc > Volver al menú principal** y confirma para regresar.

El menú está configurado como la escena inicial de la compilación. También se configura como escena de inicio al pulsar Play en el editor. Para probar una escena concreta durante el desarrollo, usa **NEXO > Probar escena actual sin pasar por el menú**. Después puedes restablecer el inicio con **NEXO > Iniciar siempre en el menú**.

## Manejo

- Ratón: clic en los botones; arrastra el control de volumen.
- Menú: flechas para moverte, Enter para confirmar y Esc para volver o abrir la confirmación de salida.
- Opciones: izquierda/derecha ajustan el volumen cuando está seleccionado; Enter activa la casilla de pantalla completa.
- Juego: A/D o flechas para desplazarte, Espacio para saltar, Esc para pausar y R para reiniciar.

La pantalla completa se aplica en el ejecutable de Windows. En Play del editor, Salir detiene la prueba, sin cerrar Unity. Los ajustes se guardan localmente mediante PlayerPrefs. En esta etapa el volumen afecta a los sonidos del menú; todavía no hay música ni efectos de combate.

## Editar el menú

La interfaz utiliza Canvas, Button, Text, Slider y Toggle de Unity UI. Abre MenuPrincipal y selecciona sus elementos dentro de **Menú principal > Composición 16 a 9** para cambiar textos, colores y distribución desde el Inspector.

- **MainMenuController.cs** conecta los botones, cambia las pantallas, guarda las preferencias y carga el nivel.
- **MainMenuBuilder.cs**, en Editor, construye la escena y configura el inicio.
- **AvanceView.cs** incluye la confirmación para volver al menú desde la pausa.
- **MainMenuSmokeTest.cs** contiene pruebas automáticas y solo las ejecuta cuando se utiliza el argumento `-nexoMenuTest`.

**NEXO > Crear o actualizar menú principal** regenera el diseño inicial de MenuPrincipal. Si ya personalizaste esa escena, guarda una copia antes de usarlo, pues reemplaza su diseño.

## Archivos de entrega

- **NEXO-MenuPrincipal.unitypackage**: paquete de respaldo para importar en Unity 6000.6.2f1.
- **NEXO-Jugable/NEXO.exe**: versión de Windows; conserva toda su carpeta para ejecutarla.
- **Respaldo-antes-del-menu.zip**: copia de NexoAvance y ProjectSettings anterior a esta integración.
- **pruebas-menu.txt**: resultados de las comprobaciones automáticas.

Para explicar el avance: “Implementé un menú principal que inicia el juego, muestra los controles, conserva las preferencias y permite salir con confirmación. Lo conecté con la escena del personaje y añadí el regreso al menú desde la pausa”.

Referencia técnica: [carga asíncrona de escenas en Unity](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html).
