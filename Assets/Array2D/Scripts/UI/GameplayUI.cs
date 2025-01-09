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
    [SerializeField] private Button pauseButton;
    [SerializeField] private Image tankProgressFillImage;
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        InitializeUI();
    }

    private void InitializeUI()
    {
        pauseButton.onClick.AddListener(OnPauseClicked);
        UpdateLevelText(1); // Default level 1
        ResetProgress();
    }

    public void UpdateLevelText(int levelNumber)
    {
        levelText.transform.DOScale(1.2f, 0.2f).OnComplete(() => {
            levelText.text = $"LEVEL {levelNumber}";
            levelText.transform.DOScale(1f, 0.2f);
        });
    }

    public void UpdateTankProgress(int current, int max, ColorType tankColorType)
    {
        // Smooth lerp ile progress bar'? doldur
        DOTween.To(() => tankProgressSlider.value,
            x => tankProgressSlider.value = x,
            (float)current / max,
            0.3f).SetEase(Ease.OutCubic);

        tankProgressText.text = $"{current}/{max}";
        tankProgressFillImage.color = ColorManager.ColorTypeToColor(tankColorType);
    }



    public void ResetProgress()
    {
        tankProgressSlider.value = 0;
        tankProgressText.text = "0/0";
    }

    private void OnPauseClicked()
    {
        AudioManager.Instance.PlayAudio(AudioKeys.UI_CLICK);
      //  GameManager.Instance.PauseGame();
    }

    public void Show()
    {
        if (canvasGroup == null)
        {
            Debug.LogError("[GameplayUI] CanvasGroup component missing!");
            return;
        }
        gameObject.SetActive(true);
        canvasGroup.DOFade(1f, fadeInDuration);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.DOFade(0f, fadeInDuration).OnComplete(() => {
            gameObject.SetActive(false);
        });
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}