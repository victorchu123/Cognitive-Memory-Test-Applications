using UnityEngine;
using System.Collections;

public class InputController {
	public static bool IsTouching()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		return Input.GetMouseButton(0);
#else
		return Input.touchCount > 0;
#endif
	}
	
	public static Vector2 GetTouchPosition()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		return (Vector2)Input.mousePosition;
#else
		return Input.GetTouch(0).position;
#endif
	}
}
