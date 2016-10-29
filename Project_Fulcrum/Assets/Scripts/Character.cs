using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class Character : MonoBehaviour {
    public Vector3 cPos; //Character position
    public Vector3 cVel; //Character velocity
    public int tileX; //Tile character is on
    public int tileY; //Tile character is on

    // Use this for initialization
    void Start()
    {
 
    }

    public void Create() {
        cPos = Vector3.zero;
        cVel = Vector3.zero;
        tileX = 0;
        tileY = 0;
    }

    public void Create(Vector3 position, int TileX, int TileY)
    {
        cPos = position;
        cVel = Vector3.zero;
        tileX = TileX;
        tileY = TileY;
    }

    // Update is called once per frame
    void Update()
    {
        cPos.x = cPos.x + 200*CrossPlatformInputManager.GetAxis("Horizontal");
        cPos.y = cPos.y + 200*CrossPlatformInputManager.GetAxis("Vertical");

        if (cPos.x > 1000) {
            tileX++;
            cPos.x -= 1000;
        }
        if (cPos.x < 0)
        {
            tileX--;
            cPos.x += 1000;
        }

        if (cPos.y > 1000)
        {
            tileY++;
            cPos.y -= 1000;
        }
        if (cPos.y < 0)
        {
            tileY--;
            cPos.y += 1000;
        }


    }
}
