using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


//ユニットの種類
public enum uType
{
	None,
    Infantry,  //歩兵
    Vehicle,   //車両
    Aircraft,  //航空機
    Warship,   //戦艦
    Submarine, //潜水艦
    Building,  //建築物
}

//ユニットの種類
public enum Unit
{
	None,				//なし   --  0
    Infantry,			//歩兵隊 I1  1
    Tank,				//戦車隊 V1  2
    Artillery,			//砲兵隊 V2  3
    AntiAircraft,		//高射砲隊 V3   4
    Transport,			//輸送車 V4     5
    LargeTransport,		//大型輸送車 V5 6
    Battleship,			//戦艦 M1       7
    Cruiser,			//巡洋艦 M2     8
    Destroyer,			//駆逐艦 M3     9
    Carrier,			//空母 M4
    LightCarrier,		//軽空母 M5
    Submarine,			//潜水艦 M6
    TransportShip,		//輸送艦 M7
    LargeTransportShip,	//大型輸送艦 M8
    Tanker,				//油槽船 M9
    LargeTanker,		//大型油槽船 M10
    RepairShip,			//工作艦 M11
    Surveillance,		//偵察機 A1
    Fighter,			//戦闘機 A2
    Attacker,			//攻撃機 A3
    Bomber,				//爆撃機 A4
    LandFighter,		//陸上戦闘機 A5
    HeavyBomber,		//大型爆撃機 A6
    CargoAircraft,		//輸送機 A7
    Urban,				//都市 B1
    Capital,			//首都 B2
    Factory,			//工場 B3
    OilWell,			//油井 B4
    Refinery,			//製油所 B5
    Airfield,			//飛行場 B6
    NavalPort,			//軍港 B7
    Camp,				//野営地 B8
    Base,				//基地 B9
    Fortress,			//要塞 B10   34
}


//ユニットの種類
public enum WorkType
{
    None,				//何もしていない
    Building,			//建造中(製造中)
    Investigating,		//研究中
    Repairing,			//修理中(補給中)
    Refueling,			//給油中
    Attacking,			//攻撃中
    Searching,			//探索中
    Returning,			//帰還中
    Loading,			//搭載中
    Unloading,			//荷下中
    Moving,				//移動中(経路探索を含まない)
    PreMove,			//経路探索中
    Newing
}

//攻撃種別
public class atkType {
    int ground_gun_pow   = 100;
    int ground_shell_pow   = 100;
    int air_shell_ = 100;
    int fuelMax = 100;
    int fuel = 100;
}

public class UnitObj : MonoBehaviour {
	
    public Unit units; /* ユニット情報 */
    public uType type; /* ユニットタイプ */
    public WorkType works = WorkType.None; /* ユニットの状態 */
    SpriteRenderer unitImage; /* ユニットのスプライト */
    public List<UnitObj> loadUnits = new List<UnitObj>();  /* 搭載ユニットリスト */
    
	public String name;
    public Vector2Int pos; /* 今の位置 */
    [SerializeField]
    int lock_timer; /* 固定状態かをチェックするタイマー */
    [SerializeField]
    int lock_max = 100; /* 固定解除閾値 */
    [SerializeField]
    int lock_retry = 0; /* 固定解除リトライ数 */
        
    //public int team;  // playerState.pnumに吸収
    public PlayerCtl player;
    public int move = 5;  //移動量
    public int hpMax = 100;
    public int hp = 100;
    public int fuelMax = 100;
    public int fuel = 100;
    public int demic = 0;  //人口・乗員数
    public int demicMax = 100;  //人口・乗員数
    public int atkmin = 1; //攻撃範囲最小
    public int atkmax = 2; //攻撃範囲最大
    public atkType atkval = new atkType(); //攻撃(配列はUnitTypesに一致,0は攻撃対象外)
    public int moveEnd = 0; //1:移動実施済み
    
	public LineRenderer renderer;
    
    public int progress = 0;
    public int progressMax = 1000;
    
    public GameObject textures;
    public GameObject baseBullet;
    public GameObject ExploadObj;

    [SerializeField]
    private float speed = 0.5f;
    [SerializeField]
    private float speedMax = 0.5f;
    
    public float forgettime_min = 3.0f;
    public float forgettime_max = 10.0f;
    
    public float yawrate = 5.0f;
    public float yawMax = 5.0f;
    public GameObject targetObj;

	public static MapCtl map;

    public AlcType targetAlc;
    
    public TeamState team;
    
    private Rigidbody2D rid2;
    [SerializeField]
    private float chdeg;
    [SerializeField]
    private float chspd;
    [SerializeField]
    public float actTimer;		//アクション間隔用タイマー
    
	// ユニット一覧テンプレート
	public static List<UnitObj> uObjTlt = new List<UnitObj>(); 
	//public static List<Sprite> unitImages;
	static UnitObj instance;
    
