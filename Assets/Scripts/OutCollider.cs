using UnityEngine;

public class OutCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameHelper.Instance.ReturnBallOut(other.gameObject);
    }
}
