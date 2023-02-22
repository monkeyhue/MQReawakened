﻿using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities;

public class TriggerReceiverEntity : SyncedEntity<TriggerReceiver>, ITriggerable
{
    public int NbActivationsNeeded => EntityData.NbActivationsNeeded;
    public int NbDeactivationsNeeded => EntityData.NbDeactivationsNeeded;
    public bool DisabledUntilTriggered => EntityData.DisabledUntilTriggered;
    public float DelayBeforeTrigger => EntityData.DelayBeforeTrigger;
    public bool ActiveByDefault => EntityData.ActiveByDefault;
    public TriggerReceiver.ReceiverCollisionType CollisionType => EntityData.CollisionType;

    private int _activations;
    private int _deactivations;

    public bool Activated = true;
    public bool Enabled = true;

    public ILogger<TriggerReceiverEntity> Logger { get; set; }

    public override void InitializeEntity() => Trigger(ActiveByDefault);

    public override object[] GetInitData(NetState netState) => new object[] { Activated ? 1 : 0 };

    public void TriggerStateChange(TriggerType triggerType, Room room, bool triggered)
    {
        Enabled = triggerType switch
        {
            TriggerType.Enable => true,
            TriggerType.Disable => false,
            _ => Enabled
        };

        Logger.LogTrace("Triggering state changed for receiver entity '{Id}'. Enabled: {Enabled}. " +
                        "Activations: {Current}/{Total}. Deactivations: {Current}/{Total}. Trigger: {Type}.",
            Id, Enabled,
            _activations, NbActivationsNeeded,
            _deactivations, NbDeactivationsNeeded,
            triggerType
        );

        if (!Enabled)
            return;

        switch (triggerType)
        {
            case TriggerType.Activate:
                if (triggered)
                    _activations++;
                else
                    _activations--;
                break;
            case TriggerType.Deactivate:
                if (triggered)
                    _deactivations++;
                else
                    _deactivations--;
                break;
        }
        
        if (_activations >= NbActivationsNeeded)
            Trigger(true);
        else if (_deactivations >= NbDeactivationsNeeded)
            Trigger(false);
    }

    public void Trigger(bool activated)
    {
        Activated = activated;

        Logger.LogTrace("Triggering entity '{Id}' to {Bool}.", Id, Activated);

        var entities = Room.Entities[Id];

        foreach (var entity in entities)
        {
            if (activated)
            {
                if (entity is IMoveable moveable)
                    moveable.GetMovement().Activate(Room.Time);
            }
            else
            {
                if (entity is IMoveable moveable)
                    moveable.GetMovement().Deactivate(Room.Time);
            }
        }

        SendTriggerState(activated);
    }

    public void SendTriggerState(bool activated) =>
        Room.SendSyncEvent(new TriggerReceiver_SyncEvent(Id.ToString(), Room.Time, "now", activated, 0));
}
