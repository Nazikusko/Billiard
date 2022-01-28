using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchTracking : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    const int LAYERMASKTOUCH = 1 << 3;
    public Camera Cam;

    private bool TraceTouchToWorldPoint(Vector2 touchPoint, out Vector3 point)
    {
        RaycastHit hitPoint;
        var ray = Cam.ScreenPointToRay(touchPoint);
        if (Physics.Raycast(ray, out hitPoint, 200.0f, LAYERMASKTOUCH))
        {
            point = hitPoint.point;
            return true;
        }
        else
        {
            point = Vector3.zero;
            return false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Vector3 worldPoint;
        if (TraceTouchToWorldPoint(eventData.position, out worldPoint))
        {
            GameHelper.Use.SetCue(worldPoint);
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPoint;
        if (TraceTouchToWorldPoint(eventData.position, out worldPoint))
        {
            GameHelper.Use.UpdateAimingCue(worldPoint);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 worldPoint;
        if (GameHelper.Use.gameStatus == GameStatus.CueAiming && TraceTouchToWorldPoint(eventData.position, out worldPoint))
        {
            GameHelper.Use.StartHit(worldPoint);
        }
    }
}
