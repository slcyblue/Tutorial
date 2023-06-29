using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverManager : Singleton<ObserverManager>, IObserver
{
    #region Variables
    public enum ObserverType { None, Tutorial, MenuAlarm }
    List<Receiver> receivers = new List<Receiver>();
    #endregion

    public override void Initialize(Action CompleteCallback = null)
    {
        TutorialObserver tutorial = new TutorialObserver(ObserverType.Tutorial);
        MenuAlarmObserver menuAlarm = new MenuAlarmObserver(ObserverType.MenuAlarm);
        receivers.Add(tutorial);
        receivers.Add(menuAlarm);

        CompleteCallback?.Invoke();
    }

    public void AddReceiver(Receiver receiver)
    {
        receivers.Add(receiver);
    }

    public void RemoveReceiver(Receiver receiver)
    {
        receivers.Remove(receiver);
    }

    public void Notify(ObservingEnum type, params long[] param)
    {
        foreach (var item in receivers)
        {
            item.OnNotify(type, param);
        }
    }

    public Receiver GetReceiver(ObserverType receiver)
    {
        foreach(var item in receivers)
        {
            if (receiver == item.GetObserverType()) return item;
        }

        return null;
    }
}
