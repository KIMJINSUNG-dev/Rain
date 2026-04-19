using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaySceneController : MonoBehaviour
{
    // 상수 데이터
    // 판정 범위(ms 단위)
    private const double PerfectRange = 0.040; // Perfect
    private const double GreatRange   = 0.080; // Great
    private const double GoodRange    = 0.120; // Good

    // 판정선 위치(y축 포지션)
    private const float JudgementLineY = -340.0f;

    // 레인 속도(기본값: ×1.0 기준, 픽셀/초)
    private const float DefaultLaneSpeed = 400.0f;

    // 레인 위치(x축 포지션)
    private static readonly float[] LaneX = { -150.0f, -50.0f, 50.0f, 150.0f };

    // 카운트다운 시간
    private const double Countdown = 2.0;


    // 플레이 중에 변경되는 데이터
    // 음악 재생 시작 시간과 현재 재생 시간(dspTime)
    private double _musicStartTime;
    private double CurrentGameTime => AudioSettings.dspTime - _musicStartTime - _pausedDuration; // _pausedDuration으로 일시정지 시간 보정

    // 일시정지 시작 시간과 일시정지 지속 시간
    private double _pauseStartTime;
    private double _pausedDuration = 0;

    // 보면 정보 및 노트 정보
    private ChartData _chartData;
    private int _nextNoteIdx = 0;
    private List<NoteObject> _activeNotes = new();

    // 점수 및 콤보 정보
    private int _currentScore = 0;
    private int _currentCombo = 0;
    private int _maxCombo = 0;
    private int _perfectCount = 0;
    private int _greatCount = 0;
    private int _goodCount = 0;
    private int _missCount = 0;

    // 레인 속도(기본값: ×1.0)
    private float _speed;
    // 레인 속도 변경 폭
    private float _speedStep = 0.5f;
    // 레인 속도 최소 및 최대 값
    private float _minSpeed = 0.5f;
    private float _maxSpeed = 5.0f;

    // 실제 스크롤 속도
    private float ScrollSpeed => DefaultLaneSpeed * _speed;

    // 게임 상태 정보
    private bool _isPaused = false;
    private bool _isEnded = false;
    private bool _isHUDInit = true;

    // 판정 표시 시간
    private float _judgeShowTimer = 0.0f;
    private float _judgeShowDuration = 0.5f;

    // 인스펙터 연결 필드
    [Header("노트")]
    [SerializeField] private RectTransform noteContainer; // 노트들이 생성될 부모 RectTransform
    [SerializeField] private GameObject notePrefab; // 노트 프리팹

    [Header("오디오")]
    [SerializeField] private AudioSource musicSource; // 음악 재생용 AudioSource

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI judgeText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("일시정지 메뉴")]
    [SerializeField] private GameObject pauseMenu;      // 일시정지 메뉴 UI
    [SerializeField] private Button     continueButton; 
    [SerializeField] private Button     restartButton;
    [SerializeField] private Button     toSelectButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.Instance == null) return;
        var gm = GameManager.Instance;

        // 음원 로드
        string songTitle = gm.SelectedSong.title;
        musicSource.clip = Resources.Load<AudioClip>($"Audio/{songTitle}");

        // 플레이 속도 설정
        _speed = gm.SelectedSpeed;

        // 보면 데이터 로드
        _chartData = ChartLoader.Load(gm.GetChartFileName());

        // PauseMenu 버튼 바인딩
        if (continueButton != null) continueButton.onClick.AddListener(ResumeGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (toSelectButton != null) toSelectButton.onClick.AddListener(GoToSelectFromPause);

        // 카운트다운 후에 음악 재생 시작
        _musicStartTime = AudioSettings.dspTime + Countdown;
        if(musicSource != null && musicSource.clip != null) musicSource.PlayScheduled(_musicStartTime);

        // 배속 조정 등 노트 처리 이외의 다른 입력을 활성화하기 위해 카운트다운 시작 전 미리 활성화
        InputReader.Instance?.EnableGamePlay();

        if(countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "2";
        }

        RefreshHUD();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPaused || _isEnded) return;
        

        // 판정 및 콤보 표시 시간 검사
        if (_judgeShowTimer > 0.0f)
        {
            _judgeShowTimer -= Time.deltaTime;
            if (_judgeShowTimer <= 0.0f && judgeText != null) judgeText.enabled = false;
        }

        SpawnUpcomingNotes();
        UpdateActiveNotes();
        RefreshHUD();
        if (!_isEnded && _nextNoteIdx == _chartData.Notes.Count && !musicSource.isPlaying)
        {
            _isEnded = true;
            EndGame();
        }
    }

    // 다음 노트가 화면 안에 들어올 시점이 되면 노트를 생성하는 메서드
    private void SpawnUpcomingNotes()
    {
        while (_nextNoteIdx < _chartData.Notes.Count)
        {
            NoteData nextNote = _chartData.Notes[_nextNoteIdx];
            float lookAhead = (Mathf.Abs(JudgementLineY) + 600.0f) / ScrollSpeed;
            if ((nextNote.time - CurrentGameTime) > lookAhead) break;
            SpawnNote(nextNote);
            _nextNoteIdx++;
        }
    }

    private void SpawnNote(NoteData data)
    {
        GameObject prefab = notePrefab;
        if (prefab == null) return;
        GameObject go = Instantiate(prefab, noteContainer);
        NoteObject note = go.GetComponent<NoteObject>();

        if(note == null) note = go.AddComponent<NoteObject>();
        note.Initialize(data, LaneX[data.lane - 1], JudgementLineY); // 레인 번호는 1~4이므로 인덱스 보정
        _activeNotes.Add(note);
    }

    private void UpdateActiveNotes()
    {
        for (int i = _activeNotes.Count - 1; i >= 0; i--)
        {
            NoteObject note = _activeNotes[i];
            if (note == null) { _activeNotes.RemoveAt(i); continue; }

            note.UpdatePosition(CurrentGameTime, ScrollSpeed);

            // 노트가 판정선을 지나치고 Good 판정 범위를 벗어났다면 자동 Miss 처리
            if (!note.IsJudged && CurrentGameTime - note.Data.time > GoodRange) ApplyJudge(JudgeType.Miss, note);

            // 판정 완료된 노트 제거
            if (note.IsJudged) { _activeNotes.RemoveAt(i); Destroy(note.gameObject); }
        }
    }


    private void OnEnable()
    {
        if(InputReader.Instance == null) return;
        InputReader.Instance.OnLanePressed  += HandleLanePressed;
        InputReader.Instance.OnLaneReleased += HandleLaneReleased;
        InputReader.Instance.OnPausePressed += HandlePausePressed;
        InputReader.Instance.OnSpeedUp      += HandleSpeedUp;
        InputReader.Instance.OnSpeedDown    += HandleSpeedDown;
    }

    private void OnDisable()
    {
        if(InputReader.Instance == null) return;
        InputReader.Instance.OnLanePressed  -= HandleLanePressed;
        InputReader.Instance.OnLaneReleased -= HandleLaneReleased;
        InputReader.Instance.OnPausePressed -= HandlePausePressed;
        InputReader.Instance.OnSpeedUp      -= HandleSpeedUp;
        InputReader.Instance.OnSpeedDown    -= HandleSpeedDown;
    }

    private void HandleLanePressed(int lane, double pressTime)
    {
        if(_isPaused || _isEnded || _activeNotes.Count == 0) return;
        double pressGameTime = pressTime - _musicStartTime - _pausedDuration;
        if (pressGameTime < 0) return; // 카운트다운 중에는 입력 무시

        NoteObject closest = null;
        double     minDiff = double.MaxValue;
        foreach (NoteObject note in _activeNotes)
        {
            if (note.IsJudged || note.Data.lane - 1 != lane) continue; // 레인 번호는 1~4이므로 인덱스 보정

            double diff = System.Math.Abs(note.Data.time - pressGameTime);
            if(diff < minDiff)
            {
                minDiff = diff;
                closest = note;
            }
        }
        if (closest == null || minDiff > GoodRange) return;


        JudgeType judge = CalcJudge(minDiff);
        ApplyJudge(judge, closest);
    }

    private JudgeType CalcJudge(double diff)
    {
        if (diff <= PerfectRange) return JudgeType.Perfect;
        else if (diff <= GreatRange) return JudgeType.Great;
        else return JudgeType.Good;
    }

    private void ApplyJudge(JudgeType judge, NoteObject note)
    {
        note.IsJudged = true;
        switch (judge)
        {
            case JudgeType.Perfect:
                _currentScore += 2;
                _currentCombo++;
                _perfectCount++;
                ShowJudge("PERFECT", Color.yellow);
                note.PlayHitEffect(Color.yellow);
                break;

            case JudgeType.Great:
                _currentScore += 1;
                _currentCombo++;
                _greatCount++;
                ShowJudge("GREAT", Color.green);
                note.PlayHitEffect(Color.green);
                break;

            case JudgeType.Good:
                _currentCombo++;
                _goodCount++;
                ShowJudge("GOOD", Color.blue);
                note.PlayHitEffect(Color.blue);
                break;

            case JudgeType.Miss:
                _missCount++;
                _currentCombo = 0;
                comboText.text = "";
                ShowJudge("MISS", Color.gray);
                note.PlayMissEffect();
                break;
        }

        if (_currentCombo > _maxCombo) _maxCombo = _currentCombo;
        RefreshHUD();
    }


    private void HandleLaneReleased(int lane)
    {
        // 롱노트 미구현 상태이므로 비워둠
    }

    private void HandlePausePressed()
    {
        if (!_isPaused) PauseGame(); else ResumeGame();
    }

    private void HandleSpeedUp()
    {
        _speed = Mathf.Clamp(_speed + _speedStep, _minSpeed, _maxSpeed);
        speedText.text = $"Lane Speed: ×{_speed}";
    }

    private void HandleSpeedDown()
    {
        _speed = Mathf.Clamp(_speed - _speedStep, _minSpeed, _maxSpeed);
        speedText.text = $"Lane Speed: ×{_speed}";
    }

    private void PauseGame()
    {
        _isPaused = true;
        _pauseStartTime = AudioSettings.dspTime;
        Time.timeScale = 0f; // 게임 시간 정지
        musicSource?.Pause(); // 음악 일시정지
        pauseMenu.SetActive(true);
    }

    private void ResumeGame()
    {
        _isPaused = false;
        _pausedDuration += (AudioSettings.dspTime - _pauseStartTime); // 일시정지된 시간만큼 게임 시간 보정
        Time.timeScale = 1f; // 게임 시간 재개
        musicSource?.UnPause(); // 음악 재개
        pauseMenu.SetActive(false);
    }

    private void RestartGame() => GameManager.Instance.GoToPlay();

    private void GoToSelectFromPause() => GameManager.Instance.GoToSelect();

    private void ShowJudge(string text, Color color)
    {
        if (judgeText == null) return;
        judgeText.text = text;
        judgeText.color = color;
        judgeText.enabled = true;
        _judgeShowTimer = _judgeShowDuration;
    }

    private void RefreshHUD()
    {
        if (_isHUDInit)
        {
            scoreText.text = "0";
            comboText.text = "";
            judgeText.text = "";
            speedText.text = $"Lane Speed: ×{_speed}";
            _isHUDInit = false;
        }
        else
        {
            scoreText.text = _currentScore.ToString();
            if(_currentCombo > 1) comboText.text = _currentCombo.ToString();
            speedText.text = $"Lane Speed: ×{_speed}";
            if (CurrentGameTime < 1)
            {
                int count = Mathf.CeilToInt((float)-CurrentGameTime);
                countdownText.text = Mathf.Clamp(count, 0, (int)Countdown).ToString();
                if (Mathf.Clamp(count, 0, (int)Countdown) == 0) countdownText.text = "GO";
            }
            else countdownText.text = "";
        }
    }

    private void EndGame()
    {
        var gm = GameManager.Instance;
        if(gm != null)
        {
            gm.FinalScore = _currentScore;
            gm.PerfectCount = _perfectCount;
            gm.GreatCount = _greatCount;
            gm.GoodCount = _goodCount;
            gm.MissCount = _missCount;
            gm.MaxCombo = _maxCombo;
            int perfectScore = _chartData.Notes.Count * 2;
            float ratio = (float)_currentScore / perfectScore * 100.0f;
            switch(ratio)
            {
                case float n when n >= 95: gm.FinalRank = GameManager.Rank.S; break;
                case float n when n >= 90: gm.FinalRank = GameManager.Rank.A; break;
                case float n when n >= 80: gm.FinalRank = GameManager.Rank.B; break;
                case float n when n >= 70: gm.FinalRank = GameManager.Rank.C; break;
                default:                   gm.FinalRank = GameManager.Rank.D; break;
            }
        }
        Invoke(nameof(GoToResult), 3.0f);
    }

    private void GoToResult() => GameManager.Instance?.GoToResult();
}

public enum JudgeType { Perfect, Great, Good, Miss }
