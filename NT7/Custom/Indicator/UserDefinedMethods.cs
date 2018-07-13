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
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    /// <summary>
    /// This file holds all user defined indicator methods.
    /// </summary>
	public enum SessionBreak {AfternoonClose, EveningOpen, MorningOpen, NextDay};
	
	public enum PriceActionType {UpWide, UpTight, DnWide, DnTight, RngWide, RngTight, UnKnown};
		
	public class ZigZagSwing {
		public int Bar_Start;
		public int Bar_End;
		public double Size;
		public double TwoBar_Ratio;
		public ZigZagSwing(int bar_start, int bar_end, double size, double twobar_ratio) {
			this.Bar_Start = bar_start;
			this.Bar_End = bar_end;
			this.Size = size;
			this.TwoBar_Ratio = twobar_ratio;
		}
	}
	
	/// <summary>
	/// Supervised Pattern Recognization
	/// 20100610;8301045:UpTight;11011245:RngWide;13011459:DnWide;
	/// </summary>
	public class SpvPR {
		public int Date;
		//Market condition for TimeRange;
		//HHMMHHMM=TimeStart*10000+TimeEnd;
		public Dictionary<int,PriceActionType> Mkt_Ctx; 
		
		public SpvPR (int date, Dictionary<int,PriceActionType> mktCtx) {
			this.Date = date;
			this.Mkt_Ctx = mktCtx;
		}
	}
	
    partial class Indicator
    {
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
		
		#region SpvPR Vars
		/// <summary>
		/// Loaded from supervised file;
		/// Key=Date;
		/// </summary>		
		protected Dictionary<string,Dictionary<int,PriceActionType>> Dict_SpvPR = null;
		
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

		public int GetYearByDateTime(DateTime dt) {
			int year = dt.Year*10000 + dt.Month*100 + dt.Day;
			return year;
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

		/// <summary>
		/// Get the zigzag swing obj fo the barNo.
		/// </summary>
		/// <param name="barNo">barNo</param>
		/// <param name="zzSwings">the list of zzSwings</param>
		/// <returns>obj of the zzSwing</returns>
		public ZigZagSwing GetZZSwing(List<ZigZagSwing> zzSwings, int barNo)
		{
			return zzSwings[barNo];			
		}		
		
		/// <summary>
		/// Set the zigzag swing obj fo the barNo.
		/// </summary>
		/// <param name="barNo">current bar to save the zzSwing</param>
		/// <param name="Bar_Start">start barNo of the zzSwing</param>
		/// <param name="Bar_End">end barNo of the zzSwing</param>
		/// <param name="Size">size of the zzSwing</param>
		public void SetZZSwing(List<ZigZagSwing> zzSwings, int barNo, int bar_Start, int bar_End, double size)
		{
//			ZigZagSwing zzSwing = new ZigZagSwing();
//			 
//			zzSwing.Bar_Start = bar_Start;
//			zzSwing.Bar_End = bar_End;
//			zzSwing.Size = size;
//			
//			zzSwings.Insert(barNo, zzSwing);
			zzSwings[barNo].Bar_Start = bar_Start;
			zzSwings[barNo].Bar_End = bar_End;
			zzSwings[barNo].Size = size;
			ZigZagSwing zzS = GetLastZZSwing(zzSwings, barNo);
			if(zzS != null && zzS.Size != 0) {
				zzSwings[barNo].TwoBar_Ratio = Math.Round(Math.Abs(size/zzS.Size), 2);
				Print("CurBar, pervBar, curSize, prevSize=" + barNo + "," + zzS.Bar_End + "," + size + "," + zzS.Size);
			}
			SaveTwoBarRatio(zzSwings[barNo]);
		}

		protected ZigZagSwing GetLastZZSwing(List<ZigZagSwing> zzSwings, int curBarNo){
			ZigZagSwing zzS = null;
			for(int idx=curBarNo-1; idx>BarsRequired; idx--) {
				if(zzSwings[idx] != null && zzSwings[idx].Size != 0) {
					//Print("CurBar, idx, curSize, prevSize=" + curBarNo + "," + idx+ "," + zzSwings[curBarNo].Size + "," + zzSwings[idx].Size);
					zzS = zzSwings[idx];
					break;
				}
			}			
			return zzS;
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
		
		/// <summary>
		/// Print zig zag swing.
		/// </summary>
		public void PrintZZSwings(List<ZigZagSwing> zzSwings, string log_file, int printOut)
		{ 
			if( ZZ_Count_0_6 == null) 
				ZZ_Count_0_6 = new Dictionary<string,double>();
			if( ZZ_Count_6_10 == null) 
				ZZ_Count_6_10 = new Dictionary<string,double>();
			if( ZZ_Count_10_16 == null) 
				ZZ_Count_10_16 = new Dictionary<string,double>();
			if( ZZ_Count_16_22 == null) 
				ZZ_Count_16_22 = new Dictionary<string,double>();
			if( ZZ_Count_22_30 == null) 
				ZZ_Count_22_30 = new Dictionary<string,double>();
			if( ZZ_Count_30_ == null) 
				ZZ_Count_30_ = new Dictionary<string,double>();
			if( ZZ_Count == null) 
				ZZ_Count = new Dictionary<string,double>();
			
			if( ZZ_Sum_0_6 == null)			
				ZZ_Sum_0_6 = new Dictionary<string,double>();
			if( ZZ_Sum_6_10 == null) 
				ZZ_Sum_6_10 = new Dictionary<string,double>();
			if( ZZ_Sum_10_16 == null) 
				ZZ_Sum_10_16 = new Dictionary<string,double>();
			if( ZZ_Sum_16_22 == null) 
				ZZ_Sum_16_22 = new Dictionary<string,double>();
			if( ZZ_Sum_22_30 == null) 
				ZZ_Sum_22_30 = new Dictionary<string,double>();
			if( ZZ_Sum_30_ == null) 
				ZZ_Sum_30_ = new Dictionary<string,double>();
			if( ZZ_Sum == null) 
				ZZ_Sum = new Dictionary<string,double>();
			
			String str_Plus = " ++ ";
			String str_Minus = " -- ";
			String str_Minutes = "m";
			Update();
			
			double zzSize = 0;
			double zzSizeAbs = -1;
			int barStart, barEnd;
			for(int idx = BarsRequired; idx < zzSwings.Count; idx++) {
				zzSize = zzSwings[idx].Size;
				barStart = zzSwings[idx].Bar_Start;
				barEnd = zzSwings[idx].Bar_End;

				zzSizeAbs = Math.Abs(zzSize);
				String str_suffix = "";
				//Print(idx.ToString() + " - ZZSizeSeries=" + zzS);
				if(zzSize>0) str_suffix = str_Plus;
				else if(zzSize<0) str_suffix = str_Minus;
				if(zzSize != 0)				
					PrintLog(true, log_file, CurrentBar + " PrintZZSize called from GS:" + zzSize + "," + barStart + "," + barEnd);
				DateTime dt = (zzSize==0||barStart<0||barEnd<0) ? Time[0] : Time[CurrentBar-barEnd];
				string key = "";
				
				if(zzSizeAbs > 0 && zzSizeAbs <6){
					key = GetDictKeyByDateTime(dt, "zz0-6", "");
					AddDictVal(ZZ_Count_0_6,key,1);
					AddDictVal(ZZ_Sum_0_6,key,zzSizeAbs);
				}
				else if(zzSizeAbs >= 6 && zzSizeAbs <10){
					key = GetDictKeyByDateTime(dt, "zz6-10", "");
					AddDictVal(ZZ_Count_6_10,key,1);
					AddDictVal(ZZ_Sum_6_10,key,zzSizeAbs);
				}
				else if(zzSizeAbs >= 10 && zzSizeAbs <16){
					key = GetDictKeyByDateTime(dt, "zz10-16", "");
					AddDictVal(ZZ_Count_10_16,key,1);
					AddDictVal(ZZ_Sum_10_16,key,zzSizeAbs);
					if(printOut > 1)
						PrintLog(true, log_file, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=10" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				else if(zzSizeAbs >= 16 && zzSizeAbs <22){
					key = GetDictKeyByDateTime(dt, "zz16-22", "");
					AddDictVal(ZZ_Count_16_22,key,1);
					AddDictVal(ZZ_Sum_16_22,key,zzSizeAbs);
					if(printOut > 1)
						PrintLog(true, log_file, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=16" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				else if(zzSizeAbs >= 22 && zzSizeAbs <30){
					key = GetDictKeyByDateTime(dt, "zz22-30", "");
					AddDictVal(ZZ_Count_22_30,key,1);
					AddDictVal(ZZ_Sum_22_30,key,zzSizeAbs);
					if(printOut > 1)
						PrintLog(true, log_file, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=22" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				else if(zzSizeAbs >= 30){
					key = GetDictKeyByDateTime(dt, "zz30-", "");
					AddDictVal(ZZ_Count_30_,key,1);
					AddDictVal(ZZ_Sum_30_,key,zzSizeAbs);
					if(printOut > 1)
						PrintLog(true, log_file, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=30" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				if(zzSize != 0) {
					//DrawZZSizeText(idx, "txt-");
					if(zzSizeAbs < 10)
						if(printOut > 2)
							PrintLog(true, log_file, idx.ToString() + "-zzS= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "]" );
					//lastZZIdx = idx;
					key = GetDictKeyByDateTime(dt, "zzCount", "");
					AddDictVal(ZZ_Count,key,1);
					AddDictVal(ZZ_Sum,key,zzSizeAbs);
				}
			}
			double ZZ_Count_Total = SumDictVal(ZZ_Count);
			double ZZ_Sum_Total = SumDictVal(ZZ_Sum);
			double ZZ_Count_Avg = ZZ_Count_Total/Day_Count;
			double ZZ_Sum_Avg = ZZ_Sum_Total/Day_Count;
			
			if(printOut > 2) {
				PrintLog(true, log_file, CurrentBar + "-" + Instrument.FullName 
					+ "\r\n ZZ_Count_Avg \t ZZ_Count \t ZZ_Count_Days \t"
					+ "\r\n" + String.Format("{0:0.##}", ZZ_Count_Avg) 
					+ "\t" + ZZ_Count_Total 
					+ "\t" + Day_Count 
					
					+ "\r\n ZZ_Count_0_6 \t" + SumDictVal(ZZ_Count_0_6)
					+ "\r\n ZZ_Count_6_10 \t" + SumDictVal(ZZ_Count_6_10)
					+ "\r\n ZZ_Count_10_16 \t" + SumDictVal(ZZ_Count_10_16)
					+ "\r\n ZZ_Count_16_22 \t" + SumDictVal(ZZ_Count_16_22)
					+ "\r\n ZZ_Count_22_30 \t" + SumDictVal(ZZ_Count_22_30) 
					+ "\r\n ZZ_Count_30_ \t" + SumDictVal(ZZ_Count_30_));
				PrintLog(true, log_file, CurrentBar + "-" + Instrument.FullName 
					+ "\r\n ZZ_Sum_Avg \t ZZ_Sum \t ZZ_Sum_Days \t"
					+ "\r\n" + String.Format("{0:0.##}", ZZ_Sum_Avg) 
					+ "\t " + ZZ_Sum_Total 
					+ "\t " + Day_Count
					
					+ "\r\n ZZ_Sum_0_6 \t" + SumDictVal(ZZ_Sum_0_6) 
					+ "\r\n ZZ_Sum_6_10 \t" + SumDictVal(ZZ_Sum_6_10) 
					+ "\r\n ZZ_Sum_10_16 \t" + SumDictVal(ZZ_Sum_10_16) 
					+ "\r\n ZZ_Sum_16_22 \t" + SumDictVal(ZZ_Sum_16_22)
					+ "\r\n ZZ_Sum_22_30 \t" + SumDictVal(ZZ_Sum_22_30) 
					+ "\r\n ZZ_Sum_30_ \t" + SumDictVal(ZZ_Sum_30_));
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
			Print(", FileName=" + fname);
			//FileTest(DateTime.Now.Minute + 100*DateTime.Now.Hour+ 10000*DateTime.Now.Day+ 1000000*DateTime.Now.Month + (long)100000000*DateTime.Now.Year);

		 	//if(barNo > 0) return;
//			FileStream F = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			
			using (System.IO.StreamWriter file = 
				new System.IO.StreamWriter(@fname, true))
			{
				for (int i = 0; i <= 3; i++) {
					file.WriteLine("Line " + i + ":" + i);
				}
			}
			return fname;
		}

		public string GetDictKeyByDateTime(DateTime dt, string prefix, string sufix) {
			string kname = prefix + "_" + dt.Year + "-" + dt.Month + "-" + dt.Day + "_" + sufix;
			Print("GetDictKeyByDateTime: " + dt.ToString() + ", DictKey=" + kname);
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
				Print("SumDictVal:" + item.Key);
				sum = sum + item.Value;
			}

			return sum;
		}
		
		#endregion

		#region Supervised pattern recognition
		
		public FileInfo[] GetSpvFile(string srcDir, string symbol) {
			Print("GetSupervisedFile src: " + srcDir);
		    DirectoryInfo DirInfo = new DirectoryInfo(srcDir);

//            var filesInOrder = from f in DirInfo.EnumerateFiles()
//                               orderbydescending f.CreationTime
//                               select f;
			
//			var filesInOrder = DirInfo.GetFiles("*.*",SearchOption.AllDirectories).OrderBy(f => f.LastWriteTime)
//								.ToList();
			//DirectoryInfo dir = new DirectoryInfo (folderpath);

			FileInfo[] filesInOrder = DirInfo.GetFiles().OrderByDescending(p => p.LastWriteTime).ToArray();
			
            foreach (FileInfo item in filesInOrder)
            {
                Print("cmdFile=" + item.FullName);
            }
			
			return filesInOrder;
		}
		
		public Dictionary<string,Dictionary<int,PriceActionType>> ReadSpvFile(string srcDir, string symbol) {
			//Dictionary<string,Dictionary<int,PriceActionType>> 
			Dict_SpvPR = new Dictionary<string,Dictionary<int,PriceActionType>>();
			string src = srcDir + symbol + ".txt";
			Print("ReadSpvPRFile src: " + src);
//			if (!src.Exists)
//			{
//				return paraMap;
//			}
	
			int counter = 0;  
			string line;

			// Read the file and display it line by line.  
			System.IO.StreamReader file =   
				new System.IO.StreamReader(src);//@"c:\test.txt");
			while((line = file.ReadLine()) != null)  
			{
				string[] pa = line.Split(';');
				Dictionary<int,PriceActionType> mkt_ctxs = new Dictionary<int,PriceActionType>();
				for(int i=1; i<pa.Length; i++) {
					string[] mkt_ctx = pa[i].Split(':');
					int dt;
					int.TryParse(mkt_ctx[0], out dt);
					mkt_ctxs.Add(dt,(PriceActionType)Enum.Parse(typeof(PriceActionType), mkt_ctx[1]));
				}
				if(mkt_ctxs.Count > 0) {
					Dict_SpvPR.Add(pa[0], mkt_ctxs);
				}
				Print(line);
				counter++;
			}

			file.Close();
			Print("There were {0} lines." + counter);
			// Suspend the screen.
			//System.Console.ReadLine();
			foreach(var pair in Dict_SpvPR) {
				Print("mktCtx: key,val=" + pair.Key + "," + pair.Value + "," + pair.ToString());
				Dictionary<int,PriceActionType> mkcnd = (Dictionary<int,PriceActionType>)pair.Value;
				foreach(var cnd in mkcnd) {
					Print("time,cnd=" + cnd.Key + "," + cnd.Value);
				}
			}
			return Dict_SpvPR;
		}
		
		public PriceActionType GetPriceActType(DateTime dt) {
			
			PriceActionType pat = PriceActionType.UnKnown;
			
			int key_year = GetYearByDateTime(dt);
			int t = dt.Hour*100 + dt.Minute;
			
			Dictionary<int,PriceActionType> mkt_ctxs = null;
			if(Dict_SpvPR != null)
				Dict_SpvPR.TryGetValue(key_year.ToString(), out mkt_ctxs);
			Print("key_year, time, Dict_SpvPR, mkt_ctxs=" + key_year.ToString() + "," + t.ToString() + "," + Dict_SpvPR + "," + mkt_ctxs);
			if(mkt_ctxs != null) {
				foreach(var mkt_ctx in mkt_ctxs) {
					Print("time,mkt_ctx=" + mkt_ctx.Key + "," + mkt_ctx.Value);
					int start = mkt_ctx.Key/10000;
					int end = mkt_ctx.Key % 10000;
					
					if(t >= start && t <= end) {
						pat = (PriceActionType)mkt_ctx.Value;
						break;
					}
				}
			}
			
//			Dictionary<string,Dictionary<int,PriceActionType>> Dict_SpvPR = new Dictionary<string,Dictionary<int,PriceActionType>>();
//			string src = srcDir + symbol + ".txt";
//			Print("ReadSpvPRFile src: " + src);
////			if (!src.Exists)
////			{
////				return paraMap;
////			}
//	
//			int counter = 0;  
//			string line;
//
//			// Read the file and display it line by line.  
//			System.IO.StreamReader file =   
//				new System.IO.StreamReader(src);//@"c:\test.txt");
//			while((line = file.ReadLine()) != null)  
//			{
//				string[] pa = line.Split(';');
//				Dictionary<int,PriceActionType> mkt_ctxs = new Dictionary<int,PriceActionType>();
//				for(int i=1; i<pa.Length; i++) {
//					string[] mkt_ctx = pa[i].Split(':');
//					int dt;
//					int.TryParse(mkt_ctx[0], out dt);
//					mkt_ctxs.Add(dt,(PriceActionType)Enum.Parse(typeof(PriceActionType), mkt_ctx[1]));
//				}
//				if(mkt_ctxs.Count > 0) {
//					Dict_SpvPR.Add(pa[0], mkt_ctxs);
//				}
//				Print(line);
//				counter++;
//			}
//
//			file.Close();
//			Print("There were {0} lines." + counter);
//			// Suspend the screen.
//			//System.Console.ReadLine();
//			foreach(var pair in Dict_SpvPR) {
//				Print("mktCtx: key,val=" + pair.Key + "," + pair.Value + "," + pair.ToString());
//				Dictionary<int,PriceActionType> mkcnd = (Dictionary<int,PriceActionType>)pair.Value;
//				foreach(var cnd in mkcnd) {
//					Print("time,cnd=" + cnd.Key + "," + cnd.Value);
//				}
//			}
			return pat;
		}		
		#endregion
		
		public void PrintLog(bool pntcon, string fpath, string text) {
			//Print("PrintLog: " + fpath);
			if(pntcon) Print(text); // return;
			using (System.IO.StreamWriter file = 
				new System.IO.StreamWriter(@fpath, true))
			{
				file.WriteLine(text);
			}
		}		
    }
}
