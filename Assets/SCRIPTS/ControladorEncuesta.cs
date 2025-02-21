using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ControladorEncuesta;
using UnityEngine.SceneManagement;

public class ControladorEncuesta : MonoBehaviour
{
    private List<Pregunta> preguntas;
    private int preguntaActualIndex = 0;      // Índice de la pregunta actual en la lista aleatoria
    private Pregunta preguntaActual;           // Para guardar la pregunta que se está mostrando actualmente
    private List<string> opcionesAleatorias;
    private List<Pregunta> preguntasAleatorias;
    //[SerializeField] private ToggleGroup grupoOpciones;
    //[SerializeField] private Toggle[] togglesOpciones;
    private bool eventosToggleHabilitados = false;

    // Temporizador variables
    public float tiempoRestante = 30f;  // Tiempo inicial del temporizador en segundos (30 segundos)
    private bool preguntaFinalizada = false;  // Flag para saber si la pregunta ha sido finalizada (cuando se pasa a la siguiente pregunta)

    // Start is called before the first frame update
    void Start()
    {
        CargarPreguntasDesdeJSON();
        AleatorizarPreguntas();
        MostrarPreguntaActual();
        desmarcarToggle();
        ConfigurarToggleListeners();

        foreach (Toggle toggle in opcionesToggleUI)
        {
            toggle.onValueChanged.RemoveAllListeners();
        }

        for (int i = 0; i < opcionesToggleUI.Length; i++)
        {
            int index = i; // ¡Captura el índice correctamente!
            opcionesToggleUI[index].onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn && eventosToggleHabilitados)
                {
                    Debug.Log($"Toggle {index} activado");
                }
            });
        }

    }

    // Método para manejar el temporizador
    void Update()
    {

        // Solo actualizar el temporizador si la pregunta no ha sido finalizada
        if (!preguntaFinalizada)
        {

            if (tiempoRestante > 0)
            {
                tiempoRestante -= Time.deltaTime; // Reduce el tiempo
            }
            else if (!preguntaFinalizada) // Verifica que la pregunta aún no se ha respondido
            {
                preguntaFinalizada = true; // Evita que el código se ejecute varias veces en un solo frame
                StartCoroutine(MostrarMismaPreguntaPor5Segundos());
            }
        }
    }

    // Corutina para mostrar la misma pregunta por 5 segundos
    IEnumerator MostrarMismaPreguntaPor5Segundos()
    {
        // Mostrar la misma pregunta y desactivar toggles
        textoPreguntaUI.text = preguntaActual.textoPregunta;
        DesactivarInteractividadOpciones();

        // Desactivar el temporizador por 5 segundos
        yield return new WaitForSeconds(5f);

        // Pasar a la siguiente pregunta  activar toggles
        ActivarInteractividadOpciones();

        siguientePregunta();
    }

    void desmarcarToggle()
    {
        // Asegurarnos que los toggles estén limpios
        foreach (Toggle toggle in opcionesToggleUI)
        {
            toggle.interactable = true;
            toggle.isOn = false;
        }
    }

    // Método para cambiar a la siguiente pregunta
    void siguientePregunta()
    {
        preguntaFinalizada = true;
        preguntaActualIndex++; // Incrementamos el índice para la siguiente pregunta
        if (preguntaActualIndex < preguntasAleatorias.Count)
        {
            MostrarPreguntaActual();  // Mostrar la siguiente pregunta
            ActivarInteractividadOpciones();
            tiempoRestante = 30f;  // Reiniciar el temporizador para la nueva pregunta
            preguntaFinalizada = false;  // Permitir que el temporizador funcione de nuevo
        }
        else
        {
            Debug.Log("Encuesta Finalizada");
            textoPreguntaUI.text = "¡Encuesta Finalizada!";
            grupoOpcionesUI.enabled = false;
        }
    }

    // Aquí tienes el resto de tu código tal como está, con las funcionalidades existentes de cargar preguntas, verificar respuestas, etc...
    // Como has indicado que ya tienes todo el sistema de preguntas y opciones, solo necesitarías integrar la parte del temporizado

    [System.Serializable]
    public class Pregunta
    {
        public string textoPregunta;
        public List<string> opcionesRespuesta;
        public int indiceRespuestaCorrecta;
    }


    [System.Serializable]
    public class ElementoPreguntas
    {
        public string elemento;
        public List<Pregunta> preguntas;
    }

    [System.Serializable]
    public class GrupoPreguntas
    {
        public string grupo;
        public List<ElementoPreguntas> elementos;
    }

    [System.Serializable]
    public class GrupoPreguntasWrapper
    {
        public List<GrupoPreguntas> gruposPreguntas;
    }

    void CargarPreguntasDesdeJSON()
    {
        TextAsset archivoJSON = Resources.Load<TextAsset>("preguntas_tabla_periodica"); // Carga el archivo JSON desde la carpeta Resources
        if (archivoJSON != null)
        {
            string jsonString = archivoJSON.text;
            // Deserializar el JSON a un objeto.  Aquí necesitamos una clase que coincida con la estructura del JSON.
            // Vamos a crear una clase para 'GrupoPreguntasWrapper' que contendrá la lista 'gruposPreguntas'
            GrupoPreguntasWrapper wrapper = JsonUtility.FromJson<GrupoPreguntasWrapper>(jsonString);
            if (wrapper != null && wrapper.gruposPreguntas != null)
            {
                preguntas = new List<Pregunta>(); // Inicializa la lista de preguntas (vaciando la anterior si existiera)
                foreach (var grupo in wrapper.gruposPreguntas)
                {
                    foreach (var elemento in grupo.elementos)
                    {
                        preguntas.AddRange(elemento.preguntas); // Añade todas las preguntas de cada elemento a la lista principal 'preguntas'
                    }
                }
                Debug.Log("Se cargaron " + preguntas.Count + " preguntas desde JSON.");
            }
            else
            {
                Debug.LogError("Error al deserializar el JSON o estructura JSON incorrecta.");
            }
        }
        else
        {
            Debug.LogError("No se pudo encontrar el archivo JSON 'preguntas_tabla_periodica' en la carpeta Resources.");
        }
    }

    // Método para aleatorizar el orden de las opciones de respuesta y asegurar que la correcta esté entre ellas
    List<string> AleatorizarOpciones(List<string> opciones, int indiceCorrecto)
    {
        List<string> opcionesAleatorias = new List<string>(opciones); // Copia las opciones originales
        string respuestaCorrecta = opcionesAleatorias[indiceCorrecto]; // Guarda la respuesta correcta

        // Algoritmo de Fisher-Yates para aleatorizar la lista
        for (int i = 0; i < opcionesAleatorias.Count - 1; i++)
        {
            int randomIndex = Random.Range(i, opcionesAleatorias.Count);
            string temp = opcionesAleatorias[randomIndex];
            opcionesAleatorias[randomIndex] = opcionesAleatorias[i];
            opcionesAleatorias[i] = temp;
        }

        // Asegurar que la respuesta correcta esté siempre presente en las opciones aleatorizadas (esto es importante!)
        if (!opcionesAleatorias.Contains(respuestaCorrecta))
        {
            opcionesAleatorias[0] = respuestaCorrecta; // Si por alguna razón no está, la coloca en la primera posición (puedes cambiar la posición si quieres)
        }
        return opcionesAleatorias;
    }


    void MostrarPreguntaActual()
    {
        // Validar que hay preguntas disponibles
        if (preguntasAleatorias == null || preguntasAleatorias.Count == 0)
        {
            Debug.LogError("No hay preguntas cargadas.");
            return;
        }

        if (preguntaActualIndex < preguntasAleatorias.Count)
        {
            preguntaActual = preguntasAleatorias[preguntaActualIndex];
            ActivarInteractividadOpciones();
            textoPreguntaUI.text = preguntaActual.textoPregunta;

            // Aleatorizar opciones y actualizar índice correcto
            opcionesAleatorias = AleatorizarOpciones(preguntaActual.opcionesRespuesta, preguntaActual.indiceRespuestaCorrecta);

            // Recalcular el índice de la respuesta correcta en las opciones aleatorizadas
            string respuestaCorrectaOriginal = preguntaActual.opcionesRespuesta[preguntaActual.indiceRespuestaCorrecta];
            int nuevoIndiceCorrecto = opcionesAleatorias.IndexOf(respuestaCorrectaOriginal);
            preguntaActual.indiceRespuestaCorrecta = nuevoIndiceCorrecto; // Actualizar el índice

            // Asignar opciones a los Toggles
            for (int i = 0; i < opcionesToggleUI.Length; i++)
            {
                if (i < opcionesAleatorias.Count)  // Si hay una opción disponible
                {
                    opcionesToggleUI[i].gameObject.SetActive(true); // Asegurar que el Toggle esté activo
                    TextMeshProUGUI textoToggle = opcionesToggleUI[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (textoToggle != null)
                    {
                        textoToggle.text = opcionesAleatorias[i];
                    }
                    opcionesToggleUI[i].isOn = false;
                }
                else
                {
                    opcionesToggleUI[i].gameObject.SetActive(false); // Desactiva Toggles adicionales
                }
            }
            grupoOpcionesUI.SetAllTogglesOff();
        }
        else
        {
            Debug.Log("Encuesta Finalizada");
            textoPreguntaUI.text = "¡Encuesta Finalizada!";
            grupoOpcionesUI.enabled = false;
        }

        tiempoRestante = 30f; // Reinicia el temporizador al mostrar la nueva pregunta
    }


    void AleatorizarPreguntas()
    {
        if (preguntas != null && preguntas.Count > 0)
        {
            preguntasAleatorias = new List<Pregunta>(preguntas); // Crea una copia de la lista original
            // Usar algoritmo de Fisher-Yates para aleatorizar la lista copiada 'preguntasAleatorias'
            for (int i = 0; i < preguntasAleatorias.Count - 1; i++)
            {
                int randomIndex = Random.Range(i, preguntasAleatorias.Count);
                Pregunta temp = preguntasAleatorias[randomIndex];
                preguntasAleatorias[randomIndex] = preguntasAleatorias[i];
                preguntasAleatorias[i] = temp;
            }
            preguntaActualIndex = 0; // Reinicia el índice de la pregunta actual al empezar la encuesta aleatorizada
        }
        else
        {
            Debug.LogWarning("No hay preguntas para aleatorizar o la lista de preguntas es nula.");
        }
    }

    // Método para mostrar feedback visual de la respuesta (check o equis)
    //void MostrarFeedbackRespuesta(bool esCorrecta, int indiceOpcion)
    //{

    //    if (indiceOpcion < 0 || indiceOpcion >= imagenesCheckRespuestaUI.Length)
    //    {
    //        Debug.LogError("Índice fuera de rango: " + indiceOpcion);
    //        return;
    //    }

    //    if (esCorrecta)
    //    {
    //        // Mostrar icono de CHECK en la opción seleccionada como CORRECTA
    //        imagenesCheckRespuestaUI[indiceOpcion].gameObject.SetActive(true); // Activar imagen de check para la opción correcta
    //                                                                           // Puedes añadir aquí feedback adicional, como cambiar el color del Toggle a verde, etc.
    //    }
    //    else
    //    {
    //        // Mostrar icono de EQUIS en la opción seleccionada como INCORRECTA
    //        imagenesEquisRespuestaUI[indiceOpcion].gameObject.SetActive(true); // Activar imagen de equis para la opción incorrecta
    //                                                                           // Puedes añadir aquí feedback adicional, como cambiar el color del Toggle a rojo, etc.

    //        // Opcional: Mostrar también la respuesta CORRECTA (puedes decidir si quieres mostrar la correcta aunque falle)
    //        imagenesCheckRespuestaUI[preguntaActual.indiceRespuestaCorrecta].gameObject.SetActive(true); // Activar imagen de check en la respuesta correcta (para indicar cuál era)
    //    }
    //}

    private void ConfigurarToggleListeners()
    {
        if (opcionesToggleUI == null || opcionesToggleUI.Length == 0)
        {
            Debug.LogError("opcionesToggleUI no está asignado o está vacío");
            return;
        }

        foreach (Toggle toggle in opcionesToggleUI)
        {
            toggle.onValueChanged.RemoveAllListeners();
        }

        for (int i = 0; i < opcionesToggleUI.Length; i++)
        {
            int index = i; // ¡Captura el índice correctamente!
            opcionesToggleUI[index].onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn && eventosToggleHabilitados)
                {
                    Debug.Log($"Toggle {index} activado");
                    VerificarRespuesta(index);
                }
            });
        }
    }

    // Método para verificar la respuesta seleccionada por el usuario
    public void VerificarRespuesta(int indiceOpcionSeleccionada)
    {
        //Debug.Log($"Estado de eventosToggleHabilitados: {eventosToggleHabilitados}");
        Debug.Log("Opción Seleccionada en verificar respuesta: " + indiceOpcionSeleccionada);

        if (!eventosToggleHabilitados) // <-- ¡NUEVA LÍNEA: VERIFICAR SI EVENTOS TOGGLE ESTÁN HABILITADOS!
        {
            Debug.LogWarning("VerificarRespuesta llamada PREMATURAMENTE al inicio. Eventos Toggle NO habilitados aún.  Saliendo del método."); // <-- Mensaje de advertencia (opcional)
            return; // <-- ¡SALIR DEL MÉTODO INMEDIATAMENTE si eventosToggleHabilitados es false!
        }

        if (preguntaActual == null)
        {
            Debug.LogError("preguntaActual es null");
            return;
        }

        Debug.Log("¡VERIFICAR RESPUESTA LLAMADA!  Opción Seleccionada: " + indiceOpcionSeleccionada); // <-- ¡NUEVA LÍNEA Debug.Log!

        Debug.Log($"Verificando respuesta. Índice seleccionado: {indiceOpcionSeleccionada}, Índice correcto: {preguntaActual.indiceRespuestaCorrecta}");
        // Desactivar la interactividad de las opciones y el botón "Siguiente Pregunta" una vez que se responde
        DesactivarInteractividadOpciones();

        if (indiceOpcionSeleccionada == preguntaActual.indiceRespuestaCorrecta)
        {
            // ¡Respuesta CORRECTA!
            Debug.Log("¡Respuesta Correcta!");
            //MostrarFeedbackRespuesta(true, indiceOpcionSeleccionada); // Mostrar feedback de respuesta correcta
        }
        else
        {
            // ¡Respuesta INCORRECTA!
            Debug.Log("Respuesta Incorrecta");
            //MostrarFeedbackRespuesta(false, indiceOpcionSeleccionada); // Mostrar feedback de respuesta incorrecta
        }

        // Preparar para la siguiente pregunta (puedes decidir cuándo avanzar a la siguiente pregunta, por ejemplo, con un botón)
        preguntaActualIndex++; // Incrementar el índice para la siguiente pregunta

        // Depuración adicional
        Debug.Log($"Índice seleccionado: {indiceOpcionSeleccionada} | Índice correcto: {preguntaActual.indiceRespuestaCorrecta}");

    }
    
    // Método para reactivar la interactividad de las opciones (Toggles) para la siguiente pregunta
    void ActivarInteractividadOpciones()
    {
        foreach (Toggle toggle in opcionesToggleUI)
        {
            toggle.interactable = true; // Reactiva la interactividad de cada Toggle de opción
        }
    }

    void DesactivarInteractividadOpciones()
    {
        foreach (Toggle toggle in opcionesToggleUI)
        {
            toggle.interactable = false; // Desactiva la interactividad de cada Toggle de opción
        }
    }



    [Header("Referencias UI")]
    public TextMeshProUGUI textoPreguntaUI;
    public ToggleGroup grupoOpcionesUI;
    public Toggle[] opcionesToggleUI;

    [Header("Referencias Feedback Respuesta")]
    public Image[] imagenesCheckRespuestaUI;
    public Image[] imagenesEquisRespuestaUI;


}