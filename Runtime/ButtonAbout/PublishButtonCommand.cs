using NonsensicalKit.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublishButtonCommand : NonsensicalMono
{
    [SerializeField] private string m_Command;


    public void PublishCommand(string command)
    {
        Publish(command);
    }
}
