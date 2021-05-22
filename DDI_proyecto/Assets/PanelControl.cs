using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelControl : MonoBehaviour
{
    public GameObject panelPrincipal;
    public GameObject panelLuces;
   
    public void ChangePanel(string panel)
    {
        if(panel.Equals("panelLuces"))
        {
            panelPrincipal.SetActive(false);
            panelLuces.SetActive(true);
        }
    }
}
