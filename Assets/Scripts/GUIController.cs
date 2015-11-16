using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GUIController : MonoBehaviour {
	public GUISkin skin;
	public ProgramState state = ProgramState.SELECTTYPE;
	public GameObject RSGPrefab;
	public GameObject IEPrefab;
	public GameObject KBISPrefab;
	public Material mat;
	
	public static Experiment experiment{get; protected set;}
	
	private List<string> screenfields = new List<string>();
	private string trialsfield;

	private GameObject lastPrefab;
#if UNITY_EDITOR || UNITY_STANDALONE
	public string dpiField = "40";
#endif

	void Awake()
	{
		// Disable screen dimming
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if !(UNITY_EDITOR || UNITY_STANDALONE)
		Data.cmToPixel = Screen.dpi * 0.393701f;
#endif
	}

	void OnGUI()
	{
		GUI.skin = skin;
		switch(state)
		{
		case ProgramState.SELECTTYPE:
			Experiment currExperiment = null;
			if(GUI.Button(new Rect(0, Screen.height / 2 - Screen.width / 6, Screen.width /3, Screen.width / 3), "Ready Set Go"))
			{
				currExperiment = (Instantiate(RSGPrefab) as GameObject).GetComponent<Experiment>();
				lastPrefab = RSGPrefab;
			}
			if(GUI.Button(new Rect(Screen.width / 3, Screen.height / 2 - Screen.width / 6, Screen.width /3, Screen.width /3), "Interval Estimation"))
			{
				currExperiment = (Instantiate(IEPrefab) as GameObject).GetComponent<Experiment>();
				lastPrefab = IEPrefab;
			}
			if(GUI.Button(new Rect(2 * Screen.width / 3, Screen.height / 2 - Screen.width / 6, Screen.width /3, Screen.width /3), "KBIS"))
			{
				currExperiment = (Instantiate(KBISPrefab) as GameObject).GetComponent<Experiment>();
				lastPrefab = KBISPrefab;
			}
			if(currExperiment != null)
			{
				experiment = currExperiment;
				state = ProgramState.CONFIG;
				for(int i = 0; i < experiment.screens.Count; i++)
				{
					screenfields.Add(experiment.screens[i].Value.ToString());
				}
				trialsfield = experiment.numberOfTrials.ToString();
			}
			break;
		case ProgramState.CONFIG:
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height), "", "box");
			float tempScrnValue;
			GUILayout.BeginVertical();
			GUILayout.Label("Screen durations (ms)");
			for(int i = 0; i < experiment.screens.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(100);
				GUILayout.Label(experiment.screens[i].Key, GUILayout.ExpandWidth(false));
				try
				{
					tempScrnValue = Convert.ToSingle(screenfields[i]);
					experiment.screens[i].Value = tempScrnValue;
				}
				catch(Exception e)
				{
					GUI.color = Color.red;
				}
				screenfields[i] = GUILayout.TextField(screenfields[i]);
				GUI.color = Color.white;
				GUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal();
			GUILayout.Label("Number of trials:", GUILayout.ExpandWidth(false));
			try
			{
				int tempTrials = Convert.ToInt32(trialsfield);
				experiment.numberOfTrials = tempTrials;
			}
			catch(Exception e)
			{
				GUI.color = Color.red;
			}
			trialsfield = GUILayout.TextField(trialsfield);
			GUI.color = Color.white;
			GUILayout.EndHorizontal();
			experiment.AddlParameters();
#if UNITY_EDITOR || UNITY_STANDALONE
			GUILayout.BeginHorizontal();
			GUILayout.Label("Simulated DPI:", GUILayout.ExpandWidth(false));
			try
			{
				int tempDPI = Convert.ToInt32(dpiField);
				Data.cmToPixel = tempDPI * 0.393701f;
			}
			catch(Exception e)
			{
				GUI.color = Color.red;
			}
			dpiField = GUILayout.TextField(dpiField);
			GUI.color = Color.white;
			GUILayout.EndHorizontal();
#endif
			if(GUILayout.Button("Save and begin experiment"))
			{
				experiment.SaveValues();
				state = ProgramState.WAITINGTOBEGIN;
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();
			break;
		case ProgramState.WAITINGTOBEGIN:
			if(GUI.Button(new Rect(Screen.width / 2 - Screen.height / 4, Screen.height / 4, Screen.height / 2, Screen.height / 2), "Start"))
			{
				state = ProgramState.RUNNING;
			}
			break;
		case ProgramState.RUNNING:
			if(experiment != null)
				experiment.activeState.Draw();
			else
				state = ProgramState.COMPLETE;
			break;
		case ProgramState.COMPLETE:
			GUI.Label(new Rect(0, 0, Screen.width, Screen.height / 2 - Screen.width / 8), "Experiment complete\nWhat would you like to do now?", "endText");
			if(GUI.Button(new Rect(0, Screen.height / 2 - Screen.width / 8, Screen.width /4, Screen.width /4), "Perform the same experiment\nwith the same parameters"))
			{
				experiment = (Instantiate(lastPrefab) as GameObject).GetComponent<Experiment>();
				experiment.SaveValues();
				state = ProgramState.WAITINGTOBEGIN;
			}
			if(GUI.Button(new Rect(Screen.width / 4, Screen.height / 2 - Screen.width / 8, Screen.width /4, Screen.width /4), "Perform the same experiment\nwith different parameters"))
			{
				experiment = (Instantiate(lastPrefab) as GameObject).GetComponent<Experiment>();
				state = ProgramState.CONFIG;
				screenfields.Clear();
				for(int i = 0; i < experiment.screens.Count; i++)
				{
					screenfields.Add(experiment.screens[i].Value.ToString());
				}
				trialsfield = experiment.numberOfTrials.ToString();
			}
			if(GUI.Button(new Rect(2 * Screen.width / 4, Screen.height / 2 - Screen.width / 8, Screen.width /4, Screen.width /4), "Choose a different experiment"))
			{
				state = ProgramState.SELECTTYPE;
				screenfields.Clear();
			}
			if(GUI.Button(new Rect(3 * Screen.width / 4, Screen.height / 2 - Screen.width / 8, Screen.width /4, Screen.width /4), "Exit the program"))
			{
				Application.Quit();
			}
			break;
		}
	}
	
	void Update()
	{
		if(state == ProgramState.RUNNING)
		{
			if(experiment != null)
				experiment.OnUpdate();
		}
	}
	
	protected void OnPostRender()
	{
		if(state == ProgramState.RUNNING)
		{
			GL.PushMatrix();
			mat.SetPass (0);
			GL.LoadPixelMatrix ();
			GL.Begin (GL.LINES);
			GL.Color (Color.black);
			experiment.DrawLine();
			GL.End ();
			GL.PopMatrix();
		}
	}

	/*public bool isActive;
	public bool isResults;
	public Material mat;
	public float time;

	public Data data;
	public Circle circle;

	public static Vector2 size{ get; private set; }
	public static float width{ get { return size.y - size.x; } }
	private float timer = Mathf.NegativeInfinity;

	void Awake()
	{
		size = new Vector2 (Screen.width * 0.1f, Screen.width * 0.9f);
		Debug.Log (size);
	}

	void Update()
	{
		if(Time.time > timer + time && timer != Mathf.NegativeInfinity)
		{
			timer = -1;
			circle.Draw(-1);
		}
	}
	
	void OnGUI()
	{
		if(!isActive)
		{
			if(GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 150, 300, 300), "Start"))
			{
				isActive = true;
				timer = Time.time;
				data.CreateDatapoint();
			}
		}
		else if(!isResults)
		{
			if(Time.time <= timer + time)
			{
				circle.Draw(data.datapoint);
			}
			else if(timer < 0 && timer > Mathf.NegativeInfinity)
			{
				if(Input.GetMouseButton(0))
				{
					circle.Draw(Mathf.Clamp(Input.mousePosition.x, size.x, size.y));
				}
				else if(Input.GetMouseButtonUp (0))
				{
					data.selectedPoint = Mathf.RoundToInt(Mathf.Clamp(Input.mousePosition.x, size.x, size.y));
					isResults = true;
				}
			}
		}
		else
		{
			GUI.Box (new Rect(0, 0, 500, 75), "");
			GUI.Label(new Rect(0, 0, 500, 25), "Point given: " + data.datapoint + "px");
			GUI.Label(new Rect(0, 25, 500, 25), "Point chosen: " + data.selectedPoint + "px");

			if(GUI.Button(new Rect(0, 50, 500, 25), "Next"))
			{
				isResults = false;
				timer = Time.time;
				data.CreateDatapoint();
			}
		}
	}

	void OnPostRender()
	{
		if(isActive)
		{
			GL.PushMatrix();
			mat.SetPass (0);
			GL.LoadPixelMatrix ();
			GL.Begin (GL.LINES);
			GL.Color (Color.black);
			GL.Vertex3 (size.x, Screen.height * 0.5f, 0);
			GL.Vertex3 (size.y, Screen.height * 0.5f, 0);
			GL.End ();
			GL.PopMatrix();
		}
	}*/
}

public enum ProgramState
{
	SELECTTYPE, CONFIG, WAITINGTOBEGIN, RUNNING, COMPLETE
}
