using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KBIS : Experiment {
	public const float lineHeightCM = 1.5f;
	public bool changingLineLength{get; private set;}
	private float firstScreenLineLength = 0.5f;
//	private string firstScreenLineField = "0.5";
	private readonly float[] possibleLineLengthMults = new float[]{0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.9f, 0.95f, 1f, 1.05f, 1.1f, 1.4f, 1.45f, 1.5f, 1.55f, 1.6f};
	
	void Awake()
	{
		if(!LoadValues("KBISValues"))
		{
			screens = new List<BasicKeyValuePair<string, float>>(){
				new BasicKeyValuePair<string, float>("Show Centered Line", 3000),
				new BasicKeyValuePair<string, float>("Blank Screen Before Prompt", 3000),
				new BasicKeyValuePair<string, float>("Show Prompt / Wait for Use", 3000),
				new BasicKeyValuePair<string, float>("Display User Answer", 3000),
				new BasicKeyValuePair<string, float>("\"Please try again!\"", 3000),
				new BasicKeyValuePair<string, float>("Blank Screen Between Experiments", 3000)
			};
		}
		currentDataValues = new Dictionary<string, float>(){{"time", -1}, {"promptLineX", -1}, {"promptLineY", -1}, {"promptLineLength", -1}, {"targetPoint", -1}, {"selectedPoint", -1}, {"selectedPointY", -1}};
	}
	public override string GetName(){return "KBIS";}
	
	public override void SaveValues()
	{
		SaveValues("KBISValues");
		
		data = new Data(new Dictionary<string, string>(){
			{"stimScreen",screens[0].Value.ToString()}, {"delay1",screens[1].Value.ToString()},
			{"respScreen",screens[2].Value.ToString()}, {"dispScreen",screens[3].Value.ToString()},
			{"delay2",screens[5].Value.ToString()}, {"blockLength",numberOfTrials.ToString()},
			{"lengthChange",(changingLineLength ? "Yes": "No")}
		},
		new string[]{"trialNum", "targetNum", "lineLengthRelative", "lineLengthActual", "touch",
			"reactionTime", "retry"});
	}

	public override void OnUpdate()
	{
		if(activeState != null)
		{
			switch(activeState.GetInput())
			{
			case 1:
				//Valid input
				CreateDatapoint(GetDataValueString(currentDataValues["selectedPoint"]), "No");
				break;
			case 2:
				//Too far away from line vertically
				CreateDatapoint(GetDataValueString(currentDataValues["selectedPoint"]), "Yes (too far vertically)");
				break;
			case 3:
				//Tapped outside line bounds
				CreateDatapoint(GetDataValueString(currentDataValues["selectedPoint"]), "Yes (too far horizontally)");
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
	
	protected void CreateDatapoint(string selectedDot, string retry)
	{
		data.CreateDatapoint(currentTrial.ToString(), GetDataValueString(currentDataValues["targetPoint"] * GetActualFirstLineLength()),
		                     currentDataValues["promptLineLength"].ToString(), GetDataValueString(GetActualPromptLineLength()), 
		                     selectedDot, ((Time.time - timer) * 1000).ToString(), retry);
	}
	
	public override float GetRandomPointFromDataSet()
	{
		return Random.Range(1, 40) * 0.025f;
	}
	public float GetRandomLineLengthFromDataSet()
	{
		if(changingLineLength)
			return possibleLineLengthMults[Random.Range(0, possibleLineLengthMults.Length)];
		else
			return 1;
	}
	
	public override void AddlParameters()
	{
//		float tempLineLengthVal;
//		GUILayout.BeginHorizontal();
//		GUILayout.Space(100);
//		GUILayout.Label("First screen line length, relative to screen size", GUILayout.ExpandWidth(false));
//		try
//		{
//			tempLineLengthVal = System.Convert.ToSingle(firstScreenLineField);
//			firstScreenLineLength = tempLineLengthVal;
//		}
//		catch(System.Exception e)
//		{
//			GUI.color = Color.red;
//		}
//		firstScreenLineField = GUILayout.TextField(firstScreenLineField);
//		GUI.color = Color.white;
//		GUILayout.EndHorizontal();
		//Will line length change
		GUILayout.BeginHorizontal();
		GUILayout.Space(100);
		changingLineLength = GUILayout.Toggle(changingLineLength, "Change Line Length");
		GUILayout.EndHorizontal();
	}

	protected override void LoadOtherValues (string key)
	{
		changingLineLength = (PlayerPrefs.GetInt (key + "_changingLine", 0) == 1);
	}
	
	protected override void SaveValues (string key)
	{
		PlayerPrefs.SetInt (key + "_changingLine", (changingLineLength ? 1 : 0));
		base.SaveValues (key);
	}
	
	public override void DrawLine()
	{
		if(activeState != null && (activeState as KBISState) != null && activeState.ShouldDrawLine())
		{
			Vector3 currentLineLoc = (activeState as KBISState).GetLineLocation();
			if(currentLineLoc.z > 0)
			{
				GL.Vertex3 (currentLineLoc.x, currentLineLoc.y, 0);
				GL.Vertex3 (currentLineLoc.x + currentLineLoc.z, currentLineLoc.y, 0);
				GL.Vertex3 (currentLineLoc.x, currentLineLoc.y + (lineHeightCM * Data.cmToPixel), 0);
				GL.Vertex3 (currentLineLoc.x, currentLineLoc.y - (lineHeightCM * Data.cmToPixel), 0);
				GL.Vertex3 (currentLineLoc.x + currentLineLoc.z, currentLineLoc.y + (lineHeightCM * Data.cmToPixel), 0);
				GL.Vertex3 (currentLineLoc.x + currentLineLoc.z, currentLineLoc.y - (lineHeightCM * Data.cmToPixel), 0);
				(activeState as KBISState).DrawHashMark((Vector2)currentLineLoc);
			}
		}
	}

	protected override ExperimentState GetFirstState(){return new KBISInit();}
	
	public float GetActualFirstLineLength()
	{
		return firstScreenLineLength * Screen.width;
	}
	
	public float GetActualPromptLineLength()
	{
		return currentDataValues["promptLineLength"] * firstScreenLineLength * Screen.width;
	}
}


#region states
public abstract class KBISState : ExperimentState
{
	public KBIS experiment {get{return (GUIController.experiment as KBIS);}}
	public virtual Vector3 GetLineLocation(){return Vector3.zero;}
	public virtual void DrawHashMark(Vector2 lineLoc){}
}

public class KBISInit : KBISState
{
	private bool isDone = false;
	public override int TimerIndex(){return -1;}
	public override ExperimentState GetNext(){return (isDone ? (new IsDoneState() as ExperimentState) : (new KBISShowFirstLine() as ExperimentState));}
	public KBISInit()
	{
		if(++experiment.currentTrial <= experiment.numberOfTrials)
		{
			experiment.currentDataValues["targetPoint"] = experiment.GetRandomPointFromDataSet();
			experiment.currentDataValues["promptLineLength"] = experiment.GetRandomLineLengthFromDataSet();
			//Should generate a line location that's within one tick's width of the edge of the screen, and should always
			//allow the entire line to fit.
			//TODO:May need to change this if the "correctness" here is based on actual distance and not relative distance
			experiment.currentDataValues["promptLineX"] = Random.Range((int)(0.025f * experiment.GetActualFirstLineLength()),
			                                                           Screen.width - (int)(experiment.GetActualPromptLineLength() + (0.025f * experiment.GetActualFirstLineLength())));
			//Should place the line vertically such that it's fully visible.
			experiment.currentDataValues["promptLineY"] = Random.Range((int)((KBIS.lineHeightCM / 2) * Data.cmToPixel),
			                                                           Screen.height - (int)((KBIS.lineHeightCM / 2) * Data.cmToPixel));
			experiment.currentDataValues["selectedPoint"] = -1;
			experiment.currentDataValues["selectedPointY"] = -1;
			experiment.currentDataValues["time"] = -1;
		}
		else isDone = true;
	}
}

public class KBISShowFirstLine : KBISState
{
	public override int TimerIndex(){return 0;}
	//TODO
	public override ExperimentState GetNext(){return new KBISBlank1();}
	public override Vector3 GetLineLocation()
	{
		return new Vector3((Screen.width - experiment.GetActualFirstLineLength()) / 2, 
							Screen.height / 2,
							experiment.GetActualFirstLineLength());
	}
	public override void DrawHashMark(Vector2 lineLoc)
	{
		GL.Vertex3 (lineLoc.x + experiment.currentDataValues["targetPoint"] * experiment.GetActualFirstLineLength(), lineLoc.y + (KBIS.lineHeightCM * Data.cmToPixel * 2) / 3, 0);
		GL.Vertex3 (lineLoc.x + experiment.currentDataValues["targetPoint"] * experiment.GetActualFirstLineLength(), lineLoc.y - (KBIS.lineHeightCM * Data.cmToPixel * 2
		) / 3, 0);
	}
}

public class KBISBlank1 : KBISState
{
	public override int TimerIndex(){return 1;}
	public override ExperimentState GetNext(){return new KBISWaitForInput();}
	public override bool ShouldDrawLine(){return false;}
}

public class KBISBlank2 : KBISState
{
	public override int TimerIndex(){return 5;}
	public override ExperimentState GetNext(){return new KBISInit();}
	public override bool ShouldDrawLine(){return false;}
}

public class KBISWaitForInput : KBISState
{
	protected static bool allowTryAgain = true;
	protected static int tryAgainType = 1;
	protected bool forceNext = false;
	protected bool acceptTouch = false;
	
	public override int TimerIndex(){return (forceNext ? -1 : 2);}
	
	public override int GetInput()
	{
		if(InputController.IsTouching())
		{
			forceNext = true;
			experiment.currentDataValues["selectedPoint"] = InputController.GetTouchPosition().x - experiment.currentDataValues["promptLineX"];
			experiment.currentDataValues["selectedPointY"] = InputController.GetTouchPosition().y - experiment.currentDataValues["promptLineY"];
			
			if(Mathf.Abs(experiment.currentDataValues["selectedPointY"]) > KBIS.lineHeightCM * Data.cmToPixel)
			{
				Debug.Log("Touched outside line vertically");
				if(allowTryAgain)
				{
					tryAgainType = 2;
					return 0;
				}
				else return 4;
			}
			//TODO:May need to change this if the "correctness" here is based on actual distance and not relative distance
			else if(InputController.GetTouchPosition().x < experiment.currentDataValues["promptLineX"] ||
			        InputController.GetTouchPosition().x > experiment.currentDataValues["promptLineX"] + experiment.GetActualPromptLineLength())
			{
				Debug.Log("Touched outside line horizontally");
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
			return new KBISShowUserTouch(true);
		}
		else
		{
			allowTryAgain = true;
			tryAgainType = 1;
			if(acceptTouch)
				//Valid input
				return new KBISShowUserTouch(false);
			else if(forceNext)
				//Failed twice
				return new KBISShowUserTouch(false);
			else
				//Out of time
				return new KBISBlank2();
		}
	}
	
	public override Vector3 GetLineLocation()
	{
		return new Vector3(experiment.currentDataValues["promptLineX"], 
		                   experiment.currentDataValues["promptLineY"],
		                   experiment.GetActualPromptLineLength());
	}
}

public class KBISShowUserTouch : KBISState
{
	private bool tryAgain;
	public KBISShowUserTouch(bool shouldTryAgain)
	{
		tryAgain = shouldTryAgain;
	}
	public override int TimerIndex(){return 3;}
	public override ExperimentState GetNext(){return (tryAgain ? (new KBISTryAgain() as ExperimentState) : (new KBISBlank2() as ExperimentState));}
	public override Vector3 GetLineLocation()
	{
		return new Vector3(experiment.currentDataValues["promptLineX"], 
		                   experiment.currentDataValues["promptLineY"],
		                   experiment.GetActualPromptLineLength());
	}
	public override void DrawHashMark(Vector2 lineLoc)
	{
		GL.Color(Color.red);
		GL.Vertex3 (lineLoc.x + experiment.currentDataValues["selectedPoint"], lineLoc.y + experiment.currentDataValues["selectedPointY"] + (KBIS.lineHeightCM * Data.cmToPixel * 2) / 3, 0);
		GL.Vertex3 (lineLoc.x + experiment.currentDataValues["selectedPoint"], lineLoc.y + experiment.currentDataValues["selectedPointY"] - (KBIS.lineHeightCM * Data.cmToPixel * 2) / 3, 0);
	}
}

public class KBISTryAgain : KBISState
{
	public override void Draw()
	{
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height/2), "Please try again!", "tryAgain");
	}
	public override bool ShouldDrawLine(){return true;}
	public override int TimerIndex(){return 4;}
	public override ExperimentState GetNext(){return new KBISShowFirstLine();}
}
#endregion
