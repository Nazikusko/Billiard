using System.Collections.Generic;
using UnityEngine;

public class TrajectoryRenderer : Singleton<TrajectoryRenderer>
{
    [SerializeField] private LineRenderer _firstLineRendererComponent;
    [SerializeField] private LineRenderer _secondLineRendererComponent;
    [SerializeField] private GameObject _transparentBall;

    private Rigidbody _ballForSecondTrajectory;

    private void Start()
    {
        HideTrajectory();
    }

    public void ShowTrajectory(Vector3 force)
    {
        Vector3[] points = new Vector3[50];
        List<Vector3> secondPointsList = new List<Vector3>();

        _firstLineRendererComponent.enabled = true;
        _ballForSecondTrajectory = null;
        _transparentBall.SetActive(false);
        _firstLineRendererComponent.positionCount = points.Length;
        GameHelper.Instance.SaveBallsPosition();

        Physics.autoSimulation = false;
        GameHelper.Instance.BallsRB[0].AddForce(force, ForceMode.Impulse);

        points[0] = GameHelper.Instance.BallsRB[0].position;

        for (int i = 1; i < points.Length; i++)
        {
            Physics.Simulate(0.2f);
            if (GameHelper.Instance.BallsRB[0].gameObject.activeSelf) points[i] = GameHelper.Instance.BallsRB[0].position;
            if (_ballForSecondTrajectory != null && _ballForSecondTrajectory.gameObject.activeSelf)
                secondPointsList.Add(_ballForSecondTrajectory.position);
        }
        Physics.autoSimulation = true;
        _ballForSecondTrajectory = null;
        GameHelper.Instance.RestoreBallsPosition();

        _firstLineRendererComponent.SetPositions(points);
        _secondLineRendererComponent.positionCount = secondPointsList.Count;
        _secondLineRendererComponent.SetPositions(secondPointsList.ToArray());
    }

    public void SwitchOnSecondTrajectory(Rigidbody rigidBody, GameObject whiteBall)
    {
        if (_ballForSecondTrajectory != null) return;
        _transparentBall.SetActive(true);
        _transparentBall.transform.position = whiteBall.transform.position;
        _ballForSecondTrajectory = rigidBody;
        _secondLineRendererComponent.enabled = true;
    }

    public void HideTrajectory()
    {
        _firstLineRendererComponent.enabled = false;
        _secondLineRendererComponent.enabled = false;
        _transparentBall.SetActive(false);
    }

}
