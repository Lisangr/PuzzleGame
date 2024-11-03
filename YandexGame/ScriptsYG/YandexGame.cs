using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;
using YandexMobileAds;
using YandexMobileAds.Base;
#if !UNITY_ANDROID
using YG.Utils.LB;
#endif
using YG.Utils.Pay;

namespace YG
{
    [HelpURL("https://ash-message-bf4.notion.site/PluginYG-d457b23eee604b7aa6076116aab647ed")]
    [DefaultExecutionOrder(-100)]
    public partial class YandexGame : MonoBehaviour
    {
        #region Android

#if !PLATFORM_WEBGL

        public static JsonEnvironmentData EnvironmentData = new JsonEnvironmentData();
        public static float timerShowAd;
        public static YandexGame Instance;
        public bool singleton;
        public static event Action GetDataEvent;
        public static bool SDKEnabled { get; set; }
      
        public static void LoadProgress()
        {
           
        }
        private void Awake()
        {
            transform.SetParent(null);
            gameObject.name = "YandexGame";

            if (singleton)
            {
                if (Instance != null)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Instance = this;
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Instance = this;
            }


            this.interstitialAdLoader = new InterstitialAdLoader();
            this.interstitialAdLoader.OnAdLoaded += this.HandleAdLoaded;
            this.interstitialAdLoader.OnAdFailedToLoad += this.HandleAdFailedToLoadInter;
            RequestInterstitial();

            LoadEditor();
        }

        public class JsonEnvironmentData
        {
            public string language;
            public string domain;
        }
        
        private void Start()
        {
            this.rewardedAdLoader = new RewardedAdLoader();
            this.rewardedAdLoader.OnAdLoaded += this.HandleAdLoaded;
            this.rewardedAdLoader.OnAdFailedToLoad += this.HandleAdFailedToLoad;
            RequestRewardedAd();
            if (bannerActive)
            {
                RequestBanner();
            }
        }
        public static void NewLeaderboardScores(string level, int floor)
        {
            Debug.Log("лидер борд не работает на андроид!");
        }
           #region Banner Android

#if !PLATFORM_WEBGL

    [SerializeField] private string keyBanner;
    public bool bannerActive = false;

    private Banner banner;

    private void RequestBanner()
    {
        //Sets COPPA restriction for user age under 13
        MobileAds.SetAgeRestrictedUser(true);

        // Replace demo Unit ID 'demo-banner-yandex' with actual Ad Unit ID
        string adUnitId;
        if (!string.IsNullOrEmpty(keyBanner))
        {
            adUnitId = keyBanner;
        }
        else
        {
            adUnitId = "demo-banner-yandex";
        }


        if (this.banner != null)
        {
            this.banner.Destroy();
        }

        // Set sticky banner width
        BannerAdSize bannerSize = BannerAdSize.StickySize(GetScreenWidthDp());
        // Or set inline banner maximum width and height
        // BannerAdSize bannerSize = BannerAdSize.InlineSize(GetScreenWidthDp(), 300);
        this.banner = new Banner(adUnitId, bannerSize, AdPosition.BottomCenter);

        this.banner.OnAdLoaded += this.HandleAdLoadedBanner;
        this.banner.OnAdFailedToLoad += this.HandleAdFailedToLoadBanner;
        this.banner.OnReturnedToApplication += this.HandleReturnedToApplicationBanner;
        this.banner.OnLeftApplication += this.HandleLeftApplicationBanner;
        this.banner.OnAdClicked += this.HandleAdClickedBanner;
        this.banner.OnImpression += this.HandleImpressionBanner;

        this.banner.LoadAd(this.CreateAdRequest());
        this.DisplayMessage("Banner is requested");
    }

    // Example how to get screen width for request
    private int GetScreenWidthDp()
    {
        int screenWidth = (int)Screen.safeArea.width;
        return ScreenUtils.ConvertPixelsToDp(screenWidth);
    }

    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder().Build();
    }

   

    #region Banner callback handlers

    public void HandleAdLoadedBanner(object sender, EventArgs args)
    {
        this.DisplayMessage("HandleAdLoaded event received");
        this.banner.Show();
    }

    public void HandleAdFailedToLoadBanner(object sender, AdFailureEventArgs args)
    {
        this.DisplayMessage("HandleAdFailedToLoad event received with message: " + args.Message);
    }

