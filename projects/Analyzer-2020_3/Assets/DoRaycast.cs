using UnityEngine;

public class DoRaycast : MonoBehaviour
{
	private void Start()
	{
		Physics.Raycast(Vector3.zero, Vector3.forward);
	}
}
