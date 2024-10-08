﻿using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.GameObjects.Attributes;

public class InvisibilityControllerComp : Component<InvisibilityController>
{
    public int DetectionLevelRequired => ComponentData.DetectionLevelRequired;
    public bool ApplyInvisibility => ComponentData.ApplyInvisibility;
}
