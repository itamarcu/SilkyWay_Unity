using System.Collections;
using UnityEngine;

public class MenuBot : Bot {

    public override void ResetConnections()
    {
        if (isConnected)
            wasConnectedPrevPulse = true;
    }
}