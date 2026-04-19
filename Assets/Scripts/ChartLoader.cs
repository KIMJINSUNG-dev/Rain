using System;
using System.IO;
using UnityEngine;

public static class ChartLoader
{
    /* **
     * 보면 파일(형식: Music1_Easy.csv)을 읽어서 보면 데이터를 제공하는 static 클래스.
     * **/

    // CSV 파일을 읽어 노트 목록 정보를 불러오는 메서드
    // CSV 파일 형식은 아래와 같음
    /* ***************************** *
     * time,lane,isLong,longDuration *
     * 3.0,0,0,0.0                   *
     * 3.5,1,0,0.0                   *
     * 4.0,2,0,0.0                   *
     * 4.5,3,0,0.0                   *
     * ***************************** */
    public static ChartData Load(string chartFileName)
    {
        // 파일 경로 조합: StreamingAssets/Charts/Music1_Easy.csv
        string path = Path.Combine(Application.streamingAssetsPath, "Charts", $"{chartFileName}.csv");

        ChartData chart = new ChartData();

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[ChartLoader] 보면 파일을 찾을 수 없습니다: {path}, 더미 보면을 사용합니다.");
            return UseDummyChart();
        }

        string[] lines = File.ReadAllLines(path);

        float noteTime;
        int noteLane;
        bool noteIsLong;
        float noteLongDuration;
        // 0번 행은 인덱스이므로 1번 행부터 읽기 시작
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            // 빈 줄 및 주석 검사
            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

            // 유효한 행을 콤마(,) 단위로 파싱
            string[] parts = line.Split(",");
            if (parts.Length < 4)
            {
                // 배열 길이가 2 ~ 3인 경우 롱노트 관련 데이터 누락으로 판단하여 배열 리사이징 후 자동적으로 값을 0으로 채움
                // 오류 가능성 있음
                if (parts.Length is 2 or 3)
                {
                    Debug.LogWarning($"[SongDatabase] {i + 1}행 형식 오류 (컬럼 수: 2 ~ 3): {line}, 롱노트 관련 데이터 누락으로 판단하여 빈 항목을 자동 할당합니다.");
                    Array.Resize(ref parts, 4);
                    parts[2] = "0";
                    parts[3] = "0.0";
                }
                // 이외의 사이즈인 경우 잘못된 데이터로 판단하여 continue 처리
                else if (parts.Length < 2)
                {
                    Debug.LogWarning($"[SongDatabase] {i + 1}행 형식 오류 (컬럼 수 부족): {line}");
                    continue;
                }
            }

            // 각 데이터 타입 검사
            if (float.TryParse(parts[0].Trim(), out float noteTimeResult))
            {
                noteTime = noteTimeResult;
            }
            else
            {
                Debug.LogWarning($"[SongDatabase] {i + 1}행 time 형식 오류: {line}");
                continue;
            }
            if (int.TryParse(parts[1].Trim(), out int noteLaneResult))
            {
                noteLane = noteLaneResult;
            }
            else
            {
                Debug.LogWarning($"[SongDatabase] {i + 1}행 lane 형식 오류: {line}");
                continue;
            }
            if (int.TryParse(parts[2].Trim(), out int noteIsLongResult))
            {
                noteIsLong = Convert.ToBoolean(noteIsLongResult);
            }
            else
            {
                Debug.LogWarning($"[SongDatabase] {i + 1}행 isLong 형식 오류: {line}");
                continue;
            }
            if (float.TryParse(parts[3].Trim(), out float noteLongDurationResult))
            {
                noteLongDuration = noteLongDurationResult;
            }
            else
            {
                Debug.LogWarning($"[SongDatabase] {i + 1}행 longDuration 형식 오류: {line}");
                continue;
            }
            // 파싱한 데이터를 곡 목록에 추가
            chart.Notes.Add(new NoteData
            {
                time = noteTime,
                lane = noteLane,
                isLong = noteIsLong,
                longDuration = noteLongDuration,
            });
        }

        // Notes의 데이터가 시간이 뒤섞여있을 수 있는 경우를 감안하여 time순으로 재정렬
        chart.Notes.Sort((a, b) => a.time.CompareTo(b.time));

        // Debug.Log($"[ChartLoader] '{chartFileName}': {chart.Notes.Count}개 노트 로드 완료.");
        return chart;
    }

    // 보면 파일을 찾지 못한 경우 임시 보면 데이터를 반환하는 메서드
    private static ChartData UseDummyChart()
    {
        ChartData dummyChart = new ChartData();

        dummyChart.Notes.Add(new NoteData { time = 3.0f, lane = 1, isLong = false, longDuration = 0.0f });
        dummyChart.Notes.Add(new NoteData { time = 3.5f, lane = 2, isLong = false, longDuration = 0.0f });
        dummyChart.Notes.Add(new NoteData { time = 4.0f, lane = 3, isLong = false, longDuration = 0.0f });
        dummyChart.Notes.Add(new NoteData { time = 4.5f, lane = 4, isLong = false, longDuration = 0.0f });

        return dummyChart;
    }
}
