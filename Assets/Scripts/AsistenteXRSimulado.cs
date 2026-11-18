using UnityEngine;
using UnityEngine.Events;
using System.Globalization;
using System.Text;

// Utilizamos el nuevo Input System si estÃ¡ habilitado.
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class AsistenteXRSimulado : MonoBehaviour
{
    // ==========================================
    // 1. OBJETOS QUE CONTROLARÃ EL ASISTENTE
    // ==========================================

    [Header("Objetos del laboratorio")]

    // Renderer del cubo para cambiar su color.
    [SerializeField] private Renderer cubo;

    // Luz que podremos encender y apagar.
    [SerializeField] private Light luz;

    // Puerta que podremos abrir y cerrar.
    [SerializeField] private Transform puerta;


    // ==========================================
    // 2. RESPUESTA DE AUDIO (OPCIONAL)
    // ==========================================

    [Header("Respuesta de audio opcional")]

    [SerializeField] private AudioSource vozAudio;

    // GrabaciÃ³n que indica que el comando fue ejecutado.
    [SerializeField] private AudioClip audioConfirmacion;


    // ==========================================
    // 3. EVENTOS DE UNITY
    // ==========================================

    [Header("Eventos del asistente")]

    public UnityEvent alCambiarColor = new UnityEvent();
    public UnityEvent alCambiarLuz = new UnityEvent();
    public UnityEvent alMoverPuerta = new UnityEvent();


    // ==========================================
    // 4. VARIABLES INTERNAS
    // ==========================================

    // Controla si el asistente acepta Ã³rdenes.
    private bool escuchando = false;

    // Guarda el estado de la puerta.
    private bool puertaAbierta = false;

    // Duración en segundos de la animación de la puerta.
    private const float duracionPuerta = 0.6f;

    // Referencia a la corrutina de animación en curso.
    private Coroutine animacionPuerta;

    // Guarda la frase escrita por el usuario.
    private string textoIngresado = "";

    // Ãšltima frase recibida por el asistente.
    private string ultimaFrase = "Ninguna";

    // IntenciÃ³n detectada por el sistema.
    private string ultimaIntencion = "NINGUNA";

    // Entidad encontrada en el comando.
    private string ultimaEntidad = "Ninguna";

    // Respuesta que aparecerÃ¡ en pantalla.
    private string respuesta = "Pulsa Num 0 para activar.";

    // Valores iniciales para restablecer el laboratorio.
    private Color colorOriginal = Color.white;
    private Quaternion rotacionOriginal;
    private bool luzOriginal;

    // Material que cambiaremos durante la ejecuciÃ³n.
    private Material materialCubo;


    // ==========================================
    // 5. INICIALIZACIÃ“N
    // ==========================================

    private void Start()
    {
        // Verificamos que el docente haya asignado
        // todos los objetos principales en el Inspector.
        if (cubo == null || luz == null || puerta == null)
        {
            Debug.LogError(
                "Faltan referencias en AsistenteXRSimulado."
            );

            // Evitamos continuar si falta algÃºn objeto.
            enabled = false;
            return;
        }

        // Obtenemos una instancia del material del cubo.
        materialCubo = cubo.material;

        // Guardamos los valores iniciales.
        colorOriginal = materialCubo.color;
        rotacionOriginal = puerta.rotation;
        luzOriginal = luz.enabled;

        Debug.Log("Asistente XR preparado.");
    }


    // ==========================================
    // 6. DETECCIÃ“N DEL TECLADO NUMÃ‰RICO
    // ==========================================

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM

        // Sistema de entrada moderno de Unity.
        Keyboard teclado = Keyboard.current;

        if (teclado == null)
            return;

        if (teclado.numpad0Key.wasPressedThisFrame)
            AlternarAsistente();

        if (teclado.numpad1Key.wasPressedThisFrame)
            ProcesarFrase("Cambia el cubo a rojo");

        if (teclado.numpad2Key.wasPressedThisFrame)
            ProcesarFrase("Pon el cubo azul");

        if (teclado.numpad3Key.wasPressedThisFrame)
            ComandoLuz();

        if (teclado.numpad4Key.wasPressedThisFrame)
            ComandoPuerta();

        if (teclado.numpad5Key.wasPressedThisFrame)
            ProcesarFrase("Restablecer entorno");

        if (teclado.numpad6Key.wasPressedThisFrame)
            ProcesarFrase("Pon el cubo amarillo");

        if (teclado.numpad7Key.wasPressedThisFrame)
            ProcesarFrase("Pon el cubo violeta");

#elif ENABLE_LEGACY_INPUT_MANAGER

        // Sistema de entrada anterior de Unity.
        if (Input.GetKeyDown(KeyCode.Keypad0))
            AlternarAsistente();

        if (Input.GetKeyDown(KeyCode.Keypad1))
            ProcesarFrase("Cambia el cubo a rojo");

        if (Input.GetKeyDown(KeyCode.Keypad2))
            ProcesarFrase("Pon el cubo azul");

        if (Input.GetKeyDown(KeyCode.Keypad3))
            ComandoLuz();

        if (Input.GetKeyDown(KeyCode.Keypad4))
            ComandoPuerta();

        if (Input.GetKeyDown(KeyCode.Keypad5))
            ProcesarFrase("Restablecer entorno");

        if (Input.GetKeyDown(KeyCode.Keypad6))
            ProcesarFrase("Pon el cubo amarillo");

        if (Input.GetKeyDown(KeyCode.Keypad7))
            ProcesarFrase("Pon el cubo violeta");

#endif
    }


    // ==========================================
    // 7. ACTIVAR O DESACTIVAR EL ASISTENTE
    // ==========================================

    public void AlternarAsistente()
    {
        // Cambia entre verdadero y falso.
        escuchando = !escuchando;

        if (escuchando)
        {
            respuesta = "Asistente activado. Te escucho.";
        }
        else
        {
            respuesta = "Asistente desactivado.";
        }

        Debug.Log(respuesta);
    }


    // ==========================================
    // 8. COMANDOS CONTEXTUALES
    // ==========================================

    private void ComandoLuz()
    {
        // Si la luz estÃ¡ encendida, solicitamos apagarla.
        // Si estÃ¡ apagada, solicitamos encenderla.
        if (luz.enabled)
            ProcesarFrase("Apaga la luz");
        else
            ProcesarFrase("Enciende la luz");
    }

    private void ComandoPuerta()
    {
        // Generamos una frase segÃºn el estado actual.
        if (puertaAbierta)
            ProcesarFrase("Cierra la puerta");
        else
            ProcesarFrase("Abre la puerta");
    }


    // ==========================================
    // 9. INTERPRETACIÃ“N DE LENGUAJE NATURAL
    // ==========================================

    public void ProcesarFrase(string frase)
    {
        // Primero verificamos si el asistente estÃ¡ activo.
        if (!escuchando)
        {
            respuesta = "Primero activa el asistente con Num 0.";
            return;
        }

        ultimaFrase = frase;

        // Convertimos el texto a minÃºsculas y
        // eliminamos las tildes para facilitar el anÃ¡lisis.
        string comando = Normalizar(frase);

        ultimaIntencion = "DESCONOCIDA";
        ultimaEntidad = "Ninguna";

        // ------------------------------------------
        // INTENCIÃ“N: RESTABLECER_ENTORNO
        // ------------------------------------------

        if (comando.Contains("restablecer") ||
            comando.Contains("restablece") ||
            comando.Contains("reinicia") ||
            comando.Contains("reiniciar"))
        {
            RestablecerEntorno();
            return;
        }

        // ------------------------------------------
        // INTENCIÃ“N: CHANGE_COLOR
        // ------------------------------------------

        if (comando.Contains("rojo") ||
            comando.Contains("roja"))
        {
            ultimaIntencion = "CHANGE_COLOR";
            ultimaEntidad = "Rojo";

            CambiarColor(Color.red, "rojo");
            return;
        }

        if (comando.Contains("azul"))
        {
            ultimaIntencion = "CHANGE_COLOR";
            ultimaEntidad = "Azul";

            CambiarColor(Color.blue, "azul");
            return;
        }

        if (comando.Contains("verde"))
        {
            ultimaIntencion = "CHANGE_COLOR";
            ultimaEntidad = "Verde";

            CambiarColor(Color.green, "verde");
            return;
        }

        if (comando.Contains("amarillo"))
        {
            ultimaIntencion = "CHANGE_COLOR";
            ultimaEntidad = "Amarillo";

            CambiarColor(Color.yellow, "amarillo");
            return;
        }

        if (comando.Contains("violeta") || comando.Contains("morado"))
        {
            ultimaIntencion = "CHANGE_COLOR";
            ultimaEntidad = "Violeta";

            CambiarColor(new Color(0.55f, 0.25f, 0.9f), "violeta");
            return;
        }

        // ------------------------------------------
        // INTENCIÃ“N: TOGGLE_LIGHT
        // ------------------------------------------

        if (comando.Contains("luz") ||
            comando.Contains("lampara"))
        {
            ultimaIntencion = "TOGGLE_LIGHT";
            ultimaEntidad = "LuzInteligente";

            if (comando.Contains("enciende") ||
                comando.Contains("prende") ||
                comando.Contains("activa"))
            {
                CambiarLuz(true);
                return;
            }

            if (comando.Contains("apaga") ||
                comando.Contains("desactiva"))
            {
                CambiarLuz(false);
                return;
            }
        }

        // ------------------------------------------
        // INTENCIÃ“N: CONTROL_DOOR
        // ------------------------------------------

        if (comando.Contains("puerta"))
        {
            ultimaIntencion = "CONTROL_DOOR";
            ultimaEntidad = "PuertaVirtual";

            if (comando.Contains("abre") ||
                comando.Contains("abrir"))
            {
                CambiarPuerta(true);
                return;
            }

            if (comando.Contains("cierra") ||
                comando.Contains("cerrar"))
            {
                CambiarPuerta(false);
                return;
            }
        }

        // Si no reconocemos la frase:
        respuesta = "No comprendo esa instrucciÃ³n.";

        Debug.LogWarning(
            "Comando desconocido: " + frase
        );
    }


    // ==========================================
    // 10. NORMALIZACIÃ“N DEL TEXTO
    // ==========================================

    private string Normalizar(string texto)
    {
        // Pasamos el texto a minÃºsculas.
        texto = texto.ToLowerInvariant();

        // Separamos caracteres y tildes.
        string normalizado =
            texto.Normalize(NormalizationForm.FormD);

        StringBuilder resultado = new StringBuilder();

        foreach (char caracter in normalizado)
        {
            // Ignoramos las marcas de acentuaciÃ³n.
            if (CharUnicodeInfo.GetUnicodeCategory(caracter)
                != UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(caracter);
            }
        }

        return resultado.ToString()
            .Normalize(NormalizationForm.FormC);
    }


    // ==========================================
    // 11. CAMBIAR EL COLOR DEL CUBO
    // ==========================================

    private void CambiarColor(Color color, string nombre)
    {
        materialCubo.color = color;

        respuesta = "Entendido. Cambiando color a " + nombre;

        // Notificamos que ocurriÃ³ el cambio.
        alCambiarColor.Invoke();

        ConfirmarAccion();
    }


    // ==========================================
    // 12. CONTROLAR LA ILUMINACIÃ“N
    // ==========================================

    private void CambiarLuz(bool encender)
    {
        luz.enabled = encender;

        respuesta = encender
            ? "Entendido. Encendiendo la luz."
            : "Entendido. Apagando la luz.";

        alCambiarLuz.Invoke();

        ConfirmarAccion();
    }


    // ==========================================
    // 13. CONTROLAR LA PUERTA
    // ==========================================

    private void CambiarPuerta(bool abrir)
    {
        puertaAbierta = abrir;

        if (abrir)
        {
            respuesta = "Entendido. Abriendo la puerta.";
        }
        else
        {
            respuesta = "Entendido. Cerrando la puerta.";
        }

        // Animamos la rotaciÃ³n en lugar de fijarla de golpe.
        Quaternion destino = abrir
            ? rotacionOriginal * Quaternion.Euler(0, 90, 0)
            : rotacionOriginal;

        if (animacionPuerta != null)
            StopCoroutine(animacionPuerta);

        animacionPuerta = StartCoroutine(
            AnimarPuerta(puerta.rotation, destino)
        );

        alMoverPuerta.Invoke();

        ConfirmarAccion();
    }


    private System.Collections.IEnumerator AnimarPuerta(
        Quaternion desde, Quaternion hasta)
    {
        float tiempo = 0f;

        while (tiempo < duracionPuerta)
        {
            tiempo += Time.deltaTime;

            puerta.rotation = Quaternion.Slerp(
                desde, hasta, tiempo / duracionPuerta
            );

            yield return null;
        }

        puerta.rotation = hasta;
    }


    // ==========================================
    // 14. RESTABLECER EL LABORATORIO
    // ==========================================

    private void RestablecerEntorno()
    {
        materialCubo.color = colorOriginal;
        luz.enabled = luzOriginal;
        puerta.rotation = rotacionOriginal;

        puertaAbierta = false;

        ultimaIntencion = "RESET_ENVIRONMENT";
        ultimaEntidad = "Laboratorio";

        respuesta = "Entorno restablecido correctamente.";

        ConfirmarAccion();
    }


    // ==========================================
    // 15. RESPUESTA DEL ASISTENTE
    // ==========================================

    private void ConfirmarAccion()
    {
        // Mostrar confirmaciÃ³n en la consola.
        Debug.Log(respuesta);

        // Si se asignÃ³ una grabaciÃ³n de voz,
        // reproducimos el audio.
        if (vozAudio != null &&
            audioConfirmacion != null)
        {
            vozAudio.PlayOneShot(audioConfirmacion);
        }
    }


    // ==========================================
    // 16. INTERFAZ VISUAL DEL SIMULADOR
    // ==========================================

    private void OnGUI()
    {
        // Creamos un panel en la esquina superior izquierda.
        GUILayout.BeginArea(
            new Rect(15, 15, 435, 420),
            GUI.skin.box
        );

        GUILayout.Label("XR VOICE LAB - UNITY 6");

        GUILayout.Space(8);

        GUILayout.Label(
            escuchando
                ? "ESTADO: ASISTENTE ACTIVADO"
                : "ESTADO: ASISTENTE DESACTIVADO"
        );

        GUILayout.Space(10);

        GUILayout.Label("Num 0: Activar / Desactivar");
        GUILayout.Label("Num 1: Cubo rojo");
        GUILayout.Label("Num 2: Cubo azul");
        GUILayout.Label("Num 3: Encender / Apagar luz");
        GUILayout.Label("Num 4: Abrir / Cerrar puerta");
        GUILayout.Label("Num 5: Restablecer entorno");
        GUILayout.Label("Num 6: Cubo amarillo");
        GUILayout.Label("Num 7: Cubo violeta");

        GUILayout.Space(12);

        GUILayout.Label("Escribir una instrucciÃ³n:");

        // Campo para ingresar frases alternativas.
        textoIngresado = GUILayout.TextField(
            textoIngresado,
            GUILayout.Width(395)
        );

        // BotÃ³n para interpretar la frase escrita.
        if (GUILayout.Button("INTERPRETAR COMANDO"))
        {
            ProcesarFrase(textoIngresado);
        }

        GUILayout.Space(10);

        GUILayout.Label("Frase: " + ultimaFrase);
        GUILayout.Label("Intent: " + ultimaIntencion);
        GUILayout.Label("Entity: " + ultimaEntidad);

        GUILayout.Space(8);

        GUILayout.Label("Respuesta:");
        GUILayout.Label(respuesta);

        GUILayout.EndArea();
    }
}