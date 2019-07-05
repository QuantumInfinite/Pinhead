using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualInputModule : MonoBehaviour
{
    [SerializeField]
    private RectTransform cursorImage;

    public float movementSpeed = 100;

    public bool useController = false;
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
        
        

        if (useController)
        {
            //controller
            vMousePos.x += Input.GetAxis("RStickX") * Time.deltaTime * movementSpeed * 10;
            vMousePos.y += Input.GetAxis("RStickY") * Time.deltaTime * movementSpeed * 10;
            vMousePos.x = Mathf.Clamp(vMousePos.x, 0, Screen.width);
            vMousePos.y = Mathf.Clamp(vMousePos.y, 0, Screen.height);
        }
        else
        {
            if (PointOnScreen(Input.mousePosition) && pMousePos != Input.mousePosition) {
                Vector3 movement = Input.mousePosition - pMousePos;
                vMousePos.x += movement.x;
                vMousePos.y += movement.y;
                pMousePos = Input.mousePosition;
            }
        }

        


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

    public void SetEnabled(bool enabled)
    {
        cursorImage.gameObject.SetActive(enabled);
    }

}
