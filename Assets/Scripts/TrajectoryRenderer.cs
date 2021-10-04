using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryRenderer : SingleTon<TrajectoryRenderer>
{
    public LineRenderer SecondLineRendereComponent;
    public LineRenderer FirstLineRendereComponent;
    public GameObject TransparentBall;

    private Rigidbody ballRBforSecondTrajectory;

    private void Start()
    {
        HideTrajectory();
    }

    public void ShowTrajectory(Vector3 force)
    {
        Vector3[] points = new Vector3[50];
        List<Vector3> listSecondpoints = new List<Vector3>();

        FirstLineRendereComponent.enabled = true;
        ballRBforSecondTrajectory = null;
        TransparentBall.SetActive(false);
        FirstLineRendereComponent.positionCount = points.Length;
        GameHelper.Use.SaveBallsPosition();

        Physics.autoSimulation = false;
        GameHelper.Use.BallsRB[0].AddForce(force, ForceMode.Impulse);

        points[0] = GameHelper.Use.BallsRB[0].position;

        for (int i = 1; i < points.Length; i++)
        {
            Physics.Simulate(0.2f);
            if (GameHelper.Use.BallsRB[0].gameObject.activeSelf == true) points[i] = GameHelper.Use.BallsRB[0].position;
            if (ballRBforSecondTrajectory != null && ballRBforSecondTrajectory.gameObject.activeSelf == true) listSecondpoints.Add(ballRBforSecondTrajectory.position);
        }
        Physics.autoSimulation = true;
        ballRBforSecondTrajectory = null;
        GameHelper.Use.RestoreBallsPosition();

        FirstLineRendereComponent.SetPositions(points);
        SecondLineRendereComponent.positionCount = listSecondpoints.Count;
        SecondLineRendereComponent.SetPositions(listSecondpoints.ToArray());

    }

    public void SwitchOnSecondTrajectory(Rigidbody rigidbody, GameObject whiteBall)
    {
        if (ballRBforSecondTrajectory != null) return;
        TransparentBall.SetActive(true);
        TransparentBall.transform.position = whiteBall.transform.position;
        ballRBforSecondTrajectory = rigidbody;
        SecondLineRendereComponent.enabled = true;
    }

    public void HideTrajectory()
    {
        FirstLineRendereComponent.enabled = false;
        SecondLineRendereComponent.enabled = false;
        TransparentBall.SetActive(false);
    }

}
