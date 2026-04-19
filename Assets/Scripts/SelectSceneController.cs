using TMPro;
using UnityEngine;

public class SelectSceneController : MonoBehaviour
{
    // SelectScene이 기억하고 있어야 하는 데이터를 담는 변수들

    // 현재 선택된 곡의 인덱스, 초기값은 0으로 설정하여 첫 번째 곡이 선택된 상태를 나타냄
    private int _songIndex = 0;
    // 현재 선택된 난이도의 인덱스, 초기값은 0으로 설정하여 Easy 난이도가 선택된 상태를 나타냄
    private int _diffIndex = 0;
    // 현재 선택된 레인 속도, 초기값은 1.0f로 설정하여 기본 속도를 나타냄
    private float _speed = 1.0f;

    // 레인 속도 변경 폭
    [SerializeField] private float speedStep = 0.5f;
    // 레인 속도 최소 및 최대 값
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 5.0f;

    // 곡 목록을 표시하는 TextMeshProUGUI 컴포넌트 배열, 인스펙터에서 연결
    [SerializeField] private TextMeshProUGUI[] _songTitleTexts;

    // 선택 중인 플레이 정보(곡명/난이도/레벨/레인속도)를 표시하는 TextMeshProUGUI 컴포넌트
    [SerializeField] private TextMeshProUGUI _selectedTitleText;
    [SerializeField] private TextMeshProUGUI _selectedDiffText;
    [SerializeField] private TextMeshProUGUI _selectedLevelText;
    [SerializeField] private TextMeshProUGUI _selectedBPMText;
    [SerializeField] private TextMeshProUGUI _speedText;

    // 반복 탐색 수행 관리를 위한 정보를 저장하는 변수

    // 현재 탐색 입력 방향, Vector2.zero는 탐색 입력이 없는 상태를 나타냄
    private Vector2 _navigateDir            = Vector2.zero;
    // 탐색 입력이 처음 발생한 후 다음 탐색 입력이 허용되기까지의 지연 시간
    private float   _navigateRepeatDelay    = 0.3f;
    // 지연 시간까지 경과한 시간을 추적하는 타이머 변수
    private float   _navigateRepeatTimer    = 0f;
    // 탐색 입력이 지속되는 동안 반복 탐색이 발생하는 간격
    private float   _navigateRepeatInterval = 0.1f;

    // delay가 지났는지 여부
    private bool _repeatStarted = false;


    // 씬 진입 후 즉각적인 입력을 막기 위해 입력 활성화 여부를 관리하기 위한 플래그
    private bool _inputEnabled = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputReader.Instance?.EnableNavigation();
        RestorePrevSelect();
        RefreshUI();

