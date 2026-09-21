using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通过 Slider 控制全局 <see cref="Time.timeScale"/>。
/// </summary>
public class TimeScaleSlider : MonoBehaviour
{
    [SerializeField] private Slider m_slider;
    [SerializeField] private TMP_Text m_valueLabel;
    [SerializeField] private float m_minTimeScale = 0f;
    [SerializeField] private float m_maxTimeScale = 2f;
    [SerializeField] private float m_defaultTimeScale = 1f;
    [SerializeField] private string m_numberFormat = "0.##";
    [SerializeField] private bool m_applyOnAwake = true;
    [SerializeField] private bool m_resetOnDestroy = true;
    [SerializeField] private bool m_scaleFixedDeltaTime = true;

    private const float DefaultFixedDeltaTime = 0.02f;
    private float m_baseFixedDeltaTime = DefaultFixedDeltaTime;

    public float CurrentTimeScale => Time.timeScale;

    private void Reset()
    {
        m_slider = GetComponent<Slider>();
        if (m_slider == null)
        {
            m_slider = GetComponentInChildren<Slider>(true);
        }
    }

    private void Awake()
    {
        m_slider ??= GetComponent<Slider>();
        if (m_slider == null)
        {
            m_slider = GetComponentInChildren<Slider>(true);
        }

        if (m_slider == null)
        {
            Debug.LogError($"{nameof(TimeScaleSlider)}: 未找到 Slider，脚本已禁用。", this);
            enabled = false;
            return;
        }

        m_baseFixedDeltaTime = Time.fixedDeltaTime;

        ApplySliderSettings();
        m_slider.onValueChanged.AddListener(OnSliderValueChanged);

        if (m_applyOnAwake)
        {
            SetTimeScale(m_defaultTimeScale);
        }
        else
        {
            SyncUiFromTimeScale(Time.timeScale);
        }
    }

    private void OnDestroy()
    {
        if (m_slider != null)
        {
            m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        if (m_resetOnDestroy)
        {
            ApplyTimeScale(1f);
        }
    }

    public void SetRange(float minTimeScale, float maxTimeScale)
    {
        m_minTimeScale = Mathf.Max(0f, Mathf.Min(minTimeScale, maxTimeScale));
        m_maxTimeScale = Mathf.Max(m_minTimeScale, Mathf.Max(minTimeScale, maxTimeScale));
        ApplySliderSettings();
        SetTimeScale(CurrentTimeScale);
    }

    public void SetTimeScale(float timeScale)
    {
        var clamped = ClampTimeScale(timeScale);
        if (m_slider != null)
        {
            m_slider.SetValueWithoutNotify(clamped);
        }

        ApplyTimeScale(clamped);
        SyncLabel(clamped);
    }

    public void ResetToDefault()
    {
        SetTimeScale(m_defaultTimeScale);
    }

    private void OnSliderValueChanged(float value)
    {
        var clamped = ClampTimeScale(value);
        ApplyTimeScale(clamped);
        SyncLabel(clamped);
    }

    private void ApplySliderSettings()
    {
        m_minTimeScale = Mathf.Max(0f, m_minTimeScale);
        m_maxTimeScale = Mathf.Max(m_minTimeScale, m_maxTimeScale);
        m_defaultTimeScale = ClampTimeScale(m_defaultTimeScale);

        if (m_slider == null)
        {
            return;
        }

        m_slider.minValue = m_minTimeScale;
        m_slider.maxValue = m_maxTimeScale;
        m_slider.wholeNumbers = false;
    }

    private void ApplyTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;

        if (m_scaleFixedDeltaTime)
        {
            // 保持物理步进与 timeScale 成比例，避免加速/减速时物理不稳定。
            Time.fixedDeltaTime = m_baseFixedDeltaTime * Mathf.Max(timeScale, 0.0001f);
        }
    }

    private float ClampTimeScale(float value)
    {
        return Mathf.Clamp(value, m_minTimeScale, m_maxTimeScale);
    }

    private void SyncUiFromTimeScale(float timeScale)
    {
        var clamped = ClampTimeScale(timeScale);
        if (m_slider != null)
        {
            m_slider.SetValueWithoutNotify(clamped);
        }

        SyncLabel(clamped);
    }

    private void SyncLabel(float timeScale)
    {
        if (m_valueLabel == null)
        {
            return;
        }

        m_valueLabel.SetText(timeScale.ToString(m_numberFormat));
    }
}
