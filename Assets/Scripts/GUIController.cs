using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GUIController : MonoBehaviour {
	public GUISkin skin;
	public GameObject RSGPrefab;
	public GameObject IEPrefab;
	public GameObject KBISPrefab;
	private GameObject currAsset;
	public Material mat;
	public Texture smileyFace;
	public Experiment currExperiment = null;
	public Data data{get; protected set;}
	public bool changeOption = false;

	public static int repeatedNumAllowed;
	public static bool intermissionCheck;
	
	public static string idField = "subjectID_condition_dateRun";
	public static ProgramState state = ProgramState.SELECTTYPE;
	public static Experiment experiment{get; protected set;}
	
	private List<string> screenfields = new List<string>();
	private string trialsfield = "";
	private string changeAllField = "3000";
	private int changeAllVal;
	private GameObject lastPrefab;

#if UNITY_EDITOR || UNITY_STANDALONE
	public string dpiField = "40";
#endif

	public void changeToggle(){
		changeOption = GUILayout.Toggle(false, "Change All", "toggle");
	}

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

			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height), "", "box3");
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
			GUILayout.EndArea();
			break;
		case ProgramState.CONFIG:

			bool isNull = false;

			bool isDotExp = (currExperiment.GetName() == "ReadySetGo" || currExperiment.GetName() == "IntervalEstimation");

			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height), "", "box");
			float tempScrnValue;
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();

			// returns back to previous selection screen and destroys current Asset
			if(GUILayout.Button("Back","defaultButton", new GUILayoutOption[] { GUILayout.Width(80), GUILayout.Height(50)}))
			{
				Destroy(currAsset);
				currExperiment = null;
				lastPrefab = null;
				state = ProgramState.SELECTTYPE;
			}

			GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperCenter;

			try
			{
				GUILayout.Label(currExperiment.GetName(), "NameLabel");
			}
			catch(Exception e){
				isNull = true;
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		
			GUILayout.BeginVertical();
			GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperLeft;

			// uncomment for testing data output
			// if(GUILayout.Button("Save test", "defaultButton"))
			// {
			// 	experiment.SaveValues();
			// 	state = ProgramState.WAITINGTOBEGIN;
			// }

			GUILayout.BeginHorizontal();
			GUILayout.Label("Screen durations (ms):");
			

			//change all dictionary screen durations to input value
			if(GUILayout.Button("Change All", "changeAll", GUILayout.ExpandWidth(false))){
				try{
					changeAllVal = Convert.ToInt32(changeAllField); 
					for (int i = 0; i < screenfields.Count; i++){
						screenfields[i] = Convert.ToString(changeAllVal);
						// Debug.Log(Convert.ToString(screenfields[i]));
					}
				}
				catch(Exception e){
				}
			}

			try{
				changeAllVal = Convert.ToInt32(changeAllField); 
			}
			catch(Exception e){
				GUI.color = Color.red;
			}

			changeAllField = GUILayout.TextField(changeAllField);
			GUI.color = Color.white;

			GUILayout.EndHorizontal();
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
				if (isNull == false && isDotExp)
				{
					repeatedNumAllowed = experiment.numberOfTrials/11;
				}
				else
				{
					repeatedNumAllowed = experiment.numberOfTrials/38;
				}
				if (repeatedNumAllowed <= 0){
					repeatedNumAllowed = 1;
				}
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
			if (isNull == false && isDotExp){
				GUILayout.BeginVertical();
				GUILayout.Label("Advance Screen:", "configLabel");
				experiment.advanceToggle();
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				GUILayout.Label("Invisible Pause:", "configLabel");
				experiment.pauseToggle();
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				GUILayout.Label("Texture:", "configLabel");
				experiment.textureToggle();
				GUILayout.EndVertical();
			}
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
			if(GUILayout.Button("Save and begin experiment", "defaultButton"))
			{
				experiment.SaveValues();
				state = ProgramState.WAITINGTOBEGIN;
			}

			GUILayout.EndVertical();
			GUILayout.EndArea();
			break;
		case ProgramState.WAITINGTOBEGIN:
			//start screen for experiment
			bool startButton = GUI.Button(new Rect(Screen.width / 2 - Screen.height / 4, Screen.height / 4, Screen.height / 2, Screen.height / 2), "", "startButton");
			if(startButton)
			{
				state = ProgramState.RUNNING;
			}
			break;
		case ProgramState.RUNNING:
			//when the experiment is running
			if (!(DotExperiment.pauseOption) && (currExperiment.GetName() == "ReadySetGo" || currExperiment.GetName() == "IntervalEstimation")){
				bool transparentButton = GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "", "transparentButton");
				if(transparentButton){
					intermissionCheck = false;
					state = ProgramState.INTERMISSION;
				}
			}
			if(experiment != null)
				experiment.activeState.Draw();
			else
				state = ProgramState.THANKYOU;
			
			break;

		case ProgramState.INTERMISSION:
			//the menu in between experiments if option is chosen
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height), "", "box");

			bool quitButton;
			bool nextButton;

			if (intermissionCheck){
				int quit_Height_Width = Screen.width / 48 - Screen.height / 96;
				quitButton = GUI.Button(new Rect(quit_Height_Width, quit_Height_Width, Screen.height / 4, Screen.height / 8), "Quit");
				nextButton = GUI.Button(new Rect(Screen.width / 2 - Screen.height / 4, Screen.height / 3, Screen.height / 2, Screen.height / 4), "Next!");
			}
			else
			{
				quitButton = GUI.Button(new Rect(Screen.width - (Screen.width / 2 - Screen.height / 16), Screen.height / 3, Screen.height / 2, Screen.height / 4), "Quit");
				nextButton = GUI.Button(new Rect(Screen.width / 8 - Screen.height / 16 , Screen.height / 3, Screen.height / 2, Screen.height / 4), "Keep Playing");
			}
			
			if (quitButton)
			{
				Experiment.data.Save(experiment.GetName());
				Destroy(currAsset);	
				currExperiment = null;
				state = ProgramState.SELECTTYPE;
				screenfields.Clear();
			}

			if (nextButton){
				state = ProgramState.RUNNING;
			}

			GUILayout.EndArea();
			break;

		case ProgramState.THANKYOU:
			//thank you screen after experiments
	
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height), "", "box");
			GUILayout.BeginVertical();

			GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperCenter;
			GUI.Label(new Rect(0, 0, Screen.width, Screen.height/3), "Thank you!", "thanks");

			GUI.DrawTexture(new Rect(Screen.width / 2 - Screen.height / 4, Screen.height / 3 - Screen.height / 12, Screen.height / 2, Screen.height / 4), smileyFace);

			bool continueButton = GUI.Button(new Rect(Screen.width / 2 - Screen.height / 4, Screen.height / 2, Screen.height / 2, Screen.height / 4), "Continue");
			if (continueButton){
				state = ProgramState.COMPLETE;
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();

			break;
		case ProgramState.COMPLETE:
			//complete screen after experiment is complete
			GUI.Label(new Rect(0, 0, Screen.width, Screen.height / 2 - Screen.width / 8), "Experiment complete\nWhat would you like to do now?", "endText");
			if(GUI.Button(new Rect(0, Screen.height / 2 - Screen.width / 8, Screen.width /4, Screen.width /4), "Perform the same experiment\nwith the same parameters"))
			{
				// createVLine();
				experiment = (Instantiate(lastPrefab) as GameObject).GetComponent<Experiment>();
				Experiment.init = true;
				experiment.SaveValues();
				state = ProgramState.WAITINGTOBEGIN;
			}
			if(GUI.Button(new Rect(Screen.width / 4, Screen.height / 2 - Screen.width / 8, Screen.width /4, Screen.width /4), "Perform the same experiment\nwith different parameters"))
			{
				// createVLine();
				experiment = (Instantiate(lastPrefab) as GameObject).GetComponent<Experiment>();
				Experiment.init = true;
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
		
	protected void OnPostRender()
	{
		//draws line for READYSETGO and INTERVAL ESTIMATION
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
}

public enum ProgramState
{
	SELECTTYPE, CONFIG, WAITINGTOBEGIN, RUNNING, INTERMISSION, THANKYOU, COMPLETE
}
