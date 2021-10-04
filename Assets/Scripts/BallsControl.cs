using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallsControl : MonoBehaviour
{
    private AudioClip sound;

    private void Start()
    {
        sound = Resources.Load<AudioClip>("Sounds/BallHit");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameHelper.Use.gameStatus == GameStatus.CueAiming && collision.gameObject.name == "Ball_00")
        {
            TrajectoryRenderer.Use.SwitchOnSecondTrajectory(gameObject.GetComponent<Rigidbody>(), collision.gameObject);
        }
        if (GameHelper.Use.gameStatus == GameStatus.BallsRool && collision.gameObject.name.Contains("Ball"))
        {
            AudioSource.PlayClipAtPoint(sound, transform.position, collision.relativeVelocity.magnitude * 0.05f);
        }
    }
}
