using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SongDatabase : MonoBehaviour
{
    /* **
     * songs.csv 파일을 읽어서 곡 목록을 제공하는 싱글톤 클래스.
     * **/

    public static SongDatabase Instance { get; private set; }

    // 곡 정보 리스트, 외부에서 읽기 전용으로 접근할 수 있도록 프로퍼티로 제공
    public List<SongInfo> SongList { get; private set; } = new();

    private void Awake()
    {
        // 인스턴스의 싱글톤 관리를 위해 기존에 생성된 인스턴스가 있는지, 그리고 현재 인스턴스가 자신이 아닌지를 확인
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        // 현재 인스턴스를 싱글톤 인스턴스로 설정
        Instance = this;
        // 모든 씬에서 사용되는 오브젝트이므로 씬이 변경되어도 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);

        LoadSongs();
    }

    // CSV 파일을 읽어 곡 목록 정보를 불러오는 메서드
    // CSV 파일 형식은 아래와 같음
    /* *********************************************************** *
     * SongName,BPM,EasyDifficulty,NormalDifficulty,HardDifficulty *
     * Song1,150,3,6,9                                             *
     * Song2,128,2,4,8                                             *
     * Song3,200,1,5,7                                             *
     * *********************************************************** */
    private void LoadSongs()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "songs.csv");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"곡 정보 파일이 존재하지 않습니다: {path}, 더미 데이터를 사용합니다.");
            UseDummyList();
            return;
        }

        string[] lines = File.ReadAllLines(path);

        // 0번 행은 인덱스이므로 1번 행부터 읽기 시작
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            // 빈 줄 및 주석 검사
            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

            // 유효한 행을 콤마(,) 단위로 파싱
            string[] parts = line.Split(",");

            // 배열 길이가 5보다 작은 경우 잘못된 데이터로 판단하여 continue 처리
            if(parts.Length < 5)
            {
                Debug.LogWarning($"[SongDatabase] {i + 1}행 형식 오류 (컬럼 수 부족): {line}");
                continue;
            }

            // 파싱한 데이터를 곡 목록에 추가
            SongList.Add(new SongInfo
            {
                title       = parts[0].Trim(),
                bpm         = float.Parse(parts[1].Trim()),
                easyLevel   = int.Parse(parts[2].Trim()),
                normalLevel = int.Parse(parts[3].Trim()),
                hardLevel   = int.Parse(parts[4].Trim()),
            });
        }

        Debug.Log($"[SongDatabase] 곡 {SongList.Count}개 로드 완료.");
    }

    private void UseDummyList()
    {
        SongList.Add(new SongInfo { title = "DummyMusic1", bpm = 128, easyLevel = 1, normalLevel = 4, hardLevel = 7 });
        SongList.Add(new SongInfo { title = "DummyMusic2", bpm = 150, easyLevel = 2, normalLevel = 5, hardLevel = 8 });
        SongList.Add(new SongInfo { title = "DummyMusic3", bpm = 200, easyLevel = 3, normalLevel = 6, hardLevel = 9 });
    }
}
