using UnityEngine;
using System.Collections;

public class StickyGameObject : MonoBehaviour {

    public Transform parentTransform;
    public Vector3 offSet;
    public bool stickPosition;
    public bool stickRotation;

    public ControlsScript cScript;
    public int customHitBoxIndex = -1;

	void FixedUpdate()
    {
        if (customHitBoxIndex != -1
            && cScript.currentMove != null
            && cScript.currentMove.animMap.customHitBoxDefinition != null
            && cScript.currentMove.animMap.customHitBoxDefinition.customHitBoxes[customHitBoxIndex].activeFrames != null
            && cScript.currentMove.animMap.customHitBoxDefinition.customHitBoxes[customHitBoxIndex].activeFrames.Length > cScript.currentMove.currentFrame)
            offSet = cScript.currentMove.animMap.customHitBoxDefinition.customHitBoxes[customHitBoxIndex].activeFrames[cScript.currentMove.currentFrame].position.ToVector();

        if (parentTransform != null) {
            if (stickPosition)
                transform.position = parentTransform.position + offSet;

            if (stickRotation)
                transform.rotation = parentTransform.rotation;
        }


	}
}
