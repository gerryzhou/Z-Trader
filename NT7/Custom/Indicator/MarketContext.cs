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
    /// This file holds market context class.
    /// </summary>
	
	/// <summary>
	/// Supervised Pattern Recognization
	/// 20100610;8301045:UpTight#10-16-3-5;11011245:RngWide#4-6;13011459:DnWide;
	/// </summary>
	public class SpvPR {
		public int Date;
		//Market condition for TimeRange;
		//HHMMHHMM=TimeStart*10000+TimeEnd;
		public Dictionary<int,PriceAction> Mkt_Ctx; 
		
		public SpvPR (int date, Dictionary<int,PriceAction> mktCtx) {
			this.Date = date;
			this.Mkt_Ctx = mktCtx;
		}
	}
	
    public class MarketContext
    {
		#region SpvPR Vars
		/// <summary>
		/// Loaded from supervised file;
		/// Key1=Date; Key2=Time;
		/// </summary>		
		protected Dictionary<string,Dictionary<int,PriceAction>> Dict_SpvPR = null;
		
		/// <summary>
		/// Bitwise op to tell which Price Action allowed to be the supervised entry approach
		/// 0111 1111: [0 UnKnown RngWide RngTight DnWide DnTight UpWide UpTight]
		/// UnKnown:spvPRBits&0100 000(64)
		/// RngWide:spvPRBits&0010 0000(32), RngTight:spvPRBits&0001 0000(16)
		/// DnWide:spvPRBits&0000 1000(8), DnTight:spvPRBits&0000 0100(4)
		/// UpWide:spvPRBits&0000 0010(2), UpTight:spvPRBits&0000 0001(1)
		/// </summary>

		protected int SpvPRBits = 0;
		
		#endregion
		
		#region Supervised pattern recognition
		
		public FileInfo[] GetSpvFile(string srcDir, string symbol) {
			//Print("GetSupervisedFile src: " + srcDir);
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
                //Print("cmdFile=" + item.FullName);
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
			//Print("ReadSpvPRFile src: " + src);
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
			foreach(string dayPR in SpvDailyPattern.spvPRDay) {
				//Print(dayPR);
				ReadSpvPRLine(dayPR);
				counter++;
			}
			//Print("ReadSpvPRList:" + counter);
			return Dict_SpvPR;
		}
		
		#endregion
    }
}
