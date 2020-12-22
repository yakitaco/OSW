using System.Collections;
using System.Collections.Generic;

	//製造コストリスト
	public static class uCost {
		class uCostEl{
			public int fuelCost;
			public int goldCost;
			public int timeCost;	//
			public Unit producer;	//製造時の親(nullあり)
			public Unit? parents;  //製造元
			
			public uCostEl(int _fuel, int _gold, int _time, Unit _producer, Unit? _parents){
				fuelCost = _fuel;
				goldCost = _gold;
				timeCost = _time;
				producer = _producer;
				parents = _parents;
				
			}
			
			public void chgUCostEl(int _fuel, int _gold, int _time){
				fuelCost += _fuel;
				goldCost += _gold;
				timeCost += _time;
				
			}
			
		}
		static List<SortedList<Unit,uCostEl>> UcList = new List<SortedList<Unit,uCostEl>>();
	    
		public static void initUcost(int _playerCnt){
			for(int x = 0 ; x < StageCtl.TileLenX ; x++){
				UcList.Add(new SortedList<Unit,uCostEl>());
			}
		}
	    
	    //ユニットコストリスト追加・更新
		public static void setUcost(int _playerNum, Unit _unit, int _fuel, int _gold, int _time, Unit _producer, Unit? _parents){
			if (UcList[_playerNum].ContainsKey( _unit )){
				UcList[_playerNum][_unit]=(new uCostEl(_fuel, _gold, _time, _producer, _parents)); //更新
			} else {
				UcList[_playerNum].Add(_unit,new uCostEl(_fuel, _gold, _time, _producer, _parents)); //新規
			}
		}
		
		//ユニットコストリスト値変更
		public static void chgUcost(int _playerNum, Unit _unit, int _fuel, int _gold, int _time){
			if (UcList[_playerNum].ContainsKey( _unit )){
				UcList[_playerNum][_unit].chgUCostEl(_fuel, _gold, _time); //更新
			}
		}
		
		//生産可能か全チェック
	    public static bool canCreate(int _playerNum, Unit _unit, int _fuel, int _gold, Unit _producer){
			if (UcList[_playerNum].ContainsKey( _unit )){	//リストに存在
				if ((UcList[_playerNum][_unit].producer == _producer)&&(UcList[_playerNum][_unit].fuelCost<=_fuel)&&(UcList[_playerNum][_unit].goldCost>=_gold)){
					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}
			return true;
	    }
	    
	    //必要燃料をリターン
	    public static int fuel(int _playerNum, Unit _unit){
	        try
	        {
	            return UcList[_playerNum][_unit].fuelCost;
	        }
	        catch (KeyNotFoundException)
	        {
	            return -1;
	        }
	    }
	    
	    //必要ゴールドをリターン
	    public static int gold(int _playerNum, Unit _unit){
	        try
	        {
	            return UcList[_playerNum][_unit].goldCost;
	        }
	        catch (KeyNotFoundException)
	        {
	            return -1;
	        }
	    }
	    
	    //必要時間をリターン
	    public static int time(int _playerNum, Unit _unit){
	        try
	        {
	            return UcList[_playerNum][_unit].timeCost;
	        }
	        catch (KeyNotFoundException)
	        {
	            return -1;
	        }
	    }
	    
	    //生産対象かチェック
	    public static bool chkType(int _playerNum, Unit _unit, Unit _producer){
	    	if (UcList[_playerNum][_unit].producer == _producer) {
	    		return true;
	    	} else {
	    		return false;
	    	}
	    }
	    
	    //親をチェック
	    public static Unit? chkParent(int _playerNum, Unit _unit){
	        try
	        {
	            return UcList[_playerNum][_unit].parents;
	        }
	        catch (KeyNotFoundException)
	        {
	            return null;
	        }
	    }
	    
	}
