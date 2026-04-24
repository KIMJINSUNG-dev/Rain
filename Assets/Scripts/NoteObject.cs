using UnityEngine;
using UnityEngine.UI;

public class NoteObject : MonoBehaviour
{
    public NoteData Data { get; private set; }
    public bool IsJudged { get; set; } = false;
    public bool IsHolding = false;

    private float _laneX;
    private float _judgeLineLocalY;

    private RectTransform _rt;
    private Image         _img;
    private RectTransform _bodyRect;


    public void Initialize(NoteData data, float laneX, float judgeLineLocalY)
    {
        Data             = data;
        _laneX           = laneX;
        _judgeLineLocalY = judgeLineLocalY;

        _rt  = GetComponent<RectTransform>();
        _img = GetComponent<Image>();

        var bodyTf = transform.Find("Body");
        if (bodyTf != null) _bodyRect = bodyTf.GetComponent<RectTransform>();

        // 롱노트가 아닌 경우 Body 오브젝트 비활성화
        if (!Data.isLong && _bodyRect != null) _bodyRect.gameObject.SetActive(false);

        // 노트 스폰시 화면에서 깜빡이는 현상 방지를 위해 초기 위치를 화면 밖으로 설정
        if (_rt != null) _rt.anchoredPosition = new Vector2(_laneX, _judgeLineLocalY + 2000.0f);
    }

    public void UpdatePosition(double currentGameTime, float pixelsPerSecond)
    {
        if (_rt == null) return;
        double remaining = Data.time - currentGameTime;
        float yPos = _judgeLineLocalY + (float)remaining * pixelsPerSecond;
        _rt.anchoredPosition = new Vector2(_laneX, yPos);

        if(Data.isLong && _bodyRect != null)
        {
            double tailRemain = (Data.time + Data.longDuration) - currentGameTime;
            float tailY = _judgeLineLocalY + (float)(tailRemain * pixelsPerSecond);
            float bodyHeight = Mathf.Max(0.0f, tailY - yPos);
            _bodyRect.sizeDelta = new Vector2(_bodyRect.sizeDelta.x, bodyHeight);
        }
    }

    public void PlayHitEffect(Color color)
    {
        if (_img == null) return;
        // 키 입력 이펙트 등의 효과 추가 예정
    }

    public void PlayMissEffect()
    {
        if (_img == null) return;
        // 미스 이펙트 등의 효과 추가 예정
    }

    public void PlayLongMissEffect()
    {
        if (_img != null) _img.color = new Color(0.35f, 0.35f, 0.35f, 0.5f); // Miss 시 반투명 회색으로 변경
        if (_bodyRect != null)
        {
            var bodyImg = _bodyRect.GetComponent<Image>();
            if (bodyImg != null) bodyImg.color = new Color(0.2f, 0.6f, 0.9f, 0.3f); // 롱노트 미스 시 본체는 더 어두운 색상으로 변경
        }
    }
}
