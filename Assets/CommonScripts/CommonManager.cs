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

    // ==========================================
    // フィールド欄（[Header("API連携用：遷移先シーン名")] の下）に追加
    // ==========================================
    [Header("API連携用：新規登録入力UI")]
    [SerializeField] private TMP_InputField regEmailInput;
    [SerializeField] private TMP_InputField regPasswordInput;
    [SerializeField] private TMP_InputField regConfirmPasswordInput;
    [SerializeField] private TMP_Text regErrorText;

    // ==========================================
    // フィールド欄（[Header("API連携用：新規登録入力UI")] の下あたり）に追加
    // ==========================================
    [Header("API連携用：アカウント画面表示UI（任意）")]
    [SerializeField] private TMP_Text accountUserIdText;
    [SerializeField] private TMP_Text accountEmailText;
    [SerializeField] private TMP_Text accountChildNameText;

    [Header("API連携用：パスワード変更ダイアログUI")]
    [SerializeField] private TMP_InputField currentPassInput;
    [SerializeField] private TMP_InputField newPassInput;
    [SerializeField] private TMP_InputField confirmNewPassInput;
    [SerializeField] private TMP_Text passChangeErrorText;

    // ==========================================
    // フィールド欄に追加
    // ==========================================
    [Header("API連携用：保護者パスワード初回設定UI")]
    [SerializeField] private TMP_InputField setParentPassInput;
    [SerializeField] private TMP_InputField setParentPassConfirmInput;
    [SerializeField] private TMP_Text setParentPassErrorText; // 任意（あれば割り当て）

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

    // ==========================================
    // メソッド欄（OnClickLogout の下）に追加
    // ==========================================

    // RegisterPanel の「登録」ボタンの OnClick に設定
    public void OnClickRegister()
    {
        if (regErrorText != null) regErrorText.text = "";

        if (regEmailInput == null || regPasswordInput == null || regConfirmPasswordInput == null)
        {
            Debug.LogError("CommonSceneManager の Inspector で新規登録用の3つの入力欄が未割り当てです");
            return;
        }

        string email = regEmailInput.text.Trim();
        string password = regPasswordInput.text.Trim();
        string confirmPassword = regConfirmPasswordInput.text.Trim();

        // 入力チェック
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            if (regErrorText != null) regErrorText.text = "メールアドレスとパスワードを入力してください";
            return;
        }

        // パスワード一致チェック
        if (password != confirmPassword)
        {
            if (regErrorText != null) regErrorText.text = "確認用パスワードが一致しません";
            return;
        }

        RegisterRequest req = new RegisterRequest
        {
            email = email,
            password = password
        };

        string json = JsonUtility.ToJson(req);

        StartCoroutine(ApiClient.Instance.PostRequest(
            "/register",
            json,
            onSuccess: (responseJson) =>
            {
                LoginResponse res = JsonUtility.FromJson<LoginResponse>(responseJson);
                if (res != null && res.success)
                {
                    Debug.Log($"新規登録成功: userId={res.userId}");

                    // 1. 登録画面の入力欄をクリアしておく
                    regEmailInput.text = "";
                    regPasswordInput.text = "";
                    regConfirmPasswordInput.text = "";

                    // 2. ログイン画面のメールアドレス欄に、今登録したアドレスを自動入力
                    if (emailInput != null) emailInput.text = email;
                    if (passwordInput != null) passwordInput.text = "";

                    // 3. ログイン画面へ移動し、完了メッセージを表示する
                    ShowPanel(LoginPanel);
                    if (loginErrorText != null)
                    {
                        loginErrorText.text = "<color=#2E7D32>新規登録が完了しました！\nパスワードを入力してログインしてください。</color>";
                    }
                }
                else
                {
                    if (regErrorText != null) regErrorText.text = "登録に失敗しました";
                }
            },
            onError: (err) =>
            {
                if (regErrorText != null) regErrorText.text = "このメールアドレスは既に登録されているか、通信エラーです";
            }
        ));
    }

    // ==========================================
    // メソッド欄（OnClickRegister の下あたり）に追加
    // ==========================================

    // TitlePanelAfterLogin の「アカウント」ボタンの OnClick に設定
    // （既存の ShowPanel(AccountPanel) の代わりにこれを呼ぶと情報取得も同時に行います）
    public void OpenAccountPanel()
    {
        ShowPanel(AccountPanel);

        if (string.IsNullOrEmpty(UserSession.UserId)) return;

        if (accountUserIdText != null) accountUserIdText.text = UserSession.UserId;
        if (accountChildNameText != null) accountChildNameText.text = UserSession.ChildName;

        // DBから最新のメールアドレス・子供の名前を取得して反映
        StartCoroutine(ApiClient.Instance.GetRequest(
            $"/user?userId={UserSession.UserId}",
            onSuccess: (json) =>
            {
                UserInfoResponse info = JsonUtility.FromJson<UserInfoResponse>(json);
                if (info != null)
                {
                    if (accountUserIdText != null) accountUserIdText.text = info.userId;
                    if (accountEmailText != null) accountEmailText.text = info.email;
                    if (accountChildNameText != null) accountChildNameText.text = info.childName;
                }
            },
            onError: (err) => Debug.LogError($"ユーザー情報取得エラー: {err}")
        ));
    }

    // AccountPanel の「変更」ボタンの OnClick に設定（入力欄を空にしてダイアログを開く）
    public void OpenPasswordChangeDialog()
    {
        if (currentPassInput != null) currentPassInput.text = "";
        if (newPassInput != null) newPassInput.text = "";
        if (confirmNewPassInput != null) confirmNewPassInput.text = "";
        if (passChangeErrorText != null) passChangeErrorText.text = "";

        OpenDialog(PasswordChangeDialog);
    }

    // PasswordChangeDialog の「変更」ボタンの OnClick に設定
    public void OnClickChangePassword()
    {
        if (passChangeErrorText != null) passChangeErrorText.text = "";

        if (currentPassInput == null || newPassInput == null || confirmNewPassInput == null)
        {
            Debug.LogError("CommonSceneManager の Inspector でパスワード変更用の3つの入力欄が未割り当てです");
            return;
        }

        string currentPass = currentPassInput.text.Trim();
        string newPass = newPassInput.text.Trim();
        string confirmPass = confirmNewPassInput.text.Trim();

        // 1. 未入力チェック
        if (string.IsNullOrEmpty(currentPass) || string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirmPass))
        {
            if (passChangeErrorText != null) passChangeErrorText.text = "すべての項目を入力してください";
            return;
        }

        // 2. 新しいパスワードの一致チェック
        if (newPass != confirmPass)
        {
            if (passChangeErrorText != null) passChangeErrorText.text = "新しいパスワード（確認用）が一致しません";
            return;
        }

        // 3. 同じパスワードへの変更防止チェック
        if (currentPass == newPass)
        {
            if (passChangeErrorText != null) passChangeErrorText.text = "現在と異なる新しいパスワードを入力してください";
            return;
        }

        PasswordChangeRequest req = new PasswordChangeRequest
        {
            userId = UserSession.UserId,
            currentPassword = currentPass,
            newPassword = newPass
        };

        string json = JsonUtility.ToJson(req);

        StartCoroutine(ApiClient.Instance.PostRequest(
            "/change-password",
            json,
            onSuccess: (responseJson) =>
            {
                Debug.Log("パスワード変更成功");
                CloseDialog(PasswordChangeDialog);
            },
            onError: (err) =>
            {
                if (passChangeErrorText != null)
                {
                    passChangeErrorText.text = "現在のパスワードが正しくありません";
                }
            }
        ));
    }

    // ==========================================
    // メソッド欄に追加（既存の UserInfoResponse クラスも差し替え）
    // ==========================================

    // TitlePanelAfterLogin の「保護者専用機能」ボタンの OnClick に設定
    public void OnClickParentFeatureButton()
    {
        if (string.IsNullOrEmpty(UserSession.UserId)) return;

        StartCoroutine(ApiClient.Instance.GetRequest(
            $"/user?userId={UserSession.UserId}",
            onSuccess: (json) =>
            {
                UserInfoResponse info = JsonUtility.FromJson<UserInfoResponse>(json);
                if (info != null && info.hasParentPassword)
                {
                    // 設定済みなら通常の保護者パスワード入力画面へ
                    ShowPanel(ParentAuthPanel);
                }
                else
                {
                    // 未設定（null）なら保護者パスワード設定画面へ
                    if (setParentPassInput != null) setParentPassInput.text = "";
                    if (setParentPassConfirmInput != null) setParentPassConfirmInput.text = "";
                    if (setParentPassErrorText != null) setParentPassErrorText.text = "";
                    ShowPanel(ParentPassSetPanel);
                }
            },
            onError: (err) => Debug.LogError($"保護者パス確認エラー: {err}")
        ));
    }

    // ParentPassSetPanel の「設定」ボタンの OnClick に設定
    public void OnClickSetParentPassword()
    {
        if (setParentPassErrorText != null) setParentPassErrorText.text = "";

        if (setParentPassInput == null || setParentPassConfirmInput == null)
        {
            Debug.LogError("CommonSceneManager の Inspector で保護者パスワード設定用の入力欄が未割り当てです");
            return;
        }

        string pass = setParentPassInput.text.Trim();
        string confirm = setParentPassConfirmInput.text.Trim();

        if (string.IsNullOrEmpty(pass))
        {
            if (setParentPassErrorText != null) setParentPassErrorText.text = "パスワードを入力してください";
            return;
        }

        if (pass != confirm)
        {
            if (setParentPassErrorText != null) setParentPassErrorText.text = "パスワード確認が一致しません";
            return;
        }

        ParentPassSetRequest req = new ParentPassSetRequest
        {
            userId = UserSession.UserId,
            parentPassword = pass
        };

        string json = JsonUtility.ToJson(req);

        StartCoroutine(ApiClient.Instance.PostRequest(
            "/parent-password",
            json,
            onSuccess: (responseJson) =>
            {
                Debug.Log("保護者パスワード初回設定完了");
                // 設定完了後、そのまま保護者シーンへ移動（認証画面に戻す場合は ShowPanel(ParentAuthPanel) に変更可）
                SceneManager.LoadScene(parentSceneName);
            },
            onError: (err) =>
            {
                if (setParentPassErrorText != null) setParentPassErrorText.text = "設定の保存に失敗しました";
            }
        ));
    }

    [System.Serializable]
    private class UserInfoResponse
    {
        public string userId;
        public string email;
        public string childName;
        public bool hasParentPassword;
    }

    [System.Serializable]
    private class ParentPassSetRequest
    {
        public string userId;
        public string parentPassword;
    }

    [System.Serializable]
    private class PasswordChangeRequest
    {
        public string userId;
        public string currentPassword;
        public string newPassword;
    }

    [System.Serializable]
    private class RegisterRequest
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    private class ParentAuthRequest
    {
        public string userId;
        public string parentPassword;
    }
}