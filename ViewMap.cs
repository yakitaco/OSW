using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    // プレイヤー、CPUが見えるマップ
public class ViewMap : MonoBehaviour
{
    // ベースとなるマップ
    MapCtl baseMap;
    
    [SerializeField]
    public List<TileObj> tileList;  //タイルの一次元リスト
    [SerializeField]
    public TileObj[,] tileMap;      //タイルの2次元配列

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //Viewマップ初期化(最初にコールすること)
    public void initTile()
    {
    	//Tilemapのクリア
		tileList = new List<TileObj>();
		//タイル要素定義
		tileMap = new TileObj[StageCtl.TileLenX, StageCtl.TileLenY];
    }
    
    // ユニット移動(move = 移動量, fov = 視野[>0])
    public void moveUnit(Vector2Int before, Vector2Int after, int fov){
        
    }
    
    // ユニット追加()
    public void moveAdd(Vector2Int pos, int fov){
        
    }
    
    // ユニット削除
    public void moveRemove(Vector2Int pos, int fov){
        
    }
    
    Vector2Int [,] dirList = new Vector2Int[2,6] {{ new Vector2Int (+1,  0), new Vector2Int( 0, -1), new Vector2Int(-1, -1), 
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
    public List<Vector2Int> tileRing(Vector2Int Pos, int rad){
		List<Vector2Int> query = new List<Vector2Int>();
		query.Add(Pos); //自分の中心点をまず追加
		
		for (int i =0 ;i< rad ; i++){
			// 一つ外側へ移動
			Vector2Int Sp = neighbor(Pos, 4);
			
			// 移動した先をリング状に追加
			for(int j = 0 ; j < 6 ; j++){
				for(int k = 0 ; k < rad ; k++){
					query.Add(Sp);
					Sp = neighbor(Sp, j);
				}
			}
		}
		

		
		//領域範囲外を除く
		return query.Where(pos => pos.x > -1 && pos.x < StageCtl.TileLenX && pos.y > -1 && pos.y < StageCtl.TileLenY).ToList();
    }
    

}
