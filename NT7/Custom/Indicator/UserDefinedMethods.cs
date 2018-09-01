#region Using declarations
using System;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Strategy;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// This file holds all user defined indicator methods.
    /// </summary>
    partial class Indicator
    {		
		private bool drawTxt = false; // User defined variables (add any user defined variables below)
		private IText it_gap = null; //the Text draw for gap on current bar
		protected string log_file = ""; //
		
		#region ZZ Vars
		/// <summary>
		/// Two Bar ZZ Swing ratio = curSize/prevSize
		/// </summary>
		protected List<double> ZZ_Ratio_0_6 = null;
		protected List<double> ZZ_Ratio_6_10 = null;
		protected List<double> ZZ_Ratio_10_16 = null;
		protected List<double> ZZ_Ratio_16_22 = null;
		protected List<double> ZZ_Ratio_22_30 = null;
		protected List<double> ZZ_Ratio_30_ = null;
		protected List<double> ZZ_Ratio = null;

		/// <summary>
		/// ZZ Swing sum for each day
		/// </summary>
		protected Dictionary<string,double> ZZ_Count_0_6 = null;
		protected Dictionary<string,double> ZZ_Count_6_10 = null;
		protected Dictionary<string,double> ZZ_Count_10_16 = null;
		protected Dictionary<string,double> ZZ_Count_16_22 = null;
		protected Dictionary<string,double> ZZ_Count_22_30 = null;
		protected Dictionary<string,double> ZZ_Count_30_ = null;
		protected Dictionary<string,double> ZZ_Count = null;
		
		/// <summary>
		/// ZZ Swing sum for each day
		/// </summary>
		protected Dictionary<string,double> ZZ_Sum_0_6 = null;
		protected Dictionary<string,double> ZZ_Sum_6_10 = null;
		protected Dictionary<string,double> ZZ_Sum_10_16 = null;
		protected Dictionary<string,double> ZZ_Sum_16_22 = null;
		protected Dictionary<string,double> ZZ_Sum_22_30 = null;
		protected Dictionary<string,double> ZZ_Sum_30_ = null;
		protected Dictionary<string,double> ZZ_Sum = null;
		
//		protected double ZZ_Avg_Daily_Count;
//		protected double ZZ_Avg_Daily_Sum;
//		protected double ZZ_Avg_Weekly_Count;
//		protected double ZZ_Avg_Weekly_Sum;
		#endregion
		
		protected double Day_Count = 0;
		protected double Week_Count = 0;
		
		public int IsLastBarOnChart() {
			try{
				if(Input.Count - CurrentBar <= 2) {
					return Input.Count;
				} else {
					return -1;
				}
			} catch(Exception ex){
				Print("IsLastBarOnChart:" + ex.Message);
				return -1;
			}
		}
		
		public void DayWeekMonthCount() {
			if(CurrentBar < BarsRequired) return;
			if(Time[0].Day != Time[1].Day) {
				Day_Count ++;
			}
			if(Time[0].DayOfWeek == DayOfWeek.Sunday &&  Time[1].DayOfWeek != DayOfWeek.Sunday) {
				Week_Count ++;
			}
			
		}
		
		#region Time Functions
		
		public string GetTimeDate(String str_timedate, int time_date) {
			char[] delimiterChars = { ' '};
			string[] str_arr = str_timedate.Split(delimiterChars);
			return str_arr[time_date];
		}
		
		public string GetTimeDate(DateTime dt, int time_date) {
			string str_dt = dt.ToString("MMddyyyy HH:mm:ss");
			char[] delimiterChars = {' '};
			string[] str_arr = str_dt.Split(delimiterChars);
			return str_arr[time_date];
		}
		
		public double GetTimeDiff(DateTime dt_st, DateTime dt_en) {
			double diff = -1;
			//if(diff < 0 ) return 100; 
			try {
			
			if((int)dt_st.DayOfWeek==(int)dt_en.DayOfWeek) { //Same day
				if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose)*CompareTimeWithSessionBreak(dt_en, SessionBreak.AfternoonClose) > 0) {
					diff = dt_en.Subtract(dt_st).TotalMinutes;
				}
				else if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose) < 0 && CompareTimeWithSessionBreak(dt_en, SessionBreak.AfternoonClose) > 0) {
					diff = GetTimeDiffSession(dt_st, SessionBreak.AfternoonClose) + GetTimeDiffSession(dt_en, SessionBreak.EveningOpen);
				}
			}
			else if((dt_st.DayOfWeek==DayOfWeek.Friday && dt_en.DayOfWeek==DayOfWeek.Sunday) || ((int)dt_st.DayOfWeek>(int)dt_en.DayOfWeek) || ((int)dt_st.DayOfWeek<(int)dt_en.DayOfWeek-1)) { // Fiday - Sunday or Cross to next Week or have day off trade
				diff = GetTimeDiffSession(dt_st, SessionBreak.AfternoonClose) + GetTimeDiffSession(dt_en, SessionBreak.EveningOpen);
			}
			else if((int)dt_st.DayOfWeek==(int)dt_en.DayOfWeek-1) { //Same day or next day
				if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose) < 0) {
					diff = GetTimeDiffSession(dt_st, SessionBreak.AfternoonClose) + GetTimeDiffSession(dt_en, SessionBreak.NextDay);
				}
				else if(CompareTimeWithSessionBreak(dt_st, SessionBreak.AfternoonClose) > 0) { // dt_st passed evening open, no need to adjust
					diff = diff = dt_en.Subtract(dt_st).TotalMinutes;
				}
			}
			else {
				diff = dt_en.Subtract(dt_st).TotalMinutes;
			}
			} catch(Exception ex) {
				Print("GetTimeDiff ex:" + dt_st.ToString() + "--" + dt_en.ToString() + "--" + ex.Message);
				diff = 100;
			}
			return Math.Round(diff, 2);
		}

		public double GetMinutesDiff(DateTime dt_st, DateTime dt_en) {
			double diff = -1;
			TimeSpan ts = dt_en.Subtract(dt_st);
			diff = ts.TotalMinutes;
			return Math.Round(diff, 2);
		}
		
		public int CompareTimeWithSessionBreak(DateTime dt_st, SessionBreak sb) {
			DateTime dt = DateTime.Now;
			try {
			switch(sb) {
				case SessionBreak.AfternoonClose:
					dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
					break;
				case SessionBreak.EveningOpen:
					if(dt_st.Hour < 16)
						dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day-1,17,0,0);
					//if(dt_st.Hour >= 17)
					else dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
					break;
				default:
					dt = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
					break;
			}
			} catch(Exception ex) {
				Print("CompareTimeWithSessionBreak ex:" + dt_st.ToString() + "--" + sb.ToString() + "--" + ex.Message);
			}
			return dt_st.CompareTo(dt);
		}

		public double GetTimeDiffSession(DateTime dt_st, SessionBreak sb) {			
			DateTime dt_session = DateTime.Now;
			TimeSpan ts = dt_session.Subtract(dt_st);
			double diff = 100;
			try{
			switch(sb) {
				case SessionBreak.AfternoonClose:
					dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day, 16, 0, 0);
					ts = dt_session.Subtract(dt_st);
					break;
				case SessionBreak.EveningOpen:
					if(dt_st.Hour < 16)
						dt_session = GetNewDateTime(dt_st.Year,dt_st.Month,dt_st.Day-1,17,0,0);
					else 
						dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day, 17, 0, 0);
					ts = dt_st.Subtract(dt_session);
					break;
				case SessionBreak.NextDay:
					dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day-1, 17, 0, 0);
					ts = dt_st.Subtract(dt_session);
					break;
				default:
					dt_session = GetNewDateTime(dt_st.Year, dt_st.Month, dt_st.Day-1, 17, 0, 0);
					ts = dt_st.Subtract(dt_session);
					break;
			}
			diff = ts.TotalMinutes;
			} catch(Exception ex) {
				Print("GetTimeDiffSession ex:" + dt_st.ToString() + "--" + sb.ToString() + "--" + ex.Message);
			}
			
			return Math.Round(diff, 2);
		}
		
		public DateTime GetNewDateTime(int year, int month, int day, int hr, int min, int sec) {
			//DateTime(dt_st.Year,dt_st.Month,dt_st.Day,16,0,0);
			if(day == 0) {
				if(month == 1) {
					year = year-1;
					month = 12;
					day = 31;
				}
				else if (month == 3) {
					month = 2;
					day = 28;
				}
				else {
					month--;
					if(month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10)
						day = 31;
					else day = 30;
				}
			}
			return new DateTime(year, month, day, hr, min, sec);
		}
		
		public string Get24HDateTime(DateTime dt) {
			return dt.ToString("MM/dd/yyyy HH:mm:ss");
		}

		public int GetDateByDateTime(DateTime dt) {
			int date = dt.Year*10000 + dt.Month*100 + dt.Day;
			return date;
		}
		
		//time=10000*H + 100*M + S
		public int GetTimeByHM(int hour, int min) {
			return 10000*hour + 100*min;
		}
				
		public bool IsTimeInSpan(DateTime dt, int start, int end) {
			int t = 100*dt.Hour + dt.Minute;
			if(start <= t && t <= end) return true;
			else return false;
		}

		public int GetTimeDiffByHM(int hour, int min, DateTime dt) {
			int t = GetTimeByHM(hour, min);
			int t0 = GetTimeByHM(dt.Hour, dt.Minute);
			Print("[hour,min]=[" + hour + "," + min + "], [dt.Hour,dt.Minute]=[" + dt.Hour + "," + dt.Minute + "]," + dt.TimeOfDay);
			return t-t0;
		}
		
		public int GetTimeDiffByHM(int start_hour, int start_min, int end_hour, int end_min) {
			int t = GetTimeByHM(end_hour, end_min);
			int t0 = GetTimeByHM(start_hour, start_min);
			Print("[start_hour,start_min]=[" + start_hour + "," + start_min + "], [end_hour,end_min]=[" + end_hour + "," + end_min + "]");
			return t-t0;
		}
		
		/// <summary>
		/// Check if now is the time allowed to put trade
		/// </summary>
		/// <param name="time_start">start time</param>
		/// <param name="time_end">end time</param>
		/// <param name="session_start">the overnight session start time: 170000 for ES</param>
		/// <returns></returns>
		public bool IsTradingTime(int time_start, int time_end, int session_start) {
			int time_now = ToTime(Time[0]);
			bool isTime= false;
			if(time_start >= session_start) {
				if(time_now >= time_start || time_now <= time_end)
					isTime = true;
			}
			else if (time_now >= time_start && time_now <= time_end) {
				isTime = true;
			}
			return isTime;
		}

		#endregion
		
		#region Pattern Functions
		/// <summary>
		/// Check the first reversal bar for the pullback under current ZigZag gap
		/// </summary>
		/// <param name="cur_gap">current ZigZag gap</param>
		/// <param name="tick_size">tick size of the symbol</param>
		/// <param name="n_bars">bar count with the pullback prior to the last reversal bar</param>
		/// <returns>is TBR or not</returns>
		public bool IsTwoBarReversal(double cur_gap, double tick_size, int n_bars) {
			bool isTBR= false;
			
			if(n_bars < 0) return isTBR;
			
			if(cur_gap > 0)
			{
				//Check if the last n_bars are pullback bars (bear bar)
				for(int i=1; i<=n_bars; i++)
				{
					if(Close[i] > Open[i])
						return isTBR;
				}
				if(Close[0]-Open[0] > tick_size && Open[1]-Close[1] >= tick_size) {
					isTBR= true;
				}
			}
			else if(cur_gap < 0)
			{
				//Check if the last n_bars are pullback bars (bull bar)
				for(int i=1; i<=n_bars; i++)
				{
					if(Close[i] < Open[i])
						return isTBR;
				}
				if(Open[0] - Close[0] > tick_size && Close[1] - Open[1] >= tick_size) {
					isTBR= true;
				}
			}
			return isTBR;
		}
		
		/// <summary>
		/// Get the Two Bar Reversal count for the past barsBack
		/// </summary>
		/// <param name="cur_gap">current ZigZag gap</param>
		/// <param name="tick_size">tick size of the symbol</param>
		/// <param name="barsBack">bar count to look back</param>
		/// <returns>pairs count for TBR during the barsBack</returns>
		public int GetTBRPairsCount(double cur_gap, double tick_size, int barsBack) {
			int tbr_count = 0;
			for(int i=0; i<barsBack; i++) {
				if(cur_gap > 0 && Close[i]-Open[i] > tick_size && Open[i+1]-Close[i+1] >= tick_size) {
					tbr_count++;
				}
				if(cur_gap < 0 && Open[i] - Close[i] > tick_size && Close[i+1] - Open[i+1] >= tick_size) {
					tbr_count++;
				}
			}
			return tbr_count;
		}

		protected void SaveTwoBarRatio(ZigZagSwing zzS){
			if( ZZ_Ratio_0_6 == null) 
				ZZ_Ratio_0_6 = new List<double>();
			if( ZZ_Ratio_6_10 == null) 
				ZZ_Ratio_6_10 = new List<double>();
			if( ZZ_Ratio_10_16 == null) 
				ZZ_Ratio_10_16 = new List<double>();
			if( ZZ_Ratio_16_22 == null) 
				ZZ_Ratio_16_22 = new List<double>();
			if( ZZ_Ratio_22_30 == null) 
				ZZ_Ratio_22_30 = new List<double>();
			if( ZZ_Ratio_30_ == null) 
				ZZ_Ratio_30_ = new List<double>();
			if( ZZ_Ratio == null) 
				ZZ_Ratio = new List<double>();

			double zzSizeAbs = Math.Abs(zzS.Size);
			if(zzSizeAbs > 0 && zzSizeAbs <6){
				ZZ_Ratio_0_6.Add(zzS.TwoBar_Ratio);
			}
			else if(zzSizeAbs >= 6 && zzSizeAbs <10){
				ZZ_Ratio_6_10.Add(zzS.TwoBar_Ratio);
			}
			else if(zzSizeAbs >= 10 && zzSizeAbs <16){
				ZZ_Ratio_10_16.Add(zzS.TwoBar_Ratio);
			}
			else if(zzSizeAbs >= 16 && zzSizeAbs <22){
				ZZ_Ratio_16_22.Add(zzS.TwoBar_Ratio);
			}
			else if(zzSizeAbs >= 22 && zzSizeAbs <30){
				ZZ_Ratio_22_30.Add(zzS.TwoBar_Ratio);
			}
			else if(zzSizeAbs >= 30){
				ZZ_Ratio_30_.Add(zzS.TwoBar_Ratio);
			}
			if(zzS.Size != 0) {
				ZZ_Ratio.Add(zzS.TwoBar_Ratio);
			}
		}
		
		protected void PrintTwoBarRatio(){
			if( ZZ_Ratio_0_6 != null) {
				Print("========ZZ_Ratio_0_6 count=" + ZZ_Ratio_0_6.Count + "=========");
				foreach(double val in ZZ_Ratio_0_6) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio_6_10 != null) {
				Print("========ZZ_Ratio_6_10 count=" + ZZ_Ratio_6_10.Count + "=========");
				foreach(double val in ZZ_Ratio_6_10) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio_10_16 != null) {
				Print("========ZZ_Ratio_10_16 count=" + ZZ_Ratio_10_16.Count + "=========");
				foreach(double val in ZZ_Ratio_10_16) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio_16_22 != null) {
				Print("========ZZ_Ratio_16_22 count=" + ZZ_Ratio_16_22.Count + "=========");
				foreach(double val in ZZ_Ratio_16_22) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio_22_30 != null) {
				Print("========ZZ_Ratio_22_30 count=" + ZZ_Ratio_22_30.Count + "=========");
				foreach(double val in ZZ_Ratio_22_30) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio_30_ != null) {
				Print("========ZZ_Ratio_30_ count=" + ZZ_Ratio_30_.Count + "=========");
				foreach(double val in ZZ_Ratio_30_) {
					Print(val + "\r");
				}
			}
			if( ZZ_Ratio != null) {
				Print("========ZZ_Ratio count=" + ZZ_Ratio.Count + "=========");
				foreach(double val in ZZ_Ratio) {
					Print(val + "\r");
				}
			}
		}
		
		#endregion
		
		#region File and Dict functions
		
		public string GetFileNameByDateTime(DateTime dt, string path, string accName, string symbol, string ext) {
			Print("GetFileNameByDateTime: " + dt.ToString());
			//path = "C:\\inetpub\\wwwroot\\nt_files\\log\\";
			//ext = "log";
			long flong = DateTime.Now.Minute + 100*DateTime.Now.Hour+ 10000*DateTime.Now.Day + 1000000*DateTime.Now.Month + (long)100000000*DateTime.Now.Year;
			string fname = path + accName + Path.DirectorySeparatorChar + accName + "_" + symbol + "_" + flong.ToString() + "." + ext;
			//Print(", FileName=" + fname);
			//FileTest(DateTime.Now.Minute + 100*DateTime.Now.Hour+ 10000*DateTime.Now.Day+ 1000000*DateTime.Now.Month + (long)100000000*DateTime.Now.Year);

		 	//if(barNo > 0) return;
//			FileStream F = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			
//			using (System.IO.StreamWriter file = 
//				new System.IO.StreamWriter(@fname, true))
//			{
//				for (int i = 0; i <= 3; i++) {
//					file.WriteLine("Line " + i + ":" + i);
//				}
//			}
			return fname;
		}

		public string GetDictKeyByDateTime(DateTime dt, string prefix, string sufix) {
			string kname = prefix + "_" + dt.Year + "-" + dt.Month + "-" + dt.Day + "_" + sufix;
			//Print("GetDictKeyByDateTime: " + dt.ToString() + ", DictKey=" + kname);
			return kname;
		}
		
		public bool AddDictVal(Dictionary<string,double> dict, string key, double val) {
			double dict_val;
			if(dict.TryGetValue(key,out dict_val)) {
				dict[key] = dict_val + val;
			} else {
				dict.Add(key, val);
			}
			return true;
		}
		
		public double SumDictVal(Dictionary<string,double> dict) {
			double sum=0;
			foreach(var item in dict){
				//Print("SumDictVal:" + item.Key);
				sum = sum + item.Value;
			}

			return sum;
		}
		
		#endregion

		
		public PriceAction GetPriceAction(DateTime dt) {
			
			PriceAction pa = new PriceAction(PriceActionType.UnKnown, -1, -1, -1, -1);
			
			int key_date = GetDateByDateTime(dt);
			int t = dt.Hour*100 + dt.Minute;
			
			Dictionary<int,PriceAction> mkt_ctxs = null;
			if(Dict_SpvPR != null)
				Dict_SpvPR.TryGetValue(key_date.ToString(), out mkt_ctxs);
			//Print("key_year, time, Dict_SpvPR, mkt_ctxs=" + key_year.ToString() + "," + t.ToString() + "," + Dict_SpvPR + "," + mkt_ctxs);
			if(mkt_ctxs != null) {
				foreach(var mkt_ctx in mkt_ctxs) {
					//Print("time,mkt_ctx=" + mkt_ctx.Key + "," + mkt_ctx.Value);
					int start = mkt_ctx.Key/10000;
					int end = mkt_ctx.Key % 10000;
					
					if(t >= start && t <= end) {
						pa = mkt_ctx.Value;
						break;
					}
				}
			}
			return pa;
		}
		
		/// <summary>
		/// Check if the price action type allowed for supervised PR 
		/// </summary>
		/// <returns></returns>
		public bool IsSpvAllowed4PAT(PriceActionType pat) {
			int i;
			switch(pat) {
				case PriceActionType.UpTight: //
					i = (1 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (1 & SpvPRBits)=" + i);
					return (1 & SpvPRBits) > 0;
				case PriceActionType.UpWide: //wide up channel
					i = (2 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (2 & SpvPRBits)=" + i);
					return (2 & SpvPRBits) > 0;
				case PriceActionType.DnTight: //
					i = (4 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (4 & SpvPRBits)=" + i);
					return (4 & SpvPRBits) > 0;
				case PriceActionType.DnWide: //wide dn channel
					i = (8 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (8 & SpvPRBits)=" + i);
					return (8 & SpvPRBits) > 0;
				case PriceActionType.RngTight: //
					i = (16 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (16 & SpvPRBits)=" + i);
					return (16 & SpvPRBits) > 0;
				case PriceActionType.RngWide: //
					i = (32 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (32 & SpvPRBits)=" + i);
					return (32 & SpvPRBits) > 0;
				case PriceActionType.UnKnown: //
					i = (64 & SpvPRBits);
					//Print("IsSpvAllowed4PAT:" + pat.ToString() + ", (64 & SpvPRBits)=" + i);
					return (64 & SpvPRBits) > 0;					
				default:
					return false;
			}			
		}

		/// <summary>
		/// Draw Gap from last ZZ to current bar
		/// </summary>
		/// <param name="y_base">the base for y axis</param>
		/// <param name="y_offset">the offset for y axis</param>
		/// <param name="zzGap">the gap size to draw text</param>
		/// <returns></returns>
		public IText DrawGapText(double zzGap, string tag, int bars_ago, double y_base, double y_offset)
		{
			IText gapText = null;
			double y = 0;
			int barNo = CurrentBar-bars_ago;
			Color up_color = Color.Green;
			Color dn_color = Color.Red;
			Color sm_color = Color.Black;
			Color draw_color = sm_color;
			if(zzGap > 0) {
				draw_color = up_color;
//				y = double.Parse(Low[0].ToString())-1 ;
				y = y_base-y_offset ;
			}
			else if (zzGap < 0) {
				draw_color = dn_color;
//				y = double.Parse(High[0].ToString())+1 ;
				y = y_base+y_offset ;
			}
			Print(barNo + "-" + Time[bars_ago] + ": y=" + y + ", zzGap=" + zzGap);
			gapText = DrawText(tag+barNo.ToString(), GetTimeDate(Time[bars_ago], 1)+"\r\n#"+barNo+"\r\nZ:"+zzGap, bars_ago, y, draw_color);
//			}
			if(gapText != null) gapText.Locked = false;
			//if(printOut > 0)
				//PrintLog(true, log_file, CurrentBar + "::" + this.ToString() + " GaP= " + gap + " - " + Time[0].ToShortTimeString());
			return gapText; 
		}
		
		public void PrintLog(bool prt_con, bool prt_file, string fpath, string text) {
			//Print("PrintLog: " + fpath);
			if(prt_con) Print(text); // return;
			if(prt_file) {
				using (System.IO.StreamWriter file = 
					new System.IO.StreamWriter(@fpath, true))
				{
					file.WriteLine(text);
				}
			}
		}		
    }
}
