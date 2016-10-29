using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using System;

public class WorldTileManager : MonoBehaviour
{
    bool[,] tileIsLive = new Boolean[12, 6];
    public Component[] activeTiles;

    void Start(){
        gameObject.AddComponent<Character>();
        GetComponent<Character>().Create();
        for (int x = 0; x < 12; x++){
            for(int y = 0; y < 6; y++){
                tileIsLive[x, y] = false;
                CreateTile(x, y);
            }
         }
    }

    void Update(){
        activeTiles = GetComponents<WorldTile>();

        Character Player = GetComponent<Character>();
     
        foreach (WorldTile extile in activeTiles)
        {
            int offsetX = (extile.tileX - Player.tileX)*1000;
            int offsetY = (extile.tileY - Player.tileY)*1000;
            extile.spritePos = new Vector3(offsetX-Player.cPos.x, offsetY-Player.cPos.y);
        }

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if ((Player.tileX + x <= 11) && (Player.tileX + x >= 0) && (Player.tileY + y <= 5) && (Player.tileY + y >= 0))
                {
                     if (tileIsLive[Player.tileX + x, Player.tileY + y] == false)
                    {
                        CreateTile(Player.tileX + x, Player.tileY + y);
                    }
                }
            }
        }

        if (CrossPlatformInputManager.GetButtonDown("Jump")) {
           
            foreach (WorldTile extile in activeTiles)
            {
                if (!extile.OnScreen()) {
                    tileIsLive[extile.tileX, extile.tileY] = false;
                    extile.Kill();
                    print("DELETION");
                }
            }
        }

    }

    void CreateTile(int x, int y){
        if (tileIsLive[x, y] == false)
        {
            WorldTile temp = gameObject.AddComponent<WorldTile>();
            temp.Create(x, y);
            tileIsLive[x, y] = true;
        }
        else {
            print("Tile already spawned.\n");
        }
    }

}

/*
 
 */
