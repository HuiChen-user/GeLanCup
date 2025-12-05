using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 用于场景切换
using UnityEngine.UI; // 用于操作UI图片


public class MainMenuManager : MonoBehaviour
{
    [Header("按钮设置")]
    public Button startButton;      // 开始游戏按钮
    public Button creditsButton;    // 制作名单按钮
    public Button quitButton;       // 退出游戏按钮

    [Header("制作名单显示")]
    public Image creditsImage;      // 你的制作名单图片（一个Image组件）
    public float fadeDuration = 0.5f; // 名单淡入淡出时间

    private bool isCreditsShowing = false; // 防止重复点击

    void Start()
    {
        // 1. 绑定按钮点击事件
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        
        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnCreditsClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // 2. 初始化制作名单为隐藏状态
        if (creditsImage != null)
        {
            Color color = creditsImage.color;
            color.a = 0; // 完全透明
            creditsImage.color = color;
            creditsImage.raycastTarget = false; // 透明时不接受点击
            creditsImage.gameObject.SetActive(false); // 先禁用，性能更好
        }

        Debug.Log("主菜单初始化完成。");
    }

    // 开始游戏按钮
    void OnStartClicked()
    {
        Debug.Log("点击了【开始游戏】");
        // 这里加载你的第一个游戏场景，名字需要和你Build Settings里的一致
        SceneManager.LoadScene("开始"); // 请将"Room1"改成你第一个房间的场景名
    }

    // 制作名单按钮
    void OnCreditsClicked()
    {
        if (isCreditsShowing || creditsImage == null) return;
        
        Debug.Log("点击了【制作名单】");
        ShowCredits(true);
    }

    // 退出游戏按钮
    void OnQuitClicked()
    {
        Debug.Log("点击了【退出游戏】");
        
        // 在Unity编辑器里会停止运行，在打包后的游戏里会退出应用
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // 核心：显示/隐藏制作名单（带简易淡入效果）
    void ShowCredits(bool show)
    {
        isCreditsShowing = true;
        creditsImage.gameObject.SetActive(true); // 先激活物体
        
        // 目标透明度
        float targetAlpha = show ? 1f : 0f;
        Color currentColor = creditsImage.color;
        currentColor.a = targetAlpha;
        
        // 这里使用简单的直接设置，如果需要更平滑的淡入淡出可以用协程（稍复杂一点）
        creditsImage.color = currentColor;
        creditsImage.raycastTarget = show; // 显示时才能被点击关闭
        
        if (show)
        {
            // 当名单显示时，点击名单任意处关闭它
            // 注意：需要给creditsImage所在的GameObject添加一个Button组件来接收点击
            Button creditsBtn = creditsImage.GetComponent<Button>();
            if (creditsBtn == null) creditsBtn = creditsImage.gameObject.AddComponent<Button>();
            creditsBtn.onClick.RemoveAllListeners();
            creditsBtn.onClick.AddListener(() => ShowCredits(false));
        }
        
        isCreditsShowing = !show; // 操作结束后重置状态
    }
}