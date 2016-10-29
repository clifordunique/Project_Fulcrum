using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class WorldTile_backup : MonoBehaviour {

    public Sprite sprite;
    public Vector3 spritePos;
    GameObject Tile;

    // Use this for initialization
    void Start (){
        Rect rect = new Rect(0, 0, 1000, 1000);
        Vector2 pivot = new Vector2(0.5f, 0.5f);

        Texture2D texture1 = new Texture2D(1000, 1000);
        TextAsset bindata = Resources.Load("sprite") as TextAsset;
        texture1.LoadImage(bindata.bytes);


        Tile = new GameObject("Tile_0_0");
        SpriteRenderer renderer = Tile.AddComponent<SpriteRenderer>();
        Sprite lol = Sprite.Create(texture1, rect, pivot, 1.0f, 0, SpriteMeshType.Tight, Vector4.zero);
        renderer.sprite = lol;
    }

    public void Move(Vector3 pos){
        spritePos = pos;
    }
	
    public GameObject Create(int x, int y){
        GameObject test = new GameObject("Tile_"+x+"-"+y);
        Move(new Vector3(1000*x, 1000*y, 1));
        return test;
    }

	// Update is called once per frame
	void Update (){
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        spritePos = (spritePos + new Vector3(h * -10, v * -10, 0));
        Tile.transform.position = spritePos;
	}
}
