using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private float sfxMinimumDistance;
    [SerializeField] private AudioSource[] sfx;//音效
    [SerializeField] private AudioSource[] bgm;//背景音乐


    public bool playBgm;
    private int bgmIndex;
    private bool canPlaySFX;


    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
        Invoke("AllowSFX", 1f);//延迟1秒允许播放音效
    }


    private void Update()
    {
        if (!playBgm)
            StopAllBGM();
        else
        {
            if (!bgm[bgmIndex].isPlaying)//如果背景音乐没有播放
                PlayBGM(bgmIndex);
        }
    }


    public void PlaySFX(int _sfxIndex, Transform _source)//播放音效
    {
        // if (sfx[_sfxIndex].isPlaying)//如果音效正在播放
        //     return;

        if (canPlaySFX == false)//如果不能播放音效
        {
            return;
        }
        if (_source != null && Vector2.Distance(PlayerManager.instance.player.transform.position, _source.position) > sfxMinimumDistance)//如果音效源不为空并且玩家与音效源的距离大于最小距离
        {
            return;
        }

        if (_sfxIndex < sfx.Length)
        {
            sfx[_sfxIndex].pitch = Random.Range(.85f, 1.15f);//设置音效的音调
            sfx[_sfxIndex].Play();
        }
    }

    public void StopSFX(int _index) => sfx[_index].Stop();//停止音效

    public void StopSFXWithTime(int _index)
    {
        // 检查索引是否有效
        if (_index < 0 || _index >= sfx.Length)
            return;

        // 检查音频源是否存在且正在播放
        if (sfx[_index] != null && sfx[_index].isPlaying)
        {
            StartCoroutine(DecreaseVolume(sfx[_index]));
        }
    }

    private IEnumerator DecreaseVolume(AudioSource _audio)
    {
        // 检查音频源是否存在
        if (_audio == null)
            yield break;

        // 保存原始音量
        float defaultVolume = _audio.volume;
        float startVolume = _audio.volume;
        float fadeDuration = 1.5f;
        float startTime = Time.time;

        // 在指定时间内平滑降低音量
        while (_audio != null && Time.time < startTime + fadeDuration)
        {
            // 如果音频源被销毁，退出协程
            if (_audio == null)
                yield break;

            // 计算已经过去的时间比例
            float t = (Time.time - startTime) / fadeDuration;
            // 线性插值降低音量
            _audio.volume = Mathf.Lerp(startVolume, 0f, t);
            // 等待一帧
            yield return null;
        }

        // 再次检查音频源是否存在
        if (_audio != null)
        {
            // 确保音量为0并停止播放
            _audio.volume = 0f;
            _audio.Stop();
            // 恢复原始音量设置
            _audio.volume = defaultVolume;
        }
    }


    public void PlayRandomBGM()//播放随机背景音乐
    {
        bgmIndex = Random.Range(0, bgm.Length);
        PlayBGM(bgmIndex);
    }



    public void PlayBGM(int _bgmIndex)//播放背景音乐
    {
        bgmIndex = _bgmIndex;

        StopAllBGM();
        bgm[bgmIndex].Play();
    }


    public void StopAllBGM()
    {
        for (int i = 0; i < bgm.Length; i++)
        {
            bgm[i].Stop();
        }
    }
    private void AllowSFX() => canPlaySFX = true;//允许播放音效


}