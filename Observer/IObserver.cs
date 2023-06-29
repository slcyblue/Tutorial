using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObserver
{
    void AddReceiver(Receiver receiver);
    void RemoveReceiver(Receiver receiver);
    void Notify(ObservingEnum type, params long[] param);
}

public interface Receiver
{
    void OnNotify(ObservingEnum type, params long[] param);
    ObserverManager.ObserverType GetObserverType();
}