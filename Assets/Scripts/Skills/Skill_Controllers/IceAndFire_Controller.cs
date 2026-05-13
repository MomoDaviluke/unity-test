using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceAndFire_Controller : ThounderStrike_Controller
{
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        base.OggerEnter2D(collision);
    }
}
