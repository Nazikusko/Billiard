using UnityEngine;

public class BallsControl : MonoBehaviour
{
    private AudioClip _sound;
    private Rigidbody _rigidBody;

    private void Awake()
    {
        _sound = Resources.Load<AudioClip>("Sounds/BallHit");
        _rigidBody = gameObject.GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameHelper.Instance.GameStatus == GameStatus.CueAiming && collision.gameObject.name == "Ball_00")
        {
            TrajectoryRenderer.Instance.SwitchOnSecondTrajectory(_rigidBody, collision.gameObject);
        }

        if (GameHelper.Instance.GameStatus == GameStatus.BallsRoll && collision.gameObject.name.Contains("Ball"))
        {
            AudioSource.PlayClipAtPoint(_sound, transform.position, collision.relativeVelocity.magnitude * 0.05f);
        }
    }
}