    public void HandleLeftApplicationBanner(object sender, EventArgs args)
    {
        this.DisplayMessage("HandleLeftApplication event received");
    }

    public void HandleReturnedToApplicationBanner(object sender, EventArgs args)
    {
        this.DisplayMessage("HandleReturnedToApplication event received");
    }

    public void HandleAdLeftApplication(object sender, EventArgs args)
    {
        this.DisplayMessage("HandleAdLeftApplication event received");
    }

    public void HandleAdClickedBanner(object sender, EventArgs args)
    {
        this.DisplayMessage("HandleAdClicked event received");
    }

    public void HandleImpressionBanner(object sender, ImpressionData impressionData)
    {
        var data = impressionData == null ? "null" : impressionData.rawData;
        this.DisplayMessage("HandleImpression event received with data: " + data);
    }

    #endregion

#endif

    #endregion
        

        #region Interstitial android

#if !PLATFORM_WEBGL


        private InterstitialAdLoader interstitialAdLoader;
        private Interstitial interstitial;
        private float timerInters;
        [SerializeField] private string keyInterstitial;


        private void Update()
        {
            if (timerInters > 0)
            {
                timerInters -= Time.deltaTime;
            }
        }

        public void _FullscreenShow()
        {
            Instance.ShowInterstitial();
        }

        public static void FullscreenShow() => Instance._FullscreenShow();

        private void RequestInterstitial()
        {
            //Sets COPPA restriction for user age under 13
            MobileAds.SetAgeRestrictedUser(true);


            string adUnitId;
            if (!string.IsNullOrEmpty(keyInterstitial))
            {
                adUnitId = keyInterstitial;
            }
            else
            {
                adUnitId = "demo-interstitial-yandex";
            }

            if (this.interstitial != null)
            {
                this.interstitial.Destroy();
            }

            this.interstitialAdLoader.LoadAd(this.CreateAdRequestInter(adUnitId));
            // this.DisplayMessage("Interstitial is requested");
        }

        public void ShowInterstitial()
        {
            if (timerInters > 0)
            {
                return;
            }

            timerInters = 60;

            if (this.interstitial == null)
            {
                return;
            }

            this.interstitial.OnAdClicked += this.HandleAdClickedInter;
            this.interstitial.OnAdShown += this.HandleAdShownInter;
            this.interstitial.OnAdFailedToShow += this.HandleAdFailedToShowInter;
            this.interstitial.OnAdImpression += this.HandleImpressionInter;
            this.interstitial.OnAdDismissed += this.HandleAdDismissedInter;

            this.interstitial.Show();
        }

        private AdRequestConfiguration CreateAdRequestInter(string adUnitId)
        {
            return new AdRequestConfiguration.Builder(adUnitId).Build();
        }


        #region Interstitial callback handlers

        public void HandleAdLoaded(object sender, InterstitialAdLoadedEventArgs args)
        {
            this.DisplayMessage("HandleAdLoaded event received");

            this.interstitial = args.Interstitial;
        }

        public void HandleAdFailedToLoadInter(object sender, AdFailedToLoadEventArgs args)
        {
            this.DisplayMessage($"HandleAdFailedToLoad event received with message: {args.Message}");
        }

        public void HandleAdClickedInter(object sender, EventArgs args)
        {
            this.DisplayMessage("HandleAdClicked event received");
        }

        public void HandleAdShownInter(object sender, EventArgs args)
        {
            this.DisplayMessage("HandleAdShown event received");
        }

        public void HandleAdDismissedInter(object sender, EventArgs args)
        {
            this.DisplayMessage("HandleAdDismissed event received");

            this.interstitial.Destroy();
            this.interstitial = null;
            RequestInterstitial();
        }

        public void HandleImpressionInter(object sender, ImpressionData impressionData)
        {
            var data = impressionData == null ? "null" : impressionData.rawData;
            this.DisplayMessage($"HandleImpression event received with data: {data}");
        }

        public void HandleAdFailedToShowInter(object sender, AdFailureEventArgs args)
        {
            this.DisplayMessage($"HandleAdFailedToShow event received with message: {args.Message}");
        }

        #endregion


#endif

        #endregion

        #region Rewarded Ad

        [SerializeField] private string keyReward;
        public static event Action<int> RewardVideoEvent;
        private string message = "";
        private RewardedAdLoader rewardedAdLoader;
        private RewardedAd rewardedAd;
        private int indexShowReward;

