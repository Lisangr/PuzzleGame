using UnityEngine;
using UnityEditor;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using UnityEngine.UI;
using YG;
#if !PLATFORM_WEBGL
[CustomEditor(typeof(LanguageYG))]
public class LocalizedTextEditor : Editor
{


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LanguageYG localizedText = (LanguageYG)target;

        EditorGUILayout.Space();

       // EditorGUILayout.LabelField("Translation", EditorStyles.boldLabel);


      
     //   localizedText.russianText = EditorGUILayout.TextField("Russian Text", localizedText.russianText);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Translate to English"))
        {
            localizedText.ru = localizedText.GetComponent<Text>().text;
            TranslateText(localizedText.ru, "ru", "en", (translation) =>
            {
             
                localizedText.en = translation;
                EditorUtility.SetDirty(localizedText);
               
            });
        }
        // Кнопка "Clear Texts"
        if (GUILayout.Button("Clear Texts"))
        {
            localizedText.ru = "";
            localizedText.en = "";
            EditorUtility.SetDirty(localizedText);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

      //  EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
            //   EditorGUILayout.TextField("English Text", localizedText.englishText);
    }

    private async void TranslateText(string text, string sourceLang, string targetLang, System.Action<string> callback)
    {
        // Формируем URL для запроса к Google Translate
        var url = $"https://translate.google.com/translate_a/single?client=gtx&dt=t&sl={sourceLang}&tl={targetLang}&q={WebUtility.UrlEncode(text)}";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Гарантирует успешный ответ (200 OK)

                string result = await response.Content.ReadAsStringAsync();

                // Простейший способ извлечь перевод из JSON-ответа
                var match = Regex.Match(result, @"\[\[\[""([^""]+)""");
                if (match.Success)
                {
                    callback(match.Groups[1].Value);
                }
                else
                {
                    Debug.LogError("Translation failed: Could not parse response");
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"Translation failed: {e.Message}");
            }
        }
    }

}
#endif
