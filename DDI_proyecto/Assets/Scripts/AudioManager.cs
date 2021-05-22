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
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
     /*Parametros para establecer conexion*/
    public string brokerEndpoint = "test.mosquitto.org";
	public int brokerPort = 1883;
    public string topic = "musicTopic";
    private MqttClient client;

    string lastMessage;

    public AudioClip[] musicList;
    private AudioSource source;
    private int currentSong;
    private int songLength;
    private int playTime;
    private int minutes;
    private int seconds;
    
    public Text songTitle;
    public Text songTime;

    public List<string> voiceCommands;
    volatile bool musicIsPlaying = false;
    volatile bool nextSong = false;
    volatile bool previusSong = false;

    /*Evento disparado desde el actuador*/
    public delegate void OnMusicCommandReceived(bool state, int id);
    public OnMusicCommandReceived onMusicCommandReceived;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        // create client instance 
		client = new MqttClient(brokerEndpoint, brokerPort, false, null);
        
        // register to message received 
		client.MqttMsgPublishReceived += client_MqttMsgPublishReceived; 
		
		string clientId = Guid.NewGuid().ToString(); 
		client.Connect(clientId);

        /*Subscribirse al topico en particular (verificar inspector)*/
        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }); 

        /*Para detectar comandos de voz*/
        VoiceCommandProcessor commandProcessor = GameObject.FindObjectOfType<VoiceCommandProcessor>();
        commandProcessor.onCommandRecognized += OnCommandRecognized; //subscribirme al evento
    }

    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) 
	{ 
		Debug.Log("[AUDIOMANAGER] Received: " + System.Text.Encoding.UTF8.GetString(e.Message)  );
		lastMessage = System.Text.Encoding.UTF8.GetString(e.Message);

        string [] subs = lastMessage.Split(',');
        musicIsPlaying = Convert.ToBoolean(subs[0]);
        nextSong = Convert.ToBoolean(subs[1]);
        previusSong = Convert.ToBoolean(subs[2]);
        Debug.Log($"[AUDIOMANAGER] play? = {musicIsPlaying}, nextSong?={nextSong}, previusSong={previusSong}");
	}

    // Update is called once per frame
    void Update()
    {
        if(musicIsPlaying)  
            PlayMusic();
        else
            StopMusic();
        
        if(nextSong)
        {
            NextSong();
            nextSong = false;
        }
            
        if(previusSong){
            PreviusSong();
            previusSong = false;
        }
            
    }

    /*Para interaccion por voz*/
    public void OnCommandRecognized(string command)
    {
        initVoiceCommands();
        Debug.Log($"[AUDIOMANAGER] Recibi: {command}");

        foreach(var com in voiceCommands)
        {
            if(com.Equals("stop music ") && command.Equals("stop music ")) //Es el unico caso donde se envia false
            {
                Debug.Log($"[AUDIOMANAGER] Comando a ejecutar: {command}");
                onMusicCommandReceived?.Invoke(false, voiceCommands.IndexOf(command));
                break;
            }
            if(com.Equals(command))
            {
                Debug.Log($"[AUDIOMANAGER] Comando a ejecutar: {command}");
                onMusicCommandReceived?.Invoke(true, voiceCommands.IndexOf(command)); 
                break;
            }
        }
    }

    public void initVoiceCommands()
    {
        voiceCommands.Add("play music ");
        voiceCommands.Add("stop music ");
        voiceCommands.Add("next song ");
        voiceCommands.Add("previous song ");
    }

    public void PlayMusic() //No usar Play, porque ya lo usa Unity
    {
        if(source.isPlaying)
        {
            return;
        }
        currentSong --;
        if(currentSong < 0)
        {
            currentSong = musicList.Length -1; //que de la vuelta
        }
        StartCoroutine("WaitForMusicEnd");
    }

    IEnumerator WaitForMusicEnd()
    {
        while(source.isPlaying)
        {
            yield return null;
            playTime = (int)source.time;
            DisplayCurrentTime();
        }
        NextSong(); //si ya acabo, que se pase a la sig. cancion
    }

    public void NextSong()
    {
        source.Stop();  /*Deten la que esta ahorita*/
        currentSong++;  
        if(currentSong > musicList.Length-1)
        {
            currentSong=0;
        }

        source.clip = musicList[currentSong];   //Inicializa la sig canción
        source.Play();  //Comienza a reproducirla

        DisplayCurrentTitle(); //mostrar en pantalla titulo
        StartCoroutine("WaitForMusicEnd");
    }

    public void PreviusSong()
    {
        source.Stop();  /*Deten la que esta ahorita*/
        currentSong--;  
        if(currentSong < 0)
        {
            currentSong = musicList.Length-1;
        }

        source.clip = musicList[currentSong];   //Inicializa la sig canción
        source.Play();  //Comienza a reproducirla

        DisplayCurrentTitle(); //mostrar en pantalla titulo
        StartCoroutine("WaitForMusicEnd");
    }

    public void StopMusic()
    {
        StopCoroutine("WaitForMusicEnd");
        source.Stop();
    }

    void DisplayCurrentTitle()
    {
        songTitle.text = source.clip.name;
        songLength = (int)source.clip.length;
    }

    void DisplayCurrentTime()
    {
        seconds = playTime % 60;
        minutes = (playTime / 60) % 60;
        songTime.text = minutes + ":" + seconds.ToString("D2")+"/"+((songLength/60)%60)+":"+(songLength/60).ToString("D2") ;
    }
}
