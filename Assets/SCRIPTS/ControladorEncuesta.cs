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
    private int preguntaActualIndex = 0;      // �ndice de la pregunta actual en la lista aleatoria
    private Pregunta preguntaActual;           // Para guardar la pregunta que se est� mostrando actualmente
    private List<string> opcionesAleatorias;
    private List<Pregunta> preguntasAleatorias;
    //[SerializeField] private ToggleGroup grupoOpciones;
    //[SerializeField] private Toggle[] togglesOpciones;
    private bool eventosToggleHabilitados = false;



    // Start is called before the first frame update
    void Start()
    {
        eventosToggleHabilitados = false; // Expl�citamente lo ponemos en false al inicio
        CargarPreguntasDesdeJSON();
        AleatorizarPreguntas();
        ConfigurarToggleListeners();

        // Asegurarnos que los toggles est�n limpios
        foreach (Toggle toggle in opcionesToggleUI)
        {
            toggle.interactable = true;
            toggle.isOn = false;
        }

        MostrarPreguntaActual();
        botonSiguientePreguntaUI.interactable = false;
        OcultarFeedbackRespuestas();

        // Ahora s� habilitamos los eventos
        eventosToggleHabilitados = true;
        Debug.Log("Eventos de Toggle habilitados correctamente");
    }

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
            // Deserializar el JSON a un objeto.  Aqu� necesitamos una clase que coincida con la estructura del JSON.
            // Vamos a crear una clase para 'GrupoPreguntasWrapper' que contendr� la lista 'gruposPreguntas'
            GrupoPreguntasWrapper wrapper = JsonUtility.FromJson<GrupoPreguntasWrapper>(jsonString);
            if (wrapper != null && wrapper.gruposPreguntas != null)
            {
                preguntas = new List<Pregunta>(); // Inicializa la lista de preguntas (vaciando la anterior si existiera)
                foreach (var grupo in wrapper.gruposPreguntas)
                {
                    foreach (var elemento in grupo.elementos)
                    {
                        preguntas.AddRange(elemento.preguntas); // A�ade todas las preguntas de cada elemento a la lista principal 'preguntas'
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

    // M�todo para ocultar todos los iconos de feedback (checks y equis)
    void OcultarFeedbackRespuestas()
    {
        // Recorre todos los arrays de imagenes de feedback (check y equis)
        for (int i = 0; i < imagenesCheckRespuestaUI.Length; i++)
        {
            if (imagenesCheckRespuestaUI[i] != null)
            {
                imagenesCheckRespuestaUI[i].gameObject.SetActive(false); // Desactiva la imagen del check
            }
            if (imagenesEquisRespuestaUI[i] != null)
            {
                imagenesEquisRespuestaUI[i].gameObject.SetActive(false); // Desactiva la imagen de la equis
            }
        }
    }

    // M�todo para reactivar la interactividad de las opciones (Toggles) para la siguiente pregunta
    void ActivarInteractividadOpciones()
    {
        foreach (Toggle toggle in opcionesToggleUI)
        {
            toggle.interactable = true; // Reactiva la interactividad de cada Toggle de opci�n
        }
    }

    // M�todo para aleatorizar el orden de las opciones de respuesta y asegurar que la correcta est� entre ellas
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

        // Asegurar que la respuesta correcta est� siempre presente en las opciones aleatorizadas (esto es importante!)
        if (!opcionesAleatorias.Contains(respuestaCorrecta))
        {
            opcionesAleatorias[0] = respuestaCorrecta; // Si por alguna raz�n no est�, la coloca en la primera posici�n (puedes cambiar la posici�n si quieres)
        }
        return opcionesAleatorias;
    }


    void MostrarPreguntaActual()
    {
        OcultarFeedbackRespuestas();

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

            // Aleatorizar opciones y actualizar �ndice correcto
            opcionesAleatorias = AleatorizarOpciones(preguntaActual.opcionesRespuesta, preguntaActual.indiceRespuestaCorrecta);

            // Recalcular el �ndice de la respuesta correcta en las opciones aleatorizadas
            string respuestaCorrectaOriginal = preguntaActual.opcionesRespuesta[preguntaActual.indiceRespuestaCorrecta];
            int nuevoIndiceCorrecto = opcionesAleatorias.IndexOf(respuestaCorrectaOriginal);
            preguntaActual.indiceRespuestaCorrecta = nuevoIndiceCorrecto; // Actualizar el �ndice

            // Asignar opciones a los Toggles
            for (int i = 0; i < opcionesToggleUI.Length; i++)
            {
                TextMeshProUGUI textoToggle = opcionesToggleUI[i].GetComponentInChildren<TextMeshProUGUI>();
                if (textoToggle != null)
                {
                    textoToggle.text = opcionesAleatorias[i];
                    opcionesToggleUI[i].isOn = false;
                }
            }
            grupoOpcionesUI.SetAllTogglesOff();
        }
        else
        {
            Debug.Log("Encuesta Finalizada");
            textoPreguntaUI.text = "�Encuesta Finalizada!";
            grupoOpcionesUI.enabled = false;
            botonSiguientePreguntaUI.interactable = false;
        }
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
            preguntaActualIndex = 0; // Reinicia el �ndice de la pregunta actual al empezar la encuesta aleatorizada
        }
        else
        {
            Debug.LogWarning("No hay preguntas para aleatorizar o la lista de preguntas es nula.");
        }
    }

    // M�todo para desactivar la interactividad de las opciones (Toggles) despu�s de responder
    //void DesactivarInteractividadOpciones()
    //{
    //    foreach (Toggle toggle in opcionesToggleUI)
    //    {
    //        toggle.interactable = false; // Desactiva la interactividad de cada Toggle de opci�n
    //    }
    //}

    // M�todo para mostrar feedback visual de la respuesta (check o equis)
    void MostrarFeedbackRespuesta(bool esCorrecta, int indiceOpcion)
    {

        if (indiceOpcion < 0 || indiceOpcion >= imagenesCheckRespuestaUI.Length)
        {
            Debug.LogError("�ndice fuera de rango: " + indiceOpcion);
            return;
        }

        if (esCorrecta)
        {
            // Mostrar icono de CHECK en la opci�n seleccionada como CORRECTA
            imagenesCheckRespuestaUI[indiceOpcion].gameObject.SetActive(true); // Activar imagen de check para la opci�n correcta
                                                                               // Puedes a�adir aqu� feedback adicional, como cambiar el color del Toggle a verde, etc.
        }
        else
        {
            // Mostrar icono de EQUIS en la opci�n seleccionada como INCORRECTA
            imagenesEquisRespuestaUI[indiceOpcion].gameObject.SetActive(true); // Activar imagen de equis para la opci�n incorrecta
                                                                               // Puedes a�adir aqu� feedback adicional, como cambiar el color del Toggle a rojo, etc.

            // Opcional: Mostrar tambi�n la respuesta CORRECTA (puedes decidir si quieres mostrar la correcta aunque falle)
            imagenesCheckRespuestaUI[preguntaActual.indiceRespuestaCorrecta].gameObject.SetActive(true); // Activar imagen de check en la respuesta correcta (para indicar cu�l era)
        }
    }

    private void ConfigurarToggleListeners()
    {
        if (opcionesToggleUI == null || opcionesToggleUI.Length == 0)
        {
            Debug.LogError("opcionesToggleUI no est� asignado o est� vac�o");
            return;
        }

        foreach (Toggle toggle in opcionesToggleUI)
        {
            toggle.onValueChanged.RemoveAllListeners();
        }

        for (int i = 0; i < opcionesToggleUI.Length; i++)
        {
            int index = i; // �Captura el �ndice correctamente!
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

    // M�todo para verificar la respuesta seleccionada por el usuario
    public void VerificarRespuesta(int indiceOpcionSeleccionada)
    {
        //Debug.Log($"Estado de eventosToggleHabilitados: {eventosToggleHabilitados}");
        Debug.Log("Opci�n Seleccionada en verificar respuesta: " + indiceOpcionSeleccionada);

        if (!eventosToggleHabilitados) // <-- �NUEVA L�NEA: VERIFICAR SI EVENTOS TOGGLE EST�N HABILITADOS!
        {
            Debug.LogWarning("VerificarRespuesta llamada PREMATURAMENTE al inicio. Eventos Toggle NO habilitados a�n.  Saliendo del m�todo."); // <-- Mensaje de advertencia (opcional)
            return; // <-- �SALIR DEL M�TODO INMEDIATAMENTE si eventosToggleHabilitados es false!
        }

        if (preguntaActual == null)
        {
            Debug.LogError("preguntaActual es null");
            return;
        }

        //Debug.Log("�VERIFICAR RESPUESTA LLAMADA!  Opci�n Seleccionada: " + indiceOpcionSeleccionada); // <-- �NUEVA L�NEA Debug.Log!

        //Debug.Log($"Verificando respuesta. �ndice seleccionado: {indiceOpcionSeleccionada}, �ndice correcto: {preguntaActual.indiceRespuestaCorrecta}");
        // Desactivar la interactividad de las opciones y el bot�n "Siguiente Pregunta" una vez que se responde
        //DesactivarInteractividadOpciones();
        botonSiguientePreguntaUI.interactable = true;

        if (indiceOpcionSeleccionada == preguntaActual.indiceRespuestaCorrecta)
        {
            // �Respuesta CORRECTA!
            Debug.Log("�Respuesta Correcta!");
            MostrarFeedbackRespuesta(true, indiceOpcionSeleccionada); // Mostrar feedback de respuesta correcta
        }
        else
        {
            // �Respuesta INCORRECTA!
            Debug.Log("Respuesta Incorrecta");
            MostrarFeedbackRespuesta(false, indiceOpcionSeleccionada); // Mostrar feedback de respuesta incorrecta
        }

        // Preparar para la siguiente pregunta (puedes decidir cu�ndo avanzar a la siguiente pregunta, por ejemplo, con un bot�n)
        preguntaActualIndex++; // Incrementar el �ndice para la siguiente pregunta

        // Depuraci�n adicional
        Debug.Log($"�ndice seleccionado: {indiceOpcionSeleccionada} | �ndice correcto: {preguntaActual.indiceRespuestaCorrecta}");

    }

    public void siguienteEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }

    [Header("Referencias UI")]
    public TextMeshProUGUI textoPreguntaUI;
    public ToggleGroup grupoOpcionesUI;
    public Toggle[] opcionesToggleUI;
    public Button botonSiguientePreguntaUI;

    [Header("Referencias Feedback Respuesta")]
    public Image[] imagenesCheckRespuestaUI;
    public Image[] imagenesEquisRespuestaUI;


}
