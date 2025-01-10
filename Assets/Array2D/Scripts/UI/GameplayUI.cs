using _Main;
using _Main._Enums;
using DG.Tweening;
using SerapKeremGameTools._Game._AudioSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider tankProgressSlider;
    [SerializeField] private TextMeshProUGUI tankProgressText;
    [SerializeField] private Image tankProgressFillImage;

    [Header("Settings")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private SettingsUI settingsUI;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float buttonClickScale = 0.9f;
    [SerializeField] private float buttonAnimDuration = 0.1f;
    [SerializeField] private float levelTextScaleDuration = 0.2f;
    [SerializeField] private float progressBarDuration = 0.3f;

    private CanvasGroup gameplayPanel;

    private void Awake()
    {
        InitializeComponents();
        InitializeUI();
        SetupSettingsButton();
    }

    private void InitializeComponents()
    {
        // CanvasGroup kontrolü
        gameplayPanel = GetComponent<CanvasGroup>();
        if (gameplayPanel == null)
        {
            gameplayPanel = gameObject.AddComponent<CanvasGroup>();
        }

        // Referans kontrolleri
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (levelText == null) Debug.LogError("[GameplayUI] Level text reference is missing!");
        if (tankProgressSlider == null) Debug.LogError("[GameplayUI] Tank progress slider reference is missing!");
        if (tankProgressText == null) Debug.LogError("[GameplayUI] Tank progress text reference is missing!");
        if (tankProgressFillImage == null) Debug.LogError("[GameplayUI] Tank progress fill image reference is missing!");
        if (settingsButton == null) Debug.LogError("[GameplayUI] Settings button reference is missing!");
        if (settingsUI == null) Debug.LogError("[GameplayUI] SettingsUI reference is missing!");
    }

    private void InitializeUI()
    {
        UpdateLevelText(1); // Default level 1
        ResetProgress();
    }

    private void SetupSettingsButton()
    {
        // Settings button click listener
        settingsButton.onClick.AddListener(OnSettingsClicked);

        // Click animasyonu
        settingsButton.onClick.AddListener(() => {
            settingsButton.transform.DOScale(buttonClickScale, buttonAnimDuration)
                .SetUpdate(true)
                .OnComplete(() => {
                    settingsButton.transform.DOScale(1f, buttonAnimDuration)
                        .SetUpdate(true);
                });
        });
    }

    private void OnSettingsClicked()
    {
        AudioManager.Instance?.PlayAudio(AudioKeys.UI_CLICK);
        settingsUI.Show();
    }

    public void UpdateLevelText(int levelNumber)
    {
        levelText.transform.DOScale(1.2f, levelTextScaleDuration)
            .OnComplete(() => {
                levelText.text = $"LEVEL {levelNumber}";
                levelText.transform.DOScale(1f, levelTextScaleDuration);
            });
    }

    public void UpdateTankProgress(int current, int max, ColorType tankColorType)
    {
        // Smooth progress bar animasyonu
        DOTween.To(() => tankProgressSlider.value,
            x => tankProgressSlider.value = x,
            (float)current / max,
            progressBarDuration)
            .SetEase(Ease.OutCubic);

        // Progress text güncelleme
        tankProgressText.text = $"{current}/{max}";

        // Fill rengi güncelleme
        tankProgressFillImage.color = ColorManager.ColorTypeToColor(tankColorType);
    }

    public void ResetProgress()
    {
        tankProgressSlider.value = 0;
        tankProgressText.text = "0/0";
    }

    public void Show()
    {
        if (gameplayPanel == null)
        {
            Debug.LogError("[GameplayUI] CanvasGroup component missing!");
            return;
        }


        gameObject.SetActive(true);

        // Fade in animasyonu
        gameplayPanel.alpha = 0f;
        gameplayPanel.DOFade(1f, fadeInDuration);

        // Interaksiyon ayarlar?
        gameplayPanel.interactable = true;
        gameplayPanel.blocksRaycasts = true;
    }

    public void Hide()
    {
        // Fade out animasyonu
        gameplayPanel.DOFade(0f, fadeInDuration)
            .OnComplete(() => {
                gameObject.SetActive(false);
            });

        // Interaksiyon ayarlar?
        gameplayPanel.interactable = false;
        gameplayPanel.blocksRaycasts = false;
    }

    private void OnDestroy()
    {
        // Cleanup
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
        }

        // Kill tweens
        DOTween.Kill(levelText.transform);
        DOTween.Kill(gameplayPanel);
        DOTween.Kill(settingsButton.transform);
    }
}