using SimplePartLoader.Objects.Furniture.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FurnitureCollider : MonoBehaviour
{
    void Start()
    {
        gameObject.name = "MODUTILS_FURNITURECOLL_" + gameObject.name;
        gameObject.layer = LayerMask.NameToLayer("Items");
    }

    void OnDestroy()
    {
        ModUtilsFurniture muf = transform.GetComponent<ModUtilsFurniture>();
        if (muf)
            muf.ChildDestroyed();
    }
}