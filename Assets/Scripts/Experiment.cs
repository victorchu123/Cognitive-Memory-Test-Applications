using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

public abstract class Experiment : MonoBehaviour {
	public int numberOfTrials;
	public List<BasicKeyValuePair<string, float>> screens;
	public Dictionary<string, float> currentDataValues;
	
	public int currentTrial = 0;
	
	public Data data{get; protected set;}
	
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
		if(activeState != null && activeState.TimerIndex() == -2)
		{
			data.Save(GetName());
//			#if UNITY_EDITOR
//			Debug.Break();
//			#endif
//			Application.Quit()
			Destroy(gameObject);
			//Somehow revert everything and go back to the main menu?  Maybe put this in the GUI controller?
		}
		else if(activeState == null || activeState.TimerIndex() == -1 || Time.time > timer + (screens[activeState.TimerIndex()].Value / 1000f))
		{
			timer = Time.time;
			activeState = (activeState == null ? GetFirstState() : activeState.GetNext());
		}
	}
	
	/*protected void ChangeState()
	{
		timer = Time.time;
		activeState = OnStateChange();
	}
	
	public abstract int OnStateChange();
	public abstract void Draw();*/
	protected abstract ExperimentState GetFirstState();
	public abstract string GetName();
	public abstract void SaveValues();
	public abstract void AddlParameters();
	public abstract float GetRandomPointFromDataSet();
	public abstract void DrawLine();
}

public abstract class ExperimentState
{
	public abstract int TimerIndex();
	public abstract ExperimentState GetNext();
	public virtual void Draw(){}
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
