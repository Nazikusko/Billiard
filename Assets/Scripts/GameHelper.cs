using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

class BallsData
{
    public Vector3 Position;
    public Quaternion Rotation;
}
public enum GameStatus
{
    Idle,
    CueAiming,
    CueHiting,
    CueHit,
    BallsRool,
    EndGame
}

public class GameHelper : SingleTon<GameHelper>
{
    public const float BALLDIAMETR = 0.572f;
    const float MAXCUEDISTANCE = 2.5f;
    const float MINCUESETDISTANCE = 0.8f;
    const float MINFORCEHITCUE = 2f;
    const float FORSECUEMULTIPLAYER = 3f;
    const float SLEEPTHRESHOLD = 0.75f;
    const float DRAWTRAJECTORYPERIOD = 0.8f;

    public GameObject Cue;
    public TrajectoryRenderer TrajectoryRendererScript;
    public UIMessageBoxScript messageBox;

    private bool isAllBallsSleep;
    public bool IsAllBallsSleep
    {
        get { return isAllBallsSleep; }
        private set
        {
            if (value == true && IsAllBallsSleep == false) gameStatus = GameStatus.Idle;
            if (value == false && IsAllBallsSleep == true) gameStatus = GameStatus.BallsRool;
            isAllBallsSleep = value;
        }
    }
    [HideInInspector]
    public Rigidbody[] BallsRB { get; private set; } = new Rigidbody[16];

    private GameObject[] Balls = new GameObject[16];
    private BallsData[] ballsCañheData = new BallsData[16];
    private Vector3 lastWorldpoint;
    private float setDistanceCue;
    private float currenDistanceCue = 0;


    public GameStatus gameStatus { get; private set; }

    private void Start()
    {
        string strI;
        for (int i = 0; i < 16; i++)
        {
            if (i > 9)
                strI = i.ToString();
            else
                strI = "0" + i.ToString();

            Balls[i] = GameObject.Find("Ball_" + strI);
            BallsRB[i] = Balls[i].GetComponent<Rigidbody>();
            BallsRB[i].sleepThreshold = SLEEPTHRESHOLD;
            ballsCañheData[i] = new BallsData();
        }
        Cue.SetActive(false);
        messageBox.HideMessageBox();
        StartCoroutine(CheckBallsVelocityStatus());
        StartCoroutine(CalculateTrajectory());
    }
    private Vector3 CalculateDirectionVectorToBall(Vector3 point)
    {
        return Balls[0].transform.position - point;
    }

    private bool CheckBallsVelocity() //return true if all balls is sleep
    {
        for (int i = 0; i < 16; i++)
        {
            if (BallsRB[i] != null && BallsRB[i].velocity.magnitude > 0.02f) return false;
        }
        return true;
    }
    private bool CheckForEndGame()
    {
        for (int i = 1; i < 16; i++)
        {
            if (Balls[i] != null)
                return false;
        }
        return true;
    }

