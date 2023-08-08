using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

class BallsData
{
    public Vector3 position;
    public Quaternion rotation;
}
public enum GameStatus
{
    Idle,
    CueAiming,
    CueHitting,
    CueHit,
    BallsRoll,
    EndGame
}

public class GameHelper : Singleton<GameHelper>
{
    public const float BALL_DIAMETR = 0.572f;
    const float MAX_CUE_DISTANCE = 2.5f;
    const float MIN_CUE_DISTANCE = 0.8f;
    const float MIN_FORCE_HIT_CUE = 2f;
    const float FORCE_CUE_MULTIPLAYER = 3f;
    const float SLEEP_THRESHOLD = 0.75f;
    const float DRAW_TRAJECTORY_PERIOD = 0.8f;

    [SerializeField] private GameObject _cue;
    [SerializeField] private TrajectoryRenderer _trajectoryRendererScript;
    [SerializeField] private UIMessageBoxScript _messageBox;

    private bool _isAllBallsSleep;
    public bool IsAllBallsSleep
    {
        get => _isAllBallsSleep;

        private set
        {
            if (value == true && _isAllBallsSleep == false) GameStatus = GameStatus.Idle;
            if (value == false && _isAllBallsSleep == true) GameStatus = GameStatus.BallsRoll;
            _isAllBallsSleep = value;
        }
    }

    public GameStatus GameStatus { get; private set; }

    [HideInInspector] public Rigidbody[] BallsRB { get; private set; } = new Rigidbody[16];

    private GameObject[] _balls = new GameObject[16];
    private BallsData[] _ballsCacheData = new BallsData[16];
    private Vector3 _lastWorldPoint;
    private float _setDistanceCue;
    private float _currentDistanceCue = 0;

    private void Start()
    {
        for (int i = 0; i < 16; i++)
        {
            string index;
            if (i > 9)
                index = i.ToString();
            else
                index = "0" + i;

            _balls[i] = GameObject.Find("Ball_" + index);
            BallsRB[i] = _balls[i].GetComponent<Rigidbody>();
            BallsRB[i].sleepThreshold = SLEEP_THRESHOLD;
            _ballsCacheData[i] = new BallsData();
        }
        _cue.SetActive(false);
        _messageBox.HideMessageBox();
        StartCoroutine(CheckGameStatus());
        StartCoroutine(CalculateTrajectory());
    }
    private Vector3 CalculateDirectionVectorToBall(Vector3 point)
    {
        return _balls[0].transform.position - point;
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
            if (_balls[i] != null)
                return false;
        }
        return true;
    }

    public void ReloadScene()
    {
        StopAllCoroutines();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    Vector3 CalculateForce(Vector3 worldPoint)
    {
        Vector3 forceVector = Vector3.ProjectOnPlane(CalculateDirectionVectorToBall(worldPoint), Vector3.up);
        return forceVector * (((_currentDistanceCue - _setDistanceCue) * FORCE_CUE_MULTIPLAYER) + MIN_FORCE_HIT_CUE);
    }

    public void SetCue(Vector3 worldPoint)
    {
        if (!_isAllBallsSleep || GameStatus == GameStatus.EndGame || Vector3.Distance(worldPoint, _balls[0].transform.position) < MIN_CUE_DISTANCE)
            return;

        GameStatus = GameStatus.CueAiming;

        _currentDistanceCue = _setDistanceCue = Vector3.Distance(_balls[0].transform.position, worldPoint);

        _cue.SetActive(true);
        _cue.transform.position = _balls[0].transform.position;
        _cue.transform.rotation = Quaternion.LookRotation(CalculateDirectionVectorToBall(worldPoint), Vector3.up);
        _cue.transform.Translate(Vector3.back * MIN_CUE_DISTANCE);

    }

    public void UpdateAimingCue(Vector3 worldPoint)
    {
        if (GameStatus != GameStatus.CueAiming) return;
        _lastWorldPoint = worldPoint;
        float dist = Vector3.Distance(_balls[0].transform.position, worldPoint);


        if (dist > _setDistanceCue && dist - _setDistanceCue < MAX_CUE_DISTANCE)
            _currentDistanceCue = dist;

        _cue.transform.position = _balls[0].transform.position;

        if (dist > MIN_CUE_DISTANCE)
            _cue.transform.rotation = Quaternion.LookRotation(CalculateDirectionVectorToBall(worldPoint), Vector3.up);

        _cue.transform.Translate(Vector3.back * MIN_CUE_DISTANCE);
        _cue.transform.Translate(new Vector3(0, 0, _setDistanceCue - _currentDistanceCue), Space.Self);
    }

    //Start moving cue
    public void StartHit(Vector3 _)
    {
        GameStatus = GameStatus.CueHitting;
    }

    public void HitCue()
    {
        if (GameStatus != GameStatus.CueHitting) return;

        _cue.SetActive(false);
        BallsRB[0].AddForce(CalculateForce(_lastWorldPoint), ForceMode.Impulse);
        GameStatus = GameStatus.BallsRoll;
        _trajectoryRendererScript.HideTrajectory();
    }

    public void WhiteBallInPocket()
    {
        StopAllCoroutines();
        GameStatus = GameStatus.EndGame;
        _messageBox.ShowMessage("YOU LOSE", ReloadScene);
    }

    public void SaveBallsPosition()
    {
        for (int i = 0; i < 16; i++)
        {
            if (_balls[i] == null) continue;
            _ballsCacheData[i].position = _balls[i].transform.position;
            _ballsCacheData[i].rotation = _balls[i].transform.rotation;
        }
    }

    public void RestoreBallsPosition()
    {
        for (int i = 0; i < 16; i++)
        {
            if (_balls[i] == null) continue;
            if (_balls[i].activeSelf == false) _balls[i].SetActive(true);
            BallsRB[i].Sleep();
            _balls[i].transform.position = _ballsCacheData[i].position;
            _balls[i].transform.rotation = _ballsCacheData[i].rotation;
        }

    }

    public void ReturnBallOut(GameObject ball)
    {
        ball.GetComponent<Rigidbody>().Sleep();
        ball.transform.localPosition = new Vector3(12.8f, 0, Random.Range(-4.8f, 4.8f)); //return out ball on the table
    }

    IEnumerator CheckGameStatus() //check if all balls have stopped
    {
        do
        {
            yield return new WaitForSecondsRealtime(0.25f);
            IsAllBallsSleep = CheckBallsVelocity();

            if (CheckForEndGame())
            {
                GameStatus = GameStatus.EndGame;
                _messageBox.ShowMessage("YOU WIN", ReloadScene);
                StopAllCoroutines();
            }
        } while (true);
    }

    IEnumerator CalculateTrajectory() //draw trajectory on 1 per time
    {
        do
        {
            yield return new WaitForSecondsRealtime(DRAW_TRAJECTORY_PERIOD);

            if (GameStatus == GameStatus.CueAiming)
                _trajectoryRendererScript.ShowTrajectory(CalculateForce(_lastWorldPoint));

        } while (true);
    }

    private void Update()
    {

        if (GameStatus == GameStatus.CueHitting) //Animation of cue hit
        {
            _cue.transform.Translate(new Vector3(0, 0, Time.deltaTime * 20f), Space.Self);
            if (Vector3.Distance(_cue.transform.position, _balls[0].transform.position) < BALL_DIAMETR / 2f)
                HitCue();
        }
    }
}
