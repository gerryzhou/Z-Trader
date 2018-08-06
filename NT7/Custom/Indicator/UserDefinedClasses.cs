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

    partial class Indicator
    {
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
			
		}
	
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
