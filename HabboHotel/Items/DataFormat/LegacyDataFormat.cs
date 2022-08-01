﻿namespace Plus.HabboHotel.Items.DataFormat;

public class LegacyDataFormat : FurniObjectData
{
    private string _data = string.Empty;

    public string Data
    {
        get => _data;
        set
        {
            _data = value;
            RaiseDataUpdated();
        }
    }

    public override FurniDataStructure StructureType => FurniDataStructure.Legacy;

    public override string Serialize() => Data;

    public override void Store(string data) => Data = data;
}