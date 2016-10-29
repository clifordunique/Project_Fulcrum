using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class WorldTile : MonoBehaviour {

    public Sprite sprite;
    public Vector3 spritePos;
    public int tileX;
    public int tileY;
    GameObject Tile;

    // Use this for initialization
    void Start (){
       
    }

    public void Move(Vector3 pos){
        spritePos = pos;
    }
	
    public GameObject Create(int x, int y){
        Tile = new GameObject("Tile_"+x+"-"+y);
        tileX = x;
        tileY = y;

        int numx = x;
        int numy = y;

        if (numx > 11)
        {
            numx -= 11;
        }
        if (numy > 5)
        {
            numy -= 5;
        }

        Texture2D texture1 = new Texture2D(1000, 100, TextureFormat.ARGB32, false);
        texture1.filterMode = FilterMode.Point;
        texture1.wrapMode = TextureWrapMode.Clamp;
        TextAsset bindata = Resources.Load("Tiles/"+numx+"-"+numy) as TextAsset;
        texture1.LoadImage(bindata.bytes);

        Rect rect = new Rect(0, 0, 1000, 1000);
        Vector2 pivot = new Vector2(0.0f, 0.0f);
        SpriteRenderer renderer = Tile.AddComponent<SpriteRenderer>();
        Sprite lol = Sprite.Create(texture1, rect, pivot, 1.0f, 0, SpriteMeshType.Tight, Vector4.zero);
        renderer.sprite = lol;


        Move(new Vector3((1000*x), (1000*y), 1));
        return Tile;
    }

    public void Kill() {
        Destroy(Tile);
        Destroy(this);
    }

    public bool OnScreen()
    {
        int scrW = Screen.width;
        int scrH = Screen.height;
        if ((spritePos.x+1000<(scrW/-2))||(spritePos.x>scrW/2)){
            return false;
        }
        if ((spritePos.y + 1000 < (scrH / -2)) || (spritePos.y > scrH / 2))
        {
            return false;
        }
        return true;
    }

	// Update is called once per frame
	void Update (){
  //      float h = CrossPlatformInputManager.GetAxis("Horizontal");
  //      float v = CrossPlatformInputManager.GetAxis("Vertical");
   //     spritePos = (spritePos + new Vector3(h * -200, v * -200, 0));
        Tile.transform.position = spritePos;
	}
}
