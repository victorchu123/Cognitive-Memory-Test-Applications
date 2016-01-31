using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class DotExperiment : Experiment {
	public Texture2D circleTex;
	public DotExpState currDotState = null;

	public static bool useLongDataSet;
	public static bool init = false; 
	private static int dictCount = 0;
	protected readonly string[] dataSetLabels = new string[]{"Use short data set", "Use long data set"};
	public static float[] shortDataSet = new float[]{2.0f, 2.4f, 2.8f, 3.2f, 3.6f, 4.0f, 4.4f, 4.8f, 5.2f, 5.6f, 6.0f};
	public static float[] longDataSet = new float[]{4.0f, 4.4f, 4.8f, 5.2f, 5.6f, 6.0f, 6.4f, 6.8f, 7.2f, 7.6f, 8.0f};

	public static List<float> dataUsed;
	public static Dictionary<string, int> dataPtFreq;

	public static ExperimentState expState{get; protected set;}


	public static float[] getCurrentSet(){
		if(useLongDataSet){
			return longDataSet;
		}
		else{
			return shortDataSet;
		}
	}

	public override float GetRandomPointFromDataSet()
	{
		if(useLongDataSet)
			return longDataSet[Random.Range(0, longDataSet.Length)];
		else
			return shortDataSet[Random.Range(0, shortDataSet.Length)];
	}
	
	public override void AddlParameters()
	{
		useLongDataSet = GUILayout.SelectionGrid((useLongDataSet ? 1 : 0), dataSetLabels, 1, "toggle") > 0;
	}
	
	public override void DrawLine()
	{
		if(activeState != null && activeState.ShouldDrawLine())
		{
			GL.Vertex3 (0, Screen.height * 0.5f, 0);
			GL.Vertex3 (Screen.width, Screen.height * 0.5f, 0);
		}
	}
	
	protected override void LoadOtherValues (string key)
	{
		useLongDataSet = (PlayerPrefs.GetInt (key + "_longSet", 0) == 1);
	}
	
	protected override void SaveValues (string key)
	{
		PlayerPrefs.SetInt (key + "_longSet", (useLongDataSet ? 1 : 0));
		base.SaveValues (key);
	}
	
	protected abstract void CreateDatapoint(string selectedDot, string retry);

	void Start(){
		Debug.Log("Start Ran");

		if(expState == null)
			init = true;

	}

	public override void OnUpdate()
	{
		if(activeState != null)
		{
			switch(activeState.GetInput())
			{
			case 1:
				//Valid input
				CreateDatapoint(GetDataValueString(currentDataValues["selectedDot"]), "No");
				break;
			case 2:
				//Too far away from line
				CreateDatapoint(GetDataValueString(currentDataValues["selectedDot"]), "Yes (too far)");
				break;
			case 3:
				//Tapped on wrong side of rightmost dot
				CreateDatapoint(GetDataValueString(currentDataValues["selectedDot"]), "Yes (wrong side)");
				break;
			case 4:
				//Invalid twice
				CreateDatapoint("N/A", "Data missing");
				break;
			case 5:
				if(Time.time > timer + (screens[activeState.TimerIndex()].Value / 1000f))
					//Out of time
					CreateDatapoint("N/A", "Data missing");
				break;
			default:break;
			}
		}
		base.OnUpdate();
	}

	public static bool generateRandAgain(float leftPt){

		int temp = 0; 

		if (dataPtFreq.TryGetValue(System.Convert.ToString(leftPt), out temp)){
			int num = dataPtFreq[System.Convert.ToString(leftPt)];
			if (GUIController.repeatedNumAllowed <= num){
				return true;
			}

			else{
				return false;
			}
		}
		else{
			return false;
		}
	}

	public static void initializeDict()
	{
		dataPtFreq = new Dictionary<string, int>();
		Debug.Log("InitializeDict Ran");
		float[] currentSet = DotExperiment.getCurrentSet();

		foreach(float datapoint in currentSet){
			// Debug.Log(System.Convert.ToString(datapoint));
			dataPtFreq.Add(System.Convert.ToString(datapoint), 0);
		}

	}

	public static void updateDictFreq(){
		
		List<float> itemsToRemove = new List<float>();

		foreach (float usedPoint in dataUsed){
			dataPtFreq[System.Convert.ToString(usedPoint)]++;
			itemsToRemove.Add(usedPoint);
		}

		foreach(float value in itemsToRemove){
			dataUsed.Remove(value);
		}
		
		printDict();
	}

	public static void printDict(){

		string output = "Dictionary Iteration #" + System.Convert.ToString(dictCount) +  ": ----------------------------";
		Debug.Log(output);
		foreach (var value in dataPtFreq){
			Debug.Log(System.Convert.ToString(value));
		}
		dictCount++;
	
	}

	public static void initializeLst(){
		dataUsed = new List<float>();
	}

	public static void addToUsedLst(float usedPt){
		dataUsed.Add(usedPt);
	}

	public static void printLst(){
		Debug.Log("Current Used DataPoint List:----------------------------");
		foreach (var value in dataUsed){
			Debug.Log(System.Convert.ToString(value));
		}
	}

}

