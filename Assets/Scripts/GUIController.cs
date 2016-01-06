using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class GUIController : MonoBehaviour {
	public GUISkin skin;
	public GameObject RSGPrefab;
	public GameObject IEPrefab;
	public GameObject KBISPrefab;
	private GameObject currAsset;
	public Material mat;
	public Texture buttonTex;
	public Experiment currExperiment = null;

	public static bool advanceOption = false;
	public static string idField = "subjectID_condition_dateRun";
	public static ProgramState state = ProgramState.SELECTTYPE;
	public static Experiment experiment{get; protected set;}
	
	private List<string> screenfields = new List<string>();
	private string trialsfield;
	private VectorLine myLine;
	private GameObject lastPrefab;

	protected readonly string[] advLabels = new string[]{"Auto", "Manual"};

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

	public void createVLine()
	{
			Vector2[] linePoints = {new Vector2(0, Screen.height/2), new Vector2(Screen.width,Screen.height/2)};
			myLine = new VectorLine("expLine", linePoints, null, 4.0f);
			myLine.color = Color.black;
	}

	public void advanceToggle()
	{
		advanceOption = GUILayout.SelectionGrid((advanceOption ? 1 : 0), advLabels, 1, "toggle") > 0;
	}

	void OnGUI()
	{
		GUI.skin = skin;
		switch(state)
		{
		case ProgramState.SELECTTYPE:
			if(GUI.Button(new Rect(0, Screen.height / 2 - Screen.width / 6, Screen.width /3, Screen.width / 3), "Ready Set Go"))
			{
				// createVLine();
				currAsset = Instantiate(RSGPrefab);
				currExperiment = (currAsset as GameObject).GetComponent<Experiment>();
				lastPrefab = RSGPrefab;
			}
			if(GUI.Button(new Rect(Screen.width / 3, Screen.height / 2 - Screen.width / 6, Screen.width /3, Screen.width /3), "Interval Estimation"))
			{
				// createVLine();
				currAsset = Instantiate(IEPrefab);
				currExperiment = (currAsset as GameObject).GetComponent<Experiment>();
				lastPrefab = IEPrefab;
			}
			if(GUI.Button(new Rect(2 * Screen.width / 3, Screen.height / 2 - Screen.width / 6, Screen.width /3, Screen.width /3), "KBIS"))
			{
				// createVLine();
				currAsset = Instantiate(KBISPrefab);
				currExperiment = (currAsset as GameObject).GetComponent<Experiment>();
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

			GUILayout.BeginHorizontal();

			// returns back to previous selection screen and destroys current Asset
			if(GUILayout.Button("Back", new GUILayoutOption[] { GUILayout.Width(80), GUILayout.Height(50)}))
			{
				Destroy(currAsset);
				state = ProgramState.SELECTTYPE;
			}

			GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperCenter;
			GUILayout.Label(currExperiment.GetName());

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		
			GUILayout.BeginVertical();
			GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperLeft;

			GUILayout.Label("Screen durations (ms):");
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
			GUILayout.BeginHorizontal();
			experiment.AddlParameters();
			advanceToggle();
			GUILayout.EndHorizontal();
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
			GUILayout.BeginHorizontal();
			GUILayout.Label("Subject ID:", GUILayout.ExpandWidth(false));
			idField = GUILayout.TextField(idField);


			GUILayout.EndHorizontal();
			if(GUILayout.Button("Save and begin experiment"))
			{
				experiment.SaveValues();
				state = ProgramState.WAITINGTOBEGIN;
			}

			GUILayout.EndVertical();
			GUILayout.EndArea();
			break;
		case ProgramState.WAITINGTOBEGIN:
			bool startButton = GUI.Button(new Rect(Screen.width / 2 - Screen.height / 4, Screen.height / 4, Screen.height / 2, Screen.height / 2), "Touch to Start!");
			if(startButton)
			{
				state = ProgramState.RUNNING;
			}
			break;
		case ProgramState.RUNNING:
			if(experiment != null)
				experiment.activeState.Draw();
			else
				state = ProgramState.THANKYOU;
			break;

		case ProgramState.INTERMISSION:
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height), "", "box");
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Quit")){
				experiment.SaveValues();
				Destroy(currAsset);
				state = ProgramState.SELECTTYPE;
			}

			GUILayout.EndHorizontal();

			if (GUILayout.Button("Continue")){
				state = ProgramState.RUNNING;
			}

			GUILayout.EndVertical();
			GUILayout.EndArea();
			break;

		case ProgramState.PAUSE:
			break;

		case ProgramState.THANKYOU:

			GUI.Label(new Rect(0, 0, Screen.width, Screen.height/3), "Thank you!", "thanks");

			if (GUILayout.Button("hi")){
				state = ProgramState.COMPLETE;
			}
			break;
		case ProgramState.COMPLETE:
			GUI.Label(new Rect(0, 0, Screen.width, Screen.height / 2 - Screen.width / 8), "Experiment complete\nWhat would you like to do now?", "endText");
			if(GUI.Button(new Rect(0, Screen.height / 2 - Screen.width / 8, Screen.width /4, Screen.width /4), "Perform the same experiment\nwith the same parameters"))
			{
				// createVLine();
				experiment = (Instantiate(lastPrefab) as GameObject).GetComponent<Experiment>();
				experiment.SaveValues();
				state = ProgramState.WAITINGTOBEGIN;
			}
			if(GUI.Button(new Rect(Screen.width / 4, Screen.height / 2 - Screen.width / 8, Screen.width /4, Screen.width /4), "Perform the same experiment\nwith different parameters"))
			{
				// createVLine();
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
				// createVLine();
				state = ProgramState.SELECTTYPE;
				screenfields.Clear();
			}
			if(GUI.Button(new Rect(3 * Screen.width / 4, Screen.height / 2 - Screen.width / 8, Screen.width /4, Screen.width /4), "Exit the program"))
			{
				// createVLine();
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

	// void OnMouseOver(){
	// 	st

	// }
		
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
			
		// 	myLine.Draw();
		}
		// else if(state == ProgramState.COMPLETE)
		// {
		// 	VectorLine.Destroy(ref myLine);
		// }
	}
}

public enum ProgramState
{
	SELECTTYPE, CONFIG, WAITINGTOBEGIN, RUNNING, INTERMISSION, PAUSE, THANKYOU, COMPLETE
}
