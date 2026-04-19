using System.Collections.Generic;

public class SongInfo
{
    public string title;             // 곡명
    public float  bpm;               // BPM
    // 난이도별 레벨(0: easy, 1: normal, 2: hard)
    public int    easyLevel;         // easy 난이도 레벨
    public int    normalLevel;       // normal 난이도 레벨
    public int    hardLevel;         // hard 난이도 레벨

    // 난이도별 레벨 반환 메서드(0: easy, 1: normal, 2: hard)
    public int GetLevel(int idx) => idx switch
    {
        0 => easyLevel,
        1 => normalLevel,
        2 => hardLevel,
        _ => 0 // 예외 처리: 0레벨 반환
    };
}

public class NoteData
{
    public float time;                // 노트가 출력되는 시간(초)
    public int   lane;                // 노트가 떨어지는 레인(d: 0,f: 1,j: 2,k: 3)
    public bool  isLong = false;      // 롱노트 여부(false: 일반노트, true: 롱노트)
    public float longDuration = 0.0f; // 롱노트의 길이(초, isLong이 1일 때만 유효)
}

public class ChartData
{
    public List<NoteData> Notes = new(); // 노트 데이터 리스트, 한 곡에 할당되는 보면 데이터
}
