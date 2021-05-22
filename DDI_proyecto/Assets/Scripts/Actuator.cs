using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using UnityEngine.UI;

using IBM.Watsson.Examples;

public class Actuator : MonoBehaviour
{
    /*Parametros para establecer conexion*/
    public string brokerEndpoint = "test.mosquitto.org";
	public int brokerPort = 1883;
	//public string topic = "light1";

    /*Para mi beneficio*/
    public string topic = "motionSensor";
    public List<string> topics;  
    public List<string> voiceCommands;
    string lastMessage;

    /*Evento disparado desde el actuador*/
    public delegate void OnLightStateChange(bool state, int id);
    public OnLightStateChange onLightStateChange;

    public delegate void OnIntensityLightChange(int intensity, int id);
    public OnIntensityLightChange onIntensityLightChange;

    private MqttClient client;

    /*Referencia a GameObjects, recibe uno diferente desde el inspector*/
    public Light[] lights;
    public Text[] textsLights;
    public Text textMoveSensor;
    //public GameObject light1;

    //public GameObject aireAcondicionado;
    volatile bool[] lightState = new bool [7];
    volatile int[] lightIntensity = new int [7];
    volatile bool isInZone = false;
   
    // Use this for initialization
	void Start () 
    {
        initTopics();   /*Solo para topicos relacionados a las luces*/

		// create client instance 
		client = new MqttClient(brokerEndpoint, brokerPort, false, null);
        
        // register to message received 
		client.MqttMsgPublishReceived += client_MqttMsgPublishReceived; 
		
		string clientId = Guid.NewGuid().ToString(); 
		client.Connect(clientId); 

        // subscribirse a los topicos de luces
        foreach (string topic in topics)
        {
            client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        /*Subscribirse al topico en particular (verificar inspector)*/
        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
		
        /*Para detectar comandos de voz*/
        VoiceCommandProcessor commandProcessor = GameObject.FindObjectOfType<VoiceCommandProcessor>();
        commandProcessor.onCommandRecognized += OnCommandRecognized; //subscribirme al evento
	}

    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) 
	{ 
		Debug.Log("[ACTUADOR] Received: " + System.Text.Encoding.UTF8.GetString(e.Message)  );
		lastMessage = System.Text.Encoding.UTF8.GetString(e.Message);

        for(int i = 0; i<topics.Count; i++)
        {
            if(e.Topic.Equals(topics[i]))
            {
                if(lastMessage.Contains("False"))
                {
                    lightState[i] = false;
                }else if(lastMessage.Contains("True"))
                {
                    lightState[i] = true;
                }
                string [] subs = lastMessage.Split(',');
                lightIntensity[i] = Convert.ToInt32(subs[1]);
            }
        }

        if(e.Topic.Equals(topic))
        {
            if(lastMessage.Contains("False"))
            {
                isInZone = false;
            }else if(lastMessage.Contains("True"))
            {
                isInZone = true;
            }
        }
	}

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i<lightState.Length; i++)
        {
            if(lightState[i] == true)
            {
                Debug.Log($"LUZ {i}, ENCENDIDA");
                textsLights[i].text = "Luz "+(i+1)+" ON";
            }
            else if(lightState[i] == false)
            {
                Debug.Log($"LUZ {i}, APAGADA");
                textsLights[i].text = "Luz "+(i+1)+" OFF";
            }
            lights[i].enabled = lightState[i]; 
            lights[i].intensity = lightIntensity[i]; 
        }

        if(isInZone == true)
        {
            Debug.Log("¡Instruso detectado!");
            textMoveSensor.text = "¡ALERTA! Se detecto movimiento";
        }else if(isInZone == false){
            Debug.Log("Todo bien en el patio");
            textMoveSensor.text = "No hay movimiento detectado";
        }
        
    }

    /*Para interaccion por voz*/
    public void OnCommandRecognized(string command)
    {
        int start;
        initVoiceCommands();

        Debug.Log($"[ACTUADOR] Recibi: {command}");
        if(command.Contains(" light ")) /*¿A quien vamos a modificar?*/
        {
            if(command.Contains(" on ")) /*¿Qué vamos a hacer? (poner espacios porque puede agarrar el "one")*/
            {
                onLightStateChange?.Invoke(true,voiceCommands.IndexOf(command)); 
            }
            else if(command.Contains(" off "))
            {
                onLightStateChange?.Invoke(false,voiceCommands.IndexOf(command)); /*Para que siga siendo su ID base*/
            }
        }else if(command.Contains("intensity"))
        {
            Debug.Log("Comando de intensidad recibido");
            start = command.IndexOf("to ");                         /*Primero: Verificar cuanto % se pide*/
            string intensityString = command.Substring(start+3,2);

            try {
                int intensity = Convert.ToInt32(intensityString);   /*Pasarlo a entero*/
                if(command.Contains("one"))                         /*Segundo: Verificar a que foco se aplica*/
                {
                    onIntensityLightChange?.Invoke(intensity,1);    /*Caso especial, el algoritmo no pone "1" pone "one"**/
                }
                else{
                    int lightId = Convert.ToInt32(command.Substring(start-2,1));    /*Pasar a entero el id (el numero de foco)*/
                    onIntensityLightChange?.Invoke(intensity,lightId);
                }
                
                Debug.Log("Comando de intensidad recibido" + intensity);
            }
            catch (OverflowException) {
                Console.WriteLine("{0} is outside the range of the Int32 type.");
            }
            catch (FormatException) {
                Console.WriteLine("The {0} value '{1}' is not in a recognizable format.");
            }   
        }
    }

    /*Solo para topicos relacionados a las luces*/
    private void initTopics()
    {
        topics.Add("light1");
        topics.Add("light2");
        topics.Add("light3");
        topics.Add("light4");
        topics.Add("light5");
        topics.Add("light6");
        topics.Add("light7");
    }

    /*Estan acomodados de manera que si es off, solo le resto 1 y se mantiene el mismo id, que es el index en el arreglo*/
    private void initVoiceCommands()
    {
        voiceCommands.Add("turn on the light number one ");
        voiceCommands.Add("turn off the light number one ");
        voiceCommands.Add("turn on the light number 2 ");
        voiceCommands.Add("turn off the light number 2 ");
        voiceCommands.Add("turn on the light number 3 ");
        voiceCommands.Add("turn off the light number 3 ");
        voiceCommands.Add("turn on the light number 4 ");
        voiceCommands.Add("turn off the light number 4 ");
        voiceCommands.Add("turn on the light number 5 ");
        voiceCommands.Add("turn off the light number 5 ");
        voiceCommands.Add("turn on the light number 6 ");
        voiceCommands.Add("turn off the light number 6 ");
        voiceCommands.Add("turn on the light number 7 ");
        voiceCommands.Add("turn off the light number 7 ");
        voiceCommands.Add("intensity number one to ");
    }
}
