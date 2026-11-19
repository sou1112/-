using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CommonUIManager : MonoBehaviour
{
    [Header("1. メインパネル（8画面）")]
    public GameObject titlePanel;
    public GameObject registerPanel;
    public GameObject loginPanel;
    public GameObject accountPanel;
    public GameObject settingPanel;
    public GameObject parentAuthPanel;      // 保護者パスワード認証
    public GameObject parentPassSetPanel;   // 保護者パスワード初回設定
    public GameObject parentPassResetPanel; // 保護者パスワード再設定

    [Header("2. サブダイアログ（モーダル）")]
    public GameObject passwordResetDialog;  // ログイン画面：パスワード忘れた
    public GameObject passwordChangeDialog; // アカウント画面：パスワード変更
    public GameObject logoutConfirmDialog;  // アカウント画面：ログアウト確認

    [Header("3. タイトル画面の切り替えボタン")]
    public GameObject registerLoginButton;
    public GameObject accountButton;

    [Header("4. 保護者認証の入力欄（任意）")]
    public TMP_InputField parentAuthInput;
    public TMP_InputField parentSetInput;

    // 仮の状態保持（DB連携前のデモ用）
    public static bool isLoggedIn = false;
    public static bool hasParentPassword = false; // 最初は未設定（初回設定画面へ行く）
    public static string savedParentPassword = "1234";

    void Start()
    {
        ShowTitlePanel();
    }

    private void HideAllPanels()
    {
        if (titlePanel != null) titlePanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (accountPanel != null) accountPanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);
        if (parentAuthPanel != null) parentAuthPanel.SetActive(false);
        if (parentPassSetPanel != null) parentPassSetPanel.SetActive(false);
        if (parentPassResetPanel != null) parentPassResetPanel.SetActive(false);

        if (passwordResetDialog != null) passwordResetDialog.SetActive(false);
        if (passwordChangeDialog != null) passwordChangeDialog.SetActive(false);
        if (logoutConfirmDialog != null) logoutConfirmDialog.SetActive(false);
    }

    // ==========================================
    // 1. 各パネルへの画面切り替え（戻るボタンも共通）
    // ==========================================
    public void ShowTitlePanel()
    {
        HideAllPanels();
        if (titlePanel != null) titlePanel.SetActive(true);
        UpdateTitleAuthButtons();
    }

    public void ShowRegisterPanel()
    {
        HideAllPanels();
        if (registerPanel != null) registerPanel.SetActive(true);
    }

    public void ShowLoginPanel()
    {
        HideAllPanels();
        if (loginPanel != null) loginPanel.SetActive(true);
    }

    public void ShowAccountPanel()
    {
        HideAllPanels();
        if (accountPanel != null) accountPanel.SetActive(true);
    }

    public void ShowSettingPanel()
    {
        HideAllPanels();
        if (settingPanel != null) settingPanel.SetActive(true);
    }

    // ==========================================
    // 2. 保護者認証フロー（3画面の連携）
    // ==========================================
    // タイトルの「保護者専用機能」ボタン押下時
    public void OnClickParentModeButton()
    {
        HideAllPanels();
        if (hasParentPassword)
        {
            if (parentAuthPanel != null) parentAuthPanel.SetActive(true);
        }
        else
        {
            if (parentPassSetPanel != null) parentPassSetPanel.SetActive(true);
        }
    }

    // 初回パスワード設定画面（ParentPassSetPanel）の「決定・登録」ボタン
    public void OnClickConfirmParentPassSet()
    {
        if (parentSetInput != null && !string.IsNullOrEmpty(parentSetInput.text))
        {
            savedParentPassword = parentSetInput.text;
        }
        hasParentPassword = true;
        // 設定完了後、そのまま認証画面（または保護者シーン）へ進む
        HideAllPanels();
        if (parentAuthPanel != null) parentAuthPanel.SetActive(true);
    }

    // 保護者認証画面（ParentAuthPanel）の「認証・決定」ボタン
    public void OnClickSubmitParentAuth()
    {
        // 中間発表デモ用：未入力でも通すか、入力があれば一致チェック
        if (parentAuthInput == null || string.IsNullOrEmpty(parentAuthInput.text) || parentAuthInput.text == savedParentPassword)
        {
            SceneManager.LoadScene("ParentScene");
        }
        else
        {
            Debug.Log("パスワードが違います（仮パスワード: " + savedParentPassword + "）");
        }
    }

    // 保護者認証画面の「パスワードを忘れた場合」リンク
    public void ShowParentPassResetPanel()
    {
        HideAllPanels();
        if (parentPassResetPanel != null) parentPassResetPanel.SetActive(true);
    }

    // 保護者パスワード再設定画面（ParentPassResetPanel）の「送信・再設定完了」ボタン
    public void OnClickCompleteParentPassReset()
    {
        hasParentPassword = false; // 再設定後は初回設定画面に戻す
        OnClickParentModeButton();
    }

    // ==========================================
    // 3. 新規登録・ログイン・ダイアログ制御
    // ==========================================
    public void OnClickDummyAuthSuccess()
    {
        isLoggedIn = true;
        ShowTitlePanel();
    }

    public void OnClickLogout()
    {
        isLoggedIn = false;
        ShowTitlePanel();
    }

    private void UpdateTitleAuthButtons()
    {
        if (registerLoginButton != null) registerLoginButton.SetActive(!isLoggedIn);
        if (accountButton != null) accountButton.SetActive(isLoggedIn);
    }

    public void OpenPasswordResetDialog() { if (passwordResetDialog != null) passwordResetDialog.SetActive(true); }
    public void ClosePasswordResetDialog() { if (passwordResetDialog != null) passwordResetDialog.SetActive(false); }

    public void OpenPasswordChangeDialog() { if (passwordChangeDialog != null) passwordChangeDialog.SetActive(true); }
    public void ClosePasswordChangeDialog() { if (passwordChangeDialog != null) passwordChangeDialog.SetActive(false); }

    public void OpenLogoutConfirmDialog() { if (logoutConfirmDialog != null) logoutConfirmDialog.SetActive(true); }
    public void CloseLogoutConfirmDialog() { if (logoutConfirmDialog != null) logoutConfirmDialog.SetActive(false); }

    // ==========================================
    // 4. 子供シーンへの遷移
    // ==========================================
    public void GoToChildScene()
    {
        SceneManager.LoadScene("ChildScene");
    }
}