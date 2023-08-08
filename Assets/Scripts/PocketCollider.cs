using UnityEngine;

public class PocketCollider : MonoBehaviour
{
    [SerializeField] private AudioClip _pocketSound;

    private void OnTriggerEnter(Collider other)
    {
        if (GameHelper.Instance.GameStatus == GameStatus.CueAiming)
        {
            other.gameObject.SetActive(false);
        }
        else
        {
            if (other.gameObject.name != "Ball_00")
            {
                AudioSource.PlayClipAtPoint(_pocketSound, transform.position, 1.8f);
                Destroy(other.gameObject);
            }
            else
                GameHelper.Instance.WhiteBallInPocket();
        }
    }
}
