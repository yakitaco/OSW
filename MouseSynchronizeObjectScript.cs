using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine;

public class MouseSynchronizeObjectScript : MonoBehaviour {
	// 位置座標
	private Vector3 position;
	// スクリーン座標をワールド座標に変換した位置座標
	private Vector3 screenToWorldPointPosition;
	
	private astar ast;
	
	public GameObject TextObj;
	
        Vector2Int spos = new Vector2Int(0, 0);
        Vector2Int epos = new Vector2Int(0, 0);
    int count = 0;
    public MapCtl GridMap; 
	
	public LineRenderer renderer;
	
	
	// Use this for initialization
	void Start () {
		ast = gameObject.GetComponent<astar>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonUp(0)) {
            Tap(Input.mousePosition);
        }
        
        if (Input.GetMouseButtonDown(1)){
        	ChangeTile(Input.mousePosition);
        }
        
        /* 座標表示 */
        TextObj.transform.position = Input.mousePosition;
        Vector3 screenToWorldPointPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = GridMap.GetComponent<Grid>().LocalToCell(screenToWorldPointPosition);
        TextObj.GetComponent<Text>().text = "["+ GridMap.offset_tile2stg_x(cellPosition.x) +","+ GridMap.offset_tile2stg_y(cellPosition.y) +"]";
	}
	
    private void Tap(Vector3 point) {
    
    
    //  陸地をチェック
    //Vector3 screenToWorldPointPosition = Camera.main.ScreenToWorldPoint(point);
    //Vector3Int cellPosition = GridMap.GetComponent<Grid>().LocalToCell(screenToWorldPointPosition);
    //var ret = GridMap.landArea( new Vector2Int(GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y)));
    //		LineRenderer lr = Instantiate(renderer);
    //
	//// 線の幅
	//lr.SetWidth(0.1f, 0.1f);
	//// 頂点の数
	//lr.SetVertexCount(ret.Count);
	//// 頂点を設定
	//for (int i=0;i<ret.Count;i++){
	//	lr.SetPosition(i, GridMap.GetComponent<Grid>().CellToLocal(new Vector3Int(GridMap.offset_stg2tile_x(ret[i].x), GridMap.offset_stg2tile_y(ret[i].y), 0)) + new Vector3(0f, 0f, -1f));
	//}
    
        // タップ時の処理を記述
        point.z = 10.0f;
        Vector3 screenToWorldPointPosition = Camera.main.ScreenToWorldPoint(point);
        //Grid grid = transform.parent.GetComponent<Grid>();
        Vector3Int cellPosition = GridMap.GetComponent<Grid>().LocalToCell(screenToWorldPointPosition);
        Debug.Log("["+ cellPosition.x +","+ cellPosition.y +"]");
        GridMap.ClickMap(cellPosition);
        
        var ret = GridMap.chkOpenSea( new Vector2Int(GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y)) , 20,TileGroup.Sea);
        Debug.Log("[chkOpenSea = "+ ret +"]");
    
        //if (count == 0) {
        //	spos = new Vector2Int(GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y));
        //    count=1;
        //    
        //} else {
        //	epos = new Vector2Int(GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y));
		//	ast.getRoute(spos, epos, GridMap.tileMap, 2, TapCb); //A-Sterによる経路取得
        //	count=0;
        //}

    }
    
    private void ChangeTile(Vector3 point) {
        // タップ時の処理を記述
        point.z = 10.0f;
        Vector3 screenToWorldPointPosition = Camera.main.ScreenToWorldPoint(point);
        //Grid grid = transform.parent.GetComponent<Grid>();
        Vector3Int cellPosition = GridMap.GetComponent<Grid>().LocalToCell(screenToWorldPointPosition);
        switch (GridMap.tileMap[GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y)].getType()){
        	case TileType.DeepSea:/* 深い海 */
	        	GridMap.putTile(TileType.ShallowSea, GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y));
        		break;
        	case TileType.ShallowSea:/* 浅い海 */
    	    	GridMap.putTile(TileType.SandyBeach, GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y));
        		break;
        	case TileType.SandyBeach:/* 砂地 */
        		GridMap.putTile(TileType.Meadow, GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y));
        		break;
        	case TileType.Meadow:/* 緑地 */
        		GridMap.putTile(TileType.Forest, GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y));
        		break;
        	case TileType.Forest:/* 森 */
        		GridMap.putTile(TileType.Mountain, GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y));
        		break;
        	case TileType.Mountain:/* 山 */
        		GridMap.putTile(TileType.OilField,GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y));
        		break;
        		
        	default: /* 資源・その他 */
        		GridMap.putTile(TileType.DeepSea,GridMap.offset_tile2stg_x(cellPosition.x), GridMap.offset_tile2stg_y(cellPosition.y));
        		break;
        };
    }
    
    //タップ結果コールバック
    public void TapCb(List<Vector2Int> rt, int ret){
    		Debug.Log (string.Format ("Min: (ret={0})", ret));
    		LineRenderer lr = Instantiate(renderer);
    
		    // 線の幅
		    lr.SetWidth(0.1f, 0.1f);
		    // 頂点の数
		    lr.SetVertexCount(rt.Count);
		    // 頂点を設定
		    for (int i=0;i<rt.Count;i++){
		    	lr.SetPosition(i, GridMap.GetComponent<Grid>().CellToLocal(new Vector3Int(GridMap.offset_stg2tile_x(rt[i].x), GridMap.offset_stg2tile_y(rt[i].y), 0)) + new Vector3(0f, 0f, -1f));
		    }
    }
	
}
