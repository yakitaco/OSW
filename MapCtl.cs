using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEngine;
using System;

public class MapCtl : MonoBehaviour
{
	public Tilemap mainMap;
	private astar ast;
	static MapCtl instance;

    [SerializeField]
    public List<TileObj> tileList = new List<TileObj>();  //タイルの一次元リスト
    [SerializeField]
    public TileObj[,] tileMap;  //タイルの2次元配列
    
    bool startFLG = true;

    // Start is called before the first frame update
    void Awake()
    {
    	TileObj.initTileTlt();
		ast = gameObject.GetComponent<astar>();
		instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /* マップ上をクリック */
	public void ClickMap(Vector3Int tile_point) {
        // タップ時の処理を記述
        int pointX = offset_tile2stg_x(tile_point.x);
        int pointY = offset_tile2stg_y(tile_point.y);
        
        Debug.Log("["+ pointX +","+ pointY +"]=" + tileMap[pointX,pointY].getType());
        
	    
	}
	
    /// <summary>
    /// マップを生成します
    /// </summary>
    public void putTile(TileType type, int pos_X, int pos_Y)
    {
    	//Tilemapに描写
		var position = new Vector2Int( offset_stg2tile_x(pos_X), offset_stg2tile_y(pos_Y) );
		if (tileMap[pos_X, pos_Y]!=null){ //前のが残っている場合は消す
			tileList.Remove(tileMap[pos_X, pos_Y]);
		}
		tileMap[pos_X, pos_Y] = TileObj.set(type, new Vector2Int(pos_X,pos_Y));
		tileList.Add(tileMap[pos_X, pos_Y]);
	    mainMap.SetTile( new Vector3Int(position.x,position.y,0), tileMap[pos_X, pos_Y].tBase );
    }
    
    // タイルのステータスを取得します
    public TileType getTileStat(int pos_X, int pos_Y)
    {
	    return tileList.First(cell => cell.getPosX() == pos_X && cell.getPosY() == pos_Y).getType();
    }
    
    //タイルの指定範囲に資源を設定する(true 成功/false 失敗)
    public bool setRandfuel(int min_X, int max_X, int min_Y, int max_Y, int num){
    	List<TileObj> query = tileList.Where(cell => cell.getPosX() > min_X && cell.getPosY() > min_Y && cell.getPosX() < max_X && cell.getPosY() < max_Y && cell.getType() == TileType.Meadow).OrderBy(i => System.Guid.NewGuid()).Take(num).ToList();
    	foreach (TileObj q in query){
    		putTile(TileType.OilField, q.getPosX(), q.getPosY()); //資源に設定
    	}
    
    	return true;
    }
    
    //指定条件のタイル位置情報をリターン (type タイプ / adjoin_type 隣接タイプ (Noneなら指定せず))
    public List<Vector2Int> getSpcTilePos(Vector2Int minPos, Vector2Int maxPos, TileType type, TileType adjoin_type) {
    	List<Vector2Int> query = tileList.Where(cell => cell.getPosX() > minPos.x && cell.getPosY() > minPos.y && cell.getPosX() < maxPos.x && cell.getPosY() < maxPos.y && cell.getType() == type).OrderBy(i => System.Guid.NewGuid()).Select(cell => new Vector2Int(cell.getPosX(), cell.getPosY())).ToList();
		if (adjoin_type>0) query = query.Where(pos => chkAdjTileType(pos, adjoin_type).Count > 0).ToList();
		//Debug.Log("[tileList.Count="+ tileList.Count +"]");
    	return query;
    }
    
    //指定タイルの周囲に指定タイプのタイルが存在するかチェック
    public List<Vector2Int> chkAdjTileType(Vector2Int Pos, TileType adjoin_type){
		//6方向開く
		List<Vector2Int> query = new List<Vector2Int>();
		
        if (Pos.y%2==0){
			if((Pos.x>0)                   && (tileMap[Pos.x-1,Pos.y].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x-1,Pos.y)); // 右.
			if((Pos.x+1<StageCtl.TileLenX) && (tileMap[Pos.x+1,Pos.y].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x+1,Pos.y)); // 左.
			if((Pos.y>0)                   && (tileMap[Pos.x,Pos.y-1].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x,Pos.y-1)); // 右.
			if((Pos.y+1<StageCtl.TileLenY) && (tileMap[Pos.x,Pos.y+1].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x,Pos.y+1)); // 右.
			if((Pos.x>0)&&(Pos.y>0)        && (tileMap[Pos.x-1,Pos.y-1].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x-1,Pos.y-1)); // 右.
			if((Pos.x>0)&&(Pos.y+1<StageCtl.TileLenY) && (tileMap[Pos.x-1,Pos.y+1].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x-1,Pos.y+1)); // 右.
        } else {
			if((Pos.x>0)                   && (tileMap[Pos.x-1,Pos.y].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x-1,Pos.y)); // 右.
			if((Pos.x+1<StageCtl.TileLenX) && (tileMap[Pos.x+1,Pos.y].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x+1,Pos.y)); // 左.
			if((Pos.y>0)                   && (tileMap[Pos.x,Pos.y-1].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x,Pos.y-1)); // 右.
			if((Pos.y+1<StageCtl.TileLenY) && (tileMap[Pos.x,Pos.y+1].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x,Pos.y+1)); // 右.
			if((Pos.x+1<StageCtl.TileLenX) && (Pos.y>0)&& (tileMap[Pos.x+1,Pos.y-1].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x+1,Pos.y-1)); // 右.
			if((Pos.x+1<StageCtl.TileLenX)&&(Pos.y+1<StageCtl.TileLenY) && (tileMap[Pos.x+1,Pos.y+1].getType() == adjoin_type)) query.Add(new Vector2Int(Pos.x+1,Pos.y+1)); // 右.
        }
    	return query;
    }
    
	//マップ初期化(最初にコールすること)
    public void initTile()
    {
    	//Tilemapのクリア
		mainMap.ClearAllTiles();
		//タイル要素定義
		tileMap = new TileObj[StageCtl.TileLenX, StageCtl.TileLenY];
    }
    
    //指定位置が指定領域かチェック(sizeマス分チェック)(true:オープン/false:クローズ)
    public bool chkOpenSea(Vector2Int Pos, int size, TileGroup type){
    	bool[,] clist = new bool[size*2+1,size*2+1]; //チェック用配列
    	bool rets = true;
		for (int i = 0 ; i < size ; i++){
			List<Vector2Int> ret = new List<Vector2Int>();
			if (i==0){
				ret.Add(Pos);
				if(tileMap[Pos.x,Pos.y].getGroup() == type) {
					//自マス+周囲マスを有効にする。
						clist[size,size] = true;
						foreach (var ttl in tileRing(Pos, 1)){
							clist[ttl.x-Pos.x+size,ttl.y-Pos.y+size] = true;
						}
					} else {
						clist[size,size] = false;
					}
			} else {
				ret = tileRing( Pos, i );
				foreach(var tl in ret){
					if (clist[tl.x-Pos.x+size,tl.y-Pos.y+size] == true){
						if (tileMap[tl.x,tl.y].getGroup() == type){
							foreach (var ttl in tileRing(tl, 1)){
								clist[ttl.x-Pos.x+size,ttl.y-Pos.y+size] = true;
							}
						} else {
							clist[tl.x-Pos.x+size,tl.y-Pos.y+size] = false;
						}
					}
				}
			}
			rets = ret.Any( tl => clist[tl.x-Pos.x+size,tl.y-Pos.y+size] == true );
			if(!rets) break;
		}
    	return rets;
    }
    
    //指定位置からの陸続きの領域を返す(陸地専用)
    public List<Vector2Int> landArea (Vector2Int pos){
    	bool[,] clist = new bool[201, 201]; //チェック用配列
    	//clist[100,100] = true;
    	List<Vector2Int> ret = new List<Vector2Int>();
    	List<Vector2Int> tmp = new List<Vector2Int>();
    	tmp.Add(pos);
		while(tmp?.Count > 0) {
			List<Vector2Int> tmp2 = new List<Vector2Int>();
			foreach(var tl in tmp){
				clist[tl.x-pos.x+100, tl.y-pos.y+100] = true;
				tmp2.AddRange(tileRing(tl, 1).Where(tll => (((tileMap[tll.x,tll.y].getGroup() == TileGroup.Ground)||(tileMap[tll.x,tll.y].getGroup() == TileGroup.Mountain)) && clist[tll.x-pos.x+100,tll.y-pos.y+100] == false)).ToList());
    		}
    		ret.AddRange(tmp);
    		tmp = tmp2;
    	}
    	return ret;
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
    
    //Posを中心とした半径radのリング
    public List<Vector2Int> tileRing(Vector2Int Pos, int rad){
		List<Vector2Int> query = new List<Vector2Int>();
		for (int i =0 ;i< rad ; i++){
			Pos = neighbor(Pos, 4);
		}
		
		for(int i = 0 ; i < 6 ; i++){
			for(int j = 0 ; j < rad ; j++){
				query.Add(Pos);
				Pos = neighbor(Pos, i);
			}
		}
		
		return query.Where(pos => pos.x > -1 && pos.x < StageCtl.TileLenX && pos.y > -1 && pos.y < StageCtl.TileLenY).ToList();
    }
    
    //2点のヘキサマップ上での直線距離
		public int MapDist(Vector2Int aPos, Vector2Int bPos) {
		var dx = Mathf.Abs (aPos.x - bPos.x);
		var dy = Mathf.Abs (aPos.y - bPos.y);
		int _heuristic =  Mathf.Max(dy, dx + (dy/2) + (( ((aPos.y%2==0) && (bPos.y%2==1) && (aPos.x < bPos.x)) || ((bPos.y%2==0) && (aPos.y%2==1) && (bPos.x < aPos.x)) ) ? 1 : 0 ));

		return _heuristic;
	}
    
    //指定位置に指定タイプのオブジェクトが通れるかチェック
    public bool chkMoveObj(Vector2Int pos, uType type){
			switch(type){
				case uType.Warship:   /* 船 */
				case uType.Submarine: /* 潜水艦 */
					if (tileMap[pos.x, pos.y].getGroup() != TileGroup.Sea) return false;
					break;
				case uType.Vehicle: /* 車両 */
					if (tileMap[pos.x, pos.y].getGroup() != TileGroup.Ground) return false;
					break;
				case uType.Infantry: /* 人 */
					if (tileMap[pos.x, pos.y].getGroup() == TileGroup.Sea) return false;
					break;
			}
    	return true;
    }
    
	//Xのステージ座標→タイル座標変換
	public static int offset_stg2tile_x(int stg_x){
		return stg_x - StageCtl.TileLenX/2;
	}
	
	//Yのステージ座標→タイル座標変換
	public static int offset_stg2tile_y(int stg_y){
		return stg_y - StageCtl.TileLenY/2;
	}
	
	public static Vector2Int offset_stg2tile(Vector2Int stg_pos){
		return new Vector2Int(stg_pos.x - StageCtl.TileLenX/2, stg_pos.y - StageCtl.TileLenY/2);
	}
	
	//Xのタイル座標→ステージ座標変換
	public static int offset_tile2stg_x(int tile_x){
		return tile_x + StageCtl.TileLenX/2;
	}
	
	//Yのタイル座標→ステージ座標変換
	public static int offset_tile2stg_y(int tile_y){
		return tile_y + StageCtl.TileLenY/2;
	}
	
	//Yのタイル座標→ステージ座標変換
	public static Vector2Int offset_tile2stg(Vector2Int tile_pos){
		return new Vector2Int(tile_pos.x + StageCtl.TileLenX/2, tile_pos.y + StageCtl.TileLenY/2);
	}
	
	//Vector2座標→ステージ座標変換
	public static Vector2Int offset_vec2stg(Vector2 pos){
		Vector3Int cellPosition = instance.GetComponent<Grid>().LocalToCell(pos);
		return offset_tile2stg(new Vector2Int(cellPosition.x, cellPosition.y));
	}
	
	public static Vector2 offset_stg2vec(Vector2Int pos){
		Vector2Int tPos = offset_stg2tile(pos);
		Vector3 vPos = instance.GetComponent<Grid>().CellToLocal(new Vector3Int(tPos.x, tPos.y, 0));
		return new Vector2(vPos.x, vPos.y);
	}
    
    //ルート取得
    public void getRoute(Vector2Int startPos, Vector2Int goalPos, int unitType, Action<List<Vector2Int>, int> callback, List<Vector2Int> Olist = null, List<Vector2Int> Alist = null){
    	ast.getRoute(startPos, goalPos, tileMap, unitType, callback, Olist, Alist);
    }
    
}
