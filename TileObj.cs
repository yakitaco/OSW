using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEngine;
using System;

public enum TileGroup
{
	None,     //なし・不明 (未探索部分)
    Sea,      //海洋 (船・航空)
    Ground,   //地上 (人・車両・航空)
    Mountain  //山岳 (人・航空)
}

public enum TileType
{
	None,         //なし・不明(未探索部分)
    DeepSea,      //深海
    ShallowSea,   //浅海
    SandyBeach,   //砂浜
    Meadow,       //草原
    Forest,       //森林
    Mountain,     //山岳
    OilField      //油田
}

public class TileObj {
	public TileBase tBase;
	TileGroup tileGroup;
	TileType tileType;
	int[] cost = new int[4]; //移動コスト(配列はUnitTypesに一致)
    int pos_x;
    int pos_y;
    
    static List<TileObj> uTileTlt = new List<TileObj>();

	// Use this for initialization
	public TileObj (TileType _type, TileGroup _group, int[] _cost) {
		tileType = _type;
		tileGroup = _group;
		cost = _cost;
	}
	
	// Update is called once per frame
	public void setType (TileType _type, TileGroup _group) {
		tileType = _type;
		tileGroup = _group;
	}
	
	//タイル情報初期化(ゲーム開始時に必ずコールすること)
	public static void initTileTlt()
	{
		uTileTlt.Add(addTileTlt("001/00_None", TileGroup.None,     TileType.None, new int[]{10,10,10,10}));		/* なし   */
		uTileTlt.Add(addTileTlt("001/01_DeepSea", TileGroup.Sea,      TileType.DeepSea, new int[]{10,10,10,10}));		/* 深海   */
		uTileTlt.Add(addTileTlt("001/02_ShallowSea", TileGroup.Sea,      TileType.ShallowSea, new int[]{10,10,10,10}));	/* 浅海   */
		uTileTlt.Add(addTileTlt("001/03_SandyBeach", TileGroup.Ground,   TileType.SandyBeach, new int[]{10,10,10,10}));		/* 砂浜   */
		uTileTlt.Add(addTileTlt("001/04_Meadow", TileGroup.Ground,   TileType.Meadow, new int[]{10,10,10,10}));		/* 草原   */
		uTileTlt.Add(addTileTlt("001/05_Forest", TileGroup.Mountain, TileType.Forest, new int[]{10,10,10,10}));		/* 森林   */
		uTileTlt.Add(addTileTlt("001/06_Mountain", TileGroup.Mountain, TileType.Mountain, new int[]{10,10,10,10}));	/* 山岳   */
		uTileTlt.Add(addTileTlt("001/07_OilField", TileGroup.Ground,   TileType.OilField, new int[]{10,10,10,10}));	/* 油田   */
	}
	
    //タイル情報のテンプレート作成
    static TileObj addTileTlt(String _image ,TileGroup _group, TileType _type, int[] _cost){
    	TileObj newTile = new TileObj(_type, _group, _cost);
    	newTile.tBase = Resources.Load<TileBase>("TileBase/" + _image);
    	return newTile;
    }
	
    public static TileObj set(TileType _type, Vector2Int pos){
    	TileObj setTile = (TileObj)uTileTlt.FirstOrDefault(t => t.tileType == _type).MemberwiseClone();
    	setTile.pos_x = pos.x;
    	setTile.pos_y = pos.y;
    	return setTile;
    }
	
	public TileType getType() {
		return tileType; //★
	}
	
	public int getPosX() {
		return pos_x;
	}
	
	public int getPosY() {
		return pos_y;
	}
	
	public TileGroup getGroup() {
		return tileGroup;
	}
	
}
