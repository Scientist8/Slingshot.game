using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SlingShot : MonoBehaviour
{
    public LineRenderer[] lineRenderers;
    public Transform[] stripPositions;
    public Transform center;
    public Transform idlePosition;

    public Vector3 currentPosition;

    public float maxLength;

    public float bottomBoundary;

    bool isMouseDown;

    public GameObject characterPrefab;

    public float characterPositionOffset;

    public float force;

    public GameObject pointPrefab;
    public GameObject[] Points;
    public int numberOfPoints;
    Vector3 firstPoint = new Vector3(0, 2, 0);
    public GameObject parentPoint;  // For trajectory points

    Vector3 characterForce;


    Rigidbody characterRb;
    Collider characterCollider;

    void Start()
    {
        lineRenderers[0].positionCount = 2;
        lineRenderers[1].positionCount = 2;
        lineRenderers[0].SetPosition(0, stripPositions[0].position);
        lineRenderers[1].SetPosition(0, stripPositions[1].position);

        CreateCharacter();


        // Drawing point shapes for the trajectory
        Points = new GameObject[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            Points[i] = Instantiate(pointPrefab, transform.position + firstPoint, Quaternion.identity);
            Points[i].transform.parent = parentPoint.transform;
        }

    }

    void CreateCharacter()
    {
        characterRb = Instantiate(characterPrefab).GetComponent<Rigidbody>();
        characterCollider = characterRb.GetComponent<Collider>();
        characterCollider.enabled = false;

        characterRb.isKinematic = true;
    }

    void Update()
    {
        if (isMouseDown)
        {
             Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10;

            
            currentPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            currentPosition = center.position + Vector3.ClampMagnitude(currentPosition - center.position, maxLength);

            currentPosition = ClampBoundary(currentPosition);

            SetStrips(currentPosition);

            if (characterCollider)
            {
                characterCollider.enabled = true;
            }
        }
        else
        {
            ResetStrips();
        }

        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i].transform.position = PointPosition(i * 0.1f); // I wrote 0.1 seconds for flying time, may change later. Trajectory will show the path for 0.1 seconds.

            }

        }
        if (Input.GetMouseButton(0))
        {
            parentPoint.transform.localRotation = new Quaternion(-currentPosition.x, -currentPosition.y, -currentPosition.z, 1f);
        }
    }

    private void OnMouseDown()
    {
        isMouseDown = true;
    }

    private void OnMouseUp()
    {
        isMouseDown = false;
        Shoot();
    }

    void Shoot()
    {
        characterRb.isKinematic = false;
        characterForce = (currentPosition - center.position) * force * -1; // -1 because we want to shoot to the opposite direction
        characterRb.velocity = characterForce;

        characterRb = null;
        characterCollider = null; // Don't interact with sling before shooting
        Invoke("CreateCharacter", 2f);
    }

    void ResetStrips()
    {
        currentPosition = idlePosition.position;
        SetStrips(currentPosition);
    }

    void SetStrips(Vector3 position)
    {
        lineRenderers[0].SetPosition(1, position);
        lineRenderers[1].SetPosition(1, position);

        if (characterRb)
        {
            Vector3 dir = position - center.position;
            characterRb.transform.position = position + dir.normalized * characterPositionOffset;
            characterRb.transform.right = -dir.normalized;
        }
    }

    Vector3 ClampBoundary (Vector3 vector)
    {
        vector.y = Mathf.Clamp(vector.y, bottomBoundary, 1000);
        return vector;
    }

    Vector2 PointPosition(float t)
    {
        // To find the trajectory of an object I used the following physics formula: P = p1 + v * t = at^2 / 2
        Vector2 currentPointPos = transform.position + (characterForce.normalized * force * t) + 0.5f * Physics.gravity * (t * t);

        return currentPointPos;
    }
}
