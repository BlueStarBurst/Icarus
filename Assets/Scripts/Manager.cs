using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;
namespace UnityChan
{


    public class Manager : MonoBehaviour
    {
        WebSocket websocket;
        public GameObject player;

        // Start is called before the first frame update
        async void Start()
        {
            // set fps to 30
            Application.targetFrameRate = 30;
            // set the dimensions of the screen
            Screen.SetResolution(1280, 720, true);


            websocket = new WebSocket("ws://localhost:2567");

            websocket.OnOpen += () =>
            {
                Debug.Log("Connection open!");
                websocket.SendText("unity");
            };

            websocket.OnError += (e) =>
            {
                Debug.Log("Error! " + e);
            };

            websocket.OnClose += (e) =>
            {
                Debug.Log("Connection closed!");
            };

            websocket.OnMessage += (bytes) =>
            {
                Debug.Log("OnMessage!");
                Debug.Log(bytes);

                // getting the message as a string
                // var message = System.Text.Encoding.UTF8.GetString(bytes);
                // Debug.Log("OnMessage! " + message);

                // getting the message as a json object which we can then parse as a dictionary of strings to any
                try
                {
                    var message = System.Text.Encoding.UTF8.GetString(bytes);
                    Debug.Log("OnMessage! " + message);
                    string[] splitMessage = message.Split(',');
                    string verb = splitMessage[0];
                    string direction = splitMessage[1];
                    float quantity = float.Parse(splitMessage[2]);
                    Debug.Log("Verb: " + verb + " Direction: " + direction + " Quantity: " + quantity);
                    // move the player in the direction specified
                    player.GetComponent<UnityChanControlScriptWithRgidBody>().Action(verb, direction, quantity);
                }
                catch (System.ArgumentException e)
                {
                    Debug.Log("Error! " + e);
                }



                // 

            };

            // Keep sending messages at every 0.3s
            // InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

            // waiting for messages
            await websocket.Connect();
        }

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
#endif
            // if escape key is pressed, close the application
            if (Input.GetKey("escape"))
            {
                Application.Quit();
            }
        }

        async void SendWebSocketMessage()
        {
            if (websocket.State == WebSocketState.Open)
            {
                // Sending bytes
                await websocket.Send(new byte[] { 10, 20, 30 });

                // Sending plain text
                await websocket.SendText("plain text message");
            }
        }

        private async void OnApplicationQuit()
        {
            await websocket.Close();
        }

    }
}