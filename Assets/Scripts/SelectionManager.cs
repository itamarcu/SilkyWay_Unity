using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    private Vector2 firstPressedPoint;
    private GameManager gm;
    private LineRenderer lineRenderer;

    // Use this for initialization
    private void Start()
    {
        firstPressedPoint = Vector2.zero;
        gm = GameManager.instance;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            firstPressedPoint =
                Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            lineRenderer.enabled = true;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 secondPressedPoint =
                Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

            float minX = Mathf.Min(firstPressedPoint.x, secondPressedPoint.x);
            float maxX = Mathf.Max(firstPressedPoint.x, secondPressedPoint.x);
            float minY = Mathf.Min(firstPressedPoint.y, secondPressedPoint.y);
            float maxY = Mathf.Max(firstPressedPoint.y, secondPressedPoint.y);
            Rect selectionRect = Rect.MinMaxRect(minX, minY, maxX, maxY);

            foreach (Bot bot in gm.allBots)
            {
                if (selectionRect.Contains(bot.transform.position))
                {
                    bot.SetHovered();
                }
            }

            lineRenderer.positionCount = 4;
            lineRenderer.loop = true;
            lineRenderer.SetPositions(new[]
            {
                new Vector3(minX, minY, 0),
                new Vector3(minX, maxY, 0),
                new Vector3(maxX, maxY, 0),
                new Vector3(maxX, minY, 0)
            });
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 secondPressedPoint =
                Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

            float minX = Mathf.Min(firstPressedPoint.x, secondPressedPoint.x);
            float maxX = Mathf.Max(firstPressedPoint.x, secondPressedPoint.x);
            float minY = Mathf.Min(firstPressedPoint.y, secondPressedPoint.y);
            float maxY = Mathf.Max(firstPressedPoint.y, secondPressedPoint.y);
            Rect selectionRect = new Rect(minX, minY, maxX - minX, maxY - minY);

            List<Bot> newSelecteds = new List<Bot>();
            foreach (Bot bot in gm.allBots)
            {
                if (bot.isAlive && bot.isConnected && selectionRect.Contains(bot.transform.position))
                {
                    newSelecteds.Add(bot);
                }
            }

            if (newSelecteds.Count > 0)
            {
                gm.currentlySelectedBots = newSelecteds;
				CancelUnselectedBotsSelectionExpectation ();
            }
            foreach (Bot bot in gm.allBots)
            {
                if (gm.currentlySelectedBots.Contains(bot))
                {
                    bot.SetSelected();
                }
                else
                {
                    bot.SetNotSelected();
                }
            }
            lineRenderer.enabled = false;
        }

		//Selection with keyboard
		float horizonalInputSelection =
			(Input.GetButtonDown("RightSelect") ? 1 : 0) + (Input.GetButtonDown("LeftSelect") ? -1 : 0);
		float verticalInputSelection = 
			(Input.GetButtonDown("UpSelect") ? 1 : 0) + (Input.GetButtonDown("DownSelect") ? -1 : 0);
		Vector2 inputSelectionDirection = new Vector2(horizonalInputSelection, verticalInputSelection);
		inputSelectionDirection.Normalize();

		if (inputSelectionDirection != Vector2.zero)
		{
			if (gm.currentlySelectedBots.Count == 0)
			{
				CancelUnselectedBotsSelectionExpectation();
				SelectBotAtRandom ();
			}
			else if (gm.currentlySelectedBots.Count > 1)
			{
				Bot bot = gm.currentlySelectedBots[0];
				DeselectAllBots();
				CancelUnselectedBotsSelectionExpectation();
				gm.currentlySelectedBots.Add(bot);
				bot.SetSelected();
			}
			else
			{
				// Select closest connected bot in that direction
				// = closest bot in that half of the screen
				Bot currentBot = gm.currentlySelectedBots[0];
				List<Bot> botsByDistance = new List<Bot>();
				botsByDistance.AddRange (gm.allBots);
				Dictionary<Bot, float> botDistances = new Dictionary<Bot, float> ();
				foreach (Bot bot in botsByDistance) {
					botDistances.Add(bot, (bot.transform.position - currentBot.transform.position).sqrMagnitude);
				}
				botsByDistance.Sort((b1, b2) => (int) (					botDistances[b1] - botDistances[b2]				));
				foreach (Bot bot in botsByDistance)
				{
					Vector2 delta = bot.transform.position - currentBot.transform.position;
					if (bot != currentBot && bot.isConnected && Vector2.Angle(delta, inputSelectionDirection) < 90.0f)
					{
						currentBot.SetNotSelected();
						bot.SetSelected();
						CancelUnselectedBotsSelectionExpectation();
						gm.currentlySelectedBots[0] = bot;
						break;
					}
				}
			}
		}
    }
	public void SelectBotAtRandom()
	{
		DeselectAllBots();
		//Select "first bot"
		foreach (Bot bot in gm.allBots)
		{
			if (bot.isConnected && bot.isAlive)
			{
				gm.currentlySelectedBots.Add(bot); //This can cause an exception of course :)
				gm.currentlySelectedBots[0].SetSelected();
				break;
			}
		}
		//Mwop mwop mwop mwooooww
	}

	private void DeselectAllBots()
	{
		foreach (Bot bot in gm.currentlySelectedBots)
		{
			bot.SetNotSelected();
		}
		gm.currentlySelectedBots.Clear();
	}

	/// <summary>
	/// Winner of the 2018 "Best Function Name" award
	/// </summary>
	private void CancelUnselectedBotsSelectionExpectation()
	{
		foreach (Bot bot in gm.allBots) {
			bot.expectsSelection = false;
		}
	}
}