using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectionLine : MonoBehaviour
{
    public LineRenderer dottedProjectionLine;

    void Update()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        int mask = ~(1 << LayerMask.NameToLayer("Cursor"));
        mask &= ~(1 << LayerMask.NameToLayer("ParticipantPlayer"));
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            dottedProjectionLine.positionCount = 2;
            dottedProjectionLine.SetPosition(0, transform.position);
            dottedProjectionLine.SetPosition(1, hit.point);
        } 
        else
        {
            dottedProjectionLine.positionCount = 0;
        }
    }
}
