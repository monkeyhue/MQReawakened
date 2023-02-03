﻿using A2m.Server;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class CharacterLightModel
{
    public CharacterCustomDataModel Customization { get; set; }
    public EquipmentModel Equipment { get; set; }
    public int PetItemId { get; set; }
    public bool Registered { get; set; }
    public int CharacterId { get; set; }
    public string CharacterName { get; set; }
    public int UserUuid { get; set; }
    public int Gender { get; set; }
    public int MaxLife { get; set; }
    public int CurrentLife { get; set; }
    public int GlobalLevel { get; set; }
    public int InteractionStatus { get; set; }
    public TribeType Allegiance { get; set; }
    public bool ForceTribeSelection { get; set; }
    public HashSet<int> DiscoveredStats { get; set; }

    public CharacterLightModel() => InitializeLists();

    public CharacterLightModel(string serverData)
    {
        var array = serverData.Split('[');

        Gender = int.Parse(array[0]);
        Customization = new CharacterCustomDataModel(array[1]);

        InitializeLists();
    }

    private void InitializeLists()
    {
        Equipment = new EquipmentModel();
        DiscoveredStats = new HashSet<int>();
    }

    public override string ToString() => GetLightCharacterData();

    public int GetGoId() => UserUuid + CharacterId;

    public string GetLightCharacterData()
    {
        var sb = new SeparatedStringBuilder('[');

        sb.Append(GetCharacterInformation());
        sb.Append(Customization);
        sb.Append(Equipment);
        sb.Append(PetItemId);
        sb.Append(Registered ? 1 : 0);
        sb.Append(BuildDiscoveredStats());

        return sb.ToString();
    }

    private string GetCharacterInformation()
    {
        var sb = new SeparatedStringBuilder('<');

        sb.Append(CharacterId);
        sb.Append(CharacterName);
        sb.Append(UserUuid);
        sb.Append(Gender);
        sb.Append(MaxLife);
        sb.Append(CurrentLife);
        sb.Append(GlobalLevel);
        sb.Append(InteractionStatus);
        sb.Append((int)Allegiance);
        sb.Append(ForceTribeSelection ? 1 : 0);

        return sb.ToString();
    }

    private string BuildDiscoveredStats()
    {
        var sb = new SeparatedStringBuilder('<');

        foreach (var item in DiscoveredStats)
            sb.Append(item);

        return sb.ToString();
    }
}
