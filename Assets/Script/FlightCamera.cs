using UnityEngine;
using System.Collections;

public class FlightCamera : MonoBehaviour {
	public GameObject m_gFlightModel = null;
	public float m_fSpeed            = 0.0f;
	public float m_fRotateSpeed      = 0.0f;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = transform.position + transform.forward * this.m_fSpeed * Time.deltaTime * 0.001f;
		transform.Rotate(new Vector3(0.0f, 1.0f, 0.0f), this.m_fRotateSpeed * Time.deltaTime*0.001f * Input.GetAxis("Horizontal"));
		
		transform.position = transform.position + new Vector3(0.0f, 15000.0f, 0.0f) * Time.deltaTime * 0.001f * Input.GetAxis("Vertical") * -1.0f;		
//		m_gFlightModel.transform.localRotation = Quaternion.Euler(new Vector3(30.0f * Input.GetAxis("Vertical"), 0.0f, -60.0f * Input.GetAxis("Horizontal")));
	
	}
}
