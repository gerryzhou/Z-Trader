#region Using declarations
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;

using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Indicator;
using NinjaTrader.Strategy;
#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// This file holds all user defined strategy methods.
    /// </summary>
 
	//public enum SessionBreak {AfternoonClose, EveningOpen, MorningOpen, NextDay};
	
	partial class Strategy
    {
		protected IndicatorProxy indicatorProxy = null;
		/*
		protected string AccName = "";
		protected int printOut = 1; // Default setting for PrintOut
		private int algo_mode = 1;
		
		private int tradeDirection = 0; // -1=short; 0-both; 1=long;
		private int tradeStyle = 0; // -1=counter trend; 1=trend following;
		private bool backTest = true; //if it runs for backtesting;
		
		//time=H*10000+M*100+S, S is skipped here;
        private int timeStartH = 1; //10100 Default setting for timeStart hour
		private int timeStartM = 1; //10100 Default setting for timeStart minute
		private int timeStart = -1; //10100 Default setting for timeStart
        private int timeEndH = 14; // Default setting for timeEnd hour
		private int timeEndM = 59; // Default setting for timeEnd minute
		private int timeEnd = -1; // Default setting for timeEnd
		*/
//		public int IsLastBarOnChart() {
//			try{
//				if(Input.Count - CurrentBar <= 2) {
//					return Input.Count;
//				} else {
//					return -1;
//				}
//			}
//			catch(Exception ex){
//				Print("IsLastBarOnChart:" + ex.Message);
//				return -1;
//			}
//		}
		/*
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

*/
		public void SetInitParams() {
			CalculateOnBarClose = true;
			// Triggers the exit on close function 30 seconds prior to session end
			ExitOnClose = true;
			ExitOnCloseSeconds = 30;
			
			SyncAccountPosition = true;
			QuantityType = QuantityType.DefaultQuantity;
			DefaultQuantity = 1;
			TimeInForce = Cbi.TimeInForce.Day;
		}
		
		public string GetFileNameByDateTime(DateTime dt, string path, string accName, string ext) {
			Print("GetFileNameByDateTime: " + dt.ToString());
			//path = "C:\\inetpub\\wwwroot\\nt_files\\log\\";
			//ext = "log";
			long flong = DateTime.Now.Minute + 100*DateTime.Now.Hour+ 10000*DateTime.Now.Day + 1000000*DateTime.Now.Month + (long)100000000*DateTime.Now.Year;
			string fname = path + accName + Path.DirectorySeparatorChar + accName + "_" + flong.ToString() + "." + ext;
			Print(", FileName=" + fname);
			//FileTest(DateTime.Now.Minute + 100*DateTime.Now.Hour+ 10000*DateTime.Now.Day+ 1000000*DateTime.Now.Month + (long)100000000*DateTime.Now.Year);

		 	//if(barNo > 0) return;
//			FileStream F = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			
			using (System.IO.StreamWriter file = 
				new System.IO.StreamWriter(@fname, true))
			{
				for (int i = 0; i <= 20; i++) {
					file.WriteLine("Line " + i + ":" + i);
				}
			}
			return fname;
		}
		

		
        #region Properties
        [Description("Print out")]
        [GridCategory("Parameters")]
        public int PrintOut
        {
            get { return printOut; }
            set { printOut = Math.Max(-1, value); }
        }
        #endregion		
//		public void PrintLog(bool pntcon, string fpath, string text) {
//			Print("PrintLog: " + fpath);
//			if(pntcon) Print(text); // return;
//			using (System.IO.StreamWriter file = 
//				new System.IO.StreamWriter(@fpath, true))
//			{
//				file.WriteLine(text);
//			}
//		}	

    }
}
