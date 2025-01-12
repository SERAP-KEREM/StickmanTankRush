using DG.Tweening;
using SerapKeremGameTools._Game._AudioSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("CanvasGroup for fading in/out the LevelComplete UI.")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Tooltip("Displays the title of the UI (e.g., 'Level Complete').")]
    [SerializeField] private TextMeshProUGUI _titleText;

    [Tooltip("Displays the player's score.")]
    [SerializeField] private TextMeshProUGUI _scoreText;

    [Tooltip("Container for the star images.")]
    [SerializeField] private Transform _starsContainer;

    [Tooltip("Array of images representing stars.")]
    [SerializeField] private Image[] _starImages;

    [Tooltip("Button to proceed to the next level.")]
    [SerializeField] private Button _nextLevelButton;

    [Header("Animation Settings")]
    [Tooltip("Delay before the UI starts showing.")]
    [SerializeField, Range(0.1f, 1f)] private float _showDelay = 0.5f;

    [Tooltip("Duration for fade-in animations.")]
    [SerializeField, Range(0.1f, 1f)] private float _fadeInDuration = 0.3f;

    [Tooltip("Duration for star animation.")]
    [SerializeField, Range(0.1f, 1f)] private float _starAnimDuration = 0.5f;

    [Tooltip("Duration for score counting animation.")]
    [SerializeField, Range(0.5f, 2f)] private float _scoreCountDuration = 1f;

    [Header("Visual Settings")]
    [Tooltip("Color for active stars.")]
    [SerializeField] private Color _starActiveColor = Color.yellow;

    [Tooltip("Color for inactive stars.")]
    [SerializeField] private Color _starInactiveColor = Color.gray;
  //  [SerializeField] private LevelManager _levelManager;
    private void Awake()
    {
       // _levelManager = GetComponent<LevelManager>();    
        _nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        Hide(true);
    }

    /// <summary>
    /// Hides the LevelComplete UI.
    /// </summary>
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
            _canvasGroup.DOFade(0f, _fadeInDuration).OnComplete(() =>
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            });
        }
    }

    /// <summary>
    /// Displays the LevelComplete UI with score and stars.
    /// </summary>
    public void Show(ScoreData scoreData)
    {
        gameObject.SetActive(true);

        if (_canvasGroup == null)
        {
            Debug.LogError("[LevelCompleteUI] CanvasGroup is missing!");
            return;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        _scoreText.text = $"Score: {scoreData.TotalScore}";

        AnimateStars(scoreData.Stars);

        AudioManager.Instance?.PlayAudio(AudioKeys.LEVEL_WIN);
    }

    /// <summary>
    /// Animates stars based on the number of stars earned.
    /// </summary>
    private void AnimateStars(int starCount)
    {
        for (int i = 0; i < _starImages.Length; i++)
        {
            float delay = 0.2f * i;
            Image star = _starImages[i];

            if (i < starCount)
            {
                DOVirtual.DelayedCall(delay, () =>
                {
                    star.color = _starActiveColor;
                    star.transform.DOScale(1f, _starAnimDuration)
                        .SetEase(Ease.OutBack);
                    AudioManager.Instance?.PlayAudio(AudioKeys.STAR_EARNED);
                });
            }
            else
            {
                star.color = _starInactiveColor;
                star.transform.localScale = Vector3.one * 0.7f;
            }
        }
    }

    /// <summary>
    /// Handles the Next Level button click event.
    /// </summary>
    private void OnNextLevelClicked()
    {
        AudioManager.Instance?.PlayAudio(AudioKeys.UI_CLICK);

        _canvasGroup.DOFade(0f, _fadeInDuration)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                LevelManager.Instance?.OnNextLevelButtonClicked();
            });
    }
}
