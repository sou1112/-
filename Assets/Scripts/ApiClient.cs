using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    public static ApiClient Instance { get; private set; }

    private const string BaseUrl = "http://localhost:8080/api";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator GetRequest(string endpoint, Action<string> onSuccess, Action<string> onError = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(BaseUrl + endpoint))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[GETエラー] {endpoint}: {request.error}");
                onError?.Invoke(request.error);
            }
        }
    }

    public IEnumerator PostRequest(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onError = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(BaseUrl + endpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[POSTエラー] {endpoint}: {request.error}");
                onError?.Invoke(request.error);
            }
        }
    }
}