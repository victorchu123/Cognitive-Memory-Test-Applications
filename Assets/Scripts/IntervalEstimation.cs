using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntervalEstimation : DotExperiment {
	
	void Awake()
	{
		if(!LoadValues("IEValues"))
		{
			screens = new List<BasicKeyValuePair<string, float>>(){
				new BasicKeyValuePair<string, float>("Show Interval:", 3000),
				new BasicKeyValuePair<string, float>("Blank Screen Before Prompt:", 3000),
				new BasicKeyValuePair<string, float>("Show Prompt / Wait for User:", 3000),
				new BasicKeyValuePair<string, float>("Display User Answer:", 3000),
				new BasicKeyValuePair<string, float>("\"Please try again:\"", 3000),
				new BasicKeyValuePair<string, float>("Blank Screen Between Trials:", 3000)
			};
		}
		currentDataValues = new Dictionary<string, float>(){{"time", -1}, {"delta", -1}, {"leftDot", -1}, {"rightDot", -1}, {"promptDot", -1}, {"selectedDot", -1}, {"selectedDotY", -1}};
	}
	
	public override string GetName(){return "IntervalEstimation";}
	
	public override void SaveValues()
	{
		SaveValues("IEValues");
		
		data = new Data(new Dictionary<string, string>(){
			{"stimScreen",screens[0].Value.ToString()}, {"delay1",screens[1].Value.ToString()},
			{"respScreen",screens[2].Value.ToString()}, {"dispScreen",screens[3].Value.ToString()},
			{"delay2",screens[5].Value.ToString()}, {"blockLength",numberOfTrials.ToString()},
			{"condition",(useLongDataSet ? "Long": "Short")}
		},
		new string[]{"trialNum", "targetDistL", "targetDistR", "targetDist", "loc1", "touch",
			"reactionTime", "retry"});
	}
	
	protected override void CreateDatapoint(string selectedDot, string retry)
	{
		data.CreateDatapoint(currentTrial.ToString(), GetDataValueString(currentDataValues["leftDot"]),
		                     GetDataValueString(currentDataValues["rightDot"]), currentDataValues["delta"].ToString(),
		                     GetDataValueString(currentDataValues["promptDot"]),
		                     selectedDot, ((Time.time - timer) * 1000).ToString(), retry);
	}
	
	protected override ExperimentState GetFirstState(){return new IEInit();}
}

#region States
//public abstract class IEState : ExperimentState
//{
//	public IntervalEstimation experiment {get{return (GUIController.experiment as IntervalEstimation);}}
//}

public class IEInit : DotExpState
{
	private bool isDone = false;
	public override int TimerIndex(){return -1;}
	public override ExperimentState GetNext(){return (isDone ? (new IsDoneState() as ExperimentState) : (new IEShowInterval() as ExperimentState));}
	public IEInit()
	{
		if(++experiment.currentTrial <= experiment.numberOfTrials)
		{
			experiment.currentDataValues["delta"] = experiment.GetRandomPointFromDataSet();
			//Choose a location for the left dot such that both dots will always appear fully on screen.
			experiment.currentDataValues["leftDot"] = Random.Range(Data.cmToPixel / 2, Screen.width - (experiment.currentDataValues["delta"] * Data.cmToPixel) - Data.cmToPixel / 2);
			//The location of the right dot is determined by the delta.
			experiment.currentDataValues["rightDot"] = (experiment.currentDataValues["delta"] * Data.cmToPixel) + experiment.currentDataValues["leftDot"];
			//Place the prompt dot somewhere in the leftmost quarter of the screen
			experiment.currentDataValues["promptDot"] = Random.Range(Data.cmToPixel / 2, Screen.width / 4);
			experiment.currentDataValues["selectedDot"] = -1;
			experiment.currentDataValues["selectedDotY"] = -1;
			experiment.currentDataValues["time"] = -1;
		}
		else isDone = true;
	}
}

public class IEShowInterval : DotExpState
{
	public override int TimerIndex(){return 0;}
	public override ExperimentState GetNext(){return new IEBlank1();}
	public override void Draw()
	{
		GUI.DrawTexture (new Rect (experiment.currentDataValues["leftDot"] - Data.cmToPixel / 2, (Screen.height - Data.cmToPixel) / 2f, Data.cmToPixel, Data.cmToPixel),
		                 experiment.circleTex);
		GUI.DrawTexture (new Rect (experiment.currentDataValues["rightDot"] - Data.cmToPixel / 2, (Screen.height - Data.cmToPixel) / 2f, Data.cmToPixel, Data.cmToPixel),
		                 experiment.circleTex);
	}
}

public class IEBlank1 : DotExpState
{
	public override int TimerIndex(){return 1;}
	public override ExperimentState GetNext()
	{
		return new IEWaitForInput();
	}
	public override bool ShouldDrawLine(){return true;}
}

public class IEBlank2 : DotExpState
{
	public override int TimerIndex(){return 5;}
	public override ExperimentState GetNext()
	{
		if (GUIController.advanceOption && experiment.currentTrial < experiment.numberOfTrials){
			GUIController.state = ProgramState.INTERMISSION;
		}
		return new IEInit();
	}
	public override bool ShouldDrawLine(){return true;}
}

public class IEWaitForInput : DotExpWaitForInput
{
	public override int TimerIndex(){return (forceNext ? -1 : 2);}
	protected override ExperimentState GetNextStateIfTouched(bool ta){return new IEShowUserTouch(ta);}
	protected override ExperimentState GetNextStateIfTimeout(){return new IEBlank2();}
	
	protected override bool TouchedTooFarLeft()
	{
		return InputController.GetTouchPosition().x <= experiment.currentDataValues["promptDot"];
	}
	
	public override void Draw()
	{
		GUI.DrawTexture (new Rect (experiment.currentDataValues["promptDot"] - Data.cmToPixel / 2, (Screen.height - Data.cmToPixel) / 2f, Data.cmToPixel, Data.cmToPixel),
		                 experiment.circleTex);
	}
}

public class IEShowUserTouch : DotExpState
{
	private bool tryAgain;
	public IEShowUserTouch(bool shouldTryAgain)
	{
		tryAgain = shouldTryAgain;
	}
	public override int TimerIndex(){return 3;}
	public override ExperimentState GetNext(){return (tryAgain ? (new IETryAgain() as ExperimentState) : (new IEBlank2() as ExperimentState));}
	public override void Draw()
	{
		GUI.DrawTexture (new Rect (experiment.currentDataValues["promptDot"] - Data.cmToPixel / 2, (Screen.height - Data.cmToPixel) / 2f, Data.cmToPixel, Data.cmToPixel),
		                 experiment.circleTex);
		GUI.DrawTexture (new Rect (experiment.currentDataValues["selectedDot"] - Data.cmToPixel / 2, Screen.height - experiment.currentDataValues["selectedDotY"] - Data.cmToPixel / 2, Data.cmToPixel, Data.cmToPixel),
		                 experiment.circleTex);
	}
}

public class IETryAgain : TryAgain
{
	public override int TimerIndex(){return 4;}
	public override ExperimentState GetNext(){return new IEShowInterval();}
}

#endregion
