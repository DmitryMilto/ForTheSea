using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelSelectController : MonoBehaviour
{
    [SerializeField] private SaveOnSceneLoad SaveSettings;

    void Awake()
    {
        SaveSettings.Init();
    }
    void Start()
    {
        SaveSettings.Save();
    }

    [Serializable]
    public class SaveOnSceneLoad
    {
        public bool MakeSave => makeSave; [SerializeField] private bool makeSave = true;
        public string Path => path; [SerializeField] private string path = "General/Last launch";
        [SerializeField, ReadOnly] private string lastLaunch;

        public void Init()
        {
            lastLaunch = ES3.KeyExists(Path) ? ES3.Load<DateTime>(Path).ToString("g") : "Never";
        }
        public void Save()
        {
            if (MakeSave is false || string.IsNullOrEmpty(Path)) return;
            ES3.Save(Path, DateTime.Now);
        }
    }
}
