using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChatMessages", menuName = "Scriptable Objects/ChatMessages")]
public class ChatMessages : ScriptableObject
{
    public List<ChatMessage> messages = new List<ChatMessage>();
}
