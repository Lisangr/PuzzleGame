using UnityEngine;
#if !UNITY_ANDROID
namespace YG.Example
{
    public class OpenAuthorizationDialog : MonoBehaviour
    {
        public void OpenAuthDialog()
        {
            YandexGame.AuthDialog();
        }
    }
}
#endif