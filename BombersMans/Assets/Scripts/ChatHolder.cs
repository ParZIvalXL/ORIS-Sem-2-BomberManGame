using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ChatHolder
{ 
    public static List<string> messages = new List<string>();
    public static string messageStr = "";

    public static void AddMessage(MessagePackage message)
    {
        messages.Add(message.Content);
        UpdateAllMessages();
    }
    public static string UpdateAllMessages()
    {
        messageStr = messages.Aggregate(messageStr, (current, message) => current + message + "\n");
        return messageStr;
    }
    
    public static string GetAllMessages()
    {
        var res = "";
        res = messages.Aggregate(res, (current, message) => current + message + "\n");
        return res;
    }
}