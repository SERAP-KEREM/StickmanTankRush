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
    [Tooltip("Displays the current level number.")]
    [SerializeField] private TextMeshProUGUI _levelText;

    [Tooltip("Slider for the tank progress bar.")]
    [SerializeField] private Slider _tankProgressSlider;

    [Tooltip("Text displaying tank progress (e.g., 3/5).")]
    [SerializeField] private TextMeshProUGUI _tankProgressText;

    [Tooltip("Image representing the fill of the tank progress bar.")]
    [SerializeField] private Image _tankProgressFillImage;

    [Header("Settings")]
    [Tooltip("Button to open settings UI.")]
    [SerializeField] private Button _settingsButton;

    [Tooltip("The Settings UI panel.")]
    [SerializeField] private SettingsUI _settingsUI;

    [Header("Animation Settings")]
    [Tooltip("Duration for fade-in/fade-out animations.")]
    [SerializeField, Range(0.1f, 2f)] private float _fadeInDuration = 0.5f;

    [Tooltip("Scale applied to the settings button when clicked.")]
    [SerializeField, Range(0.5f, 1f)] private float _buttonClickScale = 0.9f;

    [Tooltip("Duration of the button click animation.")]
    [SerializeField, Range(0.05f, 1f)] private float _buttonAnimDuration = 0.1f;

    [Tooltip("Duration of the level text scaling animation.")]
    [SerializeField, Range(0.1f, 1f)] private float _levelTextScaleDuration = 0.2f;

    [Tooltip("Duration for the progress bar update animation.")]
    [SerializeField, Range(0.1f, 1f)] private float _progressBarDuration = 0.3f;

    private CanvasGroup _gameplayPanel;

    private void Awake()
    {
        InitializeComponents();
        InitializeUI();
        SetupSettingsButton();
    }

    /// <summary>
    /// Initializes necessary components and validates references.
    /// </summary>
    private void InitializeComponents()
    {
        _gameplayPanel = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        ValidateReferences();
    }

    /// <summary>
    /// Checks for missing references and logs warnings if any are null.
    /// </summary>
    private void ValidateReferences()
    {
        if (_levelText == null) Debug.LogWarning("[GameplayUI] Level text reference is missing!");
        if (_tankProgressSlider == null) Debug.LogWarning("[GameplayUI] Tank progress slider reference is missing!");
        if (_tankProgressText == null) Debug.LogWarning("[GameplayUI] Tank progress text reference is missing!");
        if (_tankProgressFillImage == null) Debug.LogWarning("[GameplayUI] Tank progress fill image reference is missing!");
        if (_settingsButton == null) Debug.LogWarning("[GameplayUI] Settings button reference is missing!");
        if (_settingsUI == null) Debug.LogWarning("[GameplayUI] Settings UI reference is missing!");
    }

    /// <summary>
    /// Sets the initial UI state.
    /// </summary>
    private void InitializeUI()
    {
        UpdateLevelText(1); // Default level
        ResetProgress();
    }

    /// <summary>
    /// Configures the settings button with animations and actions.
    /// </summary>
    private void SetupSettingsButton()
    {
        _settingsButton.onClick.AddListener(OnSettingsClicked);

        _settingsButton.onClick.AddListener(() =>
        {
            _settingsButton.transform.DOScale(_buttonClickScale, _buttonAnimDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _settingsButton.transform.DOScale(1f, _buttonAnimDuration).SetUpdate(true);
                });
        });
    }

    /// <summary>
    /// Handles the settings button click event.
    /// </summary>
    private void OnSettingsClicked()
    {
        AudioManager.Instance?.PlayAudio(AudioKeys.UI_CLICK);
        _settingsUI.Show();
    }

    /// <summary>
    /// Updates the level text with animation.
    /// </summary>
    public void UpdateLevelText(int levelNumber)
    {
        _levelText.transform.DOScale(1.2f, _levelTextScaleDuration)
            .OnComplete(() =>
            {
                _levelText.text = $"LEVEL {levelNumber}";
                _levelText.transform.DOScale(1f, _levelTextScaleDuration);
            });
    }

    /// <summary>
    /// Updates the tank progress bar and text.
    /// </summary>
    public void UpdateTankProgress(int current, int max, ColorType tankColorType)
    {
        DOTween.To(() => _tankProgressSlider.value, x => _tankProgressSlider.value = x, (float)current / max, _progressBarDuration)
            .SetEase(Ease.OutCubic);

        _tankProgressText.text = $"{current}/{max}";
        _tankProgressFillImage.color = ColorManager.ColorTypeToColor(tankColorType);
    }

    /// <summary>
    /// Resets the progress bar and text.
    /// </summary>
    public void ResetProgress()
    {
        _tankProgressSlider.value = 0;
        _tankProgressText.text = "0/0";
    }

    /// <summary>
    /// Shows the UI with a fade-in animation.
    /// </summary>
    public void Show()
    {
        if (_gameplayPanel == null)
        {
            Debug.LogError("[GameplayUI] CanvasGroup component missing!");
            return;
        }

        gameObject.SetActive(true);
        _gameplayPanel.alpha = 0f;
        _gameplayPanel.DOFade(1f, _fadeInDuration);

        _gameplayPanel.interactable = true;
        _gameplayPanel.blocksRaycasts = true;
    }

    /// <summary>
    /// Hides the UI with a fade-out animation.
    /// </summary>
    public void Hide()
    {
        _gameplayPanel.DOFade(0f, _fadeInDuration)
            .OnComplete(() => { gameObject.SetActive(false); });

        _gameplayPanel.interactable = false;
        _gameplayPanel.blocksRaycasts = false;
    }

    /// <summary>
    /// Cleans up resources and stops animations when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (_settingsButton != null) _settingsButton.onClick.RemoveAllListeners();

        DOTween.Kill(_levelText.transform);
        DOTween.Kill(_gameplayPanel);
        DOTween.Kill(_settingsButton.transform);
    }
}