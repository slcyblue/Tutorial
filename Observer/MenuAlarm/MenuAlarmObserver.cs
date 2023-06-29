using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAlarmObserver : Receiver
{
    public ObserverManager.ObserverType observerType { get; private set; }

    List<MenuAlarmEnum> alarms = new List<MenuAlarmEnum>();
    MenuAlarmManager mgr = null;

    public MenuAlarmObserver(ObserverManager.ObserverType observerType)
    {
        this.observerType = observerType;
    }

    public void OnNotify(ObservingEnum type, params long[] param)
    {
        if (FindAlarmType(type, param) == null || alarms.Count <= 0) return;

       if(mgr!= null ) mgr.UpdateAlarm(alarms, param);
    }

    public ObserverManager.ObserverType GetObserverType()
    {
        return observerType;
    }

    List<MenuAlarmEnum> FindAlarmType(ObservingEnum type,params long[] param)
    {
        alarms.Clear();

        switch (type)
        {
            case ObservingEnum.Mail:
                alarms.Add(MenuAlarmEnum.Mail);
                return alarms;
            case ObservingEnum.GachaCount:
            case ObservingEnum.StageChallenge:
            case ObservingEnum.StageClear:
            case ObservingEnum.StatUpgrade:
            case ObservingEnum.StatAwaken:
            case ObservingEnum.RuneUpgrade:
            case ObservingEnum.RuneAwaken:
            case ObservingEnum.WeaponAwaken:
            case ObservingEnum.WeaponUpgrade:
                alarms.Add(MenuAlarmEnum.Daily_Mission);
                return alarms;
            case ObservingEnum.BuyShopProduct:
                alarms.Add(MenuAlarmEnum.Shop);
                return alarms;
            case ObservingEnum.AssetGain:
                if (param[0] == (int)AssetEnum.Weapon) alarms.Add(MenuAlarmEnum.Equip);
                else if(param[0] == (int)AssetEnum.Rune) alarms.Add(MenuAlarmEnum.Rune);

                return alarms;
            case ObservingEnum.Notice:
                alarms.Add(MenuAlarmEnum.Notice);
                return alarms;
            default:
                return null;
        }
    }

    public void AddMenuAlarmManager(MenuAlarmManager mgr)
    {
        this.mgr = mgr;
    }
}
