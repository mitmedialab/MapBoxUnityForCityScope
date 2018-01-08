using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour
{
    public Camera m_Camera;

    void Update()
    {
        m_Camera = Camera.main;
        transform.LookAt(new Vector3(m_Camera.transform.position.x,0f,m_Camera.transform.position.z));
        transform.Rotate(Vector3.up, 180f);
    }
}