        // 씬이 시작된 후 0.3초 후에 입력을 활성화하는 메서드를 호출하여 초기 입력을 방지
        Invoke(nameof(EnableInput), 0.3f);
    }

    // Update is called once per frame
    private void Update()
    {
        if (_navigateDir == Vector2.zero) return;

        _navigateRepeatTimer += Time.deltaTime;

        // delay가 지나기 전인지 검사하여 반복을 막음
        if (!_repeatStarted)
        {
            if (_navigateRepeatTimer < _navigateRepeatDelay) return;
            _repeatStarted = true;
            _navigateRepeatTimer = 0f;
        }

        // delay가 지난 후에는 interval마다 탐색이 반복되도록 함
        if (_navigateRepeatTimer >= _navigateRepeatInterval)
        {
            _navigateRepeatTimer = 0f;
            ApplyNavigation(_navigateDir);
        }
    }
    private void EnableInput() => _inputEnabled = true;

    private void OnEnable()
    {
        if (InputReader.Instance != null)
        {
            InputReader.Instance.OnNavigate         += HandleNavigate;
            InputReader.Instance.OnNavigateCanceled += HandleNavigateCanceled;
            InputReader.Instance.OnConfirmed        += HandleConfirm;
            InputReader.Instance.OnSpeedUp          += HandleSpeedUp;
            InputReader.Instance.OnSpeedDown        += HandleSpeedDown;
        }
    }

    private void OnDisable()
    {
        if (InputReader.Instance != null)
        {
            InputReader.Instance.OnNavigate         -= HandleNavigate;
            InputReader.Instance.OnNavigateCanceled -= HandleNavigateCanceled;
            InputReader.Instance.OnConfirmed        -= HandleConfirm;
            InputReader.Instance.OnSpeedUp          -= HandleSpeedUp;
            InputReader.Instance.OnSpeedDown        -= HandleSpeedDown;
        }
    }

    private void HandleNavigate(Vector2 dir)
    {
        // 입력이 아직 활성화되지 않은 경우 무시
        if (!_inputEnabled) return;
        
        _navigateDir = dir;

        ApplyNavigation(dir);
    }

    private void HandleNavigateCanceled()
    {
        _navigateDir         = Vector2.zero;
        _navigateRepeatTimer = 0f;
        _repeatStarted       = false;
    }

    private void HandleConfirm()
    {
        // 입력이 아직 활성화되지 않은 경우 무시
        if (!_inputEnabled) return;

        // 입력이 한 번 발생하면 다시 비활성화하여 중복 입력 방지
        _inputEnabled = false;

        var gm = GameManager.Instance;
        if(gm != null)
        {
            gm.SelectedSong    = SongDatabase.Instance.SongList[_songIndex];
            gm.SelectedDiffIdx = _diffIndex;
            gm.SelectedSpeed   = _speed;

            // 플레이 씬으로 전환
            gm.GoToPlay();
        }
    }

    private void HandleSpeedUp()
    {
        // 입력이 아직 활성화되지 않은 경우 무시
        if (!_inputEnabled) return;

        _speed = Mathf.Clamp(_speed + speedStep, minSpeed, maxSpeed);
        RefreshUI();
    }

    private void ApplyNavigation(Vector2 dir)
    {
        var songList = SongDatabase.Instance?.SongList;
        if (songList == null || songList.Count == 0) return;

        // 위로 탐색
        if (dir.y > 0)
        {
            if (_songIndex == 0)
            {
                _songIndex = songList.Count - 1;
            }
            else
            {
                _songIndex--;
            }
        }

        // 아래로 탐색
        if (dir.y < 0)
        {
            if (_songIndex == songList.Count - 1)
            {
                _songIndex = 0;
            }
            else
            {
                _songIndex++;
            }
        }

        // 왼쪽으로 탐색
        if (dir.x < 0)
        {
            if (_diffIndex == 0)
            {
                _diffIndex = 2;
            }
            else
            {
                _diffIndex--;
            }
        }

        // 오른쪽으로 탐색
        if (dir.x > 0)
        {
            if (_diffIndex == 2)
            {
                _diffIndex = 0;
            }
            else
            {
                _diffIndex++;
            }
        }

        RefreshUI();
    }

    private void HandleSpeedDown()
    {
        // 입력이 아직 활성화되지 않은 경우 무시
        if (!_inputEnabled) return;
        
        _speed = Mathf.Clamp(_speed - speedStep, minSpeed, maxSpeed);
        RefreshUI();
    }

    // 이전에 선택했던 곡/난이도/레인속도 정보를 GameManager에서 가져와서
    // SelectSceneController의 변수에 복원하는 메서드
    private void RestorePrevSelect()
    {
        var gm = GameManager.Instance;
        if( gm != null )
        {
            _speed = gm.SelectedSpeed;
            _diffIndex = gm.SelectedDiffIdx;
            if(SongDatabase.Instance != null)
            {
                var songList = SongDatabase.Instance?.SongList;
                if(songList != null && songList.Count > 0)
                {
                    int idx = songList.IndexOf(gm.SelectedSong);
                    if (idx != -1) _songIndex = idx;
                }
            }
        }
    }

    private void RefreshUI()
    {
        var songList = SongDatabase.Instance?.SongList;
        if (songList == null || songList.Count == 0) return;

        _selectedTitleText.text = songList[_songIndex].title;

        for(int i = 0; i < _songTitleTexts.Length; i++)
        {
            if (i >= songList.Count)
            {
                _songTitleTexts[i].text = "";
                continue;
            }

            _songTitleTexts[i].text = songList[i].title;
            _songTitleTexts[i].color = (i == _songIndex ) ? Color.yellow : Color.white;
        }

        switch (_diffIndex)
        {
            case 0:
                _selectedDiffText.text  = "EASY";
                _selectedLevelText.text = songList[_songIndex].easyLevel.ToString();
                break;
            case 1:
                _selectedDiffText.text  = "NORMAL";
                _selectedLevelText.text = songList[_songIndex].normalLevel.ToString();
                break;
            case 2:
                _selectedDiffText.text  = "HARD";
                _selectedLevelText.text = songList[_songIndex].hardLevel.ToString();
                break;
        }

        _selectedBPMText.text = $"BPM {songList[_songIndex].bpm}";
        _speedText.text       = $"×{_speed}";
    }
}
