using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public struct ChatMessage : INetworkSerializable
{
    public string senderName;
    public string senderSanitizedName;
    public Color senderColor;
    public string message;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref senderName);
        serializer.SerializeValue(ref senderSanitizedName);

        serializer.SerializeValue(ref senderColor.r);
        serializer.SerializeValue(ref senderColor.g);
        serializer.SerializeValue(ref senderColor.b);
        serializer.SerializeValue(ref senderColor.a);

        serializer.SerializeValue(ref message);
    }
}