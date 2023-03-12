using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int xIndex, yIndex;

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
        StartCoroutine(MoveRoutine(new Vector3(x, y, 0), timeToMove));
    }

    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;
        bool reachedDestination = false;
        float elapsedTime = 0f;

        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                reachedDestination = true;
                transform.position = destination;
                SetCoordinates((int)destination.x, (int)destination.y);
            }

            elapsedTime += Time.deltaTime;
            float timeInterpolation = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
            transform.position = Vector3.Lerp(startPosition, destination, timeInterpolation);

            yield return null;
        }
    }
}
