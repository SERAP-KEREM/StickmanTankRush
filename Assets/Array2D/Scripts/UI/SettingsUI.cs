using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using SerapKeremGameTools._Game._AudioSystem;

public class SettingsUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private RectTransform contentPanel;

    [Header("Sound Settings")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicValueText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float panelAnimDuration = 0.5f;
    [SerializeField] private float slideDistance = 400f;
    [SerializeField] private float buttonHoverScale = 1.1f;
    [SerializeField] private float buttonHoverDuration = 0.2f;

    private bool isOpen;

    private void Awake()
    {
        if (settingsPanel == null)
            settingsPanel = GetComponent<CanvasGroup>();
        InitializeUI();
        HidePanel(true);
        gameObject.SetActive(false);
    }

    private void InitializeUI()
    {
        // Validate references
        ValidateReferences();

        // Button listeners
        closeButton.onClick.AddListener(() => HidePanel());
        restartButton.onClick.AddListener(OnRestartClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        // Slider listeners
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        // Initialize slider values
        InitializeSliderValues();

        // Setup button animations
        SetupButtonAnimations();
    }

    private void ValidateReferences()
    {
        if (settingsPanel == null) Debug.LogError("[SettingsUI] SettingsPanel reference is missing!");
        if (contentPanel == null) Debug.LogError("[SettingsUI] ContentPanel reference is missing!");
        if (musicSlider == null) Debug.LogError("[SettingsUI] MusicSlider reference is missing!");
        if (closeButton == null) Debug.LogError("[SettingsUI] CloseButton reference is missing!");
        if (restartButton == null) Debug.LogError("[SettingsUI] RestartButton reference is missing!");
        if (mainMenuButton == null) Debug.LogError("[SettingsUI] MainMenuButton reference is missing!");
    }

    private void InitializeSliderValues()
    {
        // Set initial slider values
        musicSlider.value = AudioManager.Instance?.MusicVolume ?? 1f;

        // Update texts
        UpdateVolumeTexts();
    }

    private void UpdateVolumeTexts()
    {
        musicValueText.text = $"{(int)(musicSlider.value * 100)}%";
    }

    private void SetupButtonAnimations()
    {
        SetupButtonAnimation(closeButton);
        SetupButtonAnimation(restartButton);
        SetupButtonAnimation(mainMenuButton);
    }

    private void SetupButtonAnimation(Button button)
    {
        button.transform.DOScale(1f, 0f);  // Reset scale

        // Button'a bas?ld???nda ve b?rak?ld???nda scale animasyonu
        button.onClick.AddListener(() => {
            button.transform.DOScale(0.95f, buttonHoverDuration).SetUpdate(true)
                .OnComplete(() => {
                    button.transform.DOScale(1f, buttonHoverDuration).SetUpdate(true);
                });
        });
    }

    public void Show()
    {
        if (isOpen) return;
        isOpen = true;

        gameObject.SetActive(true);

        // Reset positions and alpha
        settingsPanel.alpha = 0f;
        contentPanel.anchoredPosition = new Vector2(slideDistance, 0f);

        // Panel fade in
        settingsPanel.DOFade(1f, fadeInDuration).SetUpdate(true);

        // Panel slide in
        contentPanel.DOAnchorPosX(0f, panelAnimDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        // Enable interaction
        settingsPanel.interactable = true;
        settingsPanel.blocksRaycasts = true;
    }

    public void HidePanel(bool instant = false)
    {
        if (!isOpen) return;
        isOpen = false;

        if (instant)
        {
            // Instant hide
            settingsPanel.alpha = 0f;
            settingsPanel.interactable = false;
            settingsPanel.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
        else
        {
            // Animated hide
            settingsPanel.DOFade(0f, fadeInDuration).SetUpdate(true);
            contentPanel.DOAnchorPosX(slideDistance, panelAnimDuration)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() => {
                    settingsPanel.interactable = false;
                    settingsPanel.blocksRaycasts = false;
                    gameObject.SetActive(false);
                });
        }
    }


    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);

        // Smooth text update
        DOTween.To(() => float.Parse(musicValueText.text.Replace("%", "")),
            x => musicValueText.text = $"{Mathf.RoundToInt(x)}%",
            value * 100f, 0.2f).SetUpdate(true);
    }

    private void OnRestartClicked()
    {
        HidePanel();
        LevelManager.Instance.RestartLevel();
    }

    private void OnMainMenuClicked()
    {
        HidePanel();
        // TODO: Ana menüye dön
        // SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        // Kill all tweens
        DOTween.Kill(settingsPanel);
        DOTween.Kill(contentPanel);
        DOTween.Kill(musicValueText);
    }
}