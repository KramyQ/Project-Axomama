using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBackgroundImageMove : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private RawImage _img;
    [SerializeField] private float _x, _y;
    [SerializeField] private int frameBeforeRedirect = 6000;
    private int direction = 1;
    
    
    void Update()
    {
        frameBeforeRedirect--;
        _img.uvRect = new Rect(_img.uvRect.position + new Vector2(direction*_x, direction*_y) * Time.deltaTime, _img.uvRect.size);
        if (frameBeforeRedirect == 0)
        {
            frameBeforeRedirect = 6000;
            direction = -direction;
        }
    }
}
