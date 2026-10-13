using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AudioManager（音频管理器）- 管理音效和背景音乐的播放
/// 功能：SFX播放（距离检测）、BGM管理、音量渐变
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ==================== 单例 ====================
    public static AudioManager instance;

    // ==================== 音频设置 ====================
    [SerializeField] private float sfxMinimumDistance;
    [SerializeField] private AudioSource[] sfx;
    [SerializeField] private AudioSource[] bgm;

    // ==================== BGM状态 ====================
    public bool playBgm;
    private int bgmIndex;
    private bool canPlaySFX;

    // ==================== 单例初始化 ====================
    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
        Invoke("AllowSFX", 1f);
    }

    // ==================== BGM更新 ====================
    private void Update()
    {
        if (!playBgm)
            StopAllBGM();
        else
        {
            if (!bgm[bgmIndex].isPlaying)
                PlayBGM(bgmIndex);
        }
    }

    // ==================== SFX播放 ====================
    public void PlaySFX(int _sfxIndex, Transform _source)
    {
        if (canPlaySFX == false)
            return;

        if (_source != null && Vector2.Distance(PlayerManager.instance.player.transform.position, _source.position) > sfxMinimumDistance)
            return;

        if (_sfxIndex < sfx.Length)
        {
            sfx[_sfxIndex].pitch = Random.Range(.85f, 1.15f);
            sfx[_sfxIndex].Play();
        }
    }

    public void StopSFX(int _index) => sfx[_index].Stop();

    // ==================== SFX渐变停止 ====================
    public void StopSFXWithTime(int _index)
    {
        if (_index < 0 || _index >= sfx.Length)
            return;

        if (sfx[_index] != null && sfx[_index].isPlaying)
            StartCoroutine(DecreaseVolume(sfx[_index]));
    }

    private IEnumerator DecreaseVolume(AudioSource _audio)
    {
        if (_audio == null)
            yield break;

        float defaultVolume = _audio.volume;
        float startVolume = _audio.volume;
        float fadeDuration = 1.5f;
        float startTime = Time.time;

        while (_audio != null && Time.time < startTime + fadeDuration)
        {
            if (_audio == null)
                yield break;

            float t = (Time.time - startTime) / fadeDuration;
            _audio.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        if (_audio != null)
        {
            _audio.volume = 0f;
            _audio.Stop();
            _audio.volume = defaultVolume;
        }
    }

    // ==================== BGM控制 ====================
    public void PlayRandomBGM()
    {
        bgmIndex = Random.Range(0, bgm.Length);
        PlayBGM(bgmIndex);
    }

    public void PlayBGM(int _bgmIndex)
    {
        bgmIndex = _bgmIndex;
        StopAllBGM();
        bgm[bgmIndex].Play();
    }

    public void StopAllBGM()
    {
        for (int i = 0; i < bgm.Length; i++)
            bgm[i].Stop();
    }

    private void AllowSFX() => canPlaySFX = true;
}
