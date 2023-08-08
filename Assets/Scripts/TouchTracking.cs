using UnityEngine;
using UnityEngine.EventSystems;

public class TouchTracking : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private const int LAYER_MASK_TOUCH = 1 << 3;

    [SerializeField] private Camera _mainCamera;

    private bool TraceTouchToWorldPoint(Vector2 touchPoint, out Vector3 worldPoint)
    {
        var ray = _mainCamera.ScreenPointToRay(touchPoint);
        if (Physics.Raycast(ray, out var hitPoint, 200.0f, LAYER_MASK_TOUCH))
        {
            worldPoint = hitPoint.point;
            return true;
        }

        worldPoint = Vector3.zero;
        return false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (TraceTouchToWorldPoint(eventData.position, out var worldPoint))
        {
            GameHelper.Instance.SetCue(worldPoint);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (TraceTouchToWorldPoint(eventData.position, out var worldPoint))
        {
            GameHelper.Instance.UpdateAimingCue(worldPoint);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (GameHelper.Instance.GameStatus == GameStatus.CueAiming && TraceTouchToWorldPoint(eventData.position, out var worldPoint))
        {
            GameHelper.Instance.StartHit(worldPoint);
        }
    }
}
