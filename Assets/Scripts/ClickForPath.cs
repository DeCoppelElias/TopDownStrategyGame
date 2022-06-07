using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickForPath : MonoBehaviour
{
    private void OnMouseDown()
    {
        this.transform.parent.GetComponent<Troop>().createPathLine();
    }
}
