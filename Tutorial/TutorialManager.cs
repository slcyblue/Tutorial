using BackEnd;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : SingletonMono<TutorialManager>
{
    enum ClearType { Accumulation = 1, Reset = 2 }
    TutorialObserver observerMgr;
    SaveIntArr userSave;

    TutorialCSVContainer CSVContainer;
            
    public Asset reward { get; private set; }
    int crrTutorialId = 1;
    public long crrTutorialValue { get; private set; } = 0;
    public TutorialMissionCSV crrMissionCsv { get; private set; }
    TutorialCSV crrTutorialcsv;

    public List<TutorialUIManager> uiMgr { get; private set; } = new List<TutorialUIManager>();

    public bool isCleared { get; private set; } = false;
    #region Initialize
    public override void Initialize(Action CompleteCallback = null)
    {
        CSVContainer = TutorialCSVContainer.GetInstance();

        userSave = DB.Get<DatabaseSaves>().GetData(UserSaveEnum.tutorialId) as SaveIntArr;

        if (userSave != null)
        {
            var value = (List<int>)userSave.GetValue();
            if (value != null)
            {
                crrTutorialId = value[0];
                crrTutorialValue = value[1];
            }
        }
        else
        {
            DB.Get<DatabaseSaves>().ChangeValue((int)UserSaveEnum.tutorialId, new List<int>() { crrTutorialId, (int)crrTutorialValue });
            userSave = DB.Get<DatabaseSaves>().GetData(UserSaveEnum.tutorialId) as SaveIntArr;
        }

        uiMgr.Clear();
        ConnectObserver();

        CompleteCallback?.Invoke();
    }


    public void ConnectObserver()
    {
        var mgr = ObserverManager.GetInstance();
        if(mgr == null)
        {
            DebugX.Log("ObserverManager is Null");
            return;
        }

        observerMgr = mgr.GetReceiver(ObserverManager.ObserverType.Tutorial) as TutorialObserver;

        observerMgr.AddTutorialManager(this);
    }

    public void SetUIManager(TutorialUIManager uiMgr)
    {
        if (isCleared) return;
        if (this.uiMgr.Count > 0 && this.uiMgr.Contains(uiMgr)) return; 
        this.uiMgr.Add(uiMgr);
    }
    #endregion

    public void StartTutorial()
    {
        StartCoroutine(CoStartTutorial());
    }

    IEnumerator CoStartTutorial()
    {
        crrTutorialcsv = CSVContainer.GetTutorialCSV(crrTutorialId);
        if (crrTutorialcsv == null)
        {
            CompleteTutorial();
            yield break;
        }


        switch (crrTutorialcsv.type)
        {
            case 1:
                StartDialog();
                break;
            case 2:
                StartMission();
                break;
        }
    }

    void StartDialog()
    {
        PopupManager.GetInstance().OpenPopup(PopupEnum.DialogPopup, (callback) =>
        {
            DialogPopup popup;
            if (callback is DialogPopup) popup = callback as DialogPopup;
            else return;

            popup.StartDialog(crrTutorialcsv.missionId);
            popup.EndDialogCallback = ClearTutorial;
        });
    }

    void StartMission()
    {
        DebugX.Log("start tutorial mission");
        crrMissionCsv = CSVContainer.GetTutorialMissionCSV(crrTutorialcsv.missionId);
        reward = new Asset(crrTutorialcsv.rewardAssetType, crrTutorialcsv.rewardAssetId, crrTutorialcsv.rewardAssetCount);

        UpdateMission();
        observerMgr.UpdateMission((MissionEnum)crrMissionCsv.missionType);
    }

    object lockObj = new object();
    public void UpdateMission(params long[] param)
    {
        long originVal = crrTutorialValue;
        lock (lockObj)
        {
            switch ((ClearType)crrMissionCsv.clearType)
            {
                case ClearType.Accumulation:
                    crrTutorialValue = PlayerStorage.GetInstance().GetUserValue(crrMissionCsv.missionType, crrMissionCsv.missionArgs);
                    break;
                case ClearType.Reset:
                    if ((MissionEnum)crrMissionCsv.missionType == MissionEnum.MonsterKill) crrTutorialValue++;
                    else if (param != null || param.Length <= 0) crrTutorialValue += FindUserValue(param);
                    break;
            }
        }
        
        if(originVal != crrTutorialValue) UpdateUserData();

        if (uiMgr == null) return;

        foreach(var item in uiMgr)
        {
            item.UpdateUI();
            if (CheckMissionCleared()) item.SetClearUI();
        }
    }

    long FindUserValue(params long[] param)
    {
        if (param == null || param.Length <= 0) return 0;

        switch ((MissionEnum)crrMissionCsv.missionType)
        {
            case MissionEnum.Daily_GemAwakenCount:
            case MissionEnum.Daily_WeaponAwakenCount:
                return 1;
            case MissionEnum.WeaponAchieveLevel:
                var list = WeaponCSVContainer.GetInstance().GetWeaponCSVWithGroup((int)crrMissionCsv.missionArgs[0]);
                foreach (var item in list)
                {
                    if (item.id == param[0]) return param[1];
                }

                return 0;
            case MissionEnum.WeaponDecomposition:
            case MissionEnum.RuneDecomposition:
                return param[0];
            case MissionEnum.MonsterKill:
                return 1;
            case MissionEnum.GachaCount:
                int gachaType = (int)param[0];
                int gachaCount = (int)param[1];
                if (gachaType == crrMissionCsv.missionArgs[0]) return gachaCount;
                else return 0;
            case MissionEnum.WeaponOptionChange:
            case MissionEnum.RuneOptionChange:
                return param[1];
            case MissionEnum.SpecificItemUse:
                int itemType = (int)param[0];
                int itemId = (int)param[1];
                int itemCount = (int)param[2];
                if (itemType == crrMissionCsv.missionArgs[0] && itemId == crrMissionCsv.missionArgs[1]) return itemCount;
                else return 0;
            default:
                return 0;
        }
    }


    public bool CheckMissionCleared()
    {
        var needValue = crrMissionCsv.clearArgs;

        if (crrTutorialValue >= needValue) return true;

        return false;
    }

    public void ClearTutorial()
    {
        SoundManager.GetInstance().PlaySFX(SoundKey.ui_btnclick);

        if (crrTutorialcsv.type == 2)
        {
            PlayerStorage.GetInstance().Gain(reward);
            ResetValue();
        }

        crrTutorialId++;

        UpdateUserData();
#if UNITY_ANDROID && !UNITY_EDITOR

        Param param = new Param();
        string format = $"Tutorial #{crrTutorialId}";
        param.Add("Tutorial_complete", format);
        BackendService.GetInstance().SendLog(BackendLogEnum.TutorialClear, param);


        
        if (crrTutorialId <= 2) FirebaseLogger.LogEvent("Tutorial Start");
#endif


        StartTutorial();
    }

    void ResetValue()
    {
        crrTutorialValue = 0;
        observerMgr.UpdateMission(MissionEnum.None);
        foreach(var item in uiMgr)
        {
            item.ResetUI();
        }
    }

    void UpdateUserData()
    {
        if(userSave != null) userSave.RemoveData();
        userSave.AddData(crrTutorialId);
        userSave.AddData((int)crrTutorialValue);

        var backend = BackupManager.GetInstance();
        backend.AddToBackupList(userSave);
        backend.ShortenLocalTimer();
    }

    void CompleteTutorial()
    {
        foreach(var item in uiMgr)
        {
            item.gameObject.SetActive(false);
        }

        observerMgr.RemoveTutorialManager(this);
        isCleared = true;
    }
}
