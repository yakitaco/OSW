using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEngine;

    // プレイヤー、CPUが見えるマップ
public class ViewMap
{
    // ベースとなるマップ
    public MapCtl baseMap;
	public Tilemap vMap;
	public Tilemap vMask;
	static TileBase black;
	static TileBase mask;
    
    [SerializeField]
    public List<TileObj> tileList;  //タイルの一次元リスト
    [SerializeField]
    public TileObj[,] tileMap;      //タイルの2次元配列

    public int[,] visible;   // ユニットによる視界リスト(1以上で視界あり)

    // タイル追加
    static ViewMap()
    {
        black = Resources.Load<TileBase>("TileMask/00_Black");
        mask  = Resources.Load<TileBase>("TileMask/01_Mask");
    }
    
    // 初期化
    public ViewMap(Tilemap _vMap = null, Tilemap _vMask = null)
    {
        vMap  = _vMap;
        vMask = _vMask;

    	//Tilemapのクリア
		tileList = new List<TileObj>();
		
		visible = new int[StageCtl.TileLenX, StageCtl.TileLenY];
		
		//タイル要素定義
		tileMap = new TileObj[StageCtl.TileLenX, StageCtl.TileLenY];
		for(int x = 0 ; x < StageCtl.TileLenX ; x++) {
			for(int y = 0 ; y < StageCtl.TileLenY ; y++) {
                if (vMask != null){
			        vMask.SetTile( new Vector3Int(MapCtl.offset_stg2tile_x(x),MapCtl.offset_stg2tile_y(y),0), mask );
			    }
			}
		}
    }
    
    

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // ユニット移動(move = 移動量, fov = 視野[>0])
    public void moveUnit(Vector2Int before, Vector2Int after, int fov){
        moveRemove(before, fov);
        moveAdd(after, fov);
        
    }
    
    // ユニット追加(fov:視界)
    public void moveAdd(Vector2Int pos, int fov){
        List<Vector2Int> vList = tileArea(pos, fov);
        foreach(Vector2Int _p in vList){
            visible[_p.x,_p.y]++;
            if ((visible[_p.x,_p.y] > 0) && (vMask != null)){
                vMask.SetTile( new Vector3Int(MapCtl.offset_stg2tile_x(_p.x),MapCtl.offset_stg2tile_y(_p.y),0), null );
            }
        }
    }
    
    // ユニット削除
    public void moveRemove(Vector2Int pos, int fov){
        List<Vector2Int> vList = tileArea(pos, fov);
        foreach(Vector2Int _p in vList){
            visible[_p.x,_p.y]--;
            if ((visible[_p.x,_p.y] == 0) && (vMask != null)){
                vMask.SetTile( new Vector3Int(MapCtl.offset_stg2tile_x(_p.x),MapCtl.offset_stg2tile_y(_p.y),0), mask );
            }
        }
    }
    
    Vector2Int [,] dirList = new Vector2Int[2,6] {{
     new Vector2Int (+1,  0), new Vector2Int ( 0, -1), new Vector2Int (-1, -1), 
     new Vector2Int (-1,  0), new Vector2Int (-1, +1), new Vector2Int ( 0, +1)},
    {new Vector2Int (+1,  0), new Vector2Int (+1, -1), new Vector2Int ( 0, -1), 
     new Vector2Int (-1,  0), new Vector2Int ( 0, +1), new Vector2Int (+1, +1)}};
    
    //隣を返す
    public Vector2Int neighbor(Vector2Int Pos, int dir){
	    int parity = Pos.y & 1;
	    Vector2Int dirs = dirList[parity,dir];
	    return new Vector2Int(Pos.x + dirs.x, Pos.y + dirs.y);
    }
    
    //Posを中心とした半径radの領域
    public List<Vector2Int> tileArea(Vector2Int Pos, int rad){
		List<Vector2Int> query = new List<Vector2Int>();
		query.Add(Pos); //自分の中心点をまず追加
		
		for (int i =0 ;i< rad ; i++){
			// 一つ外側へ移動
			Pos = neighbor(Pos, 4);
			Vector2Int Sp = Pos;
			// 移動した先をリング状に追加
			for(int j = 0 ; j < 6 ; j++){
				for(int k = 0 ; k < i+1 ; k++){
					query.Add(Sp);
					Sp = neighbor(Sp, j);
				}
			}
		}
		
		//領域範囲外を除く
		return query.Where(pos => pos.x > -1 && pos.x < StageCtl.TileLenX && pos.y > -1 && pos.y < StageCtl.TileLenY).ToList();
    }
    
    // 視界を開ける
    public void openView(Vector2Int Pos){
        if (tileMap[Pos.x, Pos.y].getType() == TileType.None){
            TileObj.set(baseMap.tileMap[Pos.x, Pos.y].getType(), new Vector2Int(Pos.x, Pos.y));
            tileList.Add(tileMap[Pos.x, Pos.y]);
            
        }
    }
    
    // 視界を閉じる
    public void closeView(Vector2Int Pos){
        
    }
    
    

}
