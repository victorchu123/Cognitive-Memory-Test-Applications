using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Data {
	public static float cmToPixel;
	private Dictionary<string, string> singleEntryData;
	private List<string> varNames;
	private List<List<string>> dataPoints;
	
	public Data(Dictionary<string, string> singleEntryVars, string[] vars)
	{
		singleEntryData = singleEntryVars;
		dataPoints = new List<List<string>>();
		varNames = new List<string>();
		for(int i = 0; i < vars.Length; i++)
		{
			varNames.Add(vars[i]);
			dataPoints.Add(new List<string>());
		}
	}
	
	public void CreateDatapoint(params string[] vals)
	{
		for(int i = 0; i < dataPoints.Count; i++)
		{
			dataPoints[i].Add(vals[i]);
		}
	}
	
	public void Save(string type)
	{
		//TODO:For now, just make each experiment its own file.  Deal with potentially merging multiple experiments later.
		System.DateTime timeID = System.DateTime.Now;
		Debug.Log(Application.persistentDataPath);
		string outputString = "";
		List<string> keys = new List<string>(singleEntryData.Keys);
		for(int i = 0; i < varNames.Count; i++)
		{
			outputString += ((i == 0 ? "" : ",") + varNames[i]);
		}
		for(int i = 0; i < keys.Count; i++)
		{
			outputString += ("," + keys[i]);
		}
		outputString += "\n";
		//I have no idea why I did it like this, but whatever.
		for(int j = 0; j < dataPoints[0].Count; j++)
		{
			for(int i = 0; i < dataPoints.Count; i++)
			{
				outputString += ((i == 0 ? "" : ",") + dataPoints[i][j]);
			}
			for(int i = 0; i < keys.Count; i++)
			{
				outputString += ("," + singleEntryData[keys[i]]);
			}
			outputString += "\n";
		}
			
		using (FileStream fs = File.Create(Application.persistentDataPath + "/Experiment_" + type + "_" +
											timeID.Month + "-" + timeID.Day + "-" + timeID.Year + "_" + timeID.Hour + "-" + timeID.Minute + "-" + timeID.Second + ".csv"))
		{
			System.Byte[] info = new UTF8Encoding(true).GetBytes(outputString);
			fs.Write(info, 0, info.Length);
		}
	}
	
	/*public int datapoint { get; private set; }
	public int selectedPoint{ get; set; }

	public void CreateDatapoint()
	{
		datapoint = Mathf.RoundToInt(Random.value * GUIController.width + GUIController.size.x);
	}*/
}
