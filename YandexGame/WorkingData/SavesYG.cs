using UnityEngine.Serialization;

    [System.Serializable]
    public class SavesYG
    {
        // "Технические сохранения" для работы плагина (Не удалять)
        public int idSave;
        public bool isFirstSession = true;
        public string language = "ru";
        public bool promptDone;


        public int firstGames;
        public string saveDataStatisticsPlayer;
        public string savePetJson;
        public string saveTrailJson;
        public string saveSkinJson;
        [FormerlySerializedAs("saveWeaponJson")] public string saveBlockJson;
        public int openLevel;
        public int currentLevel = 1;
        public int soundOn = 1;
        public float floorTower;
        public int riberhtIndex = 0;
    }
