using System;
using System.Threading;
using UnityEngine;

namespace DefaultNamespace
{
	public class UsingDateTimeNow : MonoBehaviour
	{
		private void Start()
		{
			var dt = DateTime.Now;
		}
	}
}