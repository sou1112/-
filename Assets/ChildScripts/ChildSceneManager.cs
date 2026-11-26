using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ChildSceneManager : MonoBehaviour
{
    [Header("7つのパネルを割り当て")]
    [SerializeField] private GameObject Tobirasentaku;
    [SerializeField] private GameObject Kuesutosentaku;
    [SerializeField] private GameObject Kuesutozikkou;
    [SerializeField] private GameObject Kuesutozikkouhousyu;
    [SerializeField] private GameObject Kuesutozikkousippai;
    [SerializeField] private GameObject Kyarakutahyouzi;
    [SerializeField] private GameObject Kyarakutakisekae;
    [SerializeField] private GameObject Kyarakutajenkou;

    [Header("API連携用：ステータス表示UI（任意）")]
    [SerializeField] private TMP_Text childNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text coinsText;

    [Header("API連携用：クエスト選択・実行・報酬表示UI（任意）")]
    [SerializeField] private TMP_Text questListSummaryText;    // クエスト一覧テキスト
    [SerializeField] private TMP_Text selectedQuestTitleText;  // 実行中のクエスト名
    [SerializeField] private TMP_Text rewardPreviewText;       // 獲得予定（または獲得した）報酬表示

    private List<QuestData> cachedQuests = new List<QuestData>();
    private QuestData currentSelectedQuest = null;

    private void Start()
    {
        // ゲーム開始時はホーム画面（SRP-02）を表示
        ShowPanel(Tobirasentaku);

        // 子供の名前をセッションから反映
        if (childNameText != null && !string.IsNullOrEmpty(UserSession.ChildName))
        {
            childNameText.text = UserSession.ChildName;
        }

        // DBからステータスとクエスト一覧を取得
        FetchUserStatus();
        FetchQuests();
    }

    // パネル切り替え用（他を全部隠して指定した1枚だけ出す）
    public void ShowPanel(GameObject targetPanel)
    {
        Tobirasentaku.SetActive(false);
        Kuesutosentaku.SetActive(false);
        Kuesutozikkou.SetActive(false);
        Kuesutozikkouhousyu.SetActive(false);
        Kuesutozikkousippai.SetActive(false);
        Kyarakutahyouzi.SetActive(false);
        Kyarakutakisekae.SetActive(false);
        Kyarakutajenkou.SetActive(false);

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

    // 1. ユーザーステータス取得（GET /api/status?userId=...）
    public void FetchUserStatus()
    {
        string userId = GetCurrentUserId();
        StartCoroutine(ApiClient.Instance.GetRequest(
            $"/status?userId={userId}",
            onSuccess: (json) =>
            {
                UserStatusData status = JsonUtility.FromJson<UserStatusData>(json);
                if (status != null)
                {
                    Debug.Log($"[子供] ステータス取得成功: Lv.{status.level}, EXP:{status.exp}, Coins:{status.coins}");
                    if (levelText != null) levelText.text = $"Lv.{status.level}";
                    if (expText != null) expText.text = $"{status.exp} EXP";
                    if (coinsText != null) coinsText.text = $"{status.coins}";
                }
            },
            onError: (err) => Debug.LogError($"[子供] ステータス取得エラー: {err}")
        ));
    }

    // 2. クエスト一覧取得（GET /api/quests?userId=...）
    public void FetchQuests()
    {
        string userId = GetCurrentUserId();
        StartCoroutine(ApiClient.Instance.GetRequest(
            $"/quests?userId={userId}",
            onSuccess: (json) =>
            {
                QuestListResponse res = JsonUtility.FromJson<QuestListResponse>(json);
                cachedQuests = res != null && res.items != null ? res.items : new List<QuestData>();
                Debug.Log($"[子供] クエスト一覧取得成功: {cachedQuests.Count}件");

                // 初期選択として先頭のクエストをセット
                if (cachedQuests.Count > 0)
                {
                    SelectQuestByIndex(0);
                }

                if (questListSummaryText != null)
                {
                    string display = "";
                    for (int i = 0; i < cachedQuests.Count; i++)
                    {
                        var q = cachedQuests[i];
                        display += $"{i + 1}. {q.title}（コイン:{q.rewardCoins} / EXP:{q.rewardExp}）\n";
                    }
                    questListSummaryText.text = string.IsNullOrEmpty(display) ? "クエストがありません" : display;
                }
            },
            onError: (err) => Debug.LogError($"[子供] クエスト取得エラー: {err}")
        ));
    }

    // 3. クエストを番号（0番目, 1番目...）で選択して実行画面へ進む
    // Kuesutosentaku の各クエストボタンの OnClick に設定（引数に 0, 1, 2... を指定）
    public void SelectQuestByIndex(int index)
    {
        if (cachedQuests == null || index < 0 || index >= cachedQuests.Count)
        {
            Debug.LogWarning("指定された番号のクエストが存在しません");
            return;
        }

        currentSelectedQuest = cachedQuests[index];
        if (selectedQuestTitleText != null) selectedQuestTitleText.text = currentSelectedQuest.title;
        if (rewardPreviewText != null)
        {
            rewardPreviewText.text = $"コイン +{currentSelectedQuest.rewardCoins} / EXP +{currentSelectedQuest.rewardExp}";
        }
    }

    // 4. クエスト完了をDBへ送信（POST /api/logs）
    // Kuesutozikkou の「できた！（完了）」ボタンの OnClick に設定
    public void OnClickCompleteQuest()
    {
        OnClickCompleteQuestWithDuration(10);
    }

    // 経過時間（分）を指定して完了送信する場合用
    public void OnClickCompleteQuestWithDuration(int durationMinutes)
    {
        string targetQuestId = currentSelectedQuest != null ? currentSelectedQuest.questId : "Q001";

        QuestLogCreateRequest req = new QuestLogCreateRequest
        {
            userId = GetCurrentUserId(),
            questId = targetQuestId,
            durationMinutes = durationMinutes
        };

        string json = JsonUtility.ToJson(req);
        StartCoroutine(ApiClient.Instance.PostRequest(
            "/logs",
            json,
            onSuccess: (responseJson) =>
            {
                Debug.Log($"[子供] クエスト完了ログ送信成功: questId={targetQuestId}");
                ShowPanel(Kuesutozikkouhousyu);
            },
            onError: (err) =>
            {
                Debug.LogError($"[子供] ログ送信エラー: {err}");
                ShowPanel(Kuesutozikkousippai);
            }
        ));
    }

    // ==========================================
    // JSON 変換用データクラス
    // ==========================================
    [System.Serializable]
    private class UserStatusData
    {
        public string userId;
        public int level;
        public int exp;
        public int coins;
    }

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
    private class QuestLogCreateRequest
    {
        public string userId;
        public string questId;
        public int durationMinutes;
    }
}