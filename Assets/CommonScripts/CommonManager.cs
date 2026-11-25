using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CommonManager : MonoBehaviour
{
    [Header("7つのパネルを割り当て")]
    [SerializeField] private GameObject TitlePanel;
    [SerializeField]private GameObject TitlePanellogin;
    [SerializeField] private GameObject ParentPassResetPanel;
    [SerializeField] private GameObject ParentPassSetPanel;
    [SerializeField] private GameObject ParentAuthPanel;
    [SerializeField] private GameObject RegisterPanel;
    [SerializeField] private GameObject LoginPanel;
    [SerializeField] private GameObject AccountPanel;
    [SerializeField] private GameObject SettingPanel;

    [SerializeField] private GameObject LogoutConfirmDialog;
    [SerializeField] private GameObject PasswordChangeDialog;

    [SerializeField] private GameObject PasswordResetDialog;



    private void Start()
    {
        // ゲーム開始時はホーム画面（SRP-02）を表示

        ShowPanel(TitlePanel);
    }

    // パネル切り替え用（他を全部隠して指定した1枚だけ出す）
    public void ShowPanel(GameObject targetPanel)
    {
        TitlePanel.SetActive(false);
        TitlePanellogin.SetActive(false);
        ParentPassResetPanel.SetActive(false);
        ParentPassSetPanel.SetActive(false);
        ParentAuthPanel.SetActive(false);
        RegisterPanel.SetActive(false);
        LoginPanel.SetActive(false);
        AccountPanel.SetActive(false);
        SettingPanel.SetActive(false);
        LogoutConfirmDialog.SetActive(false);
        PasswordChangeDialog.SetActive(false);
        PasswordResetDialog.SetActive(false);

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