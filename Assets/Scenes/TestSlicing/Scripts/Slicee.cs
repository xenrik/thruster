using BzKovSoft.ObjectSlicer;
using System;
using UnityEngine;

public class Slicee : BzSliceableObjectBase, IBzSliceableNoRepeat {
    public void Slice(Plane plane, int sliceId, Action<BzSliceTryResult> callBack) {
		Slice(plane, callBack);
    }

    protected override void OnSliceFinished(BzSliceTryResult result) {
    }

    protected override BzSliceTryData PrepareData(Plane plane) {
        var colliders = gameObject.GetComponentsInChildren<Collider>();

		return new BzSliceTryData() {
            componentManager = new StaticComponentManager(gameObject, plane, colliders),
            plane = plane,
            addData = true
        };
    }
}