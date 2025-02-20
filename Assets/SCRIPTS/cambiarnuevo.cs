using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class cambiarnuevo : MonoBehaviour
{

    [SerializeField] private GameObject m_loginUI = null;
    [SerializeField] private GameObject m_registroUI = null;
    [SerializeField] private GameObject m_errorUI = null;


    public TMP_InputField inputemail; 
    public TMP_InputField inputpassword;
    public TMP_Text txtnotificacion;



    // funcion ver login 
    public void showlogin()
    {
        m_loginUI.SetActive(true);
        m_registroUI.SetActive(false);

    }
    //funcion ver registro
    public void showregistro()
    {
        m_registroUI.SetActive(true);
        m_loginUI.SetActive(false);


    }

   public void loginUser()
    {
        if(string.IsNullOrEmpty(inputemail.text)&& string.IsNullOrEmpty(inputpassword.text))
        {
            notificacionerror("Ningun campo puede quedar vacío, por favor ingrese email y contraseña");
            return;

        }

    }

    private void notificacionerror(string message)
    {
        txtnotificacion.text = "" + message;

        m_errorUI.SetActive(true);

    }

    private void closenotificacion()
    {
        txtnotificacion.text = "";

        m_errorUI.SetActive(false);
    }


}
