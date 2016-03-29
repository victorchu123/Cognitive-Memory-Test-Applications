using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;

public abstract class Experiment : MonoBehaviour {
	public int numberOfTrials;
	public List<BasicKeyValuePair<string, float>> screens;
	public Dictionary<string, float> currentDataValues;
	public int currentTrial = 0;
	private int dictCount = 0;
	public static bool init = true; 
	public static Data data{get; protected set;}

	public List<float> dataUsed;
	public Dictionary<string, int> dataPtFreq;
	
	protected float timer;
	
	public ExperimentState activeState{get; protected set;}

	public string GetDataValueString(float input)
	{
		return (input / Data.cmToPixel).ToString ();
	}

	protected bool LoadValues(string key)
	{
		numberOfTrials = PlayerPrefs.GetInt(key + "_trials", 10);
		if (PlayerPrefs.HasKey (key))
		{
			using(StringReader reader = new StringReader(PlayerPrefs.GetString(key)))
			{
				screens = new XmlSerializer(typeof(List<BasicKeyValuePair<string, float>>)).Deserialize(reader) as List<BasicKeyValuePair<string, float>>;
			}
			LoadOtherValues(key);
			return true;
		}
		else
			return false;
	}

	protected virtual void LoadOtherValues(string key){}
	
	protected virtual void SaveValues(string key)
	{
		PlayerPrefs.SetInt(key + "_trials", numberOfTrials);
		using (StringWriter writer = new StringWriter())
		{
			new XmlSerializer(typeof(List<BasicKeyValuePair<string, float>>)).Serialize(writer, screens);
			PlayerPrefs.SetString(key, writer.ToString());
		}
	}
	
	public virtual void OnUpdate()
	{
		// Debug.Log("OnUpdate Ran");
		if(activeState != null && activeState.TimerIndex() == -2)
		{
			data.Save(GetName());
//			#if UNITY_EDITOR
//			Debug.Break();
//			#endif
//			Application.Quit()
			Destroy(gameObject);
			Camera.main.GetComponent<AudioSource>().Play();
			//Somehow revert everything and go back to the main menu?  Maybe put this in the GUI controller?
		}
		else if(activeState == null || activeState.TimerIndex() == -1 || Time.time > timer + (screens[activeState.TimerIndex()].Value / 1000f))
		{
			timer = Time.time;
			activeState = (activeState == null ? GetFirstState() : activeState.GetNext());
		}
	}

	/* checks if the current datapoint can be used in new trial; if it has reached its maximum number of unique uses 
	then we pick a new random datapoint */

	public bool generateRandAgain(float deltaPt)
	{
		int temp; 
		double roundedPt = Math.Round(deltaPt, 3);

		if(dataPtFreq != null){
			if (dataPtFreq.TryGetValue(System.Convert.ToString(roundedPt), out temp)){
				int num = dataPtFreq[System.Convert.ToString(roundedPt)];
				if (GUIController.repeatedNumAllowed <= num)
					return true;
				else
					return false;
			}
			else{
				return false;
			}
	 }
	 else{
	 	return false;
	 }

	}

	//intializes dictionary
	public void initializeDict(float[] dataSet)
	{
		dataPtFreq = new Dictionary<string, int>();
		Debug.Log("InitializeDict Ran");
		float[] currentSet = dataSet;

		foreach(float datapoint in currentSet){
			// Debug.Log(System.Convert.ToString(datapoint));
			dataPtFreq.Add(System.Convert.ToString(datapoint), 0);
		}
	}

	public void printDict(){
		string output = "Dictionary Iteration #" + System.Convert.ToString(dictCount) +  ": ----------------------------";
		Debug.Log(output);
		foreach (var value in dataPtFreq){
			Debug.Log(System.Convert.ToString(value));
		}
	}

	//updates datapoint used; stored in a dictionary
	public void updateDictFreq(float usedPoint)
	{
		int temp;
		double roundedPt = Math.Round(usedPoint, 3);
	
		if(dataPtFreq != null){
			if (dataPtFreq.TryGetValue(System.Convert.ToString(roundedPt), out temp)){
				Debug.Log("Entry already in dictionary.");
				dataPtFreq[System.Convert.ToString(roundedPt)] = dataPtFreq[System.Convert.ToString(roundedPt)] + 1;
			}
			else{
				Debug.Log("Added entry to dictionary.");
				dataPtFreq.Add(System.Convert.ToString(roundedPt), 1);
			}
			printDict();
			dictCount++;
		}
	}

	protected abstract ExperimentState GetFirstState();
	public abstract string GetName();
	public abstract void SaveValues();
	public abstract void AddlParameters();
	public abstract float GetRandomPointFromDataSet();
	public abstract void DrawLine();
	public abstract void textureToggle();
	public abstract void advanceToggle();
	public abstract void pauseToggle();
}

public abstract class ExperimentState
{
	public abstract int TimerIndex();
	public abstract ExperimentState GetNext();
	public virtual void Draw(){}
	public virtual void OnInitialUpdate(){}
	public virtual void OnUpdate(){}
	public virtual int GetInput(){return 0;}
	public virtual bool ShouldDrawLine(){return true;}
//	public virtual bool GetInput(out Vector2 tapPoint){tapPoint = Vector2.zero; return false;}
}

public class IsDoneState : ExperimentState
{
	public override int TimerIndex(){return -2;}
	public override ExperimentState GetNext(){return null;}
}

public class BasicKeyValuePair<T, R>
{
	public T Key;
	public R Value;
	public BasicKeyValuePair()
	{
		Key = default(T);
		Value = default(R);
	}
	
	public BasicKeyValuePair(T t, R r)
	{
		Key = t;
		Value = r;
	}
}
