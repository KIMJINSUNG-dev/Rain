using UnityEngine;
using UnityEngine.UI;

public class NoteObject : MonoBehaviour
{
    public NoteData Data { get; private set; }
    public bool IsJudged { get; set; } = false;


    private float _laneX;
    private float _judgeLineLocalY;

    private RectTransform _rt;
    private Image         _img;


    public void Initialize(NoteData data, float laneX, float judgeLineLocalY)
    {
        Data             = data;
        _laneX           = laneX;
        _judgeLineLocalY = judgeLineLocalY;

        _rt  = GetComponent<RectTransform>();
        _img = GetComponent<Image>();

        // 노트 스폰시 화면에서 깜빡이는 현상 방지를 위해 초기 위치를 화면 밖으로 설정
        if (_rt != null) _rt.anchoredPosition = new Vector2(_laneX, _judgeLineLocalY + 2000.0f);
    }

    public void UpdatePosition(double currentGameTime, float pixelsPerSecond)
    {
        if (_rt == null) return;
        double remaining = Data.time - currentGameTime;
        float yPos = _judgeLineLocalY + (float)remaining * pixelsPerSecond;
        _rt.anchoredPosition = new Vector2(_laneX, yPos);
    }

    public void PlayHitEffect(Color color)
    {
        if (_img == null) return;
        _img.color = new Color(color.r, color.g, color.b, 0.5f); // 노트 처리시 판정에 따른 반투명 색상 출력
    }

    public void PlayMissEffect()
    {
        if (_img == null) return;
        _img.color = new Color(0.35f, 0.35f, 0.35f, 0.5f); // Miss 시 반투명 회색으로 변경
    }
}
