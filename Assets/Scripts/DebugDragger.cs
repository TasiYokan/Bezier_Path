using UnityEngine;
using System.Collections;

public class DebugDragger : MonoBehaviour
{
    private bool isDragging;
    private IDraggable target;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo;
            target = ReturnClickedObject(out hitInfo);
            if (target != null)
            {
                isDragging = true;

                target.OnDragged(
                    Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)));
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            //target?.OnDropped(
            //    Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)));
        }

        if (isDragging)
        {
            //target?.OnDragStay(
            //    Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)));
        }

    }

    /// <summary>
    /// It will ray cast to mousepostion and return any hit objet.
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    IDraggable ReturnClickedObject(out RaycastHit hit)
    {
        IDraggable target = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction * 10, out hit))
        {
            target = hit.collider.gameObject.GetComponent<IDraggable>();
        }

        return target;
    }
}
