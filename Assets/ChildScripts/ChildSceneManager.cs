using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
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


    private void Start()
    {
        // ゲーム開始時はホーム画面（SRP-02）を表示

        ShowPanel(Tobirasentaku);
    }

    // パネル切り替え用（他を全部隠して指定した1枚だけ出す）
    public void ShowPanel(GameObject targetPanel)    {
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
    public void OpenDialog(GameObject dialogPanel)    {
        if (dialogPanel != null) dialogPanel.SetActive(true);
    }

    public void CloseDialog(GameObject dialogPanel)
{
    if (dialogPanel != null) dialogPanel.SetActive(false);
}

// シーン移動用（共通シーンへ戻る）
public void LoadSceneByName(string sceneName)    {
SceneManager.LoadScene(sceneName);
    }
}