public class ReadySetGo : DotExperiment {
	
	void Awake()
	{
		if(!LoadValues("RSGValues"))
		{
			screens = new List<BasicKeyValuePair<string, float>>(){
				new BasicKeyValuePair<string, float>("First Dot:", 3000),
				new BasicKeyValuePair<string, float>("Second Dot:", 3000),
				new BasicKeyValuePair<string, float>("Wait for User:", 3000),
				new BasicKeyValuePair<string, float>("Display User Answer:", 3000),
				new BasicKeyValuePair<string, float>("\"Please try again:\"", 3000),
				new BasicKeyValuePair<string, float>("Blank screen Between Trials:", 3000)
			};
		}
		currentDataValues = new Dictionary<string, float>(){{"time", -1}, {"delta", -1}, {"leftDot", -1}, {"rightDot", -1}, {"selectedDot", -1}, {"selectedDotY", -1}};
	}
	
	public override string GetName(){return "ReadySetGo";}
	
	public override void SaveValues()
	{
		SaveValues("RSGValues");
		
		data = new Data(new Dictionary<string, string>(){
							{"subjectID", GUIController.idField},
							{"stimScreen1",screens[0].Value.ToString()}, {"stimScreen2",screens[1].Value.ToString()},
							{"respScreen",screens[2].Value.ToString()}, {"dispScreen",screens[3].Value.ToString()},
							{"delay",screens[5].Value.ToString()}, {"blockLength",numberOfTrials.ToString()},
							{"condition",(useLongDataSet ? "Long": "Short")}
						},
						new string[]{"trialNum", "targetDist1", "targetDist2", "targetDist", "touch",
									"reactionTime", "retry"});
	}
	
	protected override void CreateDatapoint(string selectedDot, string retry)
	{
		data.CreateDatapoint(currentTrial.ToString(), GetDataValueString(currentDataValues["leftDot"]),
		                     GetDataValueString(currentDataValues["rightDot"]), currentDataValues["delta"].ToString(),
		                     selectedDot, ((Time.time - timer) * 1000).ToString(), retry);
	}

	protected override ExperimentState GetFirstState(){
		return new RSGInit();
	}
}

#region States
public abstract class DotExpState : ExperimentState
{	
	public DotExperiment experiment {get{return (GUIController.experiment as DotExperiment);}}
}

public class RSGInit : DotExpState
{	
	private bool isDone = false;
	public override int TimerIndex(){return -1;}
	public override ExperimentState GetNext(){
		return (isDone ? (new IsDoneState() as ExperimentState) : (new RSGShowLeft() as ExperimentState));
	}

	public RSGInit()
	{	
		if(++experiment.currentTrial <= experiment.numberOfTrials)
		{	
			if (DotExperiment.init == true){
				DotExperiment.initializeDict();
				DotExperiment.initializeLst();
				DotExperiment.init = false;
			}

			float dataFromSet = experiment.GetRandomPointFromDataSet();

			while(DotExperiment.generateRandAgain(dataFromSet)){
				dataFromSet = experiment.GetRandomPointFromDataSet();
			}
			experiment.currentDataValues["delta"] = dataFromSet;
			experiment.currentDataValues["leftDot"] = Random.Range(0.5f, 3.5f) * Data.cmToPixel;
			experiment.currentDataValues["rightDot"] = (experiment.currentDataValues["delta"] * Data.cmToPixel) + experiment.currentDataValues["leftDot"];

			experiment.currentDataValues["selectedDot"] = -1;
			experiment.currentDataValues["selectedDotY"] = -1;
			experiment.currentDataValues["time"] = -1;

			DotExperiment.addToUsedLst(dataFromSet);
			DotExperiment.updateDictFreq();
		}
		else isDone = true;
	}
}

public class RSGShowLeft : DotExpState
{
	public override int TimerIndex(){return 0;}
	public override ExperimentState GetNext(){return new RSGShowRight();}
	public override void Draw()
	{
		GUI.DrawTexture (new Rect (experiment.currentDataValues["leftDot"] - Data.cmToPixel / 2, (Screen.height - Data.cmToPixel) / 2f, Data.cmToPixel, Data.cmToPixel),
		                 experiment.circleTex);
	}
}

