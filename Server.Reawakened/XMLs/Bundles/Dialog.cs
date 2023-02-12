﻿using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

internal class Dialog : DialogXML, ILocalizationXml
{
    public string BundleName => "Dialog";
    public string LocalizationName => "DialogDict_en-US";

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = true;

        this.SetField<DialogXML>("_localizationDict", new Dictionary<int, string>());
        this.SetField<DialogXML>("_dialogDict", new Dictionary<int, List<Conversation>>());
    }

    public void EditXml(XmlDocument xml)
    {
    }

    public void ReadXml(string xml) => ReadDescriptionXml(xml);

    public void ReadLocalization(string xml) => ReadLocalizationXml(xml);

    public void FinalizeBundle()
    {
    }
}
