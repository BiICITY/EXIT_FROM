using System;
using UnityEngine;

[Serializable]
public struct GhostFrame
{
    public Vector2 move;   // (x,z)
    public bool interact;  // F
    public bool jump;      // Space
}
