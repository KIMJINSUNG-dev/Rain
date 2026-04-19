using TMPro;
using UnityEngine;

public class ResultSceneController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI songTitleText;
    [SerializeField] private TextMeshProUGUI diffText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI perfectText;
    [SerializeField] private TextMeshProUGUI greatText;
    [SerializeField] private TextMeshProUGUI goodText;
    [SerializeField] private TextMeshProUGUI missText;
    [SerializeField] private TextMeshProUGUI maxComboText;

    private bool _inputEnabled = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputReader.Instance?.EnableNavigation();
        SetResultData();
        Invoke(nameof(EnableInput), 0.3f);
    }

    private void OnEnable()
    {
        if(InputReader.Instance != null ) InputReader.Instance.OnConfirmed += HandleConfirm;
    }

    private void OnDisable()
    {
        if(InputReader.Instance != null ) InputReader.Instance.OnConfirmed -= HandleConfirm;
    }

    private void HandleConfirm()
    {
        if(!_inputEnabled) return;
        _inputEnabled = false;
        GameManager.Instance?.GoToSelect();
    }

    private void SetResultData()
    {
        var gm = GameManager.Instance;
        if(gm == null) return;
        songTitleText.text = gm.SelectedSong.title;
        switch (gm.SelectedDiffIdx)
        {
            case 0: diffText.text = "EASY";   break;
            case 1: diffText.text = "NORMAL"; break;
            case 2: diffText.text = "HARD";   break;
        }
        scoreText.text = "SCORE: " + gm.FinalScore.ToString();
        rankText.text = gm.FinalRank.ToString();
        perfectText.text = "PERFECT: " + gm.PerfectCount.ToString();
        greatText.text = "GREAT: " + gm.GreatCount.ToString();
        goodText.text = "GOOD: " + gm.GoodCount.ToString();
        missText.text = "MISS: " + gm.MissCount.ToString();
        maxComboText.text = "MAX COMBO: " + gm.MaxCombo.ToString();
    }

    private void EnableInput() => _inputEnabled = true;
}
