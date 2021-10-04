using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameHelper.Use.ReturnBallOut(other.gameObject);
    }
}
