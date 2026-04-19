using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    /* **
     * 게임 전반에 걸쳐 사용자 입력을 관리하는 싱글톤 클래스.
     * **/
    public static InputReader Instance { get; private set; }

    // InputSystem 구조: InputActionAsset > InputActionMap > InputAction

    // 최상위 그룹: InputActionAsset
    // 인스펙터에서 사용할 InputActionAsset을 드래그해서 등록할 수 있도록 SerializeField와 Tooltip 속성 사용
    [SerializeField]
    [Tooltip("여기에 사용할 InputActionAsset을 드래그해서 등록")]
    private InputActionAsset _actionAsset;

    // 중간 그룹: InputActionMap
    private InputActionMap _gamePlayMap;   // 게임 플레이 관련 입력을 관리하는 InputActionMap
    private InputActionMap _navigationMap; // UI 탐색 관련 입력을 관리하는 InputActionMap

    // 최하위 입력 단위: InputAction
    // _gamePlayMap에서 레인 입력과 플레이 속도 조절, 일시정지 입력을 관리하는 InputAction들
    private InputAction _laneD, _laneF, _laneJ, _laneK; // 레인 입력 InputAction
    private InputAction _pauseAction;                   // 일시정지 입력 InputAction
    private InputAction _playSpeedUp, _playSpeedDown;   // 플레이 속도 조절 입력 InputAction

    // _navigationMap에서 UDLR 탐색과 확인, 플레이 속도 조절 입력을 관리하는 InputAction들
    private InputAction _navigate;                  // UDLR 탐색 입력 InputAction
    private InputAction _confirm;                   // 확인 입력 InputAction
    private InputAction _navSpeedUp, _navSpeedDown; // 플레이 속도 조절 입력 InputAction

    // 외부에서 구독할 수 있는 이벤트들, 각 입력이 발생했을 때 해당 이벤트가 호출됨
    public event Action<int, double> OnLanePressed;      // 레인 입력 Action: int 매개변수 => 레인 번호(0~3), double 매개변수 => 입력이 발생한 시간(초)
    public event Action<int>         OnLaneReleased;     // 레인 입력 해제 Action: int 매개변수 => 레인 번호(0~3)
    public event Action              OnPausePressed;     // 일시정지 입력 Action
    public event Action              OnSpeedUp;          // 플레이 속도 증가 입력 Action
    public event Action              OnSpeedDown;        // 플레이 속도 감소 입력 Action
    public event Action<Vector2>     OnNavigate;         // UDLR 탐색 입력 Action: Vector2 매개변수 => 탐색 방향 벡터(x: -1=좌, 1=우, y: -1=하, 1=상)
    public event Action              OnNavigateCanceled; // UDLR 탐색 입력 취소 Action
    public event Action              OnConfirmed;        // 확인 입력 Action


    private void Awake()
    {
        // 인스턴스의 싱글톤 관리를 위해 기존에 생성된 인스턴스가 있는지, 그리고 현재 인스턴스가 자신이 아닌지를 확인
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        // 현재 인스턴스를 싱글톤 인스턴스로 설정
        Instance = this;
        // 모든 씬에서 사용되는 오브젝트이므로 씬이 변경되어도 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);

        SetupActions();
    }

    // InputReader가 강제 파괴될 때(=앱 종료 시) 호출되는 메서드
    // InputActionAsset을 비활성화하여 입력 처리를 중지 및 메모리 누수 방지
    private void OnDestroy()
    {
        _actionAsset?.Disable();
    }

    // InputActionAsset에서 필요한 ActionMap과 Action들을 찾아서 변수에 할당하는 메서드
    private void SetupActions()
    {
        // 최상위 그룹: InputActionAsset
        // InputActionAsset이 할당되지 않은 경우를 대비해 null 체크를 수행
        if (_actionAsset == null)
        {
            Debug.Log("InputActionAsset이 할당되지 않았습니다. 인스펙터에서 드래그해서 등록해주세요.");
            return;
        }

        // 중간 그룹: InputActionMap
        // InputActionAsset에서 "GamePlay"와 "Navigation"이라는 이름의 ActionMap을 찾아서 변수에 할당, 찾지 못하면 예외 발생
        _gamePlayMap   = _actionAsset.FindActionMap("GamePlay", throwIfNotFound: true);
        _navigationMap = _actionAsset.FindActionMap("Navigation", throwIfNotFound: true);

        // 최하위 입력 단위: InputAction
        // _gamePlayMap에서 레인 입력과 플레이 속도 조절, 일시정지 입력을 관리하는 Action들을 찾아서 변수에 할당, 찾지 못하면 예외 발생
        _laneD         = _gamePlayMap.FindAction("LaneD", throwIfNotFound: true);
        _laneF         = _gamePlayMap.FindAction("LaneF", throwIfNotFound: true);
        _laneJ         = _gamePlayMap.FindAction("LaneJ", throwIfNotFound: true);
        _laneK         = _gamePlayMap.FindAction("LaneK", throwIfNotFound: true);
        _pauseAction   = _gamePlayMap.FindAction("Pause", throwIfNotFound: true);
        _playSpeedUp   = _gamePlayMap.FindAction("SpeedUp", throwIfNotFound: true);
        _playSpeedDown = _gamePlayMap.FindAction("SpeedDown", throwIfNotFound: true);

        // _navigationMap에서 UDLR 탐색과 확인, 플레이 속도 조절 입력을 관리하는 Action들을 찾아서 변수에 할당, 찾지 못하면 예외 발생
        _navigate     = _navigationMap.FindAction("Navigate", throwIfNotFound: true);
        _confirm      = _navigationMap.FindAction("Confirm", throwIfNotFound: true);
        _navSpeedUp   = _navigationMap.FindAction("SpeedUp", throwIfNotFound: true);
        _navSpeedDown = _navigationMap.FindAction("SpeedDown", throwIfNotFound: true);

        // 각 Action에 대한 콜백 등록
        // OnLanePressed: ctx.time을 사용하지 않고 AudioSettings.dspTime을 사용하여
        // 입력이 발생한 정확한 시간(초)을 전달하도록 수정
        // performed: 입력이 발생했을 때 호출되는 이벤트, canceled: 입력이 해제되었을 때 호출되는 이벤트
        _laneD.performed += ctx => OnLanePressed?.Invoke(0, AudioSettings.dspTime);
        _laneF.performed += ctx => OnLanePressed?.Invoke(1, AudioSettings.dspTime);
        _laneJ.performed += ctx => OnLanePressed?.Invoke(2, AudioSettings.dspTime);
        _laneK.performed += ctx => OnLanePressed?.Invoke(3, AudioSettings.dspTime);

        _laneD.canceled += ctx => OnLaneReleased?.Invoke(0);
        _laneF.canceled += ctx => OnLaneReleased?.Invoke(1);
        _laneJ.canceled += ctx => OnLaneReleased?.Invoke(2);
        _laneK.canceled += ctx => OnLaneReleased?.Invoke(3);

        _pauseAction.performed   += ctx => OnPausePressed?.Invoke();
        _playSpeedUp.performed   += ctx => OnSpeedUp?.Invoke();
        _playSpeedDown.performed += ctx => OnSpeedDown?.Invoke();

        // navigate Action의 경우 OnLanePressed와 마찬가지로 입력 해제의 경우 canceled 이벤트도 처리함
        _navigate.performed += ctx => OnNavigate?.Invoke(ctx.ReadValue<Vector2>());
        _navigate.canceled  += ctx => OnNavigateCanceled?.Invoke();

        _confirm.performed      += ctx => OnConfirmed?.Invoke();
        _navSpeedUp.performed   += ctx => OnSpeedUp?.Invoke();
        _navSpeedDown.performed += ctx => OnSpeedDown?.Invoke();
    }


    // 각 InputActionMap들을 활성화/비활성화하는 메서드들
    public void EnableGamePlay()
    {
        _gamePlayMap.Enable();
        _navigationMap.Disable();
    }

    public void EnableNavigation()
    {
        _gamePlayMap.Disable();
        _navigationMap.Enable();
    }

    private void DisableAll()
    {
        _gamePlayMap.Disable();
        _navigationMap.Disable();
    }
}
