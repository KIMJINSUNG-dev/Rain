using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /* **
     * 게임 전반에 걸쳐 필요한 정보를 관리하는 싱글톤 클래스.
     * 외부에서 데이터에 직접 접근하는 것은 위험하므로
     * 상수를 제외한 데이터는 기본적으로 프로퍼티 방식을 사용함
     * **/

    // GameManager 클래스의 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    // 씬 이름 상수 정의
    public const string SCENE_TITLE  = "TitleScene";  // 타이틀 씬 이름
    public const string SCENE_SELECT = "SelectScene"; // 곡 선택 씬 이름
    public const string SCENE_PLAY   = "PlayScene";   // 플레이 씬 이름
    public const string SCENE_RESULT = "ResultScene"; // 결과 씬 이름

    // 개별 씬 전환 메서드 정의
    public void GoToTitle()  => SwitchScene(SCENE_TITLE);  // 타이틀 씬으로 전환
    public void GoToSelect() => SwitchScene(SCENE_SELECT); // 곡 선택 씬으로 전환
    public void GoToPlay()   => SwitchScene(SCENE_PLAY);   // 플레이 씬으로 전환
    public void GoToResult() => SwitchScene(SCENE_RESULT); // 결과 씬으로 전환
    
    // 공통으로 사용하는 씬 전환 메서드, sceneName 매개변수로 전환할 씬 이름을 받음
    private void SwitchScene(string sceneName)
    {
        Time.timeScale = 1.0f; // 씬 전환 시 일시정지 등 timeScale이 변경된 경우를 대비해 기본값으로 초기화
        SceneManager.LoadScene(sceneName); // 필요한 작업을 전부 마친 후 씬 전환
    }

    // SelectScene에서 PlayScene으로 전달할 정보
    public SongInfo SelectedSong            { get; set; }           // 현재 선택된 곡 정보
    public int      SelectedDiffIdx         { get; set; } = 0;      // 현재 선택된 난이도 인덱스(0: easy, 1: normal, 2: hard), 기본값: 0(easy)
    public float    SelectedSpeed           { get; set; } = 1.0f;   // 현재 선택된 레인 속도, 기본값: 1.0f
    public int      InputOffsetStep         { get; set; } = 0;      // 부동소수점 오차 문제를 해결하기 위해 판정선 조절을 정수 연산을 거쳐 작업
    public double   SelectedInputOffset => InputOffsetStep * 0.005; // 판정선 조절값

    // PlayScene에서 ResultScene으로 전달할 정보
    public enum Rank { S, A, B, C, D } // 랭크 등급 열거형 정의

    public int  FinalScore   { get; set; } // 플레이어의 최종 점수
    public Rank FinalRank    { get; set; } // 플레이어의 최종 랭크
    public int  PerfectCount { get; set; } // 플레이어의 퍼펙트 판정 개수
    public int  GreatCount   { get; set; } // 플레이어의 그레이트 판정 개수
    public int  GoodCount    { get; set; } // 플레이어의 굿 판정 개수
    public int  MissCount    { get; set; } // 플레이어의 미스 판정 개수
    public int  MaxCombo     { get; set; } // 플레이어의 최대 콤보 수


    private void Awake()
    {
        // 인스턴스의 싱글톤 관리를 위해 기존에 생성된 인스턴스가 있는지, 그리고 현재 인스턴스가 자신이 아닌지를 확인
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        // 현재 인스턴스를 싱글톤 인스턴스로 설정
        Instance = this;
        // 모든 씬에서 사용되는 오브젝트이므로 씬이 변경되어도 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);
    }

    // 현재 선택된 곡과 난이도에 해당하는 보면 파일 이름을 반환하는 메서드
    public string GetChartFileName()
    {
        if (SelectedSong == null) return "";
        string diff = SelectedDiffIdx switch
        {
            0 => "Easy",
            1 => "Normal",
            2 => "Hard",
            _ => "Easy" // 예외 처리: easy 난이도 반환
        };
        return $"{SelectedSong.title}_{diff}"; // 예시: "SongTitle_Easy"
    }
}
