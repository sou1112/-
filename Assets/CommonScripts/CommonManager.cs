using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CommonManager : MonoBehaviour
{
    [Header("7つのパネルを割り当て")]
    [SerializeField] private GameObject TitlePanel;
    [SerializeField] private GameObject TitlePanellogin;
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

    [Header("API連携用：ログイン入力UI")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text loginErrorText;

    [Header("API連携用：保護者パスワード入力UI")]
    [SerializeField] private TMP_InputField parentPasswordInput;
    [SerializeField] private TMP_Text parentErrorText;

    [Header("API連携用：遷移先シーン名")]
    [SerializeField] private string parentSceneName = "ParentScene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionOnPlay()
    {
        UserSession.UserId = null;
        UserSession.ChildName = null;
    }

    private void Start()
    {
        // ログイン済みで共通シーンに戻ってきた場合はログイン後タイトル、未ログインなら通常タイトルを表示
        if (!string.IsNullOrEmpty(UserSession.UserId))
        {
            ShowPanel(TitlePanellogin);
        }
        else
        {
            ShowPanel(TitlePanel);
        }
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

    // シーン移動用（共通シーンへ戻る、または空白タップで子供シーンへ移動）
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ==========================================
    // ここから API 連携メソッド
    // ==========================================

    // LoginPanel の「ログイン」ボタンの OnClick に設定
    public void OnClickLogin()
    {
        if (loginErrorText != null) loginErrorText.text = "";

        if (emailInput == null || passwordInput == null)
        {
            Debug.LogError("CommonSceneManager の Inspector で Email Input / Password Input が未割り当てです");
            return;
        }

        LoginRequest req = new LoginRequest
        {
            email = emailInput.text.Trim(),
            password = passwordInput.text.Trim()
        };

        string json = JsonUtility.ToJson(req);

        StartCoroutine(ApiClient.Instance.PostRequest(
            "/login",
            json,
            onSuccess: (responseJson) =>
            {
                LoginResponse res = JsonUtility.FromJson<LoginResponse>(responseJson);
                if (res != null && res.success)
                {
                    UserSession.UserId = res.userId;
                    UserSession.ChildName = res.childName;

                    Debug.Log($"ログイン成功: userId={UserSession.UserId}, childName={UserSession.ChildName}");
                    ShowPanel(TitlePanellogin);
                }
                else
                {
                    if (loginErrorText != null) loginErrorText.text = "ログインに失敗しました";
                }
            },
            onError: (err) =>
            {
                if (loginErrorText != null) loginErrorText.text = "メールアドレスまたはパスワードが違います";
            }
        ));
    }

    // ParentAuthPanel の「決定（認証）」ボタンの OnClick に設定
    public void OnClickParentAuth()
    {
        if (parentErrorText != null) parentErrorText.text = "";

        if (parentPasswordInput == null)
        {
            Debug.LogError("CommonSceneManager の Inspector で Parent Password Input が未割り当てです");
            return;
        }

        ParentAuthRequest req = new ParentAuthRequest
        {
            userId = UserSession.UserId,
            parentPassword = parentPasswordInput.text.Trim()
        };

        string json = JsonUtility.ToJson(req);

        StartCoroutine(ApiClient.Instance.PostRequest(
            "/parent-auth",
            json,
            onSuccess: (responseJson) =>
            {
                Debug.Log("保護者認証成功");
                LoadSceneByName(parentSceneName);
            },
            onError: (err) =>
            {
                if (parentErrorText != null) parentErrorText.text = "保護者パスワードが違います";
            }
        ));
    }

    // LogoutConfirmDialog の「はい（ログアウト）」ボタンの OnClick に設定
    public void OnClickLogout()
    {
        UserSession.UserId = null;
        UserSession.ChildName = null;
        ShowPanel(TitlePanel);
    }

    [System.Serializable]
    private class ParentAuthRequest
    {
        public string userId;
        public string parentPassword;
    }
}