	//移動リスト
	[SerializeField]
	List<Vector2> destList = new List<Vector2>();
	[SerializeField]
    public Vector2 LastDest; /* 最終目標(障害物等を考慮しない) */
	[SerializeField]
	int destDir = 999;  /* 移動末端と最終目標の距離差分 */
	
	private Material material;
	private float imgFill = 0.1f;
	
    void Awake()
    {
    	instance = this;
    	map = GameObject.Find("Grid").GetComponent<MapCtl>();
    }
	
	//ユニット情報初期化(ゲーム開始時に必ずコールすること)
	public static List<UnitObj> initUnitTlt(PlayerCtl p)
	{
		List<UnitObj> uObjTlt = new List<UnitObj>();
		//ユニットテンプレートを追加
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.None,     Unit.None,			100, 100, 100, 10, 10));		/* なし   */
        uObjTlt.Add(addObjTlt("001/I1_Infantry", uType.Infantry, Unit.Infantry,		100, 100, 100, 10, 10));		/* 歩兵隊 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Vehicle,  Unit.Tank,			100, 100, 100, 10, 10));		/* 戦車隊 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Vehicle,  Unit.Artillery,		100, 100, 100, 10, 10));		/* 砲兵隊 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Vehicle,  Unit.AntiAircraft,	100, 100, 100, 10, 10));		/* 高射砲隊 */
        uObjTlt.Add(addObjTlt("001/V4_Track", uType.Vehicle,  Unit.Transport,		100, 100, 100, 10, 10));		/* 輸送車 */
        uObjTlt.Add(addObjTlt("001/V4_Track", uType.Vehicle,  Unit.LargeTransport,	100, 100, 100, 1, 1));		/* 大型輸送車 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Warship,  Unit.Battleship,		100, 100, 100, 10, 10));		/* 戦艦 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Warship,  Unit.Cruiser,		100, 100, 100, 10, 10));		/* 巡洋艦 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Warship,  Unit.Destroyer,		100, 100, 100, 10, 10));		/* 駆逐艦 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Warship,  Unit.Carrier,		100, 100, 100, 10, 10));		/* 空母 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Warship,  Unit.LightCarrier,	100, 100, 100, 10, 10));		/* 軽空母 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Submarine, Unit.Submarine,		100, 100, 100, 10, 10));		/* 潜水艦 */
        uObjTlt.Add(addObjTlt("001/M7_TransportShip", uType.Warship,  Unit.TransportShip,	100, 100, 100, 1, 1));		/* 輸送艦 */
        uObjTlt.Add(addObjTlt("001/M7_TransportShip", uType.Warship,  Unit.LargeTransportShip,	 100, 100, 100, 1, 1));	/* 大型輸送艦 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Warship,  Unit.Tanker,			100, 100, 100, 10, 10));		/* 油槽船 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Warship,  Unit.LargeTanker,	100, 100, 100, 10, 10));		/* 大型油槽船 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Warship,  Unit.RepairShip,		100, 100, 100, 10, 10));		/* 工作艦 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Aircraft, Unit.Surveillance,	100, 100, 100, 10, 10));		/* 偵察機 */
        uObjTlt.Add(addObjTlt("001/A2_Fighter",  uType.Aircraft, Unit.Fighter,		100, 100, 100, 3.0f, 2.0f));		/* 戦闘機 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Aircraft, Unit.Attacker,		100, 100, 100, 10, 10));		/* 攻撃機 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Aircraft, Unit.Bomber,			100, 100, 100, 10, 10));		/* 爆撃機 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Aircraft, Unit.LandFighter,	100, 100, 100, 10, 10));		/* 陸上戦闘機 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Aircraft, Unit.HeavyBomber,	100, 100, 100, 10, 10));		/* 大型爆撃機 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Aircraft, Unit.CargoAircraft,	100, 100, 100, 10, 10));		/* 輸送機 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Building, Unit.Urban,			100, 100, 100, 10, 10));		/* 都市 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Building, Unit.Capital,		100, 100, 100, 0, 0));		/* 首都 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Building, Unit.Factory,		100, 100, 100, 0, 0));		/* 工場 */
        uObjTlt.Add(addObjTlt("001/B4_OilWell" , uType.Building, Unit.OilWell,		100, 100, 100, 0, 0));		/* 油井 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Building, Unit.Refinery,		100, 100, 100, 0, 0));		/* 製油所 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Building, Unit.Airfield,		100, 100, 100, 0, 0));		/* 飛行場 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Building, Unit.NavalPort,		100, 100, 100, 0, 0));		/* 軍港 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Building, Unit.Camp,			100, 100, 100, 0, 0));		/* 野営地 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Building, Unit.Base,			100, 100, 100, 0, 0));		/* 基地 */
        uObjTlt.Add(addObjTlt("001/B2_Refinery", uType.Building, Unit.Fortress,		100, 100, 100, 0, 0));		/* 要塞 */
        return uObjTlt;
	}
    
    //ユニットオブジェクトのテンプレート作成
    static UnitObj addObjTlt(String _image ,uType _type, Unit _units, int _cost, int _hpMax, int _fuelMax, float _speedMax, float _yawMax){
    	UnitObj newObj = Instantiate(instance);
    	newObj.unitImage = newObj.transform.GetComponent<SpriteRenderer>(); //newObj.transform.Find("Texture").GetComponent<SpriteRenderer>()
    	newObj.unitImage.sprite = Resources.Load<Sprite>("UnitImg/" + _image);
        newObj.units = _units;
        newObj.type = _type;
        newObj.hpMax = _hpMax;
    	newObj.fuelMax = _fuelMax;
    	newObj.speedMax = _speedMax;
    	newObj.yawMax = _yawMax;
    	newObj.yawrate = _yawMax;
    	newObj.gameObject.SetActive(false); //非表示
    	newObj.gameObject.name = "Tlt_" + _units.ToString();
    	
	    //switch (newObj.type){
	    //    case uType.Infantry:  /* 歩兵   */
    	//		newObj.gameObject.layer = LayerMask.NameToLayer("Infantry");
		//		break;
	    //    case uType.Vehicle:   /* 車両   */
    	//		newObj.gameObject.layer = LayerMask.NameToLayer("Vehicle");
		//		break;
	    //    case uType.Aircraft:  /* 航空機 */
    	//		newObj.gameObject.layer = LayerMask.NameToLayer("Aircraft");
		//		break;
	    //    case uType.Warship:   /* 戦艦   */
    	//		newObj.gameObject.layer = LayerMask.NameToLayer("Warship");
		//		break;
	    //    case uType.Submarine: /* 潜水艦 */
    	//		newObj.gameObject.layer = LayerMask.NameToLayer("Submarine");
		//		break;
	    //    case uType.Building:
    	//		newObj.gameObject.layer = LayerMask.NameToLayer("Building");
		//		break;
		//	default:              /* その他 */
		//		break;
        //
		//}
		newObj.gameObject.layer = LayerMask.NameToLayer(newObj.type.ToString());
    	
    	//建物の場合は、位置と向きを固定
    	if (_type == uType.Building){
	    	var rb = newObj.GetComponent<Rigidbody2D>();
	    	rb.constraints = RigidbodyConstraints2D.FreezeAll;
	    	newObj.GetComponent<CircleCollider2D>().isTrigger = true; //コライダーを無効化
	    	
    	}
    	
    	return newObj;
    }
    
    //ユニット作成(リスト追加は外部で実施すること)
    public static UnitObj Create(String _name, Unit _type, Vector2Int _pos, PlayerCtl _player)
    {
    	UnitObj newObj = Instantiate(_player.utltList.FirstOrDefault(u => u.units == _type));
	    //対象のシェーダー情報を取得
	    Shader sh = _player.utltList.FirstOrDefault(u => u.units == _type).transform.GetComponent<SpriteRenderer>().material.shader; //Find("Texture").
	    //取得したシェーダーを元に新しいマテリアルを作成
        Material mat = new Material(sh);
        newObj.transform.GetComponent<SpriteRenderer>().material = mat; //Find("Texture").
        
        newObj.name = _name;
        newObj.pos  = _pos;
        newObj.hp   = newObj.hpMax;
    	newObj.fuel = newObj.fuelMax;
    	newObj.gameObject.SetActive(true); //表示
    	newObj.lock_max = UnityEngine.Random.Range(10, 100);
    	newObj.player = _player;
    	newObj.gameObject.SetActive(true);
    	newObj.GetComponent<SpriteRenderer>().color = _player.pColor;
		newObj.chgWorkType(WorkType.Newing);
    	newObj.gameObject.name = "Obj_" + _player.pnum + "_" + _type.ToString();
    	
        if (newObj.type != uType.Building){
        	StageCtl.UnitList[newObj.player.pnum].Insert(0, newObj);  //先頭
        } else {
        	StageCtl.UnitList[newObj.player.pnum].Add(newObj);  //末尾
        }
    	newObj.transform.position = map.GetComponent<Grid>().GetCellCenterWorld( new Vector3Int( MapCtl.offset_stg2tile_x(_pos.x), MapCtl.offset_stg2tile_y(_pos.y), 0 ) ) - new Vector3(0f, 0f, 1f);
    	return newObj;
    }
    
    //ユニット破壊削除(リストから削除は外部で実施すること)
    public void delete()
    {
    	StageCtl.UnitList[player.pnum].Remove(this);
    	Destroy(gameObject, 1.0f);
    }
    
    void chgWorkType(WorkType _type){
    	if (type != uType.Building){
	    	switch (_type){
				case WorkType.Moving:      /* 移動中 */
				case WorkType.Attacking:   /* 攻撃中 */
				case WorkType.Loading:     /* 搭乗中 */
				case WorkType.Unloading:    /* 降載中 */
					var rb = this.GetComponent<Rigidbody2D>();
					rb.constraints = RigidbodyConstraints2D.None;

					break;
		        default: /* それ以外 */
		  	    	var rb2 = this.GetComponent<Rigidbody2D>();
		    		rb2.constraints = RigidbodyConstraints2D.FreezeAll;
		        	break;
	    	}
    	}
    	works = _type;
    }
    
    
	// Use this for initialization
	void Start () {
		if (GetComponent<Rigidbody2D>() != null){
			rid2 = GetComponent<Rigidbody2D>();
			rid2.velocity = Vector2.up * speed;
			changeMove();
		}
		material = gameObject.GetComponent<Renderer>().material;
		unitImage = transform.GetComponent<SpriteRenderer>(); //.Find("Texture")
	}
	
	public void addMoveList(List<Vector2> moveList){
		destList.AddRange(moveList);
	}
	
	// ユニットを作成 (false:失敗)
	public bool doBuildUnit(Unit _type) {
		Vector2Int? buildPos;
		if ((uCost.canCreate(player.pnum, _type, fuel, 0, this.units ))&&(works == WorkType.None)){
		
			//港の場合はどこか開いている場所にする
			if (this.units == Unit.NavalPort){
				buildPos = map.tileRing(pos, 1).FirstOrDefault(tll => ((map.tileMap[tll.x,tll.y].getGroup() == TileGroup.Sea)&&(StageCtl.UnitList[player.pnum].Any(u => (u.pos == tll)&&((u.type == uType.Warship)||(u.type == uType.Submarine))) == false)));
				if (buildPos == null) return false;
			} else {
				buildPos = pos;
			}
		
			//ユニット上に人・車がある場合はfalse
			if (StageCtl.UnitList[player.pnum].Any(u => (u.pos == buildPos)&&((u.type == uType.Infantry)||(u.type == uType.Vehicle))) == true){
				Debug.Log("ERR!! "+ _type);
				return false;
			}
			
			chgWorkType(WorkType.Building);
			fuel -= uCost.fuel(player.pnum, _type);
			
			//対象を作成
			UnitObj newObj2 = Create("BBB", _type, (Vector2Int)buildPos, player);
			
			//親が存在
			var _parents = uCost.chkParent(player.pnum, _type);
			if (_parents != null){
				UnitObj newObj = Create("BBB", (Unit)_parents, (Vector2Int)buildPos, player);
				newObj.transform.parent = transform;
				newObj2.chgWorkType(WorkType.Loading);
				newObj.transform.parent.GetComponent<UnitObj>().loadUnits.Add(newObj2);
				newObj2.transform.parent = newObj.transform;
			} else {
				newObj2.transform.parent = transform;
			}
			
			return true;
		}
		return false;
	}
	
	// ユニットを展開 (false:失敗)
	public bool doDeployUnit() {
		if (loadUnits.Count()<1) return false;
		
		//ユニット上に建物がある場合はfalse
		if (StageCtl.UnitList[player.pnum].Any(u => (u.pos == pos)&&((u.type == uType.Building))) == true){
			return false;
		}
			
	    switch (loadUnits[0].units){
	    	case Unit.OilWell: /* 油井 */
	    		/* 展開場所が油田の場合 */
	    		if (map.tileMap[pos.x, pos.y].getType() != TileType.OilField){
	    			return false;
	    		}
	    		break;
	        case Unit.LargeTransport: /* 大型輸送車 */
	        	break;
	        case Unit.Base: /* 軍港 */
	        	/* 展開場所の周囲に海がある場合 */
	        	if (map.tileRing(pos, 1).TrueForAll(tll => (((map.tileMap[tll.x,tll.y].getGroup() != TileGroup.Sea))))){
	    			return false;
	        	}
	        	break;
	        	
	        default: /* 資源・その他 */
	        	break;
	    }
	    loadUnits[0].gameObject.SetActive(true);
	    loadUnits[0].transform.parent = null;
	    loadUnits[0].chgWorkType(WorkType.Newing);
	    loadUnits[0].pos = pos;
	    loadUnits.Remove(loadUnits[0]);
	    delete();
	    
		return true;
	}
	
	//ユニットに乗れる (false 乗れない)
	public UnitObj canLoad(UnitObj tgtObj= null) {
		if (tgtObj != null){
			if (map.MapDist(pos, tgtObj.pos) < 2) return tgtObj;
		} else {
			//if (works == WorkType.None) {
			//	//Debug.Log (string.Format ("MapDist: ({0})", player.utltList.Count));
			//	UnitObj unt = StageCtl.UnitList[player.pnum].FirstOrDefault(u => (u.units == Unit.TransportShip || u.units == Unit.LargeTransportShip) && u.works == WorkType.None && (map.MapDist(pos, u.pos) < 2));//).Min(u => map.MapDist(pos, u.pos)); //
			//	if (unt != null) Debug.Log (string.Format ("MapDist: ({0})", map.MapDist(pos, unt.pos)));
			//	return unt;
			//}
		}
			return null;
	}
	
	// ユニットに乗る (false: 失敗)
	public bool doLoad(UnitObj dst) {
		if (map.MapDist(pos, dst.pos) > 1) return false;
		Debug.Log (string.Format ("Load!!"));
	    destList.Clear();
	    destList.Add(new Vector2(map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(dst.pos.x),MapCtl.offset_stg2tile_y(dst.pos.y), 0)).x,
			    		                                                                   map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(dst.pos.x), MapCtl.offset_stg2tile_y(dst.pos.y), 0)).y));  //dst.pos);
		chgWorkType(WorkType.Loading);
		transform.parent = dst.transform;
		//dst.chgWorkType(WorkType.Loading);
		return true;
	}
	
	// ユニットから降りる (false: 失敗 dst 降りる先)
	public bool doUnload(UnitObj tgtObj, Vector2Int? dst = null) {
		Debug.Log (string.Format ("Unload!!"));
		//
		if (dst != null){
			
		//降りる候補を選定
		} else {
			Vector2Int? dsts = map.tileRing(pos, 1).FirstOrDefault(tll => (map.tileMap[tll.x,tll.y].getGroup() == TileGroup.Ground));
		    if (dsts != null){
		    	//候補がある場合
		    	tgtObj.gameObject.SetActive(true);
		    	tgtObj.destList.Clear();
		    	tgtObj.destList.Add(new Vector2(map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x((int)dsts?.x),MapCtl.offset_stg2tile_y((int)dsts?.y), 0)).x,
		    								map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x((int)dsts?.x), MapCtl.offset_stg2tile_y((int)dsts?.y), 0)).y));
		    	tgtObj.transform.parent = null;
		    	loadUnits.Remove(tgtObj);
		    	tgtObj.chgWorkType(WorkType.Unloading);
		    	//tgtObj.pos = (Vector2Int)dsts;
			} else {
				//候補がない場合
				return false;
			}
		}
		//if (map.MapDist(pos, dst.pos) > 1) return false;
	    //destList.Clear();
	    //destList.Add(new Vector2(map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(dst.pos.x),MapCtl.offset_stg2tile_y(dst.pos.y), 0)).x,
		//	    		                                                                   map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(dst.pos.x), MapCtl.offset_stg2tile_y(dst.pos.y), 0)).y));  //dst.pos);
		//chgWorkType(WorkType.Loading);
		//
		return true;
	}
	
	// 油井(トラック)を作成[首都のみ]
	public bool bldOilWell() {
		if ((units != Unit.Capital)||(works != WorkType.None)||(fuel < uCost.fuel(player.pnum, Unit.OilWell))){
			return false;
		}
		if (StageCtl.UnitList[player.pnum].Any(u => (u.pos == pos)&&((u.type == uType.Infantry)||(u.type == uType.Vehicle))) == true){
			return false;
		}
		//Debug.Log (string.Format ("bldOilWell!!"));
		chgWorkType(WorkType.Building);
		fuel -= uCost.fuel(player.pnum, Unit.OilWell);
		//
		UnitObj newObj = Create("BBB", Unit.LargeTransport, pos, player);
		newObj.transform.parent = transform;
		UnitObj newObj2 = Create("BBB", Unit.OilWell, pos, player);
		newObj.transform.parent.GetComponent<UnitObj>().loadUnits.Add(newObj2);
		newObj2.transform.parent = newObj.transform;
		newObj2.chgWorkType(WorkType.Loading);
		return true;
	}
	
	// 港(トラック)を作成[首都のみ]
	public bool bldNavalPort() {
		if ((units != Unit.Capital)||(works != WorkType.None)||(fuel < uCost.fuel(player.pnum, Unit.OilWell))){
			return false;
		}
		if (StageCtl.UnitList[player.pnum].Any(u => (u.pos == pos)&&((u.type == uType.Infantry)||(u.type == uType.Vehicle))) == true){
			return false;
		}
		chgWorkType(WorkType.Building);
		fuel -= uCost.fuel(player.pnum, Unit.OilWell);
		UnitObj newObj = Create("BBB", Unit.LargeTransport, pos, player);
		newObj.transform.parent = transform;
		UnitObj newObj2 = Create("BBB", Unit.NavalPort, pos, player);
		newObj.transform.parent.GetComponent<UnitObj>().loadUnits.Add(newObj2);
		newObj2.transform.parent = newObj.transform;
		newObj2.chgWorkType(WorkType.Loading);
		return true;
	}
	
	//飛行場(トラック)を作成[首都のみ]
	public bool bldAirfield() {
		if ((units != Unit.Capital)||(works != WorkType.None)||(fuel < 100)){
			return false;
		}
		chgWorkType(WorkType.Building);
		fuel -= 100;
		//
		
		return true;
	}
	
	// 製油所(トラック)を作成[首都のみ]
	public bool bldRefinery() {
		if ((units != Unit.Capital)||(works != WorkType.None)||(fuel < 100)){
			return false;
		}
		chgWorkType(WorkType.Building);
		fuel -= 100;
		//
		
		return true;
	}
	
	// 工場(トラック)を作成[首都のみ]
	public bool bldFactory() {
		if ((units != Unit.Capital)||(works != WorkType.None)||(fuel < 100)){
			return false;
		}
		chgWorkType(WorkType.Building);
		fuel -= 100;
		//
		
		return true;
	}
	
	// 野営地(トラック)を作成[首都のみ]
	public bool bldCamp() {
		if ((units != Unit.Capital)||(works != WorkType.None)||(fuel < 100)){
			return false;
		}
		chgWorkType(WorkType.Building);
		fuel -= 100;
		//
		
		return true;
	}
	
	// 基地(トラック)を作成[首都のみ]
	public bool bldBase() {
		if ((units != Unit.Capital)||(works != WorkType.None)||(fuel < 100)){
			return false;
		}
		chgWorkType(WorkType.Building);
		fuel -= 100;
		//
		
		return true;
	}
	
	// 要塞(トラック)を作成[首都のみ]
	public bool bldFortress() {
		if ((units != Unit.Capital)||(works != WorkType.None)||(fuel < 100)){
			return false;
		}
		chgWorkType(WorkType.Building);
		fuel -= 100;
		//
		
		return true;
	}
	
	void Update () {
	
	
		//if(Input.GetMouseButtonDown(0)){
		//	var mousePos = Input.mousePosition;
		//	mousePos.z = 10.0f;
		//	destList.Add(Camera.main.ScreenToWorldPoint(mousePos));
		//	Debug.Log(Camera.main.ScreenToWorldPoint(mousePos));
		//} 
		
		if ((destList.Count > 0)||(type == uType.Aircraft)){
			if (destList.Count == 0) destList.Add(new Vector2(0,0));
			
			Vector2 dif = destList[0] - (Vector2)transform.position;
			
			// ラジアン
			float radian = Mathf.Atan2 (dif.y, dif.x);
			// 角度
			float degree = radian * Mathf.Rad2Deg + 270.0f;
			float degree_diff = (rid2.rotation - degree + 360.0f) % 360.0f;
				if (( degree_diff > 0.0f )&&(degree_diff < 180.0f)) {
					rid2.rotation -= yawrate;
					if (rid2.rotation < 0f) rid2.rotation += 360.0f;
				} else {
					rid2.rotation += yawrate;
				}

			if ((degree_diff > 45.0f)&&(degree_diff < 325.0f)&&(type != uType.Aircraft)){
				if (speed > 0.0f) {
				    speed -= 0.01f;
				} else {
				    speed = 0.0f;
				}
			} else {
				if (speed < speedMax) {
				    speed += 0.01f;
				} else {
				    speed = speedMax;
				}
			}
			
			//到着時はリスト削除
			if (Vector2.Distance(destList[0], (Vector2)transform.position) < 0.1f) {
				if ((type != uType.Aircraft)||(destList.Count()>1)){
					destList.Remove(destList[0]);
				}
			}
				
			Vector3Int cellPosition = map.GetComponent<Grid>().LocalToCell(transform.position);
			pos = new Vector2Int(MapCtl.offset_tile2stg_x(cellPosition.x), MapCtl.offset_tile2stg_y(cellPosition.y));
			

			
			
		} else {
			if (speed > 0.0f) {
			    speed -= 0.05f;
			} else {
			    speed = 0.0f;
			}
			if ((works == WorkType.Moving)||(works == WorkType.Unloading)) {
				//if (units == Unit.TransportShip){
				//	if (map.tileRing(pos, 1).Exists(tll => (((map.tileMap[tll.x,tll.y].getGroup() == TileGroup.Ground)||(map.tileMap[tll.x,tll.y].getGroup() == TileGroup.Mountain))))){
				//		chgWorkType(WorkType.None);
				//	} else {
				//		chgWorkType(WorkType.None);
				//	}
				//} else {
				
					chgWorkType(WorkType.None);
					destDir = 999;
				//}
			} else if  (works == WorkType.Loading) {
				gameObject.SetActive(false); //非表示
				transform.parent.GetComponent<UnitObj>().loadUnits.Add(this);
			}
		}
		
		if (works == WorkType.Newing){
			imgFill+=0.01f;
			material.SetFloat("_FillAmount", imgFill);
			if (imgFill>1.0f){
				if (type == uType.Aircraft){
					chgWorkType(WorkType.None);
				} else {
					chgWorkType(WorkType.None);
					if (transform.parent != null){
						transform.parent.GetComponent<UnitObj>().chgWorkType(WorkType.None);
						transform.parent = null;
					}
				}
			}
		}
		
		rid2.velocity = transform.up * speed;
		
	}
	
	void changeMove(){
		chdeg = UnityEngine.Random.Range(-90.0f, 90.0f);
		if (chdeg < 0.0f) {
			chdeg -= 90.0f;
		} else {
			chdeg += 90.0f;
		}
		chspd = UnityEngine.Random.Range(-(speedMax/2.0f), speedMax);
		if (chspd < 0.0f) chspd = 0.0f;
		Invoke("changeMove", UnityEngine.Random.Range(5.0f, 20.0f));
	}
	
    public void SsDiscover(Collider2D other)
    {
    //	if (other.gameObject.layer == 31){
	//		//if (chdeg < 0.0f) {
	//		//	chdeg = -2.0f;
	//		//} else {
	//		//	chdeg = 2.0f;
	//		//}
    //	} else {
	//    	if (targetObj == null) {
	//    		if (other.gameObject.GetComponent<UnitCtl>().unitType == UnitType.Carnivore) {
	//    			targetObj = other.gameObject;
	//    			targetType = TargetType.Enemy;
	//    		} else if (satiety < maxsatiety*0.8) {
	//    			targetObj = other.gameObject;
	//    			targetType = TargetType.Food;
	//    		}
	//        } else if (targetObj == other.gameObject) {
	//        	CancelInvoke();
	//        }
    //    }
    }
    
    public void SsNowdiscover(Collider2D other)
    {

    }
    
    public void SsUndiscover(Collider2D other)
    {
    	if (targetObj == other.gameObject)
    		Invoke("forgetTarget", UnityEngine.Random.Range(forgettime_min, forgettime_max));
    }
	
	void forgetTarget(){
		if (targetObj != null){
			targetObj = null;
			targetAlc = AlcType.None;
		}
	}
	
	//A-Starでの移動(OList = 移動不可の位置)
	public void doMove(Vector2Int epos, Vector2Int? _spos = null, List<Vector2Int> Olist = null){
		if (works != WorkType.Moving) chgWorkType(WorkType.PreMove);
		var spos = _spos ?? pos;
		LastDest = map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(epos.x),MapCtl.offset_stg2tile_y(epos.y), 0));
		//Debug.Log (string.Format ("doMove: ({0},{1})", spos, epos));
		
	    switch (type){
	        case uType.Infantry:  /* 歩兵   */
				map.getRoute(spos, epos, 2, doAstMoveCb, Olist); //A-Sterによる経路取得
				break;
	        case uType.Vehicle:   /* 車両   */
				map.getRoute(spos, epos, 1, doAstMoveCb, Olist); //A-Sterによる経路取得
				break;
	        case uType.Aircraft:  /* 航空機 */
	        	destList.Clear();
	        	chgWorkType(WorkType.Moving);
	        	destList.Add(epos);
				break;
	        case uType.Warship:   /* 戦艦   */
	        case uType.Submarine: /* 潜水艦 */
				map.getRoute(spos, epos, 0, doAstMoveCb, Olist); //A-Sterによる経路取得
				break;
			default:              /* その他 */
				break;

		}
	}
	
	public void addMove(Vector2Int epos, Vector2Int? _spos = null, List<Vector2Int> Olist = null){
		if (works != WorkType.Moving) chgWorkType(WorkType.PreMove);
		var spos = _spos ?? pos;
	    switch (type){
	        case uType.Infantry:  /* 歩兵   */
				map.getRoute(spos, epos, 2, doAstMoveCb, Olist); //A-Sterによる経路取得
				break;
	        case uType.Vehicle:   /* 車両   */
				map.getRoute(spos, epos, 1, doAstMoveCb, Olist); //A-Sterによる経路取得
				break;
	        case uType.Aircraft:  /* 航空機 */
	        	destList.Add(epos);
				break;
	        case uType.Warship:   /* 戦艦   */
	        case uType.Submarine: /* 潜水艦 */
				map.getRoute(spos, epos, 0, doAstMoveCb, Olist); //A-Sterによる経路取得
				break;
			default:              /* その他 */
				break;

		}
	}
	
	void doAstMoveCb(List<Vector2Int> rt, int ret){
		if ((works == WorkType.PreMove)||(works == WorkType.Moving)){
		
			chgWorkType(WorkType.Moving);


			if (rt?.Count > 0) {
				var startPos = new Vector2(map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(rt[0].x),MapCtl.offset_stg2tile_y(rt[0].y), 0)).x,
			    		                                                                   map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(rt[0].x), MapCtl.offset_stg2tile_y(rt[0].y), 0)).y);
				rt.RemoveAt(0);
				if (lock_timer==0){
					if ((destList.Count==0)||((destList.Last().x == startPos.x)&&(destList.Last().y == startPos.y))){
				    	if (destDir > ret){

				    		this.addMoveList(rt.Select(pos => new Vector2(map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(pos.x),MapCtl.offset_stg2tile_y(pos.y), 0)).x,
				    		                                                                   map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(pos.x), MapCtl.offset_stg2tile_y(pos.y), 0)).y)).ToList());
				    		                                                                   
							destDir = ret;
					    	if ( ret > 0 ){
					    		if (destList?.Count > 0) doMove(MapCtl.offset_vec2stg(LastDest), MapCtl.offset_vec2stg(destList.Last()));
					    	}
				        }
			        }
			        lock_retry = 0;
		        } else {
					if ((destList.Count==0)||((destList.Last().x != startPos.x)&&(destList.Last().y != startPos.y))){
						destList.Clear();
				    	this.addMoveList(rt.Select(pos => new Vector2(map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(pos.x),MapCtl.offset_stg2tile_y(pos.y), 0)).x,
				    		                                                                   map.GetComponent<Grid>().CellToLocal(new Vector3Int(MapCtl.offset_stg2tile_x(pos.x), MapCtl.offset_stg2tile_y(pos.y), 0)).y)).ToList());
				    	//destList.RemoveAt(0);                                                               
						destDir = ret;
					    if ( ret > 0 ){
					    	if (destList?.Count > 0) doMove(MapCtl.offset_vec2stg(LastDest), MapCtl.offset_vec2stg(destList.Last()));
					    }
			    		lock_timer = 0;
	    				lock_retry ++;
			        }
		        }
	        }
	        

	    
			// 線の幅
			renderer.SetWidth(0.1f, 0.1f);
			// 頂点の数
			renderer.SetVertexCount(destList.Count);
			// 頂点を設定
			for (int i=0;i<destList.Count;i++){
				renderer.SetPosition(i, new Vector3(destList[i].x, destList[i].y, -1f));
			}
		}
	}
	    
	//目的地と移動できる場所が異なる
    public bool ChkGoalMove()
    {
    	
    	
		return false;
    }
	    
	//現在の作業内容
    public WorkType WorkingType()
    {
    	return works;
    }
	    
	//接触
    public void OnCollision2D(Collider2D other)
    {
		List<Vector2Int> Olist = new List<Vector2Int>();
		Olist.Add(MapCtl.offset_vec2stg(destList[1]));
		doMove(MapCtl.offset_vec2stg(LastDest), null, Olist);
    }

	//接触中
    void OnCollisionStay2D(Collision2D other)
    {
    	if (works == WorkType.Moving){
			lock_timer++;
			if (lock_timer==lock_max){
				if (lock_retry<3){
					if (destList?.Count > 1)destList.RemoveAt(0);
					var mList = map.tileRing(pos,1).Where(c => map.chkMoveObj(c,type)).OrderBy(i => System.Guid.NewGuid()).ToList();//
					if (mList?.Count > 1)destList.Insert(0, MapCtl.offset_stg2vec(mList[0]));
					//destList.Insert(0, new Vector2(transform.position.x + UnityEngine.Random.Range(-1, 2), transform.position.y + UnityEngine.Random.Range(-1, 2)));
					lock_timer = 0;
    				lock_retry ++;
					//List<Vector2Int> Olist = new List<Vector2Int>();
					//if (destList?.Count > 1) Olist.Add(MapCtl.offset_vec2stg(destList[1]));
					//doMove(MapCtl.offset_vec2stg(LastDest), null, Olist);
					
				/* 3回リトライしても解決しない場合は行動クリア */
				} else {
					destList.Clear();
					chgWorkType(WorkType.None);
					destDir = 999;
			    	lock_timer = 0;
    				lock_retry = 0;
				}
			}
		}
    }
    
    //接触終了
    void OnCollisionExit2D(Collision2D other){
    	lock_max = UnityEngine.Random.Range(10, 100);
    	lock_timer = 0;
    	//lock_retry = 0;
    }
    

}
