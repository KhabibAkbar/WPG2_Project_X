using UnityEngine;
using System.Collections.Generic;

public class Teleportable : MonoBehaviour
{
    public List<string> teleportIDs = new List<string>();

    [HideInInspector]
    public float lastTeleportTime;
}