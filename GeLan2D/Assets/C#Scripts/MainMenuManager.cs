using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // ���ڳ����л�
using UnityEngine.UI; // ���ڲ���UIͼƬ


public class MainMenuManager : MonoBehaviour
{
    [Header("��ť����")]
    public Button startButton;      // ��ʼ��Ϸ��ť
    public Button creditsButton;    // ����������ť
    public Button quitButton;       // �˳���Ϸ��ť

    [Header("����������ʾ")]
    public Image creditsImage;

    // �����������ͼƬ��һ��Image�����
    public float fadeDuration = 0.5f; // �������뵭��ʱ��

    private bool isCreditsShowing = false; // ��ֹ�ظ����

    void Start()
    {
        // 1. �󶨰�ť����¼�
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        
        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnCreditsClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // 2. ��ʼ����������Ϊ����״̬
        if (creditsImage != null)
        {
            Color color = creditsImage.color;
            color.a = 0; // ��ȫ͸��
            creditsImage.color = color;
            creditsImage.raycastTarget = false; // ͸��ʱ�����ܵ��
            creditsImage.gameObject.SetActive(false); // �Ƚ��ã����ܸ���
        }

        Debug.Log("���˵���ʼ����ɡ�");
    }

    // ��ʼ��Ϸ��ť
    void OnStartClicked()
    {
        Debug.Log("����ˡ���ʼ��Ϸ��");
        // ���������ĵ�һ����Ϸ������������Ҫ����Build Settings���һ��
        SceneManager.LoadScene("��ʼ"); // �뽫"Room1"�ĳ����һ������ĳ�����
    }

    // ����������ť
    void OnCreditsClicked()
    {
        if (isCreditsShowing || creditsImage == null) return;
        
        Debug.Log("����ˡ�����������");
        ShowCredits(true);
    }

    // �˳���Ϸ��ť
    void OnQuitClicked()
    {
        Debug.Log("����ˡ��˳���Ϸ��");
        
        // ��Unity�༭�����ֹͣ���У��ڴ�������Ϸ����˳�Ӧ��
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // ���ģ���ʾ/�������������������׵���Ч����
    void ShowCredits(bool show)
    {
        isCreditsShowing = true;
        creditsImage.gameObject.SetActive(true);
        // �ȼ�������
        
        // Ŀ��͸����
        float targetAlpha = show ? 1f : 0f;
        Color currentColor = creditsImage.color;
        currentColor.a = targetAlpha;
        
        // ����ʹ�ü򵥵�ֱ�����ã������Ҫ��ƽ���ĵ��뵭��������Э�̣��Ը���һ�㣩
        creditsImage.color = currentColor;
        creditsImage.raycastTarget = show; // ��ʾʱ���ܱ�����ر�
        
        if (show)
        {
            // ��������ʾʱ������������⴦�ر���
            // ע�⣺��Ҫ��creditsImage���ڵ�GameObject���һ��Button��������յ��
            Button creditsBtn = creditsImage.GetComponent<Button>();
            if (creditsBtn == null) creditsBtn = creditsImage.gameObject.AddComponent<Button>();
            creditsBtn.onClick.RemoveAllListeners();
            creditsBtn.onClick.AddListener(() => ShowCredits(false));
        }
        
        isCreditsShowing = !show; // ��������������״̬
    }
}