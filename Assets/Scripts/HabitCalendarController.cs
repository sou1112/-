using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class HabitCalendarController : MonoBehaviour
{
    [Header("ヘッダーUI")]
    [SerializeField] private TextMeshProUGUI monthTitleText;
    [SerializeField] private Button prevMonthButton;
    [SerializeField] private Button nextMonthButton;

    [Header("カレンダーの親")]
    [SerializeField] private Transform calendarGridParent;

    [Header("文字色・背景色の設定")]
    [SerializeField] private Color currentMonthTextColor = Color.black;
    [SerializeField] private Color otherMonthTextColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color todayBackgroundColor = new Color(1f, 0.95f, 0.7f, 1f);
    [SerializeField] private Color normalBackgroundColor = new Color(0.96f, 0.95f, 0.9f, 1f);

    // 日付がタップされたときに呼ばれるイベント（画面ごとに異なる処理を設定可能）
    public UnityEvent<DateTime> onDateSelected;

    private DateTime currentDisplayMonth;

    private void Start()
    {
        DateTime now = DateTime.Now;
        currentDisplayMonth = new DateTime(now.Year, now.Month, 1);

        prevMonthButton.onClick.AddListener(() => ChangeMonth(-1));
        nextMonthButton.onClick.AddListener(() => ChangeMonth(1));

        UpdateCalendar();
    }

    public void ChangeMonth(int deltaMonths)
    {
        currentDisplayMonth = currentDisplayMonth.AddMonths(deltaMonths);
        UpdateCalendar();
    }

    public void UpdateCalendar()
    {
        monthTitleText.text = $"{currentDisplayMonth.Year}年{currentDisplayMonth.Month}月";

        DateTime firstDayOfMonth = new DateTime(currentDisplayMonth.Year, currentDisplayMonth.Month, 1);
        int startOffset = (int)firstDayOfMonth.DayOfWeek;
        DateTime gridStartDate = firstDayOfMonth.AddDays(-startOffset);

        DateTime today = DateTime.Today;
        int cellCount = calendarGridParent.childCount;

        for (int i = 0; i < cellCount; i++)
        {
            Transform cellTransform = calendarGridParent.GetChild(i);
            DateTime cellDate = gridStartDate.AddDays(i);
            bool isCurrentMonth = (cellDate.Month == currentDisplayMonth.Month);
            bool isToday = (cellDate.Date == today);

            TextMeshProUGUI dayText = cellTransform.GetComponentInChildren<TextMeshProUGUI>();
            if (dayText != null)
            {
                dayText.text = cellDate.Day.ToString();
                dayText.color = isCurrentMonth ? currentMonthTextColor : otherMonthTextColor;
            }

            Image cellBg = cellTransform.GetComponentInChildren<Image>();
            if (cellBg != null)
            {
                cellBg.color = isToday ? todayBackgroundColor : normalBackgroundColor;
            }

            // 各マス（Button）に日付タップ時の処理を登録
            Button cellButton = cellTransform.GetComponent<Button>();
            if (cellButton != null)
            {
                cellButton.onClick.RemoveAllListeners();
                cellButton.onClick.AddListener(() => OnCellClicked(cellDate));
            }
        }
    }

    private void OnCellClicked(DateTime selectedDate)
    {
        Debug.Log($"選択された日付: {selectedDate:yyyy/MM/dd}");
        onDateSelected?.Invoke(selectedDate);
    }
}