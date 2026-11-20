using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void Start()
    {
        // ゲーム開始時はホーム画面（SRP-02）を表示
        ShowPanel(parentHomeCalenderPanel);
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
}