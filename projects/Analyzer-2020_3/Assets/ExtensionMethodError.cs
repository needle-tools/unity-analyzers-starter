using System;
using UnityEngine;

public static class ExtensionMethods
{
	public static bool IsNumeric(this object obj)
	{
		if (obj == null)
			return false;
		return obj.GetType().IsPrimitive || obj is double || (obj is Decimal || obj is DateTime) || obj is TimeSpan;
	} 
}

public class ExtensionMethodError : MonoBehaviour
{
	private void Start()
	{
		if ("test".EndsWith("test"))
		{
			Debug.Log("OK");
		}
	}
	
	private void FixedUpdate()
	{
	}
}