        public static void RewVideoShow(int id)
        {
            //   indexShowReward = id;
            Instance.ShowRewardedAd(id);
        }

     


        private void RequestRewardedAd()
        {
            this.DisplayMessage("RewardedAd is not ready yet");
            //Sets COPPA restriction for user age under 13
            MobileAds.SetAgeRestrictedUser(true);

            if (this.rewardedAd != null)
            {
                this.rewardedAd.Destroy();
            }

            string adUnitId;
            if (!string.IsNullOrEmpty(keyReward))
            {
                adUnitId = keyReward;
            }
            else
            {
                adUnitId = "demo-rewarded-yandex";
            }


            this.rewardedAdLoader.LoadAd(this.CreateAdRequest(adUnitId));
            this.DisplayMessage("Rewarded Ad is requested");
        }


        public void ShowRewardedAd(int index)
        {
            indexShowReward = index;
            //rewardManager = manager;

            if (this.rewardedAd == null)
            {
                this.DisplayMessage("RewardedAd is not ready yet");
                return;
            }

            this.rewardedAd.OnAdClicked += this.HandleAdClicked;
            this.rewardedAd.OnAdShown += this.HandleAdShown;
            this.rewardedAd.OnAdFailedToShow += this.HandleAdFailedToShow;
            this.rewardedAd.OnAdImpression += this.HandleImpression;
            this.rewardedAd.OnAdDismissed += this.HandleAdDismissed;
            this.rewardedAd.OnRewarded += this.HandleRewarded;

            this.rewardedAd.Show();
        }


        private AdRequestConfiguration CreateAdRequest(string adUnitId)
        {
            return new AdRequestConfiguration.Builder(adUnitId).Build();
        }

        private void DisplayMessage(String message)
        {
            this.message = message + (this.message.Length == 0 ? "" : "\n--------\n" + this.message);
            print(message);
        }


        public void HandleAdLoaded(object sender, RewardedAdLoadedEventArgs args)
        {
            this.DisplayMessage("HandleAdLoaded event received");
            this.rewardedAd = args.RewardedAd;
        }

        public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            this.DisplayMessage(
                $"HandleAdFailedToLoad event received with message: {args.Message}");
        }

        public void HandleAdClicked(object sender, EventArgs args)
        {
            this.DisplayMessage("HandleAdClicked event received");
        }

        public void HandleAdShown(object sender, EventArgs args)
        {
            this.DisplayMessage("HandleAdShown event received");
        }

        public void HandleAdDismissed(object sender, EventArgs args)
        {
            this.DisplayMessage("HandleAdDismissed event received");

            this.rewardedAd.Destroy();
            this.rewardedAd = null;
            RequestRewardedAd();
        }

        public void HandleImpression(object sender, ImpressionData impressionData)
        {
            var data = impressionData == null ? "null" : impressionData.rawData;
            this.DisplayMessage($"HandleImpression event received with data: {data}");
        }

        public void HandleRewarded(object sender, Reward args)
        {
            RewardVideoEvent(indexShowReward);
            this.DisplayMessage($"HandleRewarded event received: amout = {args.amount}, type = {args.type}");
        }

        public void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
        {
            this.DisplayMessage(
                $"HandleAdFailedToShow event received with message: {args.Message}");
        }

        #endregion

#endif

        #endregion

        #region Web

