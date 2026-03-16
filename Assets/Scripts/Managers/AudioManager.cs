using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton AudioManager - tự tạo âm thanh bằng code (không cần file âm thanh ngoài).
/// Đặt GameObject này vào scene đầu tiên (StartScene) và nó sẽ tồn tại suốt game.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.25f;
    [Range(0f, 1f)] public float sfxVolume = 0.75f;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    private bool _isMuted = false;

    // Procedural clips
    private AudioClip _correctWordClip;
    private AudioClip _gameOverClip;
    private AudioClip _winClip;
    private AudioClip _buttonClickClip;
    private AudioClip _bgMusicClip;
    private AudioClip _categoryCompleteClip;

    private const int SAMPLE_RATE = 44100;

    // ─── Lifecycle ──────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
            GenerateAllClips();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.OnCorrectWord    += HandleCorrectWord;
        GameEvents.OnGameOver       += HandleGameOver;
        GameEvents.OnBoardCompleted += HandleBoardCompleted;
        GameEvents.OnCategoryCompleted += HandleCategoryCompleted;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEvents.OnCorrectWord    -= HandleCorrectWord;
        GameEvents.OnGameOver       -= HandleGameOver;
        GameEvents.OnBoardCompleted -= HandleBoardCompleted;
        GameEvents.OnCategoryCompleted -= HandleCategoryCompleted;
    }

    // Khởi động lại nhạc nền mỗi khi chuyển scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_musicSource.isPlaying)
        {
            PlayBackgroundMusic();
        }
    }

    // ─── Setup ──────────────────────────────────────────────────────────────────

    private void SetupAudioSources()
    {
        _musicSource        = gameObject.AddComponent<AudioSource>();
        _musicSource.loop   = true;
        _musicSource.volume = musicVolume;
        _musicSource.priority = 256;

        _sfxSource        = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop   = false;
        _sfxSource.volume = sfxVolume;
        _sfxSource.priority = 128;
    }

    private void GenerateAllClips()
    {
        // Đúng từ: arpeggio đi lên vui vẻ  C5 → E5 → G5
        _correctWordClip = GenerateArpeggio(new[] { 523.25f, 659.25f, 783.99f }, 0.12f, 0.7f);

        // Game over: giai điệu đi xuống buồn
        _gameOverClip = GenerateArpeggio(new[] { 392f, 329.63f, 261.63f, 196f }, 0.2f, 0.55f);

        // Thắng màn: arpeggio lên rồi kết thúc cao
        _winClip = GenerateArpeggio(new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.15f, 0.75f);

        // Hoàn thành category: fanfare ngắn
        _categoryCompleteClip = GenerateFanfare();

        // Click nút: tiếng bật ngắn gọn
        _buttonClickClip = GenerateTick(900f, 0.04f, 0.5f);

        // Nhạc nền: giai điệu nhẹ nhàng lặp
        _bgMusicClip = GenerateBackgroundMusic();

        PlayBackgroundMusic();
    }

    // ─── Playback ───────────────────────────────────────────────────────────────

    private void PlayBackgroundMusic()
    {
        if (_bgMusicClip == null) return;
        _musicSource.clip = _bgMusicClip;
        _musicSource.mute = _isMuted;
        _musicSource.Play();
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || _isMuted) return;
        _sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayCorrectWord()      => PlaySFX(_correctWordClip);
    public void PlayGameOver()         => PlaySFX(_gameOverClip);
    public void PlayWin()              => PlaySFX(_winClip);
    public void PlayCategoryComplete() => PlaySFX(_categoryCompleteClip);
    public void PlayButtonClick()      => PlaySFX(_buttonClickClip);

    // ─── Mute Toggle ────────────────────────────────────────────────────────────

    public void ToggleMute()
    {
        _isMuted = !_isMuted;
        _musicSource.mute = _isMuted;
        _sfxSource.mute   = _isMuted;
    }

    public bool IsMuted => _isMuted;

    // ─── Event Handlers ─────────────────────────────────────────────────────────

    private void HandleCorrectWord(string word, List<int> indexes) => PlayCorrectWord();
    private void HandleGameOver()         => PlayGameOver();
    private void HandleBoardCompleted()   => PlayWin();
    private void HandleCategoryCompleted() => PlayCategoryComplete();

    // ─── Procedural Audio Generation ────────────────────────────────────────────

    /// Tạo arpeggio từ mảng tần số, mỗi nốt duration giây
    private AudioClip GenerateArpeggio(float[] frequencies, float noteDuration, float volume)
    {
        int samplesPerNote = Mathf.RoundToInt(SAMPLE_RATE * noteDuration);
        int total = samplesPerNote * frequencies.Length;
        float[] data = new float[total];

        for (int n = 0; n < frequencies.Length; n++)
        {
            float freq = frequencies[n];
            int start = n * samplesPerNote;
            for (int i = 0; i < samplesPerNote; i++)
            {
                float t = (float)i / samplesPerNote;
                // Envelope: fade in 10% + fade out cuối
                float env = Mathf.Sin(t * Mathf.PI) * volume;
                // Sine wave + chút overtone cho ấm hơn
                data[start + i] = (Mathf.Sin(2f * Mathf.PI * freq * i / SAMPLE_RATE) * 0.7f
                                 + Mathf.Sin(2f * Mathf.PI * freq * 2f * i / SAMPLE_RATE) * 0.2f
                                 + Mathf.Sin(2f * Mathf.PI * freq * 3f * i / SAMPLE_RATE) * 0.1f)
                                 * env;
            }
        }

        return MakeClip("arpeggio", data);
    }

    /// Tạo một nốt đơn ngắn (cho tiếng click)
    private AudioClip GenerateTick(float freq, float duration, float volume)
    {
        int total = Mathf.RoundToInt(SAMPLE_RATE * duration);
        float[] data = new float[total];
        for (int i = 0; i < total; i++)
        {
            float t = (float)i / total;
            float env = Mathf.Pow(1f - t, 3f) * volume; // decay nhanh
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / SAMPLE_RATE) * env;
        }
        return MakeClip("tick", data);
    }

    /// Fanfare ngắn cho hoàn thành category
    private AudioClip GenerateFanfare()
    {
        float[] freqs = { 523.25f, 659.25f, 783.99f, 659.25f, 783.99f, 1046.5f };
        float[] durs  = { 0.12f,  0.10f,  0.10f,  0.08f,  0.08f,  0.35f };
        float volume  = 0.7f;

        int total = 0;
        int[] starts = new int[freqs.Length];
        int[] lengths = new int[freqs.Length];
        for (int i = 0; i < freqs.Length; i++)
        {
            starts[i]  = total;
            lengths[i] = Mathf.RoundToInt(SAMPLE_RATE * durs[i]);
            total += lengths[i];
        }

        float[] data = new float[total];
        for (int n = 0; n < freqs.Length; n++)
        {
            int len = lengths[n];
            for (int i = 0; i < len; i++)
            {
                float t = (float)i / len;
                float env = Mathf.Sin(t * Mathf.PI) * volume;
                data[starts[n] + i] = Mathf.Sin(2f * Mathf.PI * freqs[n] * i / SAMPLE_RATE) * env;
            }
        }
        return MakeClip("fanfare", data);
    }

    /// Nhạc nền: giai điệu nhẹ nhàng dạng waltz đơn giản, loop ~8 giây
    private AudioClip GenerateBackgroundMusic()
    {
        // Các nốt nhạc C major theo kiểu waltz nhẹ
        float[] melody = {
            261.63f, 329.63f, 392f, 329.63f,
            349.23f, 440f,    349.23f, 0f,
            392f,    493.88f, 392f, 329.63f,
            261.63f, 329.63f, 261.63f, 0f
        };
        float noteDur = 0.5f;
        float volume  = 0.18f;

        int samplesPerNote = Mathf.RoundToInt(SAMPLE_RATE * noteDur);
        int total = samplesPerNote * melody.Length;
        float[] data = new float[total];

        for (int n = 0; n < melody.Length; n++)
        {
            if (melody[n] < 1f) continue; // nốt lặng
            int start = n * samplesPerNote;
            float freq = melody[n];

            for (int i = 0; i < samplesPerNote; i++)
            {
                float t = (float)i / samplesPerNote;
                float env = Mathf.Sin(t * Mathf.PI) * volume;
                // Sine thuần + harmonic nhẹ cho âm thanh "đàn"
                data[start + i] = (Mathf.Sin(2f * Mathf.PI * freq * i / SAMPLE_RATE)
                                 + Mathf.Sin(2f * Mathf.PI * freq * 2f * i / SAMPLE_RATE) * 0.3f)
                                 * env;
            }
        }

        // Tạo bass nhẹ chạy nền
        float[] bassNotes = { 130.81f, 174.61f, 196f, 130.81f, 174.61f, 196f, 130.81f, 130.81f };
        float bassDur = noteDur * 2;
        int bassSamples = Mathf.RoundToInt(SAMPLE_RATE * bassDur);

        for (int n = 0; n < bassNotes.Length && n * bassSamples < total; n++)
        {
            int start = n * bassSamples;
            float freq = bassNotes[n];
            int end = Mathf.Min(start + bassSamples, total);

            for (int i = start; i < end; i++)
            {
                float t = (float)(i - start) / bassSamples;
                float env = Mathf.Sin(t * Mathf.PI) * 0.08f;
                data[i] += Mathf.Sin(2f * Mathf.PI * freq * (i - start) / SAMPLE_RATE) * env;
            }
        }

        return MakeClip("bgmusic", data);
    }

    private AudioClip MakeClip(string clipName, float[] data)
    {
        var clip = AudioClip.Create(clipName, data.Length, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        return clip;
    }
}
