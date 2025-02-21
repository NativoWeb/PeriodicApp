using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text TextTimer;  // Referencia al componente Text de la UI
    public float segundos = 30f;

    void Start()
    {
        // Establece el texto inicial en 00:00
        ActualizarTextoTiempo();
    }

    void Update()
    {
        // Incrementa el tiempo transcurrido en cada frame
        if (segundos > 0)
        {
            segundos -= Time.deltaTime;  // Resta el tiempo en cada frame
        }
        else
        {
            segundos = 0;  // Evita que el contador se vuelva negativo
        }

        // Actualiza el texto en la UI
        ActualizarTextoTiempo();
    }

    // Método para actualizar el texto en la UI con formato de minutos:segundos
    void ActualizarTextoTiempo()
    {
        TextTimer.text = segundos.ToString("00" + " Segundos");

    }
}