public class RSGShowRight : DotExpState
{
	public override int TimerIndex(){return 1;}
	public override ExperimentState GetNext(){return new RSGWaitForInput();}
	public override void Draw()
	{
		GUI.DrawTexture (new Rect (experiment.currentDataValues["rightDot"] - Data.cmToPixel / 2, (Screen.height - Data.cmToPixel) / 2f, Data.cmToPixel, Data.cmToPixel),
		                 experiment.circleTex);
	}
}

public abstract class DotExpWaitForInput : DotExpState
{
	protected const float lineDistanceCM = 0.5f;
	protected static bool allowTryAgain = true;
	protected static int tryAgainType = 1;
	protected bool forceNext = false;
	protected bool acceptTouch = false;
	
	protected abstract bool TouchedTooFarLeft();
	protected abstract ExperimentState GetNextStateIfTouched(bool ta);
	protected abstract ExperimentState GetNextStateIfTimeout();
	
	public override int GetInput()
	{
		if(InputController.IsTouching())
		{
			forceNext = true;
			experiment.currentDataValues["selectedDot"] = InputController.GetTouchPosition().x;
			experiment.currentDataValues["selectedDotY"] = InputController.GetTouchPosition().y;

			if(Mathf.Abs(InputController.GetTouchPosition().y - Screen.height * 0.5f) > lineDistanceCM * Data.cmToPixel) //controls how far above line the point will be allowed to be
			{
				Debug.Log("Touched outside line");
				if(allowTryAgain)
				{
					tryAgainType = 2;
					return 0;
				}
				else return 4;
			}
			else if(TouchedTooFarLeft())
			{
				Debug.Log("Too far left");
				if(allowTryAgain)
				{
					tryAgainType = 3;
					return 0;
				}
				else return 4;
			}
			else
			{
				//Valid input
				acceptTouch = true;
				allowTryAgain = false;
				return tryAgainType;
			}
		}
		return 5;
	}
	
	public override ExperimentState GetNext()
	{
		if(!acceptTouch && forceNext && allowTryAgain)
		{
			//Try again
			allowTryAgain = false;
			return GetNextStateIfTouched(true);
		}
		else
		{
			allowTryAgain = true;
			tryAgainType = 1;
			if(acceptTouch)
				//Valid input
				return GetNextStateIfTouched(false);
			else if(forceNext)
				//Failed twice
				return GetNextStateIfTouched(false);
			else
				//Out of time
				return GetNextStateIfTimeout();
		}
	}
}

public class RSGWaitForInput : DotExpWaitForInput
{
	public override int TimerIndex(){return (forceNext ? -1 : 2);}
	protected override ExperimentState GetNextStateIfTouched(bool ta){return new RSGShowUserTouch(ta);}
	protected override ExperimentState GetNextStateIfTimeout(){return new RSGBlank();}
	
	protected override bool TouchedTooFarLeft()
	{
		return InputController.GetTouchPosition().x <= experiment.currentDataValues["rightDot"];
	}
}

public class RSGShowUserTouch : DotExpState
{
	private bool tryAgain;
	public RSGShowUserTouch(bool shouldTryAgain)
	{
		tryAgain = shouldTryAgain;
	}
	public override int TimerIndex(){return 3;}
	public override ExperimentState GetNext(){return (tryAgain ? (new RSGTryAgain() as ExperimentState) : (new RSGBlank() as ExperimentState));}
	public override void Draw()
	{
		GUI.DrawTexture (new Rect (experiment.currentDataValues["selectedDot"] - Data.cmToPixel / 2, (Screen.height - Data.cmToPixel) / 2f, Data.cmToPixel, Data.cmToPixel),
		                 experiment.circleTex);
	}
}

public abstract class TryAgain : ExperimentState
{
	public override void Draw()
	{
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height/2), "Please try again!", "tryAgain");
	}
	public override bool ShouldDrawLine(){return true;}
}

public class RSGTryAgain : TryAgain
{
	public override int TimerIndex(){return 4;}
	public override ExperimentState GetNext(){return new RSGShowLeft();}
}

public class RSGBlank : DotExpState
{
	public override int TimerIndex(){return 5;}
	public override ExperimentState GetNext()
	{

		if (GUIController.advanceOption && experiment.currentTrial < experiment.numberOfTrials){
			GUIController.intermissionCheck = true;
			GUIController.state = ProgramState.INTERMISSION;
		}

		DotExperiment.printDict();
		return new RSGInit();
	}
	public override bool ShouldDrawLine(){return true;}
}
#endregion

