using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class GoalTransmitter : Transmitter
{
    public float signalMaintenanceTimeNeeded;
    public float recieveRadius;

    private float timeSinceSignalMaintainStarted;
    private LineRenderer lineRenderer;

    protected override void Awake()
    {
        base.Awake();
        timeSinceSignalMaintainStarted = 0;
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (isConnected)
        {
            timeSinceSignalMaintainStarted += Time.deltaTime;
            RenderGoalSignal();
        }
        else
        {
            timeSinceSignalMaintainStarted = 0;
            lineRenderer.enabled = false;
        }

        if (timeSinceSignalMaintainStarted >= signalMaintenanceTimeNeeded)
        {
            OnTrigger();
        }
    }

    protected virtual void OnTrigger()
    {
        Object.FindObjectOfType<GameManager>().EndLevel();
    }

    private void RenderGoalSignal()
    {
        lineRenderer.enabled = true;
        List<Vector3> nodes = new List<Vector3>();
        Transmitter currentTransmitter = this;
        currentTransmitter.connectedParents.Sort((p1, p2) => p1.Item1.CompareTo(p2.Item1));
        while (currentTransmitter != GameManager.instance.homeBase)
        {
            nodes.Add(currentTransmitter.transform.position);
            currentTransmitter = currentTransmitter.connectedParents.First().Item2;
        }
        nodes.Add(currentTransmitter.transform.position);
        lineRenderer.positionCount = nodes.Count;
        lineRenderer.SetPositions(nodes.ToArray());
    }
}