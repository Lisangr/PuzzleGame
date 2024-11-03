using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using YG;

public class TimerBeforeAdsYG : MonoBehaviour
{
#if !UNITY_ANDROID
    [SerializeField,
     Tooltip("Объект таймера перед показом рекламы. Он будет активироваться и деактивироваться в нужное время.")]
    private GameObject secondsPanelObject;

    [SerializeField,
     Tooltip(
         "Массив объектов, которые будут показываться по очереди через секунду. Сколько объектов вы поместите в массив, столько секунд будет отчитываться перед показом рекламы.\n\nНапример, поместите в массив три объекта: певый с текстом '3', второй с текстом '2', третий с текстом '1'.\nВ таком случае произойдёт отчет трёх секунд с показом объектов с цифрами перед рекламой.")]
    private GameObject[] secondObjects;

    [SerializeField,
     Tooltip("Работа таймера в реальном времени, независимо от time scale.")]
    private bool realtimeSeconds;

    [Space(20)] [SerializeField] private UnityEvent onShowTimer;
    [SerializeField] private UnityEvent onHideTimer;

    [SerializeField] private Button continueButton;
    [SerializeField] private Image backGround;

    private void Start()
    {
        continueButton.onClick.AddListener(ContinueGame);
        if (secondsPanelObject)
            secondsPanelObject.SetActive(false);

        for (int i = 0; i < secondObjects.Length; i++)
            secondObjects[i].SetActive(false);

        if (secondObjects.Length > 0)
            StartCoroutine(CheckTimerAd());
        else
            Debug.LogError("Fill in the array 'secondObjects'");
    }

    private void ContinueGame()
    {
        Time.timeScale = 1;
        continueButton.gameObject.SetActive(false);
        backGround.gameObject.SetActive(false);
    }

    IEnumerator CheckTimerAd()
    {
        bool checking = true;
        while (checking)
        {
            if (YandexGame.timerShowAd >= YandexGame.Instance.infoYG.fullscreenAdInterval)
            {
                Time.timeScale = 0;
                onShowTimer?.Invoke();
                objSecCounter = 0;
                if (secondsPanelObject)
                    secondsPanelObject.SetActive(true);

                StartCoroutine(TimerAdShow());
                yield return checking = false;
            }

            if (!realtimeSeconds)
                yield return new WaitForSeconds(1.0f);
            else
                yield return new WaitForSecondsRealtime(1.0f);
        }
    }

    int objSecCounter;

    IEnumerator TimerAdShow()
    {
        bool process = true;
        while (process)
        {
            if (objSecCounter < secondObjects.Length)
            {
                for (int i2 = 0; i2 < secondObjects.Length; i2++)
                    secondObjects[i2].SetActive(false);

                secondObjects[objSecCounter].SetActive(true);
                objSecCounter++;

                if (!realtimeSeconds)
                    yield return new WaitForSeconds(1.0f);
                else
                    yield return new WaitForSecondsRealtime(1.0f);
            }

            if (objSecCounter == secondObjects.Length)
            {
                YandexGame.FullscreenShow();
                continueButton.gameObject.SetActive(true);
                backGround.gameObject.SetActive(true);
                StartCoroutine(BackupTimerClosure());

                while (!YandexGame.nowFullAd)
                    yield return null;

                secondsPanelObject.SetActive(false);
                onHideTimer?.Invoke();
                objSecCounter = 0;
                StartCoroutine(CheckTimerAd());
                process = false;
            }
        }
    }

    IEnumerator BackupTimerClosure()
    {
        if (!realtimeSeconds)
            yield return new WaitForSeconds(2.5f);
        else
            yield return new WaitForSecondsRealtime(2.5f);

        if (objSecCounter != 0)
        {
            secondsPanelObject.SetActive(false);
            onHideTimer?.Invoke();
            objSecCounter = 0;
            StopCoroutine(TimerAdShow());
        }
    }
#else // UNITY_ANDROID
   [SerializeField, Tooltip("Объект таймера перед показом рекламы. Он будет активироваться и деактивироваться в нужное время.")]
    private GameObject secondsPanelObject;

