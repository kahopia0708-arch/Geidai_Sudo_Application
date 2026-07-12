using UnityEngine;
using TMPro;

public class WeeklyTextController : MonoBehaviour
{
    [Header("週替わりテキスト配列")]
    [SerializeField] private string[] weeklyTexts = new string[]
    {
        "Kirakira",
        "DonDon",
        "FuwaFuwa",
        "ChiriChiri",
        "SariSari",
        "GoboGobo",
        "Fuwan",
        "SariRa",
        "ChiriRa",
        "FuwaRa",
        "DonSari",
        "ChiriGobo",
        "DonChiri"
    };

    [Header("テキストコンポーネント")]
    [SerializeField] private TextMeshProUGUI textComponent;

    private void Start()
    {
        UpdateWeeklyText();
    }

    /// <summary>
    /// 現在の週に基づいて週替わりテキストを更新する
    /// </summary>
    private void UpdateWeeklyText()
    {
        if (textComponent != null && weeklyTexts.Length > 0)
        {
            int weekIndex = GetWeekOfYear() % weeklyTexts.Length;
            textComponent.text = weeklyTexts[weekIndex];
        }
    }

    /// <summary>
    /// 現在の週番号を取得する（1年を52週として計算）
    /// </summary>
    private int GetWeekOfYear()
    {
        System.DateTime now = System.DateTime.Now;
        System.DateTime startOfYear = new System.DateTime(now.Year, 1, 1);
        
        // 最初の週の開始日（月曜日）を計算
        int daysToMonday = ((int)System.DayOfWeek.Monday - (int)startOfYear.DayOfWeek + 7) % 7;
        System.DateTime firstMonday = startOfYear.AddDays(daysToMonday);

        if (now < firstMonday)
        {
            // 去年の最終週
            System.DateTime lastYearEnd = new System.DateTime(now.Year - 1, 12, 31);
            return GetWeekOfYearFromDate(lastYearEnd);
        }

        return GetWeekOfYearFromDate(now);
    }

    /// <summary>
    /// 指定された日付の週番号を取得
    /// </summary>
    private int GetWeekOfYearFromDate(System.DateTime date)
    {
        System.DateTime startOfYear = new System.DateTime(date.Year, 1, 1);
        int daysToMonday = ((int)System.DayOfWeek.Monday - (int)startOfYear.DayOfWeek + 7) % 7;
        System.DateTime firstMonday = startOfYear.AddDays(daysToMonday);

        if (date < firstMonday)
        {
            return 52;
        }

        System.TimeSpan diff = date - firstMonday;
        return (diff.Days / 7) + 1;
    }

    /// <summary>
    /// テキストを手動で更新する（Inspectorから呼び出し可能）
    /// </summary>
    public void RefreshText()
    {
        UpdateWeeklyText();
    }
}