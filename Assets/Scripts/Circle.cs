using UnityEngine;
using System.Collections;

[System.Serializable]
public class Circle {
	public Texture2D tex;
	public float size;

	private float pos = -1;

	public void Draw(float p)
	{
		pos = p;
		Draw ();
	}

	public void Draw()
	{
		if(pos >= 0)
			GUI.DrawTexture (new Rect ((pos - size) / 2, (Screen.height - size) / 2f, size, size), tex);
	}
}
