using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallPlat : MonoBehaviour
{
	public float fallTime = 0.5f;
	public float appearTime = 2.5f;
	private MeshRenderer m_MR;
	private Collider m_Collider;
	private void Awake()
	{
		m_Collider = GetComponent<Collider>();
		m_MR = GetComponent<MeshRenderer>();
	}
	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bulldog") || collision.gameObject.CompareTag("Runner"))
			StartCoroutine(Disappear(fallTime));
	}
	IEnumerator Disappear(float time)
	{
		yield return new WaitForSeconds(time);
		m_Collider.enabled = false;
		m_MR.enabled = false;
		StartCoroutine(Appear(appearTime));
	}
	IEnumerator Appear(float time)
	{
		yield return new WaitForSeconds(time);
		m_Collider.enabled = true;
		m_MR.enabled = true;
	}
}