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
	
	public class IndicatorParams {
		//public TrendDirection trendDir = TrendDirection.UnKnown;
		
	}
	
    partial class Indicator
    {		
		protected IndicatorSignal indicatorSignal = null;
	
		#region Signal Functions
		public virtual IndicatorSignal CheckIndicatorSignal() {
			return null;
		}
		
		public IndicatorSignal GetIndicatorSignal() {
			return indicatorSignal;
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
		public void PrintZZSwings(List<ZigZagSwing> zzSwings, int printOut, bool back_test, int timeStartHM, int timeEndHM)
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
					PrintLog(GetPrintOut()>3, !back_test, CurrentBar + " PrintZZSize called from GS:" + zzSize + "," + barStart + "," + barEnd);
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
						PrintLog(true, !back_test, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=10" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				else if(zzSizeAbs >= 16 && zzSizeAbs <22){
					key = GetDictKeyByDateTime(dt_end, "zz16-22", "");
					AddDictVal(ZZ_Count_16_22,key,1);
					AddDictVal(ZZ_Sum_16_22,key,zzSizeAbs);
					if(printOut > 1)
						PrintLog(true, !back_test, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=16" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				else if(zzSizeAbs >= 22 && zzSizeAbs <30){
					key = GetDictKeyByDateTime(dt_end, "zz22-30", "");
					AddDictVal(ZZ_Count_22_30,key,1);
					AddDictVal(ZZ_Sum_22_30,key,zzSizeAbs);
					if(printOut > 1)
						PrintLog(true, !back_test, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=22" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				else if(zzSizeAbs >= 30){
					key = GetDictKeyByDateTime(dt_end, "zz30-", "");
					AddDictVal(ZZ_Count_30_,key,1);
					AddDictVal(ZZ_Sum_30_,key,zzSizeAbs);
					if(printOut > 1)
						PrintLog(true, !back_test, idx.ToString() + "-ZZ= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "] >=30" + str_suffix + GetTimeDiff(Time[CurrentBar-barStart], Time[CurrentBar-barEnd]) + str_Minutes + ",r=" + zzSwings[barEnd].TwoBar_Ratio);
				}
				if(zzSize != 0) {
					//DrawZZSizeText(idx, "txt-");
					if(zzSizeAbs < 10)
						if(printOut > 2)
							PrintLog(true, !back_test, idx.ToString() + "-zzS= " + zzSize + " [" + Time[CurrentBar-barStart].ToString() + "-" + Time[CurrentBar-barEnd].ToString() + "]" );
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
				PrintLog(true, !back_test, CurrentBar + "-" + Instrument.FullName 
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
				
				PrintLog(true, !back_test, CurrentBar + "-" + Instrument.FullName 
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
		
    }
}
