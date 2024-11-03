using UnityEngine;
using YG;

public class RewardAdYandexGame : MonoBehaviour
{
    private void OnEnable() => YandexGame.RewardVideoEvent += Rewarded;

    private void OnDisable() => YandexGame.RewardVideoEvent -= Rewarded;


    public void Rewarded(int id)
    {
        if (id == 1)
        {
            Debug.Log("Получить награда 1");
        }

        else if (id == 2)
        {
            Debug.Log("Получить награда 2");
        }
    }

    public void ExampleOpenRewardAd(int id)
    {
        YandexGame.RewVideoShow(id);
    }
}