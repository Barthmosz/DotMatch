using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    private bool isMoving = false;
    public int xIndex, yIndex;

    public enum InterpolationType
    {
        Linear,
        EaseIn,
        EaseOut,
        SmoothStep,
        SmootherStep
    }
    public InterpolationType interpolation = InterpolationType.SmootherStep;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move((int)transform.position.x + 1, (int)transform.position.y, 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move((int)transform.position.x - 1, (int)transform.position.y, 0.5f);
        }
    }

    public void SetCoordinates(int x, int y)
    {
        this.xIndex = x;
        this.yIndex = y;
    }

    public void Move(int x, int y, float timeToMove)
    {
        if (!this.isMoving)
        {
            StartCoroutine(MoveRoutine(new Vector3(x, y, 0), timeToMove));
        }
    }

    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;
        bool reachedDestination = false;
        float elapsedTime = 0f;
        this.isMoving = true;

        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                reachedDestination = true;
                transform.position = destination;
                SetCoordinates((int)destination.x, (int)destination.y);
                break;
            }

            elapsedTime += Time.deltaTime;
            float timeInterpolation = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

            switch (this.interpolation)
            {
                case InterpolationType.Linear:
                    break;
                case InterpolationType.EaseIn:
                    timeInterpolation = 1 - Mathf.Cos(timeInterpolation * Mathf.PI * 0.5f);
                    break;
                case InterpolationType.EaseOut:
                    timeInterpolation = Mathf.Sin(timeInterpolation * Mathf.PI * 0.5f);
                    break;
                case InterpolationType.SmoothStep:
                    timeInterpolation = timeInterpolation * timeInterpolation * (3 - 2 * timeInterpolation);
                    break;
                case InterpolationType.SmootherStep:
                    timeInterpolation = timeInterpolation * timeInterpolation * timeInterpolation * (timeInterpolation * (timeInterpolation * 6 - 15) + 10);
                    break;
            }

            transform.position = Vector3.Lerp(startPosition, destination, timeInterpolation);
            yield return null;
        }

        this.isMoving = false;
    }
}
