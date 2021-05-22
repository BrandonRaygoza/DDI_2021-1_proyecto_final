using UnityEngine;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using UnityEngine.UI;

using System;

public class MusicSensor : MonoBehaviour
{
    /*Configuración del cliente*/
    public string brokerEndpoint = "test.mosquitto.org";
    public string musicTopic = "musicTopic";
	public int brokerPort = 1883;
    private MqttClient client;

    /*Atributos del sensor*/
    public bool isPlaying = false;       //1 = play, 0 = stop
    public bool nextSong = false; 
    public bool previusSong = false;   
    public float reportRate = 5f;
    private float reportTimer = 0f;

    void Start () {
		// create client instance 
		client = new MqttClient(brokerEndpoint, brokerPort, false, null);
		string clientId = Guid.NewGuid().ToString(); 
		client.Connect(clientId); 

        AudioManager handleAudioManager = GameObject.FindObjectOfType<AudioManager>();
        handleAudioManager.onMusicCommandReceived+=OnMusicCommandReceived;
   
	}

    void Update ()
    {
        if(!client.IsConnected){
            Debug.LogWarning("Music Sensor: No conectado");
            return;
        }

        if((reportTimer += Time.deltaTime) >= reportRate){
            String message = isPlaying.ToString() + "," + nextSong.ToString() + "," + previusSong.ToString();
            Debug.Log($"[MUSICSENSOR] Sending report topic: {musicTopic}, Status: {message}...");
			client.Publish(musicTopic, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
			Debug.Log("sent");
            reportTimer=0f;
            nextSong = false;       /*Solo quiero que se mande una vez la orden*/
            previusSong = false;
        }
    }

    public void OnMusicCommandReceived(bool state, int id)
    {
        Debug.Log("LLEGUE AL EVENTO EN AUDIOMANAGER");
        switch(id)
        {
            case 0: Debug.Log($"[MUSICSENSOR] Comando recibido: {state}");
                    this.isPlaying = state;
                    break;
            case 1: Debug.Log($"[MUSICSENSOR] Comando recibido: {state}");
                    this.isPlaying = state;
                    break;
            case 2: Debug.Log($"[MUSICSENSOR] Comando recibido: {state}");
                    this.nextSong = state;
                    break;
            case 3: Debug.Log($"[MUSICSENSOR] Comando recibido: {state}");
                    this.previusSong = state;
                    break;
        }
    }
}
