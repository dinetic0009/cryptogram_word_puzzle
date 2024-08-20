using UnityEngine;
using System.Collections;

public class RotateIndicator : MonoBehaviour
{
	Vector3 angle;

	void Start()
	{
		angle = transform.eulerAngles;
	}

	void Update()
	{
		angle.z += Time.deltaTime * -100;
		transform.eulerAngles = angle;
	}

}
