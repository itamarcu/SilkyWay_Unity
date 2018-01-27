using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour
{
    public float robotMoveSpeed;
    public float robotTransmissionRadius;
    public float updateConnectivityClockPeriod;
    public float connectionDangerZoneFraction;
    [HideInInspector] public ResourceHolder resourceHolder;
    public Transmitter homeBase, endBase;

    public static GameManager instance;

    [HideInInspector] public List<Bot> currentlySelectedBots;

    [HideInInspector] public List<Bot> allBots;

    private void Awake()
    {
        GameManager.instance = this;
        allBots = Object.FindObjectsOfType<Bot>().ToList();
        currentlySelectedBots = new List<Bot>();
        homeBase.isConnected = true;
        resourceHolder = GetComponent<ResourceHolder>();
        StartCoroutine(UpdateConnectivityClock());
    }

    private void Update()
    {
        InterpretInput();
    }

    private void InterpretInput()
    {
        if (Input.GetButtonDown("Restart"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        float horizontalInputMovement = Input.GetAxis("HorizontalMove");
        float verticalInputMovement = Input.GetAxis("VerticalMove");
        Vector2 inputMovement = new Vector2(horizontalInputMovement, verticalInputMovement);
        inputMovement.Normalize();

        foreach (Bot selectedBot in currentlySelectedBots)
        {
            selectedBot.moveDirection = inputMovement * robotMoveSpeed;
            if (inputMovement != Vector2.zero)
            {
                selectedBot.rig.angularVelocity = 0;
            }
        }

        
    }

    private IEnumerator UpdateConnectivityClock()
    {
		yield return new WaitForSeconds(updateConnectivityClockPeriod);
		UpdateConnectivity();
		GetComponentInChildren<SelectionManager>().SelectBotAtRandom();
        while (true)
        {
			yield return new WaitForSeconds(updateConnectivityClockPeriod);
			UpdateConnectivity();     
        }
    }

    private void UpdateConnectivity()
    {
        endBase.ResetConnections();
        foreach (Bot bot in allBots)
        {
            bot.ResetConnections();
        }
        homeBase.SendAckPulse(robotTransmissionRadius);


        foreach (Bot bot in allBots)
        {
            // Check if the bot was reconnected this frame.
			if (!bot.wasConnectedPrevPulse && bot.isConnected)
            {
                bot.moveDirection = Vector2.zero;
                bot.advisedFaceDirection =
                    bot.connectedParents.First().Item2.transform.position - bot.transform.position;
				if (bot.expectsSelection && !currentlySelectedBots.Contains(bot))
				{
					currentlySelectedBots.Add (bot);
					bot.SetSelected ();
				}
            }

            // Check if the bot was disconnected this frame.
			if (bot.wasConnectedPrevPulse && !bot.isConnected)
            {
				if (currentlySelectedBots.Contains(bot) && bot.isAlive)
                {
                    currentlySelectedBots.Remove(bot);
					bot.SetNotSelected ();
					bot.expectsSelection = true;
                }
				bot.wasConnectedPrevPulse = false;
            }

            bot.bodyAnimator.SetBool("isConnected", bot.isConnected);
            bot.connectedParents.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        }
    }

    public void EndLevel()
    {
        // TODO some congratulatory message
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void EnterLevel(int levelID)
    {
        // TODO some congratulatory message
        SceneManager.LoadScene(levelID);
    }
}