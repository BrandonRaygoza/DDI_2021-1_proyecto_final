using UnityEngine;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using UnityEngine.UI;

using System;

public class MotionSensor : MonoBehaviour
{
    /*Configuración del cliente*/
    public string brokerEndpoint = "test.mosquitto.org";
    public string motionTopic = "motionSensor";
	public int brokerPort = 1883;
    private MqttClient client;

    /*Atributos del sensor*/
    public bool isInZone = false;
    public float reportRate = 1f;
    private float reportTimer = 0f;

    // Use this for initialization
	void Start () {
		// create client instance 
		client = new MqttClient(brokerEndpoint, brokerPort, false, null);
		string clientId = Guid.NewGuid().ToString(); 
		client.Connect(clientId); 
	}

    void Update ()
    {
        if(!client.IsConnected){
            Debug.LogWarning("Sensor de movimiento: No conectado");
            return;
        }

        //if((reportTimer += Time.deltaTime) >= reportRate){
            String message = isInZone.ToString();
            Debug.Log($"[MOTIONSENSOR] Sending report topic: {motionTopic}, Status: {message}...");
			client.Publish(motionTopic, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
			Debug.Log("Sent");
            reportTimer=0f;
        //}
    }

    void OnTriggerEnter(Collider other){  //Es un collider que entre dentro del area de la esfera.
        if(!other.CompareTag("intruso")){
            return;
        }else{
            isInZone = true;
            Debug.Log("Entro en el area");
            //interactionButtonUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other){
        if(!other.CompareTag("intruso")){
            return;
        }else{
            isInZone = false;
            Debug.Log("Se salio del area");
            //interactionButtonUI.SetActive(false);
        }
    }
}
