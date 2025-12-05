using UnityEngine;

public class FlickerLight2DManager : MonoBehaviour
{
    public static FlickerLight2DManager Instance;

    [Header("全局闪烁设置")]
    public float flickerInterval = 0.1f;
    public bool smooth = false;

    private float timer;
    private float targetValue;      // 0~1 的同步随机值

    void Awake()
    {
        Instance = this;
        targetValue = 1f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 每隔 flickerInterval 生成一个新的 0~1 的随机闪烁值
        if (timer >= flickerInterval)
        {
            targetValue = Random.Range(0f, 1f);
            timer = 0f;
        }
    }

    // 获取同步闪烁值（给所有灯用）
    public float GetSyncValue()
    {
        if (smooth)
        {
            // 平滑变化（类似呼吸效果）
            return Mathf.Lerp(targetValue, Random.Range(0f, 1f), Time.deltaTime * 5f);
        }

        return targetValue; // 直接跳变
    }
}


