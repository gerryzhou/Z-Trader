#region Using declarations
using System;
using System.ComponentModel;
using System.IO;
using System.Drawing;
//using System.Collections;
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
    /// This file holds all user defined indicator classes.
    /// </summary>
	public enum SessionBreak {AfternoonClose, EveningOpen, MorningOpen, NextDay};
	
	//public enum PriceActionType {UpTight, UpWide, DnTight, DnWide, RngTight, RngWide, UnKnown};
	
	/// <summary>
	/// PriceAction include PriceAtionType and the volatility measurement
	/// min and max ticks of up/down expected
	/// shrinking, expanding, or paralleling motion;
	/// </summary>
//	public class PriceAction {
//		public PriceActionType paType;
//		public int minUpTicks;
//		public int maxUpTicks;
//		public int minDownTicks;
//		public int maxDownTicks;
//		public PriceAction(PriceActionType pat, int min_UpTicks, int max_UpTicks, int min_DnTicks, int max_DnTicks) {
//			this.paType = pat;
//			this.minUpTicks = min_UpTicks;
//			this.maxUpTicks = max_UpTicks;
//			this.minDownTicks = min_DnTicks;
//			this.maxDownTicks = max_DnTicks;
//		}
//	}
	
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
	
    partial class Indicator
    {
		/*
		#region SpvPR Vars
		/// <summary>
		/// Loaded from supervised file;
		/// Key1=Date; Key2=Time;
		/// </summary>		
		protected Dictionary<string,Dictionary<int,PriceAction>> Dict_SpvPR1 = null;
		
		protected int SpvPRBits1 = 0;
		
		#endregion
		
		protected double Day_Count1 = 0;
		protected double Week_Count1 = 0;
		
		public void DayWeekMonthCount1() {
			if(CurrentBar < BarsRequired) return;
			if(Time[0].Day != Time[1].Day) {
				Day_Count1 ++;
			}
			if(Time[0].DayOfWeek == DayOfWeek.Sunday &&  Time[1].DayOfWeek != DayOfWeek.Sunday) {
				Week_Count1 ++;
			}
			
		} */
	
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
		
		protected void ReadSpvPRLine(string line) {
				string[] line_pa = line.Split(';');
				Dictionary<int,PriceAction> mkt_ctxs = new Dictionary<int,PriceAction>();
				for(int i=1; i<line_pa.Length; i++) {
					int t, minUp, maxUp, minDn, maxDn;
					
					string[] mkt_ctx = line_pa[i].Split(':');
					int.TryParse(mkt_ctx[0], out t);//parse the time of the PA;
					
					string[] pa = mkt_ctx[1].Split('#');
					PriceActionType pat = (PriceActionType)Enum.Parse(typeof(PriceActionType), pa[0]);//parse the PA type;
					
					string[] v = pa[1].Split('-');
					int.TryParse(v[0], out minUp);
					int.TryParse(v[1], out maxUp);
					int.TryParse(v[2], out minDn);
					int.TryParse(v[3], out maxDn);					
					
					mkt_ctxs.Add(t, new PriceAction(pat, minUp, maxUp, minDn, maxDn));
				}
				if(mkt_ctxs.Count > 0) {
					Dict_SpvPR.Add(line_pa[0], mkt_ctxs);
				}			
		}
		
		/// <summary>
		/// 20170522;9501459:UpTight#10-16-3-5
		/// </summary>
		/// <param name="srcDir"></param>
		/// <param name="symbol"></param>
		/// <returns></returns>
		public Dictionary<string,Dictionary<int,PriceAction>> ReadSpvFile(string srcDir, string symbol) {
			//Dictionary<string,Dictionary<int,PriceActionType>> 
			Dict_SpvPR = new Dictionary<string,Dictionary<int,PriceAction>>();
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
				if(line.StartsWith("//")) continue; //comments line, skip it;
				
				ReadSpvPRLine(line.Substring(1, line.Length-3));//remove leading " and ending ",
				//Print(line);
				counter++;
			}

			file.Close();

//			foreach(var pair in Dict_SpvPR) {
				//Print("mktCtx: key,val=" + pair.Key + "," + pair.Value + "," + pair.ToString());
//				Dictionary<int,PriceAction> mkcnd = (Dictionary<int,PriceAction>)pair.Value;
//				foreach(var cnd in mkcnd) {
//					Print("time,cnd=" + cnd.Key + "," + cnd.Value);
//				}
//			}
			return Dict_SpvPR;
		}
		
		/// <summary>
		/// 20170522;9501459:UpTight#10-16-3-5
		/// </summary>
		/// <param name="srcDir"></param>
		/// <param name="symbol"></param>
		/// <returns></returns>
		public Dictionary<string,Dictionary<int,PriceAction>> ReadSpvPRList() {
			//Dictionary<string,Dictionary<int,PriceActionType>> 
			Dict_SpvPR = new Dictionary<string,Dictionary<int,PriceAction>>();			
			
			int counter = 0;  
			//string line;
			foreach(string dayPR in spvPRDay) {
				Print(dayPR);
				ReadSpvPRLine(dayPR);
				counter++;
			}
			Print("ReadSpvPRList:" + counter);
			return Dict_SpvPR;
		}
		
		#endregion
		
		#region ZigZagSwing Functions
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
				//Print("CurBar, pervBar, curSize, prevSize=" + barNo + "," + zzS.Bar_End + "," + size + "," + zzS.Size);
			}
			//SaveTwoBarRatio(zzSwings[barNo]);
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
		
		public double GetZZSwingSize(int startBar, int endBar) {
			double zzSize = 0;
			if(Close[CurrentBar-endBar] > Close[CurrentBar-startBar]) {
				zzSize = High[CurrentBar-endBar] - Low[CurrentBar-startBar];
			}
			
			if(Close[CurrentBar-endBar] < Close[CurrentBar-startBar]) {
				zzSize = Low[CurrentBar-endBar] - High[CurrentBar-startBar];
			}
			return zzSize;
		}
		
		protected ILine DrawZigZagLine(int startBarsAgo, int endBarsAgo, double zzGap, string tag, Color color) {
			//Print("DrawZigZag called");
			ILine zzLine = null;
			
			double startY = zzGap > 0? Low[startBarsAgo] : High[startBarsAgo];
			double endY = zzGap > 0? High[endBarsAgo] : Low[endBarsAgo];
			
//			int startBar = GetLastReverseBar(endBar);
			//gap = 0;
//			if(startBar > 0 && CurrentBar > BarsRequired) {
//				if(printOut > 3)
//					Print("DrawLine CurrentBar, zzSwings= " + CurrentBar + "," + zzSwings.Count + ", CurrentBar-startBar, Value[CurrentBar-startBar], 0, reverseValue[0]=" + (CurrentBar-startBar) + "," + Value[CurrentBar-startBar] + "," + 0 + "," + reverseValue[0]);
			//DrawLine("My line" + CurrentBar, CurrentBar-startBar, sarSeries[CurrentBar-startBar], 0, sarSeries[0], Color.Blue);
				zzLine = DrawLine(tag + CurrentBar, startBarsAgo, startY, endBarsAgo, endY, color);
//				curZZGap = endValue - reverseValue[CurrentBar-startBar] ;
//			}
//			curPriceAction = GetPriceAction(Time[0]);
			//Print("Time, Pat=" + Time[0] + "," + curPriceActType.ToString());
//			SetZZSwing(zzSwings, endBar, startBar, endBar, curZZGap);
			return zzLine;
		}
		
		/// <summary>
		/// Print zig zag swing.
		/// </summary>
		public void PrintZZSwings(List<ZigZagSwing> zzSwings, string log_file, int printOut, bool back_test, int timeStartHM, int timeEndHM)
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
					PrintLog(printOut>3, !back_test, log_file, CurrentBar + " PrintZZSize called from GS:" + zzSize + "," + barStart + "," + barEnd);
				DateTime dt_start = (zzSize==0||barStart<0||barEnd<0) ? Time[0] : Time[CurrentBar-barStart];
				DateTime dt_end = (zzSize==0||barStart<0||barEnd<0) ? Time[0] : Time[CurrentBar-barEnd];
				
				if (!IsTimeInSpan(dt_start, timeStartHM, timeEndHM))
					continue;
				
				string key = "";
				
				if(zzSizeAbs > 0 && zzSizeAbs <6){
					key = GetDictKeyByDateTime(dt_end, "zz0-6", "");
					AddDictVal(ZZ_Count_0_6,key,1);
					AddDictVal(ZZ_Sum_0_6,key,zzSizeAbs);
				}
				else if(zzSizeAbs >= 6 && zzSizeAbs <10){
					key = GetDictKeyByDateTime(dt_end, "zz6-10", "");
					AddDictVal(ZZ_Count_6_10,key,1);
					AddDictVal(ZZ_Sum_6_10,key,zzSizeAbs);
				}
				else if(zzSizeAbs >= 10 && zzSizeAbs <16){
					key = GetDictKeyByDateTime(dt_end, "zz10-16", "");
					AddDictVal(ZZ_Count_10_16,key,1);
					AddDictVal(ZZ_Sum_10_16,key,zzSizeAbs);
					if(printOut > 1)
						PrintLog(true, !back_test, log_file, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=10" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				else if(zzSizeAbs >= 16 && zzSizeAbs <22){
					key = GetDictKeyByDateTime(dt_end, "zz16-22", "");
					AddDictVal(ZZ_Count_16_22,key,1);
					AddDictVal(ZZ_Sum_16_22,key,zzSizeAbs);
					if(printOut > 1)
						PrintLog(true, !back_test, log_file, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=16" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				else if(zzSizeAbs >= 22 && zzSizeAbs <30){
					key = GetDictKeyByDateTime(dt_end, "zz22-30", "");
					AddDictVal(ZZ_Count_22_30,key,1);
					AddDictVal(ZZ_Sum_22_30,key,zzSizeAbs);
					if(printOut > 1)
						PrintLog(true, !back_test, log_file, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=22" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				else if(zzSizeAbs >= 30){
					key = GetDictKeyByDateTime(dt_end, "zz30-", "");
					AddDictVal(ZZ_Count_30_,key,1);
					AddDictVal(ZZ_Sum_30_,key,zzSizeAbs);
					if(printOut > 1)
						PrintLog(true, !back_test, log_file, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=30" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				if(zzSize != 0) {
					//DrawZZSizeText(idx, "txt-");
					if(zzSizeAbs < 10)
						if(printOut > 2)
							PrintLog(true, !back_test, log_file, idx.ToString() + "-zzS= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "]" );
					//lastZZIdx = idx;
					key = GetDictKeyByDateTime(dt_end, "zzCount", "");
					AddDictVal(ZZ_Count,key,1);
					AddDictVal(ZZ_Sum,key,zzSizeAbs);
				}
			}
			
			double ZZ_Count_Total = SumDictVal(ZZ_Count);
			double ZZ_Sum_Total = SumDictVal(ZZ_Sum);
			double ZZ_Count_Avg = ZZ_Count_Total/Day_Count;
			double ZZ_Sum_Avg = ZZ_Sum_Total/Day_Count;
			
			double zzCount_0_6 = SumDictVal(ZZ_Count_0_6);
			double zzCount_6_10 = SumDictVal(ZZ_Count_6_10);
			double zzCount_10_16 = SumDictVal(ZZ_Count_10_16);
			double zzCount_16_22 = SumDictVal(ZZ_Count_16_22);
			double zzCount_22_30 = SumDictVal(ZZ_Count_22_30);
			double zzCount_30_ = SumDictVal(ZZ_Count_30_);
			
			double zzSum_0_6 = SumDictVal(ZZ_Sum_0_6);
			double zzSum_6_10 = SumDictVal(ZZ_Sum_6_10);
			double zzSum_10_16 = SumDictVal(ZZ_Sum_10_16);
			double zzSum_16_22 = SumDictVal(ZZ_Sum_16_22);
			double zzSum_22_30 = SumDictVal(ZZ_Sum_22_30);
			double zzSum_30_ = SumDictVal(ZZ_Sum_30_);
			
			if(printOut > 2) {
				PrintLog(true, !back_test, log_file, CurrentBar + "-" + Instrument.FullName 
					+ "\r\n ZZ_Count_Avg \t ZZ_Count \t ZZ_Count_Days \t"
					+ "\r\n" + String.Format("{0:0.##}", ZZ_Count_Avg) 
					+ "\t" + ZZ_Count_Total 
					+ "\t" + Day_Count 
					
					+ "\r\n ZZ_Count_0_6 \t" + zzCount_0_6 + "\t" + String.Format("{0:0.#}", 100*zzCount_0_6/ZZ_Count_Total) + "%"
					+ "\r\n ZZ_Count_6_10 \t" + zzCount_6_10 + "\t" + String.Format("{0:0.#}", 100*zzCount_6_10/ZZ_Count_Total) + "%"
					+ "\r\n ZZ_Count_10_16 \t" + zzCount_10_16 + "\t" + String.Format("{0:0.#}", 100*zzCount_10_16/ZZ_Count_Total) + "%"
					+ "\r\n ZZ_Count_16_22 \t" + zzCount_16_22 + "\t" + String.Format("{0:0.#}", 100*zzCount_16_22/ZZ_Count_Total) + "%"
					+ "\r\n ZZ_Count_22_30 \t" + zzCount_22_30 + "\t" + String.Format("{0:0.#}", 100*zzCount_22_30/ZZ_Count_Total) + "%"
					+ "\r\n ZZ_Count_30_ \t" + zzCount_30_ + "\t" + String.Format("{0:0.#}", 100*zzCount_30_/ZZ_Count_Total) + "%");
				
				PrintLog(true, !back_test, log_file, CurrentBar + "-" + Instrument.FullName 
					+ "\r\n ZZ_Sum_Avg \t ZZ_Sum \t ZZ_Sum_Days \t"
					+ "\r\n" + String.Format("{0:0.##}", ZZ_Sum_Avg) 
					+ "\t " + ZZ_Sum_Total 
					+ "\t " + Day_Count
					
					+ "\r\n ZZ_Sum_0_6 \t" + zzSum_0_6 + "\t" + String.Format("{0:0.#}", 100*zzSum_0_6/ZZ_Sum_Total) + "%" 
					+ "\r\n ZZ_Sum_6_10 \t" + zzSum_6_10 + "\t" + String.Format("{0:0.#}", 100*zzSum_6_10/ZZ_Sum_Total) + "%"  
					+ "\r\n ZZ_Sum_10_16 \t" + zzSum_10_16 + "\t" + String.Format("{0:0.#}", 100*zzSum_10_16/ZZ_Sum_Total) + "%"  
					+ "\r\n ZZ_Sum_16_22 \t" + zzSum_16_22 + "\t" + String.Format("{0:0.#}", 100*zzSum_16_22/ZZ_Sum_Total) + "%" 
					+ "\r\n ZZ_Sum_22_30 \t" + zzSum_22_30 + "\t" + String.Format("{0:0.#}", 100*zzSum_22_30/ZZ_Sum_Total) + "%"  
					+ "\r\n ZZ_Sum_30_ \t" + zzSum_30_ + "\t" + String.Format("{0:0.#}", 100*zzSum_30_/ZZ_Sum_Total) + "%" );
			}
		}
		
		#endregion
		
		#region spvPRDay
		
		protected List<string> spvPRDay = new List<string>(){
			"20160714;7501059:RngWide#6-10-6-10",
			"20160715;7501059:RngWide#6-10-6-10",
			"20160718;7501059:RngWide#6-10-6-10",
			"20160719;7501059:RngWide#6-10-6-10",
			"20160720;7501059:RngWide#6-10-6-10",
			"20160721;7501059:RngWide#6-10-6-10",
			"20160722;7501059:RngWide#6-10-6-10",
			"20160725;7501059:RngWide#6-10-6-10",
			"20160726;7501059:RngWide#6-10-6-10",
			"20160727;7501059:RngWide#6-10-6-10",
			"20160728;7501059:RngWide#6-10-6-10",
			"20160729;7501059:RngWide#6-10-6-10",
			"20160731;7501059:RngWide#6-10-6-10",
			"20160801;7501059:RngWide#6-10-6-10",
			"20160802;7501059:RngWide#6-10-6-10",
			"20160803;7501059:RngWide#6-10-6-10",
			"20160804;7501059:RngWide#6-10-6-10",
			"20160807;7501059:RngWide#6-10-6-10",
			"20160808;7501059:RngWide#6-10-6-10",
			"20160809;7501059:RngWide#6-10-6-10",
			"20160810;7501059:RngWide#6-10-6-10",
			"20160811;7501059:RngWide#6-10-6-10",
			"20160814;7501059:RngWide#6-10-6-10",
			"20160815;7501059:RngWide#6-10-6-10",
			"20160816;7501059:RngWide#6-10-6-10",
			"20160817;7501059:RngWide#6-10-6-10",
			"20160818;7501059:RngWide#6-10-6-10",
			"20160821;7501059:RngWide#6-10-6-10",
			"20160822;7501059:RngWide#6-10-6-10",
			"20160823;7501059:RngWide#6-10-6-10",
			"20160824;7501059:RngWide#6-10-6-10",
			"20160825;7501059:RngWide#6-10-6-10",
			"20160828;7501059:RngWide#6-10-6-10",
			"20160829;7501059:RngWide#6-10-6-10",
			"20160830;7501059:RngWide#6-10-6-10",
			"20160831;7501059:RngWide#6-10-6-10",
			"20160901;7501059:RngWide#6-10-6-10",
			"20160904;7501059:RngWide#6-10-6-10",
			"20160905;7501059:RngWide#6-10-6-10",
			"20160906;7501059:RngWide#6-10-6-10",
			"20160907;7501059:RngWide#6-10-6-10",
			"20160908;7501059:RngWide#6-10-6-10",
			"20160909;7501059:RngWide#6-10-6-10",
			"20170522;9501459:UpTight#10-16-3-5",
			"20170523;8501059:UpTight#10-16-3-5",
			"20170524;13051459:UpTight#10-16-3-5",
			"20170525;8401459:UpTight#10-16-3-5",
			"20170526;8401459:RngTight#10-16-3-5",
			"20170529;8401459:RngTight#10-16-3-5",
			"20170530;8401459:RngTight#10-16-3-5",
			"20170531;8200910:DnTight#10-16-3-5;13101459:UpTight#10-16-3-5",
			"20170601;8401459:UpTight#10-16-3-5",
			"20170602;8591459:UpTight#10-16-3-5",
			"20170605;8401459:RngTight#10-16-3-5",
			"20170606;8401401:UpWide#10-16-3-5",
			"20170607;8401151:DnWide#10-16-3-5;11521459:UpTight#10-16-3-5",
			"20170608;8401123:UpTight#10-16-3-5;11301459:DnTight#10-16-3-5",
			"20170609;8040940:UpTight#10-16-3-5;9411459:DnTight#10-16-3-5",
			"20170612;8401205:RngWide#10-16-3-5;13011459:RngTight#10-16-3-5",
			"20170613;1000700:RngTight#10-16-3-5;8301000:DnTight#10-16-3-5;10001459:UpTight#10-16-3-5",
			"20170614;7301105:DnTight#10-16-3-5;11011305:RngTight#10-16-3-5;13061415:DnTight#10-16-3-5;14161459:UpTight#10-16-3-5",
			"20170615;1300800:DnTight#10-16-3-5;8300930:RngWide#10-16-3-5;9301459:UpTight#10-16-3-5",
			"20170616;6301000:DnTight#10-16-3-5;10051459:UpTight#10-16-3-5",
			//April 2018
			"20180402;7501059:UpWide#6-22-6-22",
			"20180403;7501059:UpWide#6-22-6-22",
			"20180404;7501059:UpWide#6-22-6-22",
			"20180405;7501059:UpWide#6-22-6-22",
			"20180406;7501059:UpWide#6-22-6-22",
			"20180409;7501059:UpWide#6-22-6-22",
			"20180410;7501059:UpWide#6-22-6-22",
			"20180411;7501059:UpWide#6-22-6-22",
			"20180412;7501059:UpWide#6-22-6-22",
			"20180413;7501059:UpWide#6-22-6-22",
			"20180416;7501059:UpWide#6-22-6-22",
			"20180417;7501059:UpWide#6-22-6-22",
			"20180418;7501059:UpWide#6-22-6-22",
			"20180419;7501059:DnWide#6-22-6-22",
			"20180420;7501059:DnWide#6-22-6-22",
			"20180423;7501059:RngWide#6-22-6-22",
			"20180424;7501059:DnWide#6-22-6-22",
			"20180425;7501059:RngWide#6-22-6-22",
			"20180426;7501059:UpWide#6-22-6-22",
			"20180427;7501059:UpWide#6-22-6-22",
			"20180430;7501059:RngWide#6-22-6-22",
			//May 2018
			"20180501;7501059:RngWide#6-22-6-22",
			"20180502;7501059:RngWide#6-22-6-22",
			"20180503;7501059:RngWide#6-22-6-22",
			"20180504;7501059:UpWide#6-22-6-22",
			"20180507;7501059:UpWide#6-22-6-22",
			"20180508;7501059:UpWide#6-22-6-22",
			"20180509;7501059:UpWide#6-22-6-22",
			"20180510;7501059:UpWide#6-22-6-22",
			"20180511;7501059:UpWide#6-22-6-22",
			"20180514;7501059:UpWide#6-22-6-22",
			"20180515;7501059:UpWide#6-22-6-22",
			"20180516;7501059:UpWide#6-22-6-22",
			"20180517;7501059:UpWide#6-22-6-22",
			"20180518;7501059:UpWide#6-22-6-22",
			"20180521;7501059:DnWide#6-22-6-22",
			"20180522;7501059:DnWide#6-22-6-22",
			"20180523;7501059:RngWide#6-22-6-22",
			"20180524;7501059:DnWide#6-22-6-22",
			"20180525;7501059:RngWide#6-22-6-22",
			"20180528;7501059:UnKnown#6-22-6-22",
			"20180529;7501059:UpWide#6-22-6-22",
			"20180530;7501059:UpWide#6-22-6-22",
			"20180531;7501059:UpWide#6-22-6-22",
		};
		#endregion spvPRDay
    }
}
