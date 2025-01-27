using DG.Tweening;
using SerapKeremGameTools._Game._AudioSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Manages the Level Failed UI, including showing animations, button functionality, 
/// and interactions with the LevelManager.
/// </summary>
public class LevelFailedUI : MonoBehaviour
{
    #region UI References
    [Header("UI References")]
    [Tooltip("The canvas group for controlling UI visibility and interaction.")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Tooltip("The title text displaying the 'Level Failed' message.")]
    [SerializeField] private TextMeshProUGUI _titleText;

    [Tooltip("Button for retrying the current level.")]
    [SerializeField] private Button _retryButton;

    [Tooltip("Button for returning to the main menu.")]
    [SerializeField] private Button _mainMenuButton;
    #endregion

    #region Animation Settings
    [Header("Animation Settings")]
    [Tooltip("Delay before showing the UI.")]
    [SerializeField, Range(0f, 2f)] private float _showDelay = 0.5f;

    [Tooltip("Duration for fading in the UI.")]
    [SerializeField, Range(0f, 1f)] private float _fadeInDuration = 0.3f;

    [Tooltip("Delay between button animations.")]
    [SerializeField, Range(0f, 1f)] private float _buttonAnimDelay = 0.2f;
    #endregion

    #region Initialization
    private void Awake()
    {
        InitializeComponents();
        InitializeButtons();
        Hide(true);
    }

    /// <summary>
    /// Initializes necessary components and validates references.
    /// </summary>
    private void InitializeComponents()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Initializes button click listeners.
    /// </summary>
    private void InitializeButtons()
    {
        _retryButton.onClick.AddListener(OnRetryClicked);
        _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }
    #endregion

    #region Show/Hide UI
    /// <summary>
    /// Displays the Level Failed UI with animations.
    /// </summary>
    public void Show()
    {
        Debug.Log("[LevelFailedUI] Show called.");
        gameObject.SetActive(true);

        ResetUI();

        DOVirtual.DelayedCall(_showDelay, () =>
        {
            _canvasGroup.DOFade(1f, _fadeInDuration);
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            _titleText.transform.DOScale(1f, _fadeInDuration).SetEase(Ease.OutBack);

            DOVirtual.DelayedCall(_buttonAnimDelay, () =>
                _retryButton.transform.DOScale(0.3f, _fadeInDuration).SetEase(Ease.OutBack));

            DOVirtual.DelayedCall(_buttonAnimDelay * 2, () =>
                _mainMenuButton.transform.DOScale(1f, _fadeInDuration).SetEase(Ease.OutBack));

            AudioManager.Instance?.PlayAudioByName(AudioKeys.LEVEL_LOSE);
        });
    }

    /// <summary>
    /// Resets the UI elements to their initial state.
    /// </summary>
    private void ResetUI()
    {
        _canvasGroup.alpha = 0f;
        _titleText.transform.localScale = Vector3.zero;
        _retryButton.transform.localScale = Vector3.zero;
        _mainMenuButton.transform.localScale = Vector3.zero;
    }

    /// <summary>
    /// Hides the Level Failed UI, optionally instantly.
    /// </summary>
    /// <param name="instant">Whether to hide the UI instantly or with a fade-out animation.</param>
    public void Hide(bool instant = false)
    {
        if (instant)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
        else
        {
            _canvasGroup.DOFade(0f, _fadeInDuration)
                .OnComplete(() =>
                {
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                    gameObject.SetActive(false);
                });
        }
    }
    #endregion

    #region Button Handlers
    /// <summary>
    /// Handles the retry button click event.
    /// </summary>
    private void OnRetryClicked()
    {
        Debug.Log("[LevelFailedUI] Retry button clicked.");
        Hide();
        LevelManager.Instance.RestartLevel();
    }

    /// <summary>
    /// Handles the main menu button click event.
    /// </summary>
    private void OnMainMenuClicked()
    {
        Debug.Log("[LevelFailedUI] Main Menu button clicked.");
        Hide();
        // TODO: Implement main menu navigation.
        // SceneManager.LoadScene("MainMenu");
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// Cleans up DOTween animations when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
    #endregion
}
