using huqiang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CommandTransfer
{
    public static Action<string, string,string> SendMessage;
    public static void Send(string cmd, string json,string type)
    {
        if (SendMessage != null)
            SendMessage(cmd, json,type);
    }
    public static Action<string, string> HotFixCallback;
    public static void Call(string cmd,string json)
    {
        if (HotFixCallback != null)
            HotFixCallback(cmd,json);
    }
}
[Serializable]
public class req
{
    public string cmd;
    public string type;
    public int userId;
    public int pin;
    public string lan;
    public int err;
}
[Serializable]
public class reqs : req
{
    public string args;
}
