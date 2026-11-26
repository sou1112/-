using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ParentSceneManager : MonoBehaviour
{
    [Header("7つのパネルを割り当て")]
    [SerializeField] private GameObject parentHomeCalenderPanel;
    [SerializeField] private GameObject parentLogDetailPanel;
    [SerializeField] private GameObject parentQuestApprovePanel;
    [SerializeField] private GameObject parentTimeZoneSetPanel;
    [SerializeField] private GameObject parentQuestEditPanel;
    [SerializeField] private GameObject parentDailyQuestSelectPanel;
    [SerializeField] private GameObject parentQuestListPanel;

    [Header("API連携用：クエスト一覧・ログ表示（任意・デバッグ/画面表示用）")]
    [SerializeField] private TMP_Text questListSummaryText;   // クエスト一覧をテキスト表示する場合
    [SerializeField] private TMP_Text unapprovedLogText;      // 未承認ログ一覧をテキスト表示する場合

    [Header("API連携用：クエスト新規作成・編集入力UI")]
    [SerializeField] private TMP_InputField questTitleInput;
    [SerializeField] private TMP_InputField rewardCoinsInput;
    [SerializeField] private TMP_InputField rewardExpInput;
    [SerializeField] private TMP_Text saveFeedbackText;

    // 取得したデータの保持リスト（承認や編集で参照）
    private List<QuestData> cachedQuests = new List<QuestData>();
    private List<QuestLogData> cachedLogs = new List<QuestLogData>();
    private string selectedLogIdForApproval = "";

    private void Start()
    {
        // ゲーム開始時はホーム画面（SRP-02）を表示
        ShowPanel(parentHomeCalenderPanel);

        // 保護者シーン起動時にDBから最新のクエスト一覧とログ一覧を取得
        FetchQuests();
        FetchLogs();
    }

    // パネル切り替え用（他を全部隠して指定した1枚だけ出す）
    public void ShowPanel(GameObject targetPanel)
    {
        parentHomeCalenderPanel.SetActive(false);
        parentLogDetailPanel.SetActive(false);
        parentQuestApprovePanel.SetActive(false);
        parentTimeZoneSetPanel.SetActive(false);
        parentQuestEditPanel.SetActive(false);
        parentDailyQuestSelectPanel.SetActive(false);
        parentQuestListPanel.SetActive(false);

        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }

    // ダイアログ用（後ろの画面を残したまま上に重ねて出す）
    public void OpenDialog(GameObject dialogPanel)
    {
        if (dialogPanel != null) dialogPanel.SetActive(true);
    }

    public void CloseDialog(GameObject dialogPanel)
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
    }

    // シーン移動用（共通シーンへ戻る）
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ==========================================
    // ここから API 連携メソッド
    // ==========================================

    private string GetCurrentUserId()
    {
        // 単体テスト時（共通シーンを通らず再生した場合）はサンプルID U001 を使用
        return string.IsNullOrEmpty(UserSession.UserId) ? "U001" : UserSession.UserId;
    }

    // 1. クエスト一覧を取得（GET /api/quests?userId=...）
    public void FetchQuests()
    {
        string userId = GetCurrentUserId();
        StartCoroutine(ApiClient.Instance.GetRequest(
            $"/quests?userId={userId}",
            onSuccess: (json) =>
            {
                QuestListResponse res = JsonUtility.FromJson<QuestListResponse>(json);
                cachedQuests = res != null && res.items != null ? res.items : new List<QuestData>();
                Debug.Log($"[保護者] クエスト一覧取得成功: {cachedQuests.Count}件");

                if (questListSummaryText != null)
                {
                    string display = "";
                    foreach (var q in cachedQuests)
                    {
                        display += $"・{q.title}（コイン:{q.rewardCoins} / EXP:{q.rewardExp}）\n";
                    }
                    questListSummaryText.text = string.IsNullOrEmpty(display) ? "登録されたクエストはありません" : display;
                }
            },
            onError: (err) => Debug.LogError($"[保護者] クエスト取得エラー: {err}")
        ));
    }

    // 2. クエストログ一覧を取得（GET /api/logs?userId=...）
    public void FetchLogs()
    {
        string userId = GetCurrentUserId();
        StartCoroutine(ApiClient.Instance.GetRequest(
            $"/logs?userId={userId}",
            onSuccess: (json) =>
            {
                QuestLogListResponse res = JsonUtility.FromJson<QuestLogListResponse>(json);
                cachedLogs = res != null && res.items != null ? res.items : new List<QuestLogData>();
                Debug.Log($"[保護者] ログ一覧取得成功: {cachedLogs.Count}件");

                selectedLogIdForApproval = "";
                string display = "";
                foreach (var log in cachedLogs)
                {
                    string status = log.parentApprovalStatus ? "【承認済】" : "【未承認】";
                    display += $"{status} {log.executedAt} (クエストID: {log.questId})\n";

                    // 一番新しい未承認ログを承認対象として自動セット
                    if (!log.parentApprovalStatus && string.IsNullOrEmpty(selectedLogIdForApproval))
                    {
                        selectedLogIdForApproval = log.logId;
                    }
                }

                if (unapprovedLogText != null)
                {
                    unapprovedLogText.text = string.IsNullOrEmpty(display) ? "クエスト履歴はありません" : display;
                }
            },
            onError: (err) => Debug.LogError($"[保護者] ログ取得エラー: {err}")
        ));
    }

    // 3. クエスト新規登録・保存（POST /api/quests）
    // ParentQuestEditPanel の「保存」ボタンの OnClick に設定
    public void OnClickSaveQuest()
    {
        if (questTitleInput == null || string.IsNullOrWhiteSpace(questTitleInput.text))
        {
            if (saveFeedbackText != null) saveFeedbackText.text = "クエスト名を入力してください";
            return;
        }

        int coins = 10;
        int exp = 20;
        if (rewardCoinsInput != null) int.TryParse(rewardCoinsInput.text, out coins);
        if (rewardExpInput != null) int.TryParse(rewardExpInput.text, out exp);

        QuestSaveRequest req = new QuestSaveRequest
        {
            userId = GetCurrentUserId(),
            title = questTitleInput.text.Trim(),
            rewardCoins = coins,
            rewardExp = exp
        };

        string json = JsonUtility.ToJson(req);
        StartCoroutine(ApiClient.Instance.PostRequest(
            "/quests",
            json,
            onSuccess: (responseJson) =>
            {
                Debug.Log("[保護者] クエスト保存成功");
                if (saveFeedbackText != null) saveFeedbackText.text = "クエストを保存しました！";
                FetchQuests(); // 一覧を再取得して最新化
                ShowPanel(parentQuestListPanel);
            },
            onError: (err) =>
            {
                if (saveFeedbackText != null) saveFeedbackText.text = "保存に失敗しました";
            }
        ));
    }

    // 4. 達成承認＆コイン・経験値付与（POST /api/logs/approve）
    // ParentQuestApprovePanel の「承認する」ボタンの OnClick に設定
    public void OnClickApproveLatestLog()
    {
        if (string.IsNullOrEmpty(selectedLogIdForApproval))
        {
            Debug.Log("未承認のクエストログがありません");
            return;
        }

        ApproveLog(selectedLogIdForApproval);
    }

    // 特定の logId を指定して承認する場合用
    public void ApproveLog(string targetLogId)
    {
        ApproveRequest req = new ApproveRequest
        {
            logId = targetLogId,
            userId = GetCurrentUserId()
        };

        string json = JsonUtility.ToJson(req);
        StartCoroutine(ApiClient.Instance.PostRequest(
            "/logs/approve",
            json,
            onSuccess: (responseJson) =>
            {
                Debug.Log($"[保護者] ログ承認完了: {targetLogId}");
                FetchLogs(); // ログ一覧を再取得して【承認済】に更新
                ShowPanel(parentHomeCalenderPanel);
            },
            onError: (err) => Debug.LogError($"[保護者] 承認エラー: {err}")
        ));
    }

    // ==========================================
    // JSON 変換用データクラス
    // ==========================================
    [System.Serializable]
    private class QuestListResponse
    {
        public List<QuestData> items;
    }

    [System.Serializable]
    private class QuestData
    {
        public string questId;
        public string userId;
        public string title;
        public int rewardCoins;
        public int rewardExp;
    }

    [System.Serializable]
    private class QuestSaveRequest
    {
        public string userId;
        public string title;
        public int rewardCoins;
        public int rewardExp;
    }

    [System.Serializable]
    private class QuestLogListResponse
    {
        public List<QuestLogData> items;
    }

    [System.Serializable]
    private class QuestLogData
    {
        public string logId;
        public string userId;
        public string questId;
        public string executedAt;
        public int durationMinutes;
        public string result;
        public bool parentApprovalStatus;
    }

    [System.Serializable]
    private class ApproveRequest
    {
        public string logId;
        public string userId;
    }
}