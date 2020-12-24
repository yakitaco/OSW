using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Linq;
using System;

public class StageCtl : MonoBehaviour
{

	public MapCtl map;
	public Tilemap vmap;
	public Sprite[] unitImages;
	
	public LineRenderer renderer;
	
	[SerializeField]
	public int _TileLenX = 100;
	public static int TileLenX = 100;
	
	[SerializeField]
	public int _TileLenY = 100;
	public static int TileLenY = 100;

	//UnitType uType;

	//ユニットリスト
	[SerializeField]
	//List<UnitObj> UnitList = new List<UnitObj>();
	public static List<List<UnitObj>> UnitList = new List<List<UnitObj>>();
	
	//ユニットコスト
	//static List<List<uCost>> UcList = new List<List<uCost>>();

	[SerializeField]
    List<PlayerCtl> players = new List<PlayerCtl>();

    public UnitObj baseUnit;  //ユニットのテンプレート。これをコピーする

	public UnitObj testUnit;

	private astar ast;
	
	//チーム通し番号
	public int teamnum = 0;



    void Awake()
    {
		TileLenX = _TileLenX;
		TileLenY = _TileLenY;
		UnitList.Add(new List<UnitObj>());
		UnitList.Add(new List<UnitObj>());
		
		/* ユニットタイプコンポーネント登録 */
		//uType = gameObject.AddComponent<UnitType>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ast = gameObject.GetComponent<astar>();
        
		//乱数初期化
	    float _seed1X = UnityEngine.Random.value * 100f;
	    float _seed1Y = UnityEngine.Random.value * 100f;
	    float _seed2X = UnityEngine.Random.value * 100f;
	    float _seed2Y = UnityEngine.Random.value * 100f;
	    
    	//マップ初期化
		map.initTile();
		for(int x = 0 ; x < TileLenX ; x++){
			for(int y = 0 ; y < TileLenY ; y++){
				if (( x == 0 )||( y == 0 )||( x == TileLenX-1 )||( y == TileLenY-1 )){
					map.putTile(TileType.DeepSea, x, y); //深い海
				//} else if (( x == 1 )||( y == 1 )||( x == TileLenX-2 )||( y == TileLenY-2 )){
				//	map.putTile(TileType.DeepSea, x, y); //浅い海
				} else {
	                //var rand = UnityEngine.Random.Range(0, 6);
	                float noise = Mathf.PerlinNoise(((float)x +_seed1X) / ((float)TileLenX / 25.0f), ((float)y +_seed1Y) / ((float)TileLenX / 25.0f)) + Mathf.PerlinNoise(((float)x +_seed2X) / ((float)TileLenX / 10.0f), ((float)y +_seed2Y) / ((float)TileLenX / 10.0f));
	                //Debug.Log(noise);
	                int rand = 0;
	                float tmp_x = ((float)x - (float)TileLenX/2.0f)/(float)TileLenX*2.0f;
	                float tmp_y = ((float)y - (float)TileLenY/2.0f)/(float)TileLenY*2.0f;
	                //Debug.Log(tmp_x);
	                rand = (int)(noise * 10.0f + 2.0f * Mathf.Min( 2.0f * Mathf.Cos(15.0f*tmp_x) + Mathf.Cos(9.0f*tmp_x), 1.0f) + 1.0f * Mathf.Min( 2.0f * Mathf.Cos(9.0f*tmp_y) + Mathf.Cos(3.0f*tmp_y), 1.0f));
	                //Debug.Log(((float)x - (float)TileLenX/2.0f)/(float)TileLenX*2.0f);
	                
	                //if (x < TileLenX/4) {
					//	rand = (int)(noise * 15 - (Mathf.Abs((float)x - (float)TileLenX/7.0f) )/((float)TileLenX/25.0f));
	                //} else if (x < TileLenX/4*3) {
	                //	rand = (int)(noise * 15 - (Mathf.Abs((float)x - (float)TileLenX/2.0f) )/((float)TileLenX/25.0f));
	                //} else {
	                //	rand = (int)(noise * 15 - (Mathf.Abs((float)x - (float)TileLenX/7.0f*6.0f) )/((float)TileLenX/25.0f));
	                //}
	                if (rand < 11){
	                	map.putTile(TileType.DeepSea, x, y); //深い海
	                } else if (rand < 13){
	                	map.putTile(TileType.ShallowSea, x, y); //浅い海
	                } else if (rand < 14) {
	                    if (( x < 4 )||( y < 4 )||( x > TileLenX-5 )||( y > TileLenY-5 )){
							map.putTile(TileType.ShallowSea, x, y); //浅い海
	                    } else {
	                        map.putTile(TileType.SandyBeach, x, y); //草地
	                    }
	                } else if (rand < 16) {
						map.putTile(TileType.Meadow, x, y); //砂地
					} else if (rand < 18) {
						map.putTile(TileType.Forest, x, y); //砂地
					} else {
	                    if (( x < 5 )||( y < 5 )||( x > TileLenX-4 )||( y > TileLenY-4 )){
	                    	map.putTile(TileType.ShallowSea, x, y); //浅い海
	                    } else {
							map.putTile(TileType.Mountain, x, y); //山
						}
					}
				}
			}
		}
		
		//資源設定
		for(int x = 0 ; x < 4 ; x++){
			for(int y = 0 ; y < 3 ; y++){
				map.setRandfuel(TileLenX/5*x, TileLenX/5*(x+1), TileLenY/3*y, TileLenY/3*(y+1), 3);
			}
		}
    
		//プレイヤー情報登録
		players.Add(new PlayerCtl("テスト1", 1, 0, new Color(0.75f, 0.0f, 0.0f, 1.0f)));teamnum++;
		players[0].vMap = new ViewMap(null, vmap);
		//UnitList.Add(new List<UnitObj>());
		//UcList[0].Add(new uCost());
		;

		
		players.Add(new PlayerCtl("テスト2", 0, 1, new Color(0.0f, 0.0f, 0.75f, 1.0f)));teamnum++;
		players[1].vMap = new ViewMap(null, null);
		//UnitList.Add(new List<UnitObj>());
		
		uCost.initUcost(teamnum);
		Debug.Log("uCost["+ uCost.fuel(0, Unit.LargeTransport)+"]");
		uCost.setUcost(0, Unit.OilWell, 10, 100, 100, Unit.Capital, Unit.LargeTransport);
		uCost.setUcost(0, Unit.NavalPort, 10, 100, 100, Unit.Capital, Unit.LargeTransport);
		uCost.setUcost(0, Unit.Airfield, 10, 100, 100, Unit.Capital, Unit.LargeTransport);
		uCost.setUcost(0, Unit.TransportShip, 10, 100, 100, Unit.NavalPort, null);
		uCost.setUcost(0, Unit.LargeTanker, 10, 100, 100, Unit.NavalPort, null);
		uCost.setUcost(0, Unit.Surveillance, 10, 100, 100, Unit.Airfield, null);
		uCost.setUcost(0, Unit.LandFighter, 10, 100, 100, Unit.Airfield, null);
		Debug.Log("uCost["+ uCost.fuel(0, Unit.LargeTransport)+"]");
		uCost.chgUcost(0, Unit.LargeTransport, 50, 50, 50);
		Debug.Log("uCost["+ uCost.fuel(0, Unit.LargeTransport)+"]");
		
		//首都
		for(int x = 4 ; x < 5 ; x++){
			for(int y = 1 ; y < 2 ; y++){
				List<Vector2Int> clist = map.getSpcTilePos(new Vector2Int(TileLenX/5*x, TileLenY/3*y), new Vector2Int (TileLenX/5*(x+1), TileLenY/3*(y+1)), TileType.Meadow, TileType.Meadow);
				if (clist.Count > 0){
					//Debug.Log("["+ clist.Count+"]");
					AddUnit("AAA" , new Vector2Int(clist[0].x,clist[0].y), players[0], Unit.Capital);
					AddUnit("AAA" , new Vector2Int(clist[1].x,clist[1].y), players[0], Unit.Fighter);
				} else {
					Debug.LogError("["+ clist.Count+"]");
				}
			}
		}
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.None)[0], players[0], Unit.LargeTransport);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.None)[0], players[0], Unit.LargeTransport);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.None)[0], players[0], Unit.LargeTransport);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.None)[0], players[0], Unit.LargeTransport);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.None)[0], players[0], Unit.LargeTransport);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.None)[0], players[0], Unit.LargeTransport);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.None)[0], players[0], Unit.LargeTransport);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.None)[0], players[0], Unit.LargeTransport);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.None)[0], players[0], Unit.LargeTransport);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.None)[0], players[0], Unit.LargeTransport);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , new Vector2Int(55,55), players[1], Unit.Tank);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		//AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		AddUnit("AAA" , map.getSpcTilePos(new Vector2Int(StageCtl.TileLenX-10,TileLenY/3), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY/3*2), TileType.DeepSea, TileType.None)[0], players[0], Unit.TransportShip);
		
		List<Vector2Int> clist2 = map.getSpcTilePos(new Vector2Int(0, 0), new Vector2Int (TileLenX, TileLenY), TileType.Meadow, TileType.ShallowSea);
		//ast.getRoute(map.chkAdjTileType(clist2[0], 1)[0], map.chkAdjTileType(clist2[1], 1)[0], map.tileMap, 0, TapCbs); //A-Sterによる経路取得
		
		List<Vector2Int> clist3 = map.tileRing(new Vector2Int(30, 30),5);
		//TapCbs(clist3);
		
		//ast.getRoute(clist2[0], clist2[1], map.tileMap, 0, TapCbs);
		//Debug.Log("["+ clist2[0] +"/"+ clist2[1] +"]");
		
		Debug.Log("["+ map.MapDist(new Vector2Int (10, 10), new Vector2Int (11, 10)));
		
    }

    // Update is called once per frame
    void Update()
    {
    	
    }
    
    

    
    //タップ結果コールバック
    public void TapCbs(List<Vector2Int> rt){
    
    		AddUnit("AAA" , new Vector2Int(rt[0].x,rt[0].y), players[0], Unit.Surveillance);
    		
    		testUnit.addMoveList(rt.Select(pos => new Vector2(map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(pos.x),MapCtl.offset_stg2tile_y(pos.y), 0)).x,
    		                                                                   map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(pos.x), MapCtl.offset_stg2tile_y(pos.y), 0)).y)).ToList());
    
		    // 線の幅
		    renderer.SetWidth(0.1f, 0.1f);
		    // 頂点の数
		    renderer.SetVertexCount(rt.Count);
		    // 頂点を設定
		    for (int i=0;i<rt.Count;i++){
		    	renderer.SetPosition(i, map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(rt[i].x), MapCtl.offset_stg2tile_y(rt[i].y), 0)) + new Vector3(0f, 0f, -1f));
		    }
    }
    
	//ユニット情報を登録(type 1:歩兵/2:車両/3:飛行機/4:船舶)
    public UnitObj AddUnit(String name ,Vector2Int point, PlayerCtl player, Unit type) {
        //UnitObj unit = Instantiate(baseUnit);
        //unit.Initialize(name, type, point, player, moveAmount, hp, fuel, atkmin, atkmax, atk, image);
        UnitObj unit = UnitObj.Create(name, type, point, player);
        //unit.gameObject.SetActive(true);
        //if (player.pnum == 0){
        //    unit.GetComponent<SpriteRenderer>().color = new Color(0.75f, 0.0f, 0.0f, 1.0f); //red transform.Find("Texture").
        //} else {
        //    unit.transform.GetComponent<SpriteRenderer>().color = new Color(0.0f, 0.0f, 0.75f, 1.0f); //blue
        //}
        //unit.transform.position = map.GetComponent<Grid>().GetCellCenterWorld( new Vector3Int( MapCtl.offset_stg2tile_x(point.x), MapCtl.offset_stg2tile_y(point.y), 0 ) ) - new Vector3(0f, 0f, 1f);
        //if (unit.type != uType.Building){
        //	UnitList[player.pnum].Insert(0, unit);  //先頭
        //} else {
        //	UnitList[player.pnum].Add(unit);  //末尾
        //}
        return unit;
    }
    
}