#if !UNITY_ANDROID
        public InfoYG infoYG;
        [Tooltip("Объект YandexGame не будет удаляться при смене сцены. При выборе опции singleton, объект YandexGame необходимо поместить только на одну сцену, которая первая загружается при запуске игры.")]
        public bool singleton;
        [Space(10)]
        public UnityEvent ResolvedAuthorization;
        public UnityEvent RejectedAuthorization;
        [Space(30)]
        public UnityEvent OpenFullscreenAd;
        public UnityEvent CloseFullscreenAd;
        public UnityEvent ErrorFullscreenAd;
        [Space(30)]
        public UnityEvent OpenVideoAd;
        public UnityEvent CloseVideoAd;
        public UnityEvent RewardVideoAd;
        public UnityEvent ErrorVideoAd;
        [Space(30)]
        public UnityEvent PurchaseSuccess;
        public UnityEvent PurchaseFailed;
        [Space(30)]
        public UnityEvent PromptDo;
        public UnityEvent PromptFail;
        public UnityEvent ReviewDo;

        #region Data Fields
        public static bool auth { get => _auth; }
        public static bool SDKEnabled { get => _SDKEnabled; }
        public static bool initializedLB { get => _initializedLB; }

        public static bool nowAdsShow
        {
            get
            {
                if (nowFullAd || nowVideoAd)
                    return true;
                else
                    return false;
            }
        }

        private static bool _auth;
        private static bool _SDKEnabled;
        private static bool _initializedLB;

        public static bool nowFullAd;
        public static bool nowVideoAd;
        public static JsonEnvironmentData EnvironmentData = new JsonEnvironmentData();
        public static YandexGame Instance;
        public static Action onAdNotification;
        public static Action GetDataEvent;
        #endregion Data Fields

        #region Methods
        private void OnEnable()
        {
            if (singleton)
                SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnDisable()
        {
            if (singleton)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Awake()
        {
            transform.SetParent(null);
            gameObject.name = "YandexGame";

            if (singleton)
            {
                if (Instance != null)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Instance = this;
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Instance = this;
            }

            if (!_SDKEnabled)
                CallInitYG();
        }

        [DllImport("__Internal")]
        private static extern void InitGame_js();

        [DllImport("__Internal")]
        private static extern void StaticRBTDeactivate();

        private void Start()
        {
            if (infoYG.AdWhenLoadingScene)
                FullscreenShow();

#if !UNITY_EDITOR
            if (!infoYG.staticRBTInGame)
                StaticRBTDeactivate();
#endif
            if (!_SDKEnabled)
            {
                if (infoYG.leaderboardEnable)
                {
#if !UNITY_EDITOR
                    Debug.Log("Init Leaderbords inGame");
                    _InitLeaderboard();
#else
                    InitializedLB();
#endif
                }
                GetPayments();

                CallStartYG();
                _SDKEnabled = true;
                GetDataInvoke();
#if !UNITY_EDITOR
                InitGame_js();
#endif
            }
        }

        static void Message(string message)
        {
            if (Instance.infoYG.debug) 
                Debug.Log(message);
        }

        public static void GetDataInvoke()
        {
            if (_SDKEnabled)
                GetDataEvent?.Invoke();
        }

        private static bool firstSceneLoad = true;
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (firstSceneLoad)
                firstSceneLoad = false;
            else if (infoYG.AdWhenLoadingScene)
                _FullscreenShow();
        }  

        #region For ECS
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStatic()
        {
            _SDKEnabled = false;
            _auth = false;
            _initializedLB = false;
            _playerName = "unauthorized";
            _playerId = null;
            _playerPhoto = null;
            _photoSize = "medium";
            nowFullAd = false;
            nowVideoAd = false;
            savesData = new SavesYG();
            EnvironmentData = new JsonEnvironmentData();
            purchases = new Purchase[0];
            Instance = null;
            timerShowAd = 0;
            GetDataEvent = null;
            onResetProgress = null;
            SwitchLangEvent = null;
            OpenFullAdEvent = null;
            CloseFullAdEvent = null;
            ErrorFullAdEvent = null;
            OpenVideoEvent = null;
            CloseVideoEvent = null;
            RewardVideoEvent = null;
            ErrorVideoEvent = null;
            onGetLeaderboard = null;
            GetPaymentsEvent = null;
            PurchaseSuccessEvent = null;
            PurchaseFailedEvent = null;
            ReviewSentEvent = null;
            PromptSuccessEvent = null;
            PromptFailEvent = null;
            onAdNotification = null;
        }
#endif
        #endregion For ECS

        #endregion Methods

        

        // Sending messages

        #region Init Leaderboard
        [DllImport("__Internal")]
        private static extern void InitLeaderboard();

        public void _InitLeaderboard()
        {
#if !UNITY_EDITOR
            InitLeaderboard();
#endif
#if UNITY_EDITOR
            Message("Initialization Leaderboards");
#endif
        }
        #endregion Init Leaderboard

        #region Fullscren Ad Show
        [DllImport("__Internal")]
        private static extern void FullAdShow();

        public void _FullscreenShow()
        {
            if (!nowAdsShow && timerShowAd >= infoYG.fullscreenAdInterval)
            {
                timerShowAd = 0;
                onAdNotification?.Invoke();
#if !UNITY_EDITOR
                FullAdShow();
#else
                Message("Fullscren Ad");
                FullAdInEditor();
#endif
            }
            else
            {
                Message($"До запроса к показу рекламы в середине игры {(infoYG.fullscreenAdInterval - timerShowAd).ToString("00.0")} сек.");
            }
        }

        public static void FullscreenShow() => Instance._FullscreenShow();

#if UNITY_EDITOR
        private void FullAdInEditor()
        {
            GameObject obj = new GameObject { name = "TestFullAd" };
            DontDestroyOnLoad(obj);
            Insides.CallingAnEvent call = obj.AddComponent(typeof(Insides.CallingAnEvent)) as Insides.CallingAnEvent;
            call.StartCoroutine(call.CallingAd(infoYG.durationOfAdSimulation));
        }
#endif
        #endregion Fullscren Ad Show

        #region Rewarded Video Show
        [DllImport("__Internal")]
        private static extern void RewardedShow(int id);

        public void _RewardedShow(int id)
        {
            Message("Rewarded Ad Show");

            if (!nowFullAd && !nowVideoAd)
            {
                onAdNotification?.Invoke();
#if !UNITY_EDITOR
                RewardedShow(id);
#else
                AdRewardInEditor(id);
#endif
            }
        }

        public static void RewVideoShow(int id) => Instance._RewardedShow(id);

#if UNITY_EDITOR
        private void AdRewardInEditor(int id)
        {
            GameObject obj = new GameObject { name = "TestVideoAd" };
            DontDestroyOnLoad(obj);
            Insides.CallingAnEvent call = obj.AddComponent(typeof(Insides.CallingAnEvent)) as Insides.CallingAnEvent;
            call.StartCoroutine(call.CallingAd(infoYG.durationOfAdSimulation, id));
        }
#endif
        #endregion Rewarded Video Show

        #region URL
        [DllImport("__Internal")]
        private static extern void OpenURL(string url);

        public static void OnURL(string url)
        {
            try
            {
                OpenURL(url);
            }
            catch (Exception error)
            {
                Debug.LogError("The first method of following the link failed! Error:\n" + error + "\nInstead of the first method, let's try to call the second method 'Application.OpenURL'");
                Application.OpenURL(url);
            }
        }

        public void _OnURL_Yandex_DefineDomain(string url)
        {
            url = "https://yandex." + EnvironmentData.domain + "/games/" + url;
            Message("URL Transition (yandexGames.DefineDomain) url: " + url);
#if !UNITY_EDITOR
            if (EnvironmentData.domain != null && EnvironmentData.domain != "")
            {
                OnURL(url);
            }
            else Debug.LogError("OnURL_Yandex_DefineDomain: Domain not defined!");
#else
            Application.OpenURL(url);
#endif
        }

        public void _OnAnyURL(string url)
        {
            Message("Any URL Transition. url: " + url);
#if !UNITY_EDITOR
            OnURL(url);
#else
            Application.OpenURL(url);
#endif
        }
        #endregion URL

        #region Leaderboard
        [DllImport("__Internal")]
        private static extern void SetLeaderboardScores(string nameLB, int score);

        public static void NewLeaderboardScores(string nameLB, int score)
        {
            if (Instance.infoYG.leaderboardEnable && auth)
            {
                if (Instance.infoYG.saveScoreAnonymousPlayers == false &&
                    playerName == "anonymous")
                    return;

#if !UNITY_EDITOR
                Message("New Liderboard Record: " + score);
                SetLeaderboardScores(nameLB, score);
#else
                Message($"New Liderboard '{nameLB}' Record: {score}");
#endif
            }
        }

        public static void NewLBScoreTimeConvert(string nameLB, float secondsScore)
        {
            if (Instance.infoYG.leaderboardEnable && auth)
            {
                if (Instance.infoYG.saveScoreAnonymousPlayers == false &&
                    playerName == "anonymous")
                    return;

                int result;
                int indexComma = secondsScore.ToString().IndexOf(",");

                if (secondsScore < 1)
                {
                    Debug.LogError("You can't record a record below zero!");
                    return;
                }
                else if (indexComma <= 0)
                {
                    result = (int)(secondsScore);
                }
                else
                {
                    string rec = secondsScore.ToString();
                    string sec = rec.Remove(indexComma);
                    string milSec = rec.Remove(0, indexComma + 1);
                    if (milSec.Length > 3) milSec = milSec.Remove(3);
                    else if (milSec.Length == 2) milSec += "0";
                    else if (milSec.Length == 1) milSec += "00";
                    rec = sec + milSec;
                    result = int.Parse(rec);
                }

                NewLeaderboardScores(nameLB, result);
            }
        }

        [DllImport("__Internal")]
        private static extern void GetLeaderboardScores(string nameLB, int maxQuantityPlayers, int quantityTop, int quantityAround, string photoSizeLB, bool auth);

        public static void GetLeaderboard(string nameLB, int maxQuantityPlayers, int quantityTop, int quantityAround, string photoSizeLB)
        {
            void NoData()
            {
                LBData lb = new LBData()
                {
                    technoName = nameLB,
                    entries = "no data",
                    players = new LBPlayerData[1]
                    {
                        new LBPlayerData()
                        {
                            name = "no data",
                            photo = null
                        }
                    }
                };
                onGetLeaderboard?.Invoke(lb);
            }

#if !UNITY_EDITOR
            if (Instance.infoYG.leaderboardEnable)
            {
                Message("Get Leaderboard");
                GetLeaderboardScores(nameLB, maxQuantityPlayers, quantityTop, quantityAround, photoSizeLB, _auth);
            }
            else
            {
                NoData();
            }
#else
            Message("Get Leaderboard - " + nameLB);

            if (Instance.infoYG.leaderboardEnable)
            {
                int indexLB = -1;
                LBData[] lb = Instance.infoYG.leaderboardSimulation;
                for (int i = 0; i < lb.Length; i++)
                {
                    if (nameLB == lb[i].technoName)
                    {
                        indexLB = i;
                        break;
                    }
                }

                if (indexLB >= 0)
                    onGetLeaderboard?.Invoke(lb[indexLB]);
                else
                    NoData();
            }
            else
            {
                NoData();
            }
#endif
        }
        #endregion Leaderboard

        #region Payments
        [DllImport("__Internal")]
        private static extern void BuyPaymentsInternal(string id);

        public static void BuyPayments(string id)
        {
#if !UNITY_EDITOR
            BuyPaymentsInternal(id);
#else
            Message($"Buy Payment. ID: {id}");
            Instance.OnPurchaseSuccess(id);
#endif
        }

        public void _BuyPayments(string id) => BuyPayments(id);


        [DllImport("__Internal")]
        private static extern void GetPaymentsInternal();

        public static void GetPayments()
        {
            Message("Get Payments");
#if !UNITY_EDITOR
            GetPaymentsInternal();
#else
            Instance.PaymentsEntries("");
#endif
        }

        public void _GetPayments() => GetPayments();

        public static Purchase PurchaseByID(string ID)
        {
            for (int i = 0; i < purchases.Length; i++)
            {
                if (purchases[i].id == ID)
                {
                    return purchases[i];
                }
            }

            return null;
        }

        [DllImport("__Internal")]
        private static extern void ConsumePurchaseInternal(string id);

        public static void ConsumePurchaseByID(string id)
        {
#if !UNITY_EDITOR
            ConsumePurchaseInternal(id);
#endif
        }

        [DllImport("__Internal")]
        private static extern void ConsumePurchasesInternal();

        public static void ConsumePurchases()
        {
#if !UNITY_EDITOR
            ConsumePurchasesInternal();
#endif
        }

        #endregion Payments

        #region Review Show
        [DllImport("__Internal")]
        private static extern void ReviewInternal();

        public void _ReviewShow(bool authDialog)
        {
            Message("Review");
#if !UNITY_EDITOR
            if (authDialog)
            {
                if (_auth) ReviewInternal();
                else _OpenAuthDialog();
            }
            else ReviewInternal();
#else
            ReviewSent("true");
#endif
        }

        public static void ReviewShow(bool authDialog)
        {
            Instance._ReviewShow(authDialog);
        }
        #endregion Review Show

        #region Prompt
        [DllImport("__Internal")]
        private static extern void PromptShowInternal();

        public static void PromptShow()
        {
#if !UNITY_EDITOR
            if (EnvironmentData.promptCanShow)
                PromptShowInternal();
#else
            savesData.promptDone = true;
            SaveProgress();

            Instance.PromptDo?.Invoke();
            PromptSuccessEvent?.Invoke();
#endif
        }
        public void _PromptShow() => PromptShow();
        #endregion Prompt

        #region Sticky Ad
        [DllImport("__Internal")]
        private static extern void StickyAdActivityInternal(bool activity);

        public static void StickyAdActivity(bool activity)
        {
            if (activity) Message("Sticky Ad Show");
            else Message("Sticky Ad Hide");
#if !UNITY_EDITOR
            StickyAdActivityInternal(activity);
#endif
        }

        public void _StickyAdActivity(bool activity) => StickyAdActivity(activity);
        #endregion Sticky Ad


        // Receiving messages

        #region Fullscren Ad
        public static Action OpenFullAdEvent;
        public void OpenFullAd()
        {
            OpenFullscreenAd.Invoke();
            OpenFullAdEvent?.Invoke();
            nowFullAd = true;
        }

        public static Action CloseFullAdEvent;
        public void CloseFullAd(string wasShown)
        {
            nowFullAd = false;
            CloseFullscreenAd.Invoke();
            CloseFullAdEvent?.Invoke();
            timerShowAd = 0;
#if !UNITY_EDITOR
            if (wasShown == "true")
            {
                Message("Closed Ad Interstitial");
            }
            else
            {
                if (infoYG.adDisplayCalls == InfoYG.AdCallsMode.until)
                {
                    Message("Реклама не была показана. Ждём следующего запроса.");
                    ResetTimerFullAd();
                }
                else Message("Реклама не была показана. Следующий запрос через: " + infoYG.fullscreenAdInterval);
            }
#endif
        }
        public void CloseFullAd() => CloseFullAd("true");

        public void ResetTimerFullAd()
        {
            timerShowAd = infoYG.fullscreenAdInterval;
        }

        public static Action ErrorFullAdEvent;
        public void ErrorFullAd()
        {
            ErrorFullscreenAd.Invoke();
            ErrorFullAdEvent?.Invoke();
        }
        #endregion Fullscren Ad

        #region Rewarded Video
        private float timeOnOpenRewardedAds;

        public static Action OpenVideoEvent;
        public void OpenVideo()
        {
            OpenVideoEvent?.Invoke();
            OpenVideoAd.Invoke();
            nowVideoAd = true;
            timeOnOpenRewardedAds = Time.unscaledTime;
        }

        public static Action CloseVideoEvent;
        public void CloseVideo()
        {
            nowVideoAd = false;

            CloseVideoAd.Invoke();
            CloseVideoEvent?.Invoke();

            if (rewardAdResult == RewardAdResult.Success)
            {
                RewardVideoAd.Invoke();
                RewardVideoEvent?.Invoke(lastRewardAdID);
            }
            else if (rewardAdResult == RewardAdResult.Error)
            {
                ErrorVideo();
            }

            rewardAdResult = RewardAdResult.None;
        }

        public static Action<int> RewardVideoEvent;
        private enum RewardAdResult { None, Success, Error };
        private static RewardAdResult rewardAdResult = RewardAdResult.None;
        private static int lastRewardAdID;

        public void RewardVideo(int id)
        {
            lastRewardAdID = id;
#if UNITY_EDITOR
            if (!Instance.infoYG.testErrorOfRewardedAdsInEditor)
                timeOnOpenRewardedAds -= 3;
#endif
            rewardAdResult = RewardAdResult.None;

            if (Time.unscaledTime > timeOnOpenRewardedAds + 2)
            {
                if (Instance.infoYG.rewardedAfterClosing)
                {
                    rewardAdResult = RewardAdResult.Success;
                }
                else
                {
                    RewardVideoAd.Invoke();
                    RewardVideoEvent?.Invoke(id);
                }
            }
            else
            {
                if (Instance.infoYG.rewardedAfterClosing)
                    rewardAdResult = RewardAdResult.Error;
                else
                    ErrorVideo();
            }
        }

        public static Action ErrorVideoEvent;
        public void ErrorVideo()
        {
            ErrorVideoAd.Invoke();
            ErrorVideoEvent?.Invoke();
        }
        #endregion Rewarded Video

        #region Leaderboard
        public static Action<LBData> onGetLeaderboard;

        public void LeaderboardEntries(string data)
        {
            JsonLB jsonLB = JsonUtility.FromJson<JsonLB>(data);

            LBData lbData = new LBData()
            {
                technoName = jsonLB.technoName,
                isDefault = jsonLB.isDefault,
                isInvertSortOrder = jsonLB.isInvertSortOrder,
                decimalOffset = jsonLB.decimalOffset,
                type = jsonLB.type,
                entries = jsonLB.entries,
                players = new LBPlayerData[jsonLB.names.Length],
                thisPlayer = null
            };

            for (int i = 0; i < jsonLB.names.Length; i++)
            {
                lbData.players[i] = new LBPlayerData();
                lbData.players[i].name = jsonLB.names[i];
                lbData.players[i].rank = jsonLB.ranks[i];
                lbData.players[i].score = jsonLB.scores[i];
                lbData.players[i].photo = jsonLB.photos[i];
                lbData.players[i].uniqueID = jsonLB.uniqueIDs[i];

                if (jsonLB.uniqueIDs[i] == playerId)
                {
                    lbData.thisPlayer = new LBThisPlayerData
                    {
                        rank = jsonLB.ranks[i],
                        score = jsonLB.scores[i]
                    };
                }
            }

            onGetLeaderboard?.Invoke(lbData);
        }

        public void InitializedLB()
        {
            LBData lb = new LBData()
            {
                entries = "initialized"
            };
            onGetLeaderboard?.Invoke(lb);
            _initializedLB = true;
        }
        #endregion Leaderboard

        #region Payments
        public static Action GetPaymentsEvent;
        public static Purchase[] purchases = new Purchase[0];

        public void PaymentsEntries(string data)
        {
#if !UNITY_EDITOR
            JsonPayments paymentsData = JsonUtility.FromJson<JsonPayments>(data);
            purchases = new Purchase[paymentsData.id.Length];

            for (int i = 0; i < purchases.Length; i++)
            {
                purchases[i] = new Purchase();
                purchases[i].id = paymentsData.id[i];
                purchases[i].title = paymentsData.title[i];
                purchases[i].description = paymentsData.description[i];
                purchases[i].imageURI = paymentsData.imageURI[i];
                purchases[i].priceValue = paymentsData.priceValue[i];
                purchases[i].consumed = paymentsData.consumed[i];
            }
#else
            purchases = Instance.infoYG.purshasesSimulation;
#endif
            GetPaymentsEvent?.Invoke();
        }

        public static Action<string> PurchaseSuccessEvent;
        public void OnPurchaseSuccess(string id)
        {
            PurchaseByID(id).consumed = true;
            PurchaseSuccess?.Invoke();
            PurchaseSuccessEvent?.Invoke(id);
        }

        public static Action<string> PurchaseFailedEvent;
        public void OnPurchaseFailed(string id)
        {
            PurchaseFailed?.Invoke();
            PurchaseFailedEvent?.Invoke(id);
        }
        #endregion Payments

        #region Review
        public static Action<bool> ReviewSentEvent;
        public void ReviewSent(string feedbackSent)
        {
            EnvironmentData.reviewCanShow = false;

            bool sent = feedbackSent == "true" ? true : false;
            ReviewSentEvent?.Invoke(sent);
            if (sent) ReviewDo?.Invoke();
        }
        #endregion Review

        #region Prompt
        public static Action PromptSuccessEvent;
        public static Action PromptFailEvent;
        public void OnPromptSuccess()
        {
            savesData.promptDone = true;
            SaveProgress();

            PromptDo?.Invoke();
            PromptSuccessEvent?.Invoke();
            EnvironmentData.promptCanShow = false;
        }

        public void OnPromptFail()
        {
            PromptFail?.Invoke();
            PromptFailEvent?.Invoke();
            EnvironmentData.promptCanShow = false;
        }
        #endregion Prompt


        // The rest

        #region Update
        public static float timerShowAd;
#if !UNITY_EDITOR
        static float timerSaveCloud = 62;
#endif

        private void Update()
        {
            // Таймер для обработки показа Fillscreen рекламы
            timerShowAd += Time.unscaledDeltaTime;

            // Таймер для облачных сохранений
#if !UNITY_EDITOR
            if (infoYG.saveCloud)
                timerSaveCloud += Time.unscaledDeltaTime;
#endif
        }
        #endregion Update

        #region Json
        public class JsonLB
        {
            public string technoName;
            public bool isDefault;
            public bool isInvertSortOrder;
            public int decimalOffset;
            public string type;
            public string entries;
            public int[] ranks;
            public string[] photos;
            public string[] names;
            public int[] scores;
            public string[] uniqueIDs;
        }

        public class JsonPayments
        {
            public string[] id;
            public string[] title;
            public string[] description;
            public string[] imageURI;
            public string[] priceValue;
            public bool[] consumed;
        }
        #endregion Json
#endif

        #endregion


      
    }
}