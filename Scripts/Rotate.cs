using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private Transform _RotateTransform;
    [SerializeField] private Vector3 _RotateAxis;
    [SerializeField] private float _RotateSpeed;

    // Update is called once per frame
    void Update()
    {
        _RotateTransform.Rotate(_RotateAxis, _RotateSpeed);
    }
}
