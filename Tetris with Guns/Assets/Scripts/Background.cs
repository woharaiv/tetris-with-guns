using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour
{
    List<GameObject> bulletHoles = new List<GameObject>();
    [SerializeField] RawImage backgroundImage;
    [SerializeField] public Vector2 movementVelocity = Vector2.one;

    [SerializeField] float backgroundScalar = 0.1f;

    new Camera camera;
    Vector2 cameraBasePos;

    public static Background instance;

    float xBound;
    float yBound;
    private void Start()
    {
        xBound = 16f;
        yBound = 9f;
        camera = Camera.main;
        cameraBasePos = camera.transform.localPosition;
        instance = this;
    }
    private void Update()
    {
        //Scroll background image
        backgroundImage.uvRect = new Rect(backgroundImage.uvRect.position - (movementVelocity * Time.deltaTime * backgroundScalar), backgroundImage.uvRect.size);

        //Move bullet holes so they look like they're moving with the background, deleting them if they go offscreen. ToArray() copies the list so we aren't deleting from the same list the foreach is moving through.
        foreach (GameObject hole in bulletHoles.ToArray())
        {
            hole.transform.position += (Vector3)(movementVelocity * Time.deltaTime);

            if (hole.transform.position.x > xBound || hole.transform.position.x < -xBound || hole.transform.position.y > yBound || hole.transform.position.y < -yBound)
                DeleteHole(hole);
        }
    }

    public void ShootBackground(Vector2 pos, GameObject bulletHole)
    {
        GameObject hole = Instantiate(bulletHole, (Vector3)pos + Vector3.forward * 9f, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        bulletHoles.Add(hole);
        hole.transform.SetParent(transform, true);
    }
    private void DeleteHole(GameObject hole)
    {
        bulletHoles.Remove(hole);
        Destroy(hole);
    }
}
