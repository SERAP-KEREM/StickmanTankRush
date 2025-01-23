using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using SerapKeremGameTools._Game._AudioSystem;

/// <summary>
/// Manages the Settings UI, including animations, sound settings, and button interactions.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("The main canvas group for controlling UI visibility.")]
    [SerializeField] private CanvasGroup _settingsPanel;

    [Tooltip("The content panel for animating slide-in and slide-out effects.")]
    [SerializeField] private RectTransform _contentPanel;

    [Header("Sound Settings")]
    [Tooltip("The slider for adjusting music volume.")]
    [SerializeField] private Slider _musicSlider;

    [Tooltip("The text displaying the current music volume percentage.")]
    [SerializeField] private TextMeshProUGUI _musicValueText;

    [Header("Buttons")]
    [Tooltip("The button for closing the settings panel.")]
    [SerializeField] private Button _closeButton;

    [Tooltip("The button for restarting the level.")]
    [SerializeField] private Button _restartButton;

    [Tooltip("The button for navigating back to the main menu.")]
    [SerializeField] private Button _mainMenuButton;

    [Header("Animation Settings")]
    [Tooltip("Duration for fading in and out the panel.")]
    [SerializeField, Range(0f, 1f)] private float _fadeInDuration = 0.3f;

    [Tooltip("Duration for sliding the panel content.")]
    [SerializeField, Range(0f, 1f)] private float _panelAnimDuration = 0.5f;

    [Tooltip("The horizontal distance for sliding the panel.")]
    [SerializeField] private float _slideDistance = 400f;

    [Tooltip("Scale factor for button hover animation.")]
    [SerializeField] private float _buttonHoverScale = 1.1f;

    [Tooltip("Duration for button hover animation.")]
    [SerializeField] private float _buttonHoverDuration = 0.2f;
   // [SerializeField] private LevelManager _levelManager;

    private void Awake()
    {
        if (_settingsPanel == null)
        {
            Debug.LogError("[SettingsUI] SettingsPanel reference is missing!");
            _settingsPanel = GetComponent<CanvasGroup>();
        }
       // _levelManager = GetComponent<LevelManager>();

        InitializeUI();

    }

    /// <summary>
    /// Initializes the UI components, validates references, and sets up listeners.
    /// </summary>
    private void InitializeUI()
    {
        ValidateReferences();

        _closeButton.onClick.AddListener(() => HidePanel());
        _restartButton.onClick.AddListener(OnRestartClicked);
        _mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        InitializeSliderValues();
        SetupButtonAnimations();
    }

    /// <summary>
    /// Validates that all necessary references are assigned.
    /// </summary>
    private void ValidateReferences()
    {
        if (_contentPanel == null) Debug.LogError("[SettingsUI] ContentPanel reference is missing!");
        if (_musicSlider == null) Debug.LogError("[SettingsUI] MusicSlider reference is missing!");
        if (_closeButton == null) Debug.LogError("[SettingsUI] CloseButton reference is missing!");
        if (_restartButton == null) Debug.LogError("[SettingsUI] RestartButton reference is missing!");
        if (_mainMenuButton == null) Debug.LogError("[SettingsUI] MainMenuButton reference is missing!");
    }

    /// <summary>
    /// Initializes the slider values based on the current music volume.
    /// </summary>
    private void InitializeSliderValues()
    {
        _musicSlider.value = AudioManager.Instance?.MusicVolume ?? 1f;
        UpdateVolumeTexts();
    }

    /// <summary>
    /// Updates the text displaying the current music volume.
    /// </summary>
    private void UpdateVolumeTexts()
    {
        _musicValueText.text = $"{(int)(_musicSlider.value * 100)}%";
    }

    /// <summary>
    /// Sets up hover and click animations for buttons.
    /// </summary>
    private void SetupButtonAnimations()
    {
        SetupButtonAnimation(_closeButton);
        SetupButtonAnimation(_restartButton);
        SetupButtonAnimation(_mainMenuButton);
    }

    /// <summary>
    /// Configures hover and click animations for a button.
    /// </summary>
    /// <param name="button">The button to configure animations for.</param>
    private void SetupButtonAnimation(Button button)
    {
        button.transform.DOScale(1f, 0f); // Reset scale

        button.onClick.AddListener(() =>
        {
            button.transform.DOScale(0.95f, _buttonHoverDuration).SetUpdate(true)
                .OnComplete(() => button.transform.DOScale(1f, _buttonHoverDuration).SetUpdate(true));
        });
    }

    /// <summary>
    /// Displays the settings panel with animations.
    /// </summary>
    public void Show()
    {
      
        gameObject.SetActive(true);

        _settingsPanel.alpha = 0f;
        _contentPanel.anchoredPosition = new Vector2(_slideDistance, 0f);

        _settingsPanel.DOFade(1f, _fadeInDuration).SetUpdate(true);
        _contentPanel.DOAnchorPosX(0f, _panelAnimDuration).SetEase(Ease.OutBack).SetUpdate(true);

        _settingsPanel.interactable = true;
        _settingsPanel.blocksRaycasts = true;
    }

    /// <summary>
    /// Hides the settings panel with optional instant effect.
    /// </summary>
    /// <param name="instant">Whether to hide the panel instantly or with animation.</param>
    public void HidePanel(bool instant = false)
    {

        gameObject.SetActive(false);

        if (instant)
        {
            _settingsPanel.alpha = 0f;
            _settingsPanel.interactable = false;
            _settingsPanel.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
        else
        {
            _settingsPanel.DOFade(0f, _fadeInDuration).SetUpdate(true);
            _contentPanel.DOAnchorPosX(_slideDistance, _panelAnimDuration).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() =>
                {
                    _settingsPanel.interactable = false;
                    _settingsPanel.blocksRaycasts = false;
                    gameObject.SetActive(false);
                });
        }
    }

    /// <summary>
    /// Updates the music volume when the slider value changes.
    /// </summary>
    /// <param name="value">The new volume value.</param>
    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);

        DOTween.To(() => float.Parse(_musicValueText.text.Replace("%", "")),
            x => _musicValueText.text = $"{Mathf.RoundToInt(x)}%",
            value * 100f, 0.2f).SetUpdate(true);
    }

    /// <summary>
    /// Handles the restart button click event.
    /// </summary>
    private void OnRestartClicked()
    {
        HidePanel();
        LevelManager.Instance.RestartLevel();
    }

    /// <summary>
    /// Handles the main menu button click event.
    /// </summary>
    private void OnMainMenuClicked()
    {
        HidePanel();
        // TODO: Implement main menu navigation.
        // SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Cleans up DOTween animations when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        DOTween.Kill(_settingsPanel);
        DOTween.Kill(_contentPanel);
        DOTween.Kill(_musicValueText);
    }
}
