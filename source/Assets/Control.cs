﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;


public class Control : MonoBehaviour {
	//https://github.com/AngryAnt/Unity--Network-Code-and-You
	public const string kServerArgument = "-server";
	public Rigidbody2D newgarf;
	
	
	//const
	int
		kPort = GlobalVariables.portNumber,
		kHostConnectionBacklog = 10;
	
	
	static Control instance;
	
	
	string message = "Awake";
	Socket socket = null;
	IPAddress ip;
	
	static Control Instance
	{
		get
		{
			if (instance == null)
			{
				instance = (Control)FindObjectOfType (typeof (Control));
			}
			
			return instance;
		}
	}
	
	
	public static Socket Socket
	{
		get
		{
			return Instance.socket;
		}
	}
	
	
	void Start ()
	{
		//Application.RegisterLogCallbackThreaded (OnLog);
		
		bool isServer = true;
		
		/*foreach (string argument in System.Environment.GetCommandLineArgs ())
		{
			if (argument == kServerArgument)
			{
				isServer = true;
				break;
			}
		}*/
		
		if (isServer)
		{
			if (Host (kPort))
			{
				Debug.Log("world " + GlobalVariables.portNumber);
				Debug.Log(kPort);
				Debug.Log("Server started");
				//gameObject.SendMessage ("OnServerStarted");
			}
		}
		else
		{
			if (Connect (IP, kPort))
			{
				gameObject.SendMessage ("OnClientStarted", socket);
			}
		}
	}
	
	
	void OnApplicationQuit ()
	{
		Debug.Log("closing everything");
		Disconnect ();
	}
	
	
	public bool Host (int port)
	{
		
		if (socket!=null)
			socket.Disconnect(true);
		socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		
		try
		{
			socket.Bind (new IPEndPoint (IP, port));
			socket.Listen (kHostConnectionBacklog);
			socket.BeginAccept (new System.AsyncCallback (OnClientConnect), socket);
			Debug.Log ("Hosting on " + IP.ToString() + ":" + port);
			
			Debug.Log("Welcome! You are using PAGI World version " + GlobalVariables.versionNumber);
		}
		catch (System.Exception e)
		{
			Debug.LogError ("Exception when attempting to host (" + port + "): " + e);
			
			socket = null;
			
			return false;
		}
		
		return true;
	}
	
	
	public bool Connect (IPAddress ip, int port)
	{
		Debug.Log ("Connecting to " + ip + " on port " + port);
		
		socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		socket.Connect (new IPEndPoint (ip, port));
		
		if (!socket.Connected)
		{
			Debug.LogError ("Failed to connect to " + ip + " on port " + port);
			
			socket = null;
			return false;
		}
		
		return true;
	}
	
	
	public void Disconnect ()
	{
		if (socket != null)
		{
			socket.BeginDisconnect (false, new System.AsyncCallback (OnEndHostComplete), socket);
		}
		
		foreach (Socket c in clients)
			c.Close();
		
		clients = new List<Socket>();
	}
	
	
	public bodyController bodyInterface;
	
	/// <summary>
	/// this will hold any part of the remaining message that was not terminated
	/// </summary>
	public string bufferUnfinished = ""; 
	
