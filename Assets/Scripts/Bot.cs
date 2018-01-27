using System.Collections;
using UnityEngine;

public class Bot : Transmitter
{
    private const float SKITTER_MOVE_PERIOD = 0.85f;
    private const float SKITTER_STAY_PERIOD = 0.12f;
    private const float BOT_REORIENT_LERPING = 7.9312f;
    private const float HALO_ROTATION_SPEED = 45f;

    [HideInInspector] public Rigidbody2D rig;

    private ConnectionLine[] connectionDrawers;
    public Animator bodyAnimator;
    public float skitterTimeLeft;
    public Vector2 moveDirection;
    public Vector2 advisedFaceDirection;
    public SpriteRenderer selectionMark;
	public bool expectsSelection;

	private Vector2 birthplace;

    protected override void Awake()
    {
        base.Awake();
		connectionDrawers = GetComponentsInChildren<ConnectionLine> ();
        rig = GetComponent<Rigidbody2D>();
        skitterTimeLeft = Random.value * Bot.SKITTER_MOVE_PERIOD;
        advisedFaceDirection = Vector2.zero;
        transform.Rotate(Vector3.forward, Random.value * 360);
		birthplace = transform.position;
        connectionDrawers = gameObject.GetComponentsInChildren<ConnectionLine>();
    }

    private void Update()
    {
        if (isAlive)
        {
            if (moveDirection != Vector2.zero)
            {
                rig.velocity = moveDirection;
                bodyAnimator.SetBool("isMoving", true);
                advisedFaceDirection = moveDirection;
            }
            else
            {
                //Velocity will fall down to 0 from friction
                bodyAnimator.SetBool("isMoving", false);
            }

			if (skitterTimeLeft > Bot.SKITTER_MOVE_PERIOD && isConnected &&
				!(GameManager.instance.currentlySelectedBots.Count == 1 && GameManager.instance.currentlySelectedBots.Contains(this)))
            {
                rig.velocity = Vector2.zero;
                bodyAnimator.SetBool("isMoving", false);
            }

            for (int i = 0; i < connectionDrawers.Length; i++)
            {
                if (connectedParents.Count <= i)
                {                    
                    connectionDrawers[i].lineRenderer.enabled = false;
                }
                else
                {
                    connectionDrawers[i].lineRenderer.enabled = true;
                    connectionDrawers[i].lineRenderer.SetPositions(new[]
                        {transform.position, connectedParents[i].Item2.transform.position});
//                    if ((transform.position - connectedParents.First().Item2.transform.position).magnitude /
//                        GameManager.instance.robotTransmissionRadius >
//                        GameManager.instance.connectionDangerZoneFraction)
//                    {
//                        connectionDrawers[i].lineRenderer.material.color = Color.red;
//                    }
//                    else
//                    {
//                        connectionDrawers[i].lineRenderer.material.color = Color.cyan;
//                    }
                }
            }

            skitterTimeLeft += Time.deltaTime;
            if (skitterTimeLeft > Bot.SKITTER_MOVE_PERIOD + Bot.SKITTER_STAY_PERIOD)
            {
                skitterTimeLeft = (Random.value + .5f) * Bot.SKITTER_MOVE_PERIOD;
            }

            if (advisedFaceDirection != Vector2.zero)
            {
                float angleToTarget = Vector2.Angle(transform.up, advisedFaceDirection);
                if (angleToTarget >= 170f) // Avoid flipping the spider.
                {
                    transform.up = Vector3.Slerp(transform.up, transform.right,
                        Bot.BOT_REORIENT_LERPING * Time.deltaTime * 2);
                }
                else
                {
                    transform.up = Vector3.Slerp(transform.up, advisedFaceDirection,
                        Bot.BOT_REORIENT_LERPING * Time.deltaTime);
                }
            }
        }
        
        //Rotate selection mark
        selectionMark.transform.Rotate(Vector3.forward, Time.deltaTime * Bot.HALO_ROTATION_SPEED);
    }

    public void BurnInLava()
    {
        StartCoroutine(LavaBurnAnimation());
    }

    private IEnumerator LavaBurnAnimation()
    {
        // Disable
        isConnected = false;
        isAlive = false;
        rig.velocity = Vector2.zero;
        rig.angularVelocity = 0;
        moveDirection = Vector2.zero;
        foreach (Collider2D collider2 in GetComponentsInChildren<Collider2D>())
        {
            collider2.enabled = false;
        }

        foreach (ConnectionLine connectionLine in connectionDrawers)
        {
            connectionLine.lineRenderer.enabled = false;
        }

        // Spin and shrink
        float spinTimeLeft = 0.9f;
        Vector3 originalScale = transform.localScale;
        while (spinTimeLeft > 0)
        {
            spinTimeLeft -= Time.deltaTime;
            transform.Rotate(Vector3.forward, Time.deltaTime * 1800 * (1 - spinTimeLeft));
            transform.localScale = originalScale * (spinTimeLeft + 0.1f);
            yield return new WaitForEndOfFrame();
        }

        // Teleport to spawn
		transform.position = birthplace;

        // Unspin and unshrink
        spinTimeLeft = 0.7f;
        while (spinTimeLeft > 0)
        {
            spinTimeLeft -= Time.deltaTime;
            transform.Rotate(Vector3.forward, -Time.deltaTime * 1800 * (1 - spinTimeLeft));
            transform.localScale = originalScale * (1 - spinTimeLeft);
            yield return new WaitForEndOfFrame();
        }

        // Undo punishment
        transform.localScale = originalScale;
        foreach (Collider2D collider2 in GetComponentsInChildren<Collider2D>())
        {
            collider2.enabled = true;
        }
        isAlive = true;
        isConnected = true;
        wasConnectedPrevPulse = true;
    }

    public void SetHovered()
    {
        selectionMark.enabled = true;
        selectionMark.color = new Color(1f, 0.8f, 0f, 1.0f);
    }
    public void SetSelected()
    {
		expectsSelection = true;
        selectionMark.enabled = true;
        selectionMark.color = new Color(1f, 0.6f, 0f, 0.8f);
    }
    public void SetNotSelected()
    {
		expectsSelection = false;
        selectionMark.enabled = false;
    }
}