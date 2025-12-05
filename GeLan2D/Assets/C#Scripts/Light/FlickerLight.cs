using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class FlickerLight : MonoBehaviour
{
    [Header("亮度范围")]
    public float minIntensity = 0.2f;   // 最暗亮度
    public float maxIntensity = 1.0f;   // 最亮亮度

    [Header("闪烁设置")]
    public float flickerInterval = 0.1f; // 闪烁间隔（秒）
    
    [Tooltip("是否使用平滑过渡（否则为随机跳动）")]
    public bool smooth = false;

    private Light2D _light2D;
    private float timer;
    private float targetIntensity;

    private void Awake()
    {
        _light2D = GetComponent<Light2D>();
        targetIntensity = _light2D.intensity;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (!smooth)
        {
            // --- 随机闪烁（跳动式） ---
            if (timer >= flickerInterval)
            {
                _light2D.intensity = Random.Range(minIntensity, maxIntensity);
                timer = 0f;
            }
        }
        else
        {
            // --- 平滑闪烁（呼吸式） ---
            if (timer >= flickerInterval)
            {
                targetIntensity = Random.Range(minIntensity, maxIntensity);
                timer = 0f;
            }

            _light2D.intensity = Mathf.Lerp(_light2D.intensity, targetIntensity, Time.deltaTime * 6f);
        }
    }
}
