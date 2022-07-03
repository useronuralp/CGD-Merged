using UnityEngine;
/// <summary>
/// Floats the attached object up and down in a subtel motion.
/// </summary>
public class Float : MonoBehaviour
{
    private Vector3 m_StartPos;
    // Update is called once per frame
    private void Start()
    {
        m_StartPos = transform.position;
    }
    void Update()
    {
        transform.position = new Vector3(transform.position.x, m_StartPos.y + Mathf.Sin(Time.time / 0.5f), transform.position.z);
    }
}
