using System;

[Serializable]
public struct RoomId
{
    public string value;

    public RoomId(string v) { value = v; }

    public override string ToString() => value;
}

