using UnityEngine;

public class RotateAngle : MonoBehaviour 
{
	public Vector3 axis;
	public float rate;

	void Update () 
	{
		transform.Rotate(axis * Time.deltaTime * rate);
	}
}