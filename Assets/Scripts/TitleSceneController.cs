using TMPro;
using UnityEngine;

public class TitleSceneController : MonoBehaviour
{
    // 인스펙터 연결 항목: 타이틀 텍스트, SelectScene으로 전환 안내 텍스트, 배경음악 AudioSource
    [Header("UI 연결 요소")]
    [Tooltip("타이틀 문구(게임 제목)")]
    public TextMeshProUGUI titleText;

    [Tooltip("곡 선택 씬으로 전환 안내 문구")]
    public TextMeshProUGUI pressEnterText;

    [Header("오디오 연결 요소")]
    [Tooltip("BGM AudioSource")]
    public AudioSource bgmSource;

    // 씬 진입 후 즉각적인 입력을 막기 위해 입력 활성화 여부를 관리하기 위한 플래그
    private bool _inputEnabled = false;

    // pressEnterText 깜빡임 기본변수
    private float _blinkTimer = 0f;

    // pressEnterText 깜빡임 간격(초)
    private float _blinkInterval = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // BGM이 설정되어 있으면 재생
        if (bgmSource != null && bgmSource.clip != null) bgmSource.Play();

        // 타이틀 씬에서는 탐색 입력만 활성화하여 곡 선택으로 이동할 수 있도록 설정
        InputReader.Instance?.EnableNavigation();

        // 씬이 시작된 후 0.3초 후에 입력을 활성화하는 메서드를 호출하여 초기 입력을 방지
        Invoke(nameof(EnableInput), 0.3f);
    }

    // Update is called once per frame
    void Update()
    {
        // 텍스트의 활성화 상태를 토글하여 깜빡임 효과
        // pressEnterText가 null이 아닌 경우에만 깜빡임 타이머 업데이트 및 텍스트 활성화 토글 수행
        if (pressEnterText == null) return;

        // 깜빡임 타이머 업데이트
        _blinkTimer += Time.deltaTime;
        if (_blinkTimer >= _blinkInterval)
        {
            pressEnterText.enabled = !pressEnterText.enabled;
            _blinkTimer = 0f;
        }
    }

    private void EnableInput() => _inputEnabled = true;

    private void OnEnable()
    {
        if(InputReader.Instance != null) InputReader.Instance.OnConfirmed += HandleConfirm;
    }

    private void OnDisable()
    {
        if(InputReader.Instance != null) InputReader.Instance.OnConfirmed -= HandleConfirm;
    }

    private void HandleConfirm()
    {
        // 입력이 아직 활성화되지 않은 경우 무시
        if (!_inputEnabled) return;

        // 입력이 한 번 발생하면 다시 비활성화하여 중복 입력 방지
        _inputEnabled = false;

        // 곡 선택 씬으로 전환
        GameManager.Instance.GoToSelect();
    }
}
