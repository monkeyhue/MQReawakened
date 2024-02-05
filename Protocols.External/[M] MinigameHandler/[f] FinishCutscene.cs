﻿using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._M__MinigameHandler;

public class FinishCutscene : ExternalProtocol
{
    public override string ProtocolName => "Mf";
    public ILogger<FinishCutscene> Logger { get; set; }

    public override void Run(string[] message)
    {
        var arenaObjectId = message[5];

        ITriggerComp trigger = null;

        if (Player.Room.Entities.TryGetValue(arenaObjectId, out var foundEntity))
            foreach (var component in foundEntity)
                if (component is ITriggerComp foundTrigger)
                    trigger = foundTrigger;

        if (trigger == null)
        {
            Logger.LogError("Could not find cutscene finish trigger for {Id}", arenaObjectId);
            return;
        }

        var players = new SeparatedStringBuilder('%');

        var ids = trigger.GetPhysicalInteractorIds().ToList();

        for (var i = ids.Count; i < 4; i++)
            ids.Add("0");

        foreach (var playerId in ids)
            players.Append(playerId);

        Player.SendXt("Mc", arenaObjectId, Player.Room.Time, 5f, players.ToString());
    }
}
