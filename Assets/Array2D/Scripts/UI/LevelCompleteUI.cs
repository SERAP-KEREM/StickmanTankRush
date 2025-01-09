using DG.Tweening;
using SerapKeremGameTools._Game._AudioSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Transform starsContainer;
    [SerializeField] private Image[] starImages;
    [SerializeField] private Button nextLevelButton;

    [Header("Animation Settings")]
    [SerializeField] private float showDelay = 0.5f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float starAnimDuration = 0.5f;
    [SerializeField] private float scoreCountDuration = 1f;

    [Header("Visual Settings")]
    [SerializeField] private Color starActiveColor = Color.yellow;
    [SerializeField] private Color starInactiveColor = Color.gray;

    private void Awake()
    {
        nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        Hide(true);

    }
    public void Hide(bool instant = false)
    {
        if (instant)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
        else
        {
            canvasGroup.DOFade(0f, 0.3f).OnComplete(() => {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            });
        }
    }

    public void Show(ScoreData scoreData)
    {
        Debug.Log($"[LevelCompleteUI] Show called with score: {scoreData.TotalScore}");

        gameObject.SetActive(true);

        // CanvasGroup kontrolü
        if (canvasGroup == null)
        {
            Debug.LogError("[LevelCompleteUI] CanvasGroup is still null!");
            return;
        }

        // UI'? göster
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Score'u göster
        scoreText.text = $"Score: {scoreData.TotalScore}";

        // Y?ld?zlar? göster
        ShowStars(scoreData.Stars);

        AudioManager.Instance?.PlayAudio(AudioKeys.LEVEL_WIN);
    }
    private void ShowStars(int starCount)
    {
        Debug.Log($"[LevelCompleteUI] Showing {starCount} stars");

        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < starCount)
            {
                starImages[i].color = Color.yellow;
                starImages[i].transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
                AudioManager.Instance?.PlayAudio(AudioKeys.STAR_EARNED);
            }
            else
            {
                starImages[i].color = Color.gray;
                starImages[i].transform.localScale = Vector3.one * 0.7f;
            }
        }
    }
    private void AnimateStars(int starCount)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            float delay = 0.2f * i;
            Image star = starImages[i];

            if (i < starCount)
            {
                // Active star animation
                DOVirtual.DelayedCall(delay, () => {
                    star.color = starActiveColor;
                    star.transform.DOScale(1f, starAnimDuration)
                        .SetEase(Ease.OutBack);
                    AudioManager.Instance.PlayAudio(AudioKeys.STAR_EARNED);
                });
            }
            else
            {
                // Inactive star
                star.color = starInactiveColor;
                star.transform.localScale = Vector3.one * 0.7f;
            }
        }
    }

    private void OnNextLevelClicked()
    {
        AudioManager.Instance.PlayAudio(AudioKeys.UI_CLICK);

        // Fade out animation
        canvasGroup.DOFade(0f, fadeInDuration)
            .OnComplete(() => {
                gameObject.SetActive(false);
                LevelManager.Instance.OnNextLevelButtonClicked();
            });
    }
}