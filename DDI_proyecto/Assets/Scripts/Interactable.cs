using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
//using IBM.Watsson.Examples;

public class Interactable : MonoBehaviour
{

    public bool inZone = false;
    public bool gazedAt = false; //VR
    public float gazeTimer = 0;
    public float maxGazeTime = 4f;
    //public KeyCode interactionKey = KeyCode.P;
    public string interactionButton = "Interact"; //Como lo nombre en el InputManager
    public GameObject interactionButtonUI;

    public string voiceCommand = "Hi";

    //void Start()
    //{
    //    VoiceCommandProcessor commandProcessor = GameObject.FindObjectOfType<VoiceCommandProcessor>();
    //    commandProcessor.onCommandRecognized += OnCommandRecognized; //subscribirme al evento
    //}

    public virtual void Update(){
        //if(inZone && CrossPlatformInputManager.GetButtonDown(interactionButton)){
        //    Interact();
        //}

        /*Para VR (quite interact para el voice command)*/
        if(gazedAt){ /*¿Lo estoy viendo?*/
            if((gazeTimer += Time.deltaTime) >= maxGazeTime){
                Interact(); /*Para VR, que se efectue el interact cuando pase el tiempo necesario*/
                gazedAt = false;
                gazeTimer = 0f;
            }
        }
        
    }

    /*Para interaccion por voz*/
    //public void OnCommandRecognized(string command)
    //{
    //    if(command.ToLower() == voiceCommand.ToLower() && gazedAt) /*El comando dicho es el registrado en el objeto y, ¿lo estoy viendo?*/
    //    {
    //        Interact();
    //    }
    //}

    /*Para VR*/
    public void SetGazedAt(bool gazedAt)
    {
        this.gazedAt = gazedAt; /*¿Lo estoy viendo?*/
        Debug.Log("Te estoy mirando");
        if(!gazedAt){
            gazeTimer = 0f;
        }
    }

    void OnTriggerEnter(Collider other){  //Es un collider que entre dentro del area de la esfera.
        if(!other.CompareTag("intruso")){
            return;
        }else{
            inZone = true;
            Debug.Log("Entro en el area");
            //interactionButtonUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other){
        if(!other.CompareTag("intruso")){
            return;
        }else{
            inZone = false;
            Debug.Log("Se salio del area");
            //interactionButtonUI.SetActive(false);
        }
    }

    

    /*Para presionar dedo en pantalla tactil*/
    //void OnMouseDown() {
    //    Interact();
    //}

    public virtual void Interact(){ //puede estar vacio, pero en otras clases se puede extender

    }
}
