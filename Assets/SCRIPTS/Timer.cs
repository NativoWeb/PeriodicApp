using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text TextTimer;  // Referencia al componente Text de la UI
    public float segundos = 30f;
    private bool enEspera = false; // Para controlar la pausa del temporizador

    void Start()
    {
        ActualizarTextoTiempo();
    }

    void Update()
    {
        if (!enEspera) // Solo resta tiempo si no está en espera
        {
            if (segundos > 0)
            {
                segundos -= Time.deltaTime;
            }
            else
            {
                StartCoroutine(MostrarMismaPreguntaPor5Segundos());
            }
        }

        ActualizarTextoTiempo();
    }

    IEnumerator MostrarMismaPreguntaPor5Segundos()
    {
        enEspera = true; // Pausar el temporizador
        segundos = 0; // Para que no muestre valores negativos
        ActualizarTextoTiempo();

        yield return new WaitForSeconds(5f); // Espera 5 segundos

        segundos = 30f; // Reinicia el temporizador
        enEspera = false; // Reactiva la cuenta regresiva
    }

    void ActualizarTextoTiempo()
    {
        TextTimer.text = segundos.ToString("00") + " Segundos";
    }
}
