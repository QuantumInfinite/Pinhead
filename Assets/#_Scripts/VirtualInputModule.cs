using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualInputModule : MonoBehaviour
{
    [SerializeField]
    private RectTransform cursorImage;
    // Start is called before the first frame update
    private Vector3 pMousePos;
    private Vector2 vMousePos;
    private Vector2 vOffset;

    // Update is
    // called once per frame
    void Start()
    {
        pMousePos = Input.mousePosition;
        vMousePos = pMousePos;
    }
    void Update()
    {
        /*
        if (PointOnScreen(Input.mousePosition) && pMousePos != Input.mousePosition)
        {
            Vector3 movement = Input.mousePosition - pMousePos;
            vMousePos.x += movement.x;
            vMousePos.y += movement.y;
            cursorImage.SetPositionAndRotation(vMousePos, Quaternion.identity);
            pMousePos = Input.mousePosition;
        }
        vMousePos.x = Mathf.Clamp(pMousePos.x + vOffset.x, 0, Screen.width);
        vMousePos.y = Mathf.Clamp(pMousePos.y + vOffset.y, 0, Screen.height);
        */
        vMousePos.x += Input.GetAxis("RStickX") * Time.deltaTime * 1000;
        vMousePos.y += Input.GetAxis("RStickY") * Time.deltaTime * 1000;
        vMousePos.x = Mathf.Clamp(vMousePos.x, 0, Screen.width);
        vMousePos.y = Mathf.Clamp(vMousePos.y, 0, Screen.height);
        cursorImage.SetPositionAndRotation(vMousePos, Quaternion.identity);

        //
    }

    bool PointOnScreen(Vector3 pos)
    {
        return pos.x >= 0 && pos.x <= Screen.width && pos.y >= 0 && pos.y <= Screen.height;
    }

    public Vector3 GetVirtualCursorPosition()
    {
        return vMousePos;
    }

    public void SetVirtualOffset(Vector3 offset)
    {
        vOffset = offset;
        vMousePos += vOffset;
    }

    public void ResetVirtualOffset()
    {
        vOffset = Vector2.zero;
        vMousePos -= vOffset;
    }

    public void Center()
    {
        vMousePos = Vector3.zero;
    }

    public void EnableCursor(bool enabled)
    {
        cursorImage.gameObject.SetActive(enabled);
    }

}
