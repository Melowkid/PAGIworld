﻿using UnityEngine;
using System.Collections;
//using UnityEditor;
using System;

public class triggerBoxController : worldObject {

	const int margin = 200;
	Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height/2 - (margin * 2));
	Rect titleBarRect = new Rect(0, 0, 100, 20);
	public bool showOptions = false;
	
	//options for this trigger box
	public bool triggerOnBodyEnter = true, triggerOnBodyExit = true,
		triggerOnLeftHandEnter = false, triggerOnRightHandEnter = false,
		triggerOnLeftHandExit = false, triggerOnRightHandExit = false,
		triggerOnLeftHandGrip = false, triggerOnRightHandGrip = false,
		triggerOnLeftHandRelease = false, triggerOnRightHandRelease = false,
		triggerOnOtherObjectsEnter = false, triggerOnOtherObjectsExit = false;
	
	// Use this for initialization
	void Start () {
		newXScale = "1.0";
		newYScale = "1.0";
		lastMouseUp = DateTime.Now;
	}
	
	public override void saveVals()
	{
		//save all triggerbox options
		System.Collections.Generic.Dictionary<string,System.Object> newValuesToSave = new System.Collections.Generic.Dictionary<string, object>();
		newValuesToSave.Add("triggerOnBodyEnter", triggerOnBodyEnter);
		newValuesToSave.Add("triggerOnBodyExit", triggerOnBodyExit);
		newValuesToSave.Add("triggerOnLeftHandEnter", triggerOnLeftHandEnter);
		newValuesToSave.Add("triggerOnLeftHandExit", triggerOnLeftHandExit);
		newValuesToSave.Add("triggerOnRightHandEnter", triggerOnRightHandEnter);
		newValuesToSave.Add("triggerOnRightHandExit", triggerOnRightHandExit);
		
		newValuesToSave.Add("triggerOnLeftHandGrip", triggerOnLeftHandGrip);
		newValuesToSave.Add("triggerOnRightHandGrip", triggerOnRightHandGrip);
		newValuesToSave.Add("triggerOnLeftHandRelease", triggerOnLeftHandRelease);
		newValuesToSave.Add("triggerOnRightHandRelease", triggerOnRightHandRelease);
		
		newValuesToSave.Add("triggerOnOtherObjectsEnter", triggerOnOtherObjectsEnter);
		newValuesToSave.Add("triggerOnOtherObjectsExit", triggerOnOtherObjectsExit);
		
		newValuesToSave.Add("name", this.objectName);
		newValuesToSave.Add("scalex", transform.localScale.x);
		newValuesToSave.Add("scaley", transform.localScale.y);
		
		valuesToSave = newValuesToSave;
	}
	
	public override void loadVals()
	{
		triggerOnBodyEnter = (bool)valuesToSave["triggerOnBodyEnter"];		
		triggerOnBodyExit = (bool)valuesToSave["triggerOnBodyExit"];
		triggerOnLeftHandEnter = (bool)valuesToSave["triggerOnLeftHandEnter"];
		triggerOnLeftHandExit = (bool)valuesToSave["triggerOnLeftHandExit"];
		triggerOnRightHandEnter = (bool)valuesToSave["triggerOnRightHandEnter"];
		triggerOnRightHandExit = (bool)valuesToSave["triggerOnRightHandExit"];
		triggerOnLeftHandGrip= (bool)valuesToSave["triggerOnLeftHandGrip"];
		triggerOnRightHandGrip= (bool)valuesToSave["triggerOnRightHandGrip"];
		triggerOnLeftHandRelease= (bool)valuesToSave["triggerOnLeftHandRelease"];
		triggerOnRightHandRelease= (bool)valuesToSave["triggerOnRightHandRelease"];
		triggerOnOtherObjectsEnter= (bool)valuesToSave["triggerOnOtherObjectsEnter"];
		triggerOnOtherObjectsExit= (bool)valuesToSave["triggerOnOtherObjectsExit"];
		
		this.objectName = (string)valuesToSave["name"];
		transform.localScale = new Vector3((float)valuesToSave["scalex"], (float)valuesToSave["scaley"]);
	}
	
	DateTime lastMouseUp;
	void OnMouseOver()
	{
		if (Input.GetMouseButtonUp(1))
		{
			//check for doubleclick
			//if ((DateTime.Now - lastMouseUp).TotalMilliseconds < 50)
				showOptions = true;
			lastMouseUp = System.DateTime.Now;
		}
	}
		
	void OnTriggerEnter2D(Collider2D other)
	{//List<string> doNotRemove = new List<string>() { "leftHand", "rightHand", "mainBody" };
		
		//Debug.Log("trigger hit " + other.gameObject);
		worldObject w = other.gameObject.GetComponent<worldObject>();
		if (w!=null)
		{
			if (triggerOnBodyEnter && w.objectName == "mainBody")
				GlobalVariables.outgoingMessages.Add("TB," + objectName + ",BE\n");
			else if (triggerOnLeftHandEnter && w.objectName == "leftHand")
				GlobalVariables.outgoingMessages.Add("TB," + objectName + ",LE\n");
			else if (triggerOnRightHandEnter && w.objectName == "rightHand")
				GlobalVariables.outgoingMessages.Add("TB," + objectName + ",RE\n");
			else if (triggerOnOtherObjectsEnter)
				GlobalVariables.outgoingMessages.Add("TB," + objectName + ",OE\n");
		}
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		worldObject w = other.gameObject.GetComponent<worldObject>();
		if (w!=null)
		{
			if (triggerOnBodyExit && w.objectName == "mainBody")
			{
				GlobalVariables.outgoingMessages.Add("TB," + objectName + ",BX\n");
				//Debug.Log("hi");
			}
			else if (triggerOnLeftHandExit && w.objectName == "leftHand")
				GlobalVariables.outgoingMessages.Add("TB," + objectName + ",LX\n");
			else if (triggerOnRightHandExit && w.objectName == "rightHand")
				GlobalVariables.outgoingMessages.Add("TB," + objectName + ",RX\n");
			else if (triggerOnOtherObjectsExit)
				GlobalVariables.outgoingMessages.Add("TB," + objectName + ",OX\n");
		}
	}
	
	/// <summary>
	/// called when a grip or release is done while a hand is in this box.
	/// </summary>
	public void GripOrReleaseHandler(bool isGrip, bool isLeftHand)
	{
		//Debug.Log("got grip from " + isGrip + " at " + isLeftHand);
		if (isGrip)
		{
			if (isLeftHand)
			{
				if (triggerOnLeftHandGrip)
					GlobalVariables.outgoingMessages.Add("TB," + objectName + ",LG\n");
			}
			else
			{
				if (triggerOnRightHandGrip)
					GlobalVariables.outgoingMessages.Add("TB," + objectName + ",RG\n");
			}
		}
		else
		{
			if (isLeftHand)
			{
				if (triggerOnLeftHandRelease)
					GlobalVariables.outgoingMessages.Add("TB," + objectName + ",LR\n");
			}
			else
			{
				if (triggerOnRightHandRelease)
					GlobalVariables.outgoingMessages.Add("TB," + objectName + ",RR\n");
			}
		}
	}
	
	void OnGUI() {
		
		if (!showOptions) {
			return;
		}
		
		windowRect = GUILayout.Window(124343, windowRect, OptionsWindow, "Trigger Box Options");
		
	}
	
	string newXScale,newYScale;
	void OptionsWindow (int windowID)
	{
		
		GUI.contentColor = Color.white;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Name:");
		this.objectName = GUILayout.TextField(this.objectName, GUILayout.Width(400));
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		//GUILayout.Label("Size:");
		GUILayout.Label("Width:");
		newXScale = GUILayout.TextField(newXScale);
		GUILayout.Label("Height:");
		newYScale = GUILayout.TextField(newYScale);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Trigger on entrance of:");
		triggerOnBodyEnter = GUILayout.Toggle(triggerOnBodyEnter, "Body");
		triggerOnLeftHandEnter = GUILayout.Toggle(triggerOnLeftHandEnter, "L.Hand");
		triggerOnRightHandEnter = GUILayout.Toggle(triggerOnRightHandEnter, "R.Hand");
		triggerOnOtherObjectsEnter = GUILayout.Toggle(triggerOnOtherObjectsEnter, "Other objs");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Trigger on exit of:");
		triggerOnBodyExit = GUILayout.Toggle(triggerOnBodyExit, "Body");
		triggerOnLeftHandExit = GUILayout.Toggle(triggerOnLeftHandExit, "L.Hand");
		triggerOnRightHandExit = GUILayout.Toggle(triggerOnRightHandExit, "R.Hand");
		triggerOnOtherObjectsExit = GUILayout.Toggle(triggerOnOtherObjectsExit, "Other objs");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Trigger on:");
		triggerOnLeftHandGrip = GUILayout.Toggle(triggerOnLeftHandGrip, "L.Hand Grip");
		triggerOnRightHandGrip = GUILayout.Toggle(triggerOnRightHandGrip, "R.Hand Grip");
		triggerOnLeftHandRelease = GUILayout.Toggle(triggerOnLeftHandRelease, "L.Hand Release");
		triggerOnRightHandRelease = GUILayout.Toggle(triggerOnRightHandRelease, "R.Hand Release");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent("OK", "Saves changes"))) {
			//Time.timeScale = Mathf.Round(10f*Mathf.Min(10f, Time.timeScale+0.1f))/10f;
			float x, y;
			if (!(float.TryParse(newXScale, out x)&&float.TryParse(newYScale, out y)))
			{
				Debug.Log("Error: couldn't parse value in size");
				x = transform.localScale.x;
				y = transform.localScale.y;
			}
			transform.localScale = new Vector3(x,y);
			
			showOptions = false;
		}
		GUILayout.EndHorizontal();
		
		
		// Allow the window to be dragged by its title bar.
		GUI.DragWindow(windowRect);
		GUI.DragWindow(titleBarRect);
	}
}
