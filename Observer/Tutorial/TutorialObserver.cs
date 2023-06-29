using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialObserver : Receiver
{
    public ObserverManager.ObserverType observerType { get; private set; }
    MissionEnum crrTutorial = MissionEnum.None;
    TutorialManager mgr = null;
    Action<long[]> UpdateCallback;

    public TutorialObserver(ObserverManager.ObserverType observerType)
    {
        this.observerType = observerType;
    }

    public ObserverManager.ObserverType GetObserverType()
    {
        return observerType;
    }

    public void OnNotify(ObservingEnum type, params long[] param)
    {
        if (mgr == null) return;

        if (FindTutorialType(type)) UpdateCallback?.Invoke(param);
        //else DebugX.LogWarning("Tutorial UI Manager is Null");
    }

    bool FindTutorialType(ObservingEnum type)
    {
        switch (type)
        {
            case ObservingEnum.StageClear:
                if (crrTutorial == MissionEnum.SpecificMapClear) 
                    return true;
                return false;
            case ObservingEnum.GachaCount:
                if (crrTutorial == MissionEnum.GachaCount)
                    return true;
                return false;

            case ObservingEnum.StatUpgrade:
            case ObservingEnum.StatAwaken:
                if (crrTutorial == MissionEnum.TotalSlotUpgradeLevel || crrTutorial == MissionEnum.SpecificStatUpgradeLevel)
                    return true;
                return false;

            case ObservingEnum.RuneUpgrade:
            case ObservingEnum.RuneAwaken:
                if (crrTutorial == MissionEnum.SpecificRuneAchieveGrade || crrTutorial == MissionEnum.Daily_GemAwakenCount || crrTutorial == MissionEnum.RuneAchieveLevel)
                    return true;
                return false;

            case ObservingEnum.RuneEquip:
                if (crrTutorial == MissionEnum.SpecificRuneEquip)
                    return true;
                return false;

            case ObservingEnum.RuneDecomposition:
                if (crrTutorial == MissionEnum.RuneDecomposition)
                    return true;
                return false;

            case ObservingEnum.WeaponUpgrade:
            case ObservingEnum.WeaponAwaken:
                if (crrTutorial == MissionEnum.SpecificWeaponAchieveGrade || crrTutorial == MissionEnum.Daily_WeaponAwakenCount || crrTutorial == MissionEnum.WeaponAchieveLevel)
                    return true;
                return false;

            case ObservingEnum.WeaponEquip:
                if (crrTutorial == MissionEnum.SpecificWeaponEquip)
                    return true;
                return false;

            case ObservingEnum.WeaponDecomposition:
                if (crrTutorial == MissionEnum.WeaponDecomposition)
                    return true;
                return false;

            case ObservingEnum.AssetGain:
                if (crrTutorial == MissionEnum.SpecificAssetGain)
                    return true;
                return false;

            case ObservingEnum.MonsterKill:
                if (crrTutorial == MissionEnum.MonsterKill)
                    return true;
                return false;

            case ObservingEnum.BuyShopProduct:
                if (crrTutorial == MissionEnum.BuyShopProduct)
                    return true;
                return false;

            case ObservingEnum.RuneOptionChange:
                if (crrTutorial == MissionEnum.RuneOptionChange)
                    return true;
                return false;

            case ObservingEnum.WeaponOptionChange:
                if (crrTutorial == MissionEnum.WeaponOptionChange)
                    return true;
                return false;

            case ObservingEnum.ItemUse:
                if (crrTutorial == MissionEnum.SpecificItemUse)
                    return true;
                return false;

            default:
                return false;
        }
    }

    public void AddTutorialManager(TutorialManager mgr)
    {
        this.mgr = mgr;
        UpdateCallback = mgr.UpdateMission;
    }

    public void RemoveTutorialManager(TutorialManager mgr)
    {
        this.mgr = null;
    }

    public void UpdateMission(MissionEnum mission)
    {
        crrTutorial = mission;
    }
}