    public void ReloadScene()
    {
        StopAllCoroutines();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    Vector3 CalculeteForce(Vector3 worldPoint)
    {
        Vector3 forcevector = Vector3.ProjectOnPlane(CalculateDirectionVectorToBall(worldPoint), Vector3.up);
        return forcevector * (((currenDistanceCue - setDistanceCue) * FORSECUEMULTIPLAYER) + MINFORCEHITCUE);
    }

    public void SetCue(Vector3 worldpoint)
    {
        if (!isAllBallsSleep || gameStatus == GameStatus.EndGame) return;
        gameStatus = GameStatus.CueAiming;

        if (Vector3.Distance(worldpoint, Balls[0].transform.position) < MINCUESETDISTANCE)
            return;

        currenDistanceCue = setDistanceCue = Vector3.Distance(Balls[0].transform.position, worldpoint);

        Cue.SetActive(true);
        Cue.transform.position = Balls[0].transform.position;
        Cue.transform.rotation = Quaternion.LookRotation(CalculateDirectionVectorToBall(worldpoint), Vector3.up);
        Cue.transform.Translate(Vector3.back * MINCUESETDISTANCE);

    }

    public void UpdateAimingCue(Vector3 worldpoint)
    {
        if (gameStatus != GameStatus.CueAiming) return;
        lastWorldpoint = worldpoint;
        float dist = Vector3.Distance(Balls[0].transform.position, worldpoint);

        //Ïðîâåðêà íà ïðåâûøåíèå ìàêñèìàëüíîãî ðàññòîÿíèÿ
        if (dist > setDistanceCue && dist - setDistanceCue < MAXCUEDISTANCE)
            currenDistanceCue = dist;

        Cue.transform.position = Balls[0].transform.position;

        if (dist > MINCUESETDISTANCE)
            Cue.transform.rotation = Quaternion.LookRotation(CalculateDirectionVectorToBall(worldpoint), Vector3.up); // ïîâîðîò êèÿ ïî âåêòîðó

        Cue.transform.Translate(Vector3.back * MINCUESETDISTANCE);
        Cue.transform.Translate(new Vector3(0, 0, setDistanceCue - currenDistanceCue), Space.Self);
    }

    //Start moving cue
    public void StartHit(Vector3 worldPoint)
    {
        gameStatus = GameStatus.CueHiting;
    }

    public void HitCue()
    {
        if (gameStatus != GameStatus.CueHiting) return;
        Cue.SetActive(false);
        BallsRB[0].AddForce(CalculeteForce(lastWorldpoint), ForceMode.Impulse);
        gameStatus = GameStatus.BallsRool;
        TrajectoryRendererScript.HideTrajectory();
    }

    public void WhiteBallInPocket()
    {
        StopAllCoroutines();
        gameStatus = GameStatus.EndGame;
        messageBox.ShowMessage("YOU LOSE");
    }

    public void SaveBallsPosition()
    {
        for (int i = 0; i < 16; i++)
        {
            if (Balls[i] == null) continue;
            ballsCañheData[i].Position = Balls[i].transform.position;
            ballsCañheData[i].Rotation = Balls[i].transform.rotation;
        }
    }

    public void RestoreBallsPosition()
    {
        for (int i = 0; i < 16; i++)
        {
            if (Balls[i] == null) continue;
            if (Balls[i].activeSelf == false) Balls[i].SetActive(true);
            BallsRB[i].Sleep();
            Balls[i].transform.position = ballsCañheData[i].Position;
            Balls[i].transform.rotation = ballsCañheData[i].Rotation;
        }

    }

    public void ReturnBallOut(GameObject ball)
    {
        ball.GetComponent<Rigidbody>().Sleep();
        ball.transform.localPosition = new Vector3(12.8f, 0, Random.Range(-4.8f, 4.8f)); //return out ball on the table
    }

    IEnumerator CheckBallsVelocityStatus() //check if all balls have stopped
    {
        do
        {
            yield return new WaitForSecondsRealtime(0.25f);
            IsAllBallsSleep = CheckBallsVelocity();

            if (CheckForEndGame())
            {
                gameStatus = GameStatus.EndGame;
                messageBox.ShowMessage("YOU WIN");
                StopAllCoroutines();
            }
        } while (true);
    }

    IEnumerator CalculateTrajectory() //drawTrajectory on 1 per time
    {
        do
        {
            yield return new WaitForSecondsRealtime(DRAWTRAJECTORYPERIOD);
            if (gameStatus == GameStatus.CueAiming)
            {
                TrajectoryRendererScript.ShowTrajectory(CalculeteForce(lastWorldpoint));
            }
        } while (true);
    }

    private void Update()
    {

        if (gameStatus == GameStatus.CueHiting) //Animation of cue hit
        {
            Cue.transform.Translate(new Vector3(0, 0, Time.deltaTime * 20f), Space.Self);
            if (Vector3.Distance(Cue.transform.position, Balls[0].transform.position) < BALLDIAMETR / 2f)
                HitCue();
        }
    }


}
