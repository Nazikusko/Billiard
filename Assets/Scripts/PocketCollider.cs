using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PocketCollider : MonoBehaviour
{
    public AudioClip sound;
    private void OnTriggerEnter(Collider other)
    {
        if (GameHelper.Use.gameStatus == GameStatus.CueAiming)
        {
            other.gameObject.SetActive(false);
        }
        else
        {
            if (other.gameObject.name != "Ball_00")
            {
                AudioSource.PlayClipAtPoint(sound, transform.position, 1.8f);
                Destroy(other.gameObject);
            }
            else
                GameHelper.Use.WhiteBallInPocket();
        }
    }
}
