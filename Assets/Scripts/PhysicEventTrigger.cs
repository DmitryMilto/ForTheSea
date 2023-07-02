using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Most of this script was copy-pasted from <see cref="EventTrigger"/> for allowing the extension of Unity's Event Trigger to accept physic events
/// </summary>
public class PhysicEventTrigger : MonoBehaviour
{
    /// <summary>
    /// Enumeration of events that should be caught (according to physic engine and available "On(...)" methods)
    /// </summary>
    public enum PhysicEventTriggerType
    {
        OnTriggerEnter,
        OnTriggerExit,
        OnTriggerStay,
        OnCollisionEnter,
        OnCollisionExit,
        OnCollisionStay,
        OnTriggerEnter2D,
        OnTriggerExit2D,
        OnTriggerStay2D,
        OnCollisionEnter2D,
        OnCollisionExit2D,
        OnCollisionStay2D
    }

    public List<Entry> Triggers
    {
        get => Delegates ??= new List<Entry>();
        set => Delegates = value;
    }

    public List<Entry> Delegates;

    [Serializable] public class Entry
    {
        public PhysicEventTriggerType eventID;
        public UnityEvent callback = new UnityEvent();
    }

    #region Callback invokers

    private void Execute(PhysicEventTriggerType id, Collision eventData)
    {
        foreach (var ent in Triggers.Where(ent => ent.eventID == id && ent.callback != null)) ent.callback.Invoke();
    }

    private void Execute(PhysicEventTriggerType id, Collision2D eventData)
    {
        foreach (var ent in Triggers.Where(ent => ent.eventID == id && ent.callback != null)) ent.callback.Invoke();
    }

    private void Execute(PhysicEventTriggerType id, Collider eventData)
    {
        foreach (var ent in Triggers.Where(ent => ent.eventID == id && ent.callback != null)) ent.callback.Invoke();
    }

    private void Execute(PhysicEventTriggerType id, Collider2D eventData)
    {
        foreach (var ent in Triggers.Where(ent => ent.eventID == id && ent.callback != null)) ent.callback.Invoke();
    }

    #endregion

    #region On(...) Triggers

    public virtual void OnCollisionEnter(Collision collision) => Execute(PhysicEventTriggerType.OnCollisionEnter, collision);
    public virtual void OnCollisionEnter2D(Collision2D collision) => Execute(PhysicEventTriggerType.OnCollisionEnter2D, collision);
    public virtual void OnCollisionExit(Collision collision) => Execute(PhysicEventTriggerType.OnCollisionExit, collision);
    public virtual void OnCollisionExit2D(Collision2D collision) => Execute(PhysicEventTriggerType.OnCollisionExit2D, collision);
    public virtual void OnCollisionStay(Collision collision) => Execute(PhysicEventTriggerType.OnCollisionStay, collision);
    public virtual void OnCollisionStay2D(Collision2D collision) => Execute(PhysicEventTriggerType.OnCollisionStay2D, collision);
    public virtual void OnTriggerStay2D(Collider2D collision) => Execute(PhysicEventTriggerType.OnTriggerStay2D, collision);
    public virtual void OnTriggerStay(Collider collision) => Execute(PhysicEventTriggerType.OnTriggerStay, collision);
    public virtual void OnTriggerExit2D(Collider2D collision) => Execute(PhysicEventTriggerType.OnTriggerExit2D, collision);
    public virtual void OnTriggerExit(Collider collision) => Execute(PhysicEventTriggerType.OnTriggerExit, collision);
    public virtual void OnTriggerEnter2D(Collider2D collision) => Execute(PhysicEventTriggerType.OnTriggerEnter2D, collision);
    public virtual void OnTriggerEnter(Collider collision) => Execute(PhysicEventTriggerType.OnTriggerEnter, collision);

    #endregion

}