    [SerializeField, Tooltip("Массив объектов, которые будут показываться по очереди через секунду. Сколько объектов вы поместите в массив, столько секунд будет отчитываться перед показом рекламы.\n\nНапример, поместите в массив три объекта: певый с текстом '3', второй с текстом '2', третий с текстом '1'.\nВ таком случае произойдёт отчет трёх секунд с показом объектов с цифрами перед рекламой.")]
    private GameObject[] secondObjects;

    [SerializeField, Tooltip("Работа таймера в реальном времени, независимо от time scale.")]
    private bool realtimeSeconds;

    [Space(20)] [SerializeField] private UnityEvent onShowTimer;
    [SerializeField] private UnityEvent onHideTimer;

    [SerializeField] private Button continueButton;
    [SerializeField] private Image backGround;

    [SerializeField, Tooltip("Интервал времени между показами рекламы в секундах.")]
    private float fullscreenAdInterval = 180f; // Укажите значение по умолчанию или настройте в инспекторе

    private float lastAdTime;

    private void Start()
    {
        continueButton.onClick.AddListener(ContinueGame);
        if (secondsPanelObject)
            secondsPanelObject.SetActive(false);

        for (int i = 0; i < secondObjects.Length; i++)
            secondObjects[i].SetActive(false);

        lastAdTime = Time.time;
        if (secondObjects.Length > 0)
            StartCoroutine(CheckTimerAd());
        else
            Debug.LogError("Fill in the array 'secondObjects'");
    }

    private void ContinueGame()
    {
        Time.timeScale = 1;
        continueButton.gameObject.SetActive(false);
        backGround.gameObject.SetActive(false);
    }

    IEnumerator CheckTimerAd()
    {
        bool checking = true;
        while (checking)
        {
            
            if (Time.time - lastAdTime >= fullscreenAdInterval)
            {
                Time.timeScale = 0;
                onShowTimer?.Invoke();
                objSecCounter = 0;
                if (secondsPanelObject)
                    secondsPanelObject.SetActive(true);

                StartCoroutine(TimerAdShow());
                checking = false;
            }

            if (!realtimeSeconds)
                yield return new WaitForSeconds(1.0f);
            else
                yield return new WaitForSecondsRealtime(1.0f);
        }
    }

    int objSecCounter;

    IEnumerator TimerAdShow()
    {
        bool process = true;
        while (process)
        {
            if (objSecCounter < secondObjects.Length)
            {
                for (int i2 = 0; i2 < secondObjects.Length; i2++)
                    secondObjects[i2].SetActive(false);

                secondObjects[objSecCounter].SetActive(true);
                objSecCounter++;

                if (!realtimeSeconds)
                    yield return new WaitForSeconds(1.0f);
                else
                    yield return new WaitForSecondsRealtime(1.0f);
            }

            if (objSecCounter == secondObjects.Length)
            {
                YandexGame.FullscreenShow();
                lastAdTime = Time.time; // Обновление времени последнего показа рекламы
                continueButton.gameObject.SetActive(true);
                backGround.gameObject.SetActive(true);
                StartCoroutine(BackupTimerClosure());

                secondsPanelObject.SetActive(false);
                onHideTimer?.Invoke();
                objSecCounter = 0;
                StartCoroutine(CheckTimerAd());
                process = false;
            }
        }
    }

    IEnumerator BackupTimerClosure()
    {
        if (!realtimeSeconds)
            yield return new WaitForSeconds(2.5f);
        else
            yield return new WaitForSecondsRealtime(2.5f);

        if (objSecCounter != 0)
        {
            secondsPanelObject.SetActive(false);
            onHideTimer?.Invoke();
            objSecCounter = 0;
            StopCoroutine(TimerAdShow());
        }
    }
#endif
}