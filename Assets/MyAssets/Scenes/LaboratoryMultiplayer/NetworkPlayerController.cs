﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioListener))]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CharacterController))]
public class NetworkPlayerController : NetworkBehaviour {

    public GameObject avatar;
    public GameObject eyes;
    public GameObject messagePrefab;
    private MeshRenderer[] mesh_renderer;
    private GameObject color_bar;
    private InputField input_field;
    private NetworkManagerHUD gui;
    private Camera cam;
    private FirstPersonController fps;
    private string text;

    [SyncVar] private Color color;

    void Start()
    {
        mesh_renderer = avatar.GetComponentsInChildren<MeshRenderer>();
        input_field = GameObject.FindGameObjectWithTag("Input").GetComponent<InputField>();
        gui = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerHUD>();
        cam = GetComponentInChildren<Camera>();
        fps = GetComponentInChildren<FirstPersonController>();

        if (!isLocalPlayer)
        {
            fps.enabled = false;
            cam.enabled = false;
            GetComponentInChildren<AudioSource>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
            GetComponentInChildren<CharacterController>().enabled = false;

            CmdSetColor(color);
            foreach (MeshRenderer i in mesh_renderer)
            {
                i.material.color = color;
            }
        }
        else
        {
            gui.showGUI = false;
            CmdSetColor(color);
            color_bar = GameObject.FindGameObjectWithTag("ColorBar");
            color_bar.GetComponentInChildren<Image>().color = color;
            //DontDestroyOnLoad(bar);
            avatar.SetActive(false);
            eyes.SetActive(false);

            SceneManager.activeSceneChanged += switchCamera; // subscribe

            //cleanup messages if any
            GameObject[] old = GameObject.FindGameObjectsWithTag("Message");
            if (old.Length > 0)
            {
                foreach (GameObject o in old)
                    o.SetActive(false);
            }
        }
    }

    private void switchCamera(Scene previousScene, Scene newScene)
    {
        cam.enabled = !cam.enabled;
    }

    // Update is called once per frame
    void Update()
    {
        if(isLocalPlayer)
        {
            if (input_field.isFocused)
            {
                gui.showGUI = false;
                fps.enabled = false;
            }
            else
            {
                //gui_.showGUI = true;
                fps.enabled = true;
            }

            text = input_field.text;

            if (text.Length > 0 && Input.GetKeyDown(KeyCode.Return))
            {
                input_field.text = "";
                CmdMessage(text);
                input_field.ActivateInputField();
            }
            if (text.Length == 0 && Input.GetKeyDown(KeyCode.Return))
                input_field.ActivateInputField();
        }
    }

    public bool isFocused()
    {
        return input_field.isFocused;
    }

    [Command]
    void CmdMessage(string t)
    {
        var msg = (GameObject)Instantiate(
            messagePrefab,
            new Vector3(0,0,0),
            new Quaternion(0,0,0,0));

        msg.GetComponentInChildren<Text>().text = t;
        msg.GetComponentInChildren<Text>().color = color;

        NetworkServer.Spawn(msg);
    }

    void OnDestroy()
    {
        if (isLocalPlayer)
        {
            if (color_bar != null)
                color_bar.GetComponentInChildren<Image>().color = Color.white;
            if (gui != null)
                gui.showGUI = true;
            if (!SceneManager.GetActiveScene().name.Equals("Laboratory"))
                SceneManager.LoadScene("Laboratory");
            SceneManager.activeSceneChanged -= switchCamera; // unsubscribe
        }
    }

    public override void OnStartClient()
    {
        if (isServer)
        {
            color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            RpcSetColor(color);
        }
    }

    [ClientRpc] void RpcSetColor(Color c)
    {
        color = c;
    }

    [Command] void CmdSetColor(Color c)
    {
        color = c;
    }
}
