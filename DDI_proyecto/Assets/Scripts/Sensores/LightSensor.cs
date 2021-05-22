﻿using UnityEngine;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using UnityEngine.UI;

using System;

public class LightSensor : MonoBehaviour
{
    /*Configuración del cliente*/
    public string brokerEndpoint = "test.mosquitto.org";
    public string lightTopic = "light1";
	public int brokerPort = 1883;
    private MqttClient client;

    /*Atributos del sensor*/
    public bool lightState = true;
    public int lightIntensity = 10;
    public float reportRate = 5f;
    private float reportTimer = 0f;

    // Use this for initialization
	void Start () {
		// create client instance 
		client = new MqttClient(brokerEndpoint, brokerPort, false, null);
		string clientId = Guid.NewGuid().ToString(); 
		client.Connect(clientId); 

        /*Subscribirme al evento que se dispara cuando se detecta un comando de voz en el actuator.*/
        Actuator changeLightState = GameObject.FindObjectOfType<Actuator>();
        changeLightState.onLightStateChange+=OnLightStateChange;

        changeLightState.onIntensityLightChange+=OnIntensityLightChange;
       
	}

    void Update ()
    {
        if(!client.IsConnected){
            Debug.LogWarning("Sensor 1: No conectado");
            return;
        }

        if((reportTimer += Time.deltaTime) >= reportRate){
            String message = lightState.ToString() + "," + lightIntensity.ToString();
            Debug.Log($"[LightSensor-1]Sending report topic: {lightTopic}, Status: {message}...");
			client.Publish(lightTopic, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
			Debug.Log("sent");
            reportTimer=0f;
        }
    }

    public void OnLightStateChange(bool state, int id)
    {
        Debug.Log("LLEGUE AL EVENTO EN SENSOR 1");
        if(id == 0 || id == 1)
        {
            Debug.Log($"[LightSensor-1] Comando recibido, estado={state}, id={id}");
            this.lightState = state;
        }
    }

    public void OnIntensityLightChange(int intensity, int id)
    {
        Debug.Log("LLEGUE AL EVENTO EN SENSOR");
        if(id == 1)
        {
            Debug.Log($"[LightSensor-1] Comando recibido, intesidad={intensity}, id={id}");
            this.lightIntensity = intensity/10;
        }
    }

    public void ToggleLight(){   /*Cambiar el valor en el sensor, para que este le diga al subscritor que apague o prenda la luz*/
        Debug.Log("[LightSensor-1] Se hizo toogle");
        this.lightState = !(this.lightState);
    }

	void OnApplicationQuit()
	{
		client.Disconnect();
	}
}
