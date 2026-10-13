using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// UI_VolumeSlider（音量滑块UI）- 控制音频混音器的音量参数
/// 功能：音量调节、加载保存的音量值
/// </summary>
public class UI_VolumeSlider : MonoBehaviour
{
    // ==================== 核心组件 ====================
    public Slider slider;
    public string parametr;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float multiplier;

    // ==================== 音量调节 ====================
    // 使用对数刻度设置混音器参数
    public void SliderValue(float _value) => audioMixer.SetFloat(parametr, Mathf.Log10(_value) * multiplier);

    // ==================== 加载音量 ====================
    public void LoadSlider(float _value)
    {
        if (_value >= 0.001f)
        {
            slider.value = _value;
            SliderValue(_value);
        }
    }
}
