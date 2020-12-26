using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CpuCtl : MonoBehaviour
{
	//CPU用タイルマップ
	struct cpuMapTile{
		public int domain; //そのマップでの支配率 ( + で味方 -で敵側 0で中立)
		public bool beachHead; //そのマップでの海岸堡か
	}

	//油田リスト
	class visOil{
		public Vector2Int pos; //油田位置
		public UnitObj tgtUnit; //その油田を担当する油井
		
		public visOil(Vector2Int _pos){
			pos = _pos;
			tgtUnit = null;
		}
		
	}
	
	//CPU使用ユニット情報
	class cpuUnit{
		public UnitObj tgtUnit; //そのユニット
		public UnitObj hopeRideShip; //乗りたいターゲット(船)
		public CpuWorkType cWorkType;
		public Vector2Int cpuTgtPos = new Vector2Int(-1,-1);	//CPUとして目的地
	}

	//CPU思考状態
	enum CpuWorkType
	{
	    None,				//何もしていない
	    SrcMove,			//移動先調査中
	    Reserved,			//予約中
	    Producting,			//製造中
	    Investigating,		//研究中
	    Repairing,			//修理中(補給中)
	    Refueling,			//給油中
	    Attacking,			//攻撃中
	    Searching,			//探索中
	    Returning,			//帰還中
	    Moving				//移動中
	}
	
	//マップ支配状況判定
    [SerializeField]
    cpuMapTile[,] cpuTileMap;  //タイルの2次元配列
    [SerializeField]
	MapCtl map;
	//ユニットリスト取得
    List<cpuUnit> cpuUnitList = new List<cpuUnit>();
    List<UnitObj> UnitList;
    
    private float timeleft;
    
    //可視油田リスト
    List<visOil> visOilList = new List<visOil>();

    // Start is called before the first frame update
    void Start()
    {
		UnitList = StageCtl.UnitList[0];
		cpuTileMap = new cpuMapTile[StageCtl.TileLenX, StageCtl.TileLenY];
		map = GameObject.Find("Grid").GetComponent<MapCtl>();
		
		//新しいユニットが生成されたかチェック
		foreach(var units in StageCtl.UnitList[0]){
			if (cpuUnitList.Any(u => u.tgtUnit == units) == false){
				cpuUnit newUnit = new cpuUnit();
				newUnit.tgtUnit = units;
				cpuUnitList.Add(newUnit);
			}
			
		}
		
		//油田リスト
		visOilList.AddRange( map.tileList.Where(t => t.getType() == TileType.OilField).Select(t => new visOil(new Vector2Int(t.getPosX(), t.getPosY()))));
		Debug.Log("[visOilList="+ visOilList.Count() +"]"+ visOilList[0].pos);
		
		foreach(var units in cpuUnitList){
	        switch (units.tgtUnit.units){
	        	case Unit.Capital: /* 首都 */
					var landList = map.landArea(units.tgtUnit.pos); /* 陸地をチェック */
					if (landList?.Count > 0){
						//Debug.Log("[landList="+ landList.Count +"]");
						foreach(var land in landList){
							//Debug.Log("[land="+ land +"]");
							cpuTileMap[land.x, land.y].domain += 10;
						}
					}
	        		break;
			}
		}
    }

    // Update is called once per frame
    void Update()
    {
    
        timeleft -= Time.deltaTime;
        //5秒おき
        if (timeleft <= 0.0) {
            timeleft = 5.0f;
			//新しいユニットが生成されたかチェック
			foreach(var units in StageCtl.UnitList[0]){
				if (cpuUnitList.Any(u => u.tgtUnit == units) == false){
					cpuUnit newUnit = new cpuUnit();
					newUnit.tgtUnit = units;
					cpuUnitList.Add(newUnit);
				}
				
			}
        }
    
    
        //Debug.Log("[cpuUnit="+ cpuUnitList.Count +"]");
        
        //ユニットごとの処理
        //foreach(var cUnit in cpuUnitList){
        for (int i=cpuUnitList.Count()-1;i>=0;i--) {
        	if (cpuUnitList[i].tgtUnit == null){
        	    cpuUnitList.RemoveAt(i);
        	    continue;
        	}
        	UnitObj units = cpuUnitList[i].tgtUnit;
	        switch (units.units){
	        	case Unit.LargeTransport: /* 大型輸送車 */
	        		if ((units.works == WorkType.None)&&(units.transform.parent == null)){  //★要改善！
		        		/* ユニットが搭載されている */
		        		if (units.loadUnits.Count > 0){
		        			switch (units.loadUnits[0].units){
	        					case Unit.OilWell: /* 油井 */
	        						/* 油田上に居る場合 */
	        						if (map.tileMap[units.pos.x, units.pos.y].getType() == TileType.OilField){
	        							units.doDeployUnit();
	        						} else {
	        							var u = units.canLoad(cpuUnitList[i].hopeRideShip);
		        						if (u != null){
		        							units.doLoad(u);
		        							units.LastDest = MapCtl.offset_stg2vec(cpuUnitList[i].cpuTgtPos);
		        							cpuUnitList[i].cWorkType = CpuWorkType.None;
		        							cpuUnitList[i].hopeRideShip = null;
		        						} else {
		        							//
		        							if (cpuUnitList[i].hopeRideShip == null){
			        							/* 移動先油田未設定 */
			        							//Debug.Log("cpuUnit["+ i +"].cpuTgtPos = " + cpuUnitList[i].cpuTgtPos +"]");
			        							if (cpuUnitList[i].cpuTgtPos.x == -1){
			        								visOil OilObj = visOilList.Where(uu => uu.tgtUnit == null).ToList().Find(s => map.MapDist(cpuUnitList[i].tgtUnit.pos, s.pos) == visOilList.Where(uu => uu.tgtUnit == null).ToList().Min(p => map.MapDist(cpuUnitList[i].tgtUnit.pos, p.pos)));
			        								//if (OilObj){
			        									cpuUnitList[i].cpuTgtPos = OilObj.pos;
			        									OilObj.tgtUnit = cpuUnitList[i].tgtUnit.loadUnits[0];
			        									//Debug.Log("cpuUnit["+ i +"].cpuTgtPos = " + cpuUnitList[i].cpuTgtPos +"]");
			        								//}
			        							}
				        						/* 開いている最短の油田へ経路探索 */
				        						if ((cpuUnitList[i].cWorkType == CpuWorkType.None)&&(cpuUnitList[i].cpuTgtPos.x > -1)){
				        							srcOil(cpuUnitList[i]);
				        							cpuUnitList[i].hopeRideShip = null;
				        							Debug.Log("srcOil cpuUnit["+ i +"].cpuTgtPos = " + units.pos + " ->" + cpuUnitList[i].cpuTgtPos +"]");
				        							//units.doMove(map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.OilField, TileType.None)[0]);
				        						}
			        						} else {
			        							units.doMove(cpuUnitList[i].hopeRideShip.pos);
			        						}
		        						}
	        						}
	        						break;
	        					case Unit.NavalPort: /* 軍港 */
	        						if (units.doDeployUnit()==false){
		        						List<Vector2Int> clist = map.getSpcTilePos(new Vector2Int(0, 0), new Vector2Int(StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.ShallowSea);
		        						units.doMove(clist[0]);
	        						}
	        						break;
	        					case Unit.Airfield: /* 飛行場 */
	        						if (units.doDeployUnit()==false){
		        						List<Vector2Int> clist = map.getSpcTilePos(new Vector2Int(0, 0), new Vector2Int(StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.Meadow);
		        						units.doMove(clist[0]);
	        						}
	        						break;
	        					case Unit.Factory: /* 工場 */
	        						if (units.doDeployUnit()==false){
		        						List<Vector2Int> clist = map.getSpcTilePos(new Vector2Int(0, 0), new Vector2Int(StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.ShallowSea);
		        						units.doMove(clist[0]);
	        						}
	        						break;
	        					case Unit.Refinery: /* 製油所 */
	        						if (units.doDeployUnit()==false){
		        						List<Vector2Int> clist = map.getSpcTilePos(new Vector2Int(0, 0), new Vector2Int(StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.ShallowSea);
		        						units.doMove(clist[0]);
	        						}
	        						break;
	        					case Unit.Camp: /* 野営地 */
	        						if (units.doDeployUnit()==false){
		        						List<Vector2Int> clist = map.getSpcTilePos(new Vector2Int(0, 0), new Vector2Int(StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.ShallowSea);
		        						units.doMove(clist[0]);
	        						}
	        						break;
	        					case Unit.Base: /* 基地 */
	        						if (units.doDeployUnit()==false){
		        						List<Vector2Int> clist = map.getSpcTilePos(new Vector2Int(0, 0), new Vector2Int(StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.ShallowSea);
		        						units.doMove(clist[0]);
	        						}
	        						break;
	        					case Unit.Fortress: /* 要塞 */
	        						if (units.doDeployUnit()==false){
		        						List<Vector2Int> clist = map.getSpcTilePos(new Vector2Int(0, 0), new Vector2Int(StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.ShallowSea);
		        						units.doMove(clist[0]);
	        						}
	        						break;
					        	default: /* その他 */
					        		break;
		        			}
		        		/* ユニットが未搭載 */
		        		} else {
		        			
		        		}
	        		}
	        		break;
	        
	        	case Unit.TransportShip: /* 輸送艦 */
	        		if (units.works == WorkType.None){
		        		/* ユニットが搭載されている */
		        		if (units.loadUnits.Count > 0){
		        			if (units.loadUnits[0].works == WorkType.Loading){
			        			//搭載ユニットの最終移動先へ
			        			units.doMove(MapCtl.offset_vec2stg(units.loadUnits[0].LastDest));
			        			units.loadUnits[0].works = WorkType.None;
		        			} else {
		        				if (map.tileRing(units.pos, 1).Exists(tll => (((map.tileMap[tll.x,tll.y].getGroup() == TileGroup.Ground))))){
			        				units.doUnload(units.loadUnits[0]);
			        				cpuUnitList[i].hopeRideShip = null;
			        			} else {
				        			units.doMove(MapCtl.offset_vec2stg(units.loadUnits[0].LastDest));
			        			}
		        			}
		        		/* ユニットが非搭載 */
		        		} else {
		        			//陸地に隣接している場合
							if (map.tileRing(units.pos, 1).Exists(tll => (((map.tileMap[tll.x,tll.y].getGroup() == TileGroup.Ground))))){
								/* 待機 */
							} else {
			        			//ランダムな首都を目指す(デバッグ用)
			        			//units.doMove(UnitList.Where(u => u.units == Unit.Capital ).OrderBy(j => System.Guid.NewGuid()).ToList()[0].pos);
			        			//ランダムな支配下領域を目指す
		        				//units.doMove(map.getSpcTilePos(new Vector2Int(0,0), new Vector2Int (StageCtl.TileLenX, StageCtl.TileLenY), TileType.Meadow, TileType.None).FirstOrDefault(t=>cpuTileMap[t.x, t.y].domain>0));
	        				}
		        		}
		        	}
	        		break;
	        		
	        	case Unit.Fighter: /* 戦闘機 */
	        		if (units.works == WorkType.None){
	        			units.doMove(new Vector2Int (3, 3));
	        			units.addMove(new Vector2Int (15, 15));
	        			
	        		}
	        	break;
	        
	        	case Unit.Capital: /* 首都 */
	        		if (units.works == WorkType.None){

		        		if (UnitList.Where( u => u.units == Unit.NavalPort ).ToList().Count < 1) {
		        			//units.bldNavalPort();
		        			units.doBuildUnit(Unit.NavalPort);
		        		} else if (UnitList.Where( u => u.units == Unit.OilWell ).ToList().Count < 5){
		        			//units.bldOilWell();
		        			units.doBuildUnit(Unit.OilWell);
		        		} else
		        		 if (UnitList.Where( u => u.units == Unit.Airfield ).ToList().Count < 1){
		        			units.doBuildUnit(Unit.Airfield);
		        		}
		        		
	        		}
	        		break;
	        		
	        	case Unit.Factory: /* 工場 */
	        		break;
	        		
	        	case Unit.NavalPort: /* 軍港 */
	        		
	        		Debug.Log("NavalPort!! "+ units.works);
	        		if (units.works == WorkType.None){
		        		if (UnitList.Where( u => u.units == Unit.TransportShip ).ToList().Count < 5) {
		        			units.doBuildUnit(Unit.TransportShip);
		        		} else if (UnitList.Where( u => u.units == Unit.LargeTanker ).ToList().Count < 5){
		        			units.doBuildUnit(Unit.LargeTanker);
		        		}
	        		}
	        		break;
	        		
	        	case Unit.Airfield: /* 飛行場 */
	        		if (units.works == WorkType.None){
		        		if (UnitList.Where( u => u.units == Unit.Surveillance ).ToList().Count < 2) { /* 偵察機 */
		        			units.doBuildUnit(Unit.Surveillance);
		        		} else if (UnitList.Where( u => u.units == Unit.LandFighter ).ToList().Count < 5){ /* 陸上戦闘機 */
		        			units.doBuildUnit(Unit.LandFighter);
		        		}
	        		}
	        		break;
	        		
	        	case Unit.Camp: /* 工場 */
	        		break;
	        		
	        	case Unit.Base: /* 軍港 */
	        		break;
	        		
	        	default: /* 資源・その他 */
	        		units.bldOilWell();
	        		break;
	        }
	        
	        //
        	
        }
        
        
    }
    
    //油田探索
    void srcOil(cpuUnit cUnit){
		cUnit.cWorkType = CpuWorkType.SrcMove;
		map.getRoute(cUnit.tgtUnit.pos, cUnit.cpuTgtPos, 1, (mvList, dist) =>{
					Debug.Log(" getRoute ret["+ dist +"] ");
			        moveChkCb(cUnit, mvList, dist);});
    }
    
    void moveChkCb(cpuUnit cUnit, List<Vector2Int> mvList, int dist){
    	if ((cUnit == null)||(cUnit.tgtUnit == null)) return;
    	switch (cUnit.tgtUnit.type){
    		case uType.Infantry: /* 歩兵 */
    		case uType.Vehicle:  /* 車両 */
   			//船を使わないといけない場合
            if ( dist > 0 ){
            
				//最短の船をチェック
				cpuUnit cTgtUnit = cpuUnitList.Where(u => u.tgtUnit != null && u.hopeRideShip == null && u.tgtUnit.units == Unit.TransportShip && u.tgtUnit.works == WorkType.None && u.tgtUnit.loadUnits.Count() == 0).ToList().
                Find(s => map.MapDist(cUnit.tgtUnit.pos, s.tgtUnit.pos) == cpuUnitList.Where(u => u.tgtUnit != null && u.hopeRideShip == null && u.tgtUnit.units == Unit.TransportShip && u.tgtUnit.works == WorkType.None && u.tgtUnit.loadUnits.Count() == 0).
                ToList().Min(p => map.MapDist(cUnit.tgtUnit.pos, p.tgtUnit.pos)));
                
                //UnitObj minObj = UnitList.Where(u => u.units == Unit.TransportShip && u.works == WorkType.None && u.loadUnits.Count() == 0).ToList().
                //Find(s => map.MapDist(cUnit.tgtUnit.pos, s.pos) == UnitList.Where(u => u.units == Unit.TransportShip && u.works == WorkType.None && u.loadUnits.Count() == 0).
                //ToList().Min(p => map.MapDist(cUnit.tgtUnit.pos, p.pos)));
                
                //minObj.doMove(cUnit.tgtUnit.pos);
                //cUnit.tgtUnit.doMove(minObj.pos);
                if (cTgtUnit != null){
                	Debug.Log("ASHP!! getRoute ret[] "+ dist);
                	cTgtUnit.hopeRideShip = cUnit.tgtUnit;
                	cUnit.hopeRideShip = cTgtUnit.tgtUnit;
					map.getRoute(cUnit.tgtUnit.pos, cTgtUnit.tgtUnit.pos, 1, (_mvList, _dist) =>{
						Debug.Log("SHP!! getRoute ret[] "+ dist);
				        moveWithShip(cUnit, cTgtUnit.tgtUnit, _mvList, _dist);});
			        } else {
		               Debug.LogWarning("err[no ships]");  //船がない！！
		               cUnit.cWorkType = CpuWorkType.None;
		               cUnit.hopeRideShip = null;
		               cUnit.tgtUnit.doMove(mvList[mvList.Count()-1]);
			        }
                //そのまま行ける場合
            } else if (dist==0) {
                //実際に移動
                Debug.Log("[doMoveCount() ="+ mvList.Count() +"]");  //ゼロになる！！
                            	Debug.Log("[doMove="+ mvList[mvList.Count()-1] +"]");
                cUnit.tgtUnit.doMove(mvList[mvList.Count()-1]);
                
                //ERROR
            } else {
                Debug.LogWarning("err[dist="+ dist +"]");  //ゼロになる！！
				cUnit.cWorkType = CpuWorkType.None;
				cUnit.hopeRideShip = null;
            }
            break;
        }
	}
	
	void moveWithShip(cpuUnit cUnit,UnitObj  minObj, List<Vector2Int> mvList, int dist){
		if (mvList.Count()>0){
    		minObj.doMove(mvList[mvList.Count()-1]);
    	} else {
    		minObj.doMove(cUnit.tgtUnit.pos);
    	}
    	cUnit.tgtUnit.doMove(minObj.pos);
    	cUnit.cWorkType = CpuWorkType.None;
	}
	
}