	void OnReceive (SocketRead read, byte[] data)
	{
		string clientSaid = Encoding.ASCII.GetString (data, 0, data.Length);

		/*string toPrint = "SAID: ";
		for (int i=0; i<clientSaid.Length; i++)
			toPrint = toPrint + ((int)clientSaid[i]).ToString() + ",";
		Debug.Log(toPrint);*/

		string message = "Client " + clients.IndexOf (read.Socket) + " says: " + clientSaid;
		Debug.Log (message);
		//process the incoming text and store in the message queue
		clientSaid = bufferUnfinished + clientSaid;
		//Debug.Log("new message is " + clientSaid);
		bufferUnfinished = "";
		//did it end in a return?
		List<string> allCommands = new List<string>(clientSaid.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries));
		if (message[message.Length-1] != '\n')
		{
			bufferUnfinished = allCommands[allCommands.Count-1];
			allCommands.RemoveAt((allCommands.Count)-1);
			Debug.Log (allCommands.Count);
		}		
		foreach (String cmd in allCommands)
		{
			AIMessage a;
			try {
				a = AIMessage.fromString(cmd);
			}
			catch(Exception e)
			{
				Debug.Log("Could not parse message due to error. Skipping.");
				GlobalVariables.outgoingMessages.Add("ERR,formattingError\n");
				continue;
			}
			bodyInterface.messageQueue.Add(a);
			//Debug.Log("added to message queue: " + a.messageType.ToString() + ", " + a.stringContent);
		}
	}
	
	
	void OnReceiveError (SocketRead read, System.Exception exception)
	{
		Debug.LogError ("Receive error: " + exception);
	}
	
	List<Socket> clients = new List<Socket> ();
	
	void OnClientConnect (System.IAsyncResult result)
	{
		Debug.Log ("Handling client connecting");
		
		try
		{
			
			//gameObject.SendMessage ("OnClientConnected", socket.EndAccept (result));
			Debug.Log ("Client connected");
			Socket client = socket.EndAccept(result);
			clients.Add (client);
			SocketRead.Begin (client, OnReceive, OnReceiveError);
		}
		catch (System.Exception e)
		{
			Debug.LogError ("Exception when accepting incoming connection: " + e);
		}
		
		try
		{
			socket.BeginAccept (new System.AsyncCallback (OnClientConnect), socket);
		}
		catch (System.Exception e)
		{
			Debug.LogError ("Exception when starting new accept process: " + e);
		}
	}
	
	
	void OnEndHostComplete (System.IAsyncResult result)
	{
		socket = null;
		Debug.Log("finished disconnecting client socket");
	}
	
	
	public IPAddress IP
	{
		get
		{
			if (ip == null)
			{
				ip = (
					from entry in Dns.GetHostEntry (Dns.GetHostName ()).AddressList
					where entry.AddressFamily == AddressFamily.InterNetwork
					select entry
					).FirstOrDefault ();
			}
			
			return ip;
		}
	}
	
	public void OnRestartServer()
	{
		//if (Input.GetKeyDown(KeyCode.Tab))
		//{
		Debug.Log("restarting server, please wait........");
		Disconnect();
		clients = new List<Socket>();
		Thread.Sleep(1000);
		Start();	
		//}
	}
	
	
	void Update()//OnGUI ()
	{
		/*if (Input.GetKeyDown(KeyCode.Tab))
		{
			Debug.Log("restarting server, please wait........");
			Disconnect();
			Thread.Sleep(1000);
			Start();	
		}*/
		//GUILayout.Label ("");
		//GUILayout.Label (message);
		
		/*//go through and delete sockets that are closed
		List<Socket> toRemove = new List<Socket>();
		foreach (Socket client in clients)
		{
			if ((client.Poll(1000, SelectMode.SelectRead) && (client.Available == 0)) || !client.Connected)
			{
				Debug.Log("client " + client.RemoteEndPoint.ToString() + " is closed, removing...");
				toRemove.Add(client);
			}
		}
		foreach (Socket client in toRemove)
			clients.Remove(client);*/
		

		//reply with outgoing messages
		while (bodyInterface.outgoingMessages.Count() > 0)
		{
			//Debug.Log("num outgoing: " + bodyInterface.outgoingMessages.Count());
			string s;
			while (!bodyInterface.outgoingMessages.TryGet(0, out s)) 
				Thread.Sleep(100);
			while (!bodyInterface.outgoingMessages.TryRemoveAt(0))
				Thread.Sleep(100);
			byte[] toSend = Encoding.ASCII.GetBytes(s);
			//send message to each open socket		
			foreach (Socket client in clients)
			{
				//is this client still open?
				if ((client.Poll(1000, SelectMode.SelectRead) && (client.Available == 0)) || !client.Connected)
				{
					Debug.Log("client " + client.RemoteEndPoint.ToString() + " is closed, skipping...");
					continue;
				}	
				Debug.Log ("sent reply to " + client.RemoteEndPoint.ToString() + ": " + s);
				//Thread.Sleep(100); //because the response generates an error otherwise
				client.Send(toSend);
			}	
			//Debug.Log ("here3");
		}
	}
	
	
	void OnLog (string message, string callStack, LogType type)
	{
		this.message = message + "\n" + this.message;
	}
}