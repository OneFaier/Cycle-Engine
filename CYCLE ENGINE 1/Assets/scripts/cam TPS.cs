using System;
using UnityEngine;

public class camTPS : MonoBehaviour
{
    public GameObject target;
    public Vector3 offset;

    private void Update()
    {
        transform.position = target.transform.position + offset;
        
        Debug.DrawLine(transform.position, target.transform.position, Color.red);
    }
}
