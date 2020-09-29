﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour, IResetObject
{

	private AcidTitration acidTitrationScript;
	private DrawGraph drawGraphScript;

	// Base settings
	private double baseMol = 0.1f;
	private double baseMl = 50f;

	// Acid settings
	private double acidMol = 0.1f;
	private double acidMl = 20f;

	// Titration settings
	private double molTitrant = 0.1f;
	private double mlTitrant = 50f;
	private double molAnalyt = 0.1f;
	private double mlAnalyt = 20f;

	[Space(10)]
    public GameObject pipet;
    private PipetAnimation pipetAnimation;

	public Toggle baseToggle;
	public Toggle acidToggle;
	public Button startButton;

    Coroutine theCoroutine;

	void Start () 
	{
		acidTitrationScript = gameObject.GetComponent<AcidTitration>();
		drawGraphScript = GameObject.Find("LineRenderer").GetComponent<DrawGraph>();
        pipetAnimation = pipet.GetComponent<PipetAnimation>();
	}

	/*private void FixedUpdate()
	{
		if (SimulationController.Instance.SimulationRunning)
		{
			startTitration();
		}
	}*/

	public void startTitration()
	{
		startButton.interactable = false;
		bool ready = initaliseComponents();

        if (ready)
        {
            playPipetAnimation();

            // Calculation and Graph
            acidTitrationScript.initialise(molTitrant, mlTitrant, molAnalyt, mlAnalyt);
            drawGraphScript.InitLine();

            // Draw the Graph when burette is opened
            theCoroutine = StartCoroutine(drawGraphScript.DrawLine());
        }
    }

    bool initaliseComponents()
	{
		if (baseToggle.isOn) // if base is titrant
		{
			molTitrant = baseMol;
			mlTitrant = baseMl;
			molAnalyt = acidMol;
			mlAnalyt = acidMl;
		}
		else // if acid is titrant
		{
			molTitrant = acidMol;
			mlTitrant = acidMl;
			molAnalyt = baseMol;
			mlAnalyt = baseMl;
		}

		if (checkForZeros(molTitrant))
            return false;
        if (checkForZeros(molAnalyt))
            return false;
        return true;


	}

    public bool checkForZeros(double value)
    {
        if (value == 0.0)
            return true;
        else
            return false;
    }

	public void ResetObject()
	{
		//pipetAnimation.resetAnalytAnimation();

		// handle of burette is not interactable
		pipetAnimation.analyt.GetComponent<SetBuretteInteractive>().disableBuretteTap();
        acidTitrationScript.resetEverything();

		baseMol = 0.1f;
		baseMl = 50.0f;

		acidMol = 0.1f;
		acidMl = 20.0f;

		baseToggle.isOn = true;
		acidToggle.isOn = false;

		pipetAnimation.resetPipet(true);
        StopCoroutine(theCoroutine);
		startButton.interactable = true;
	}

    // Animation: Pipet takes fluid and puts it into erlenmeyer flask
    void playPipetAnimation()
    {
		pipetAnimation.resetTrigger("reset");
		pipetAnimation.setPipetBool(baseToggle.isOn);
    }

	public void setBaseToggleTitrant(bool value)
	{
		if (!acidToggle.IsActive())
			acidToggle.isOn = !acidToggle.isOn;
	}

	public void setAcidToggleTitrant(bool value)
	{
		if (!baseToggle.IsActive())
			baseToggle.isOn = !baseToggle.isOn;
	}

	public bool getBaseToggleTitrant()
	{
		return baseToggle.isOn;
	}


	public void setMolBase(float value)
	{
		baseMol = (double)value;
	}

	public void setMlBase(float value)
	{
		baseMl = (double)value;
	}

	public void setMolAcid(float value)
	{
		acidMol = (double)value;
	}

	public void setMlAcid(float value)
	{
		acidMl = (double)value;
	}

}
