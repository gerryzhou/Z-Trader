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
    /// This file holds all user defined strategy classes.
    /// </summary>
 
	//public enum SessionBreak {AfternoonClose, EveningOpen, MorningOpen, NextDay};
	
	public class CmdObject {
		private Strategy instStrategy = null;
		
        #region Properties
		public Strategy GetStrategy() {
			return this.instStrategy;
		}
        #endregion
	}

    partial class Strategy {
		protected CmdObject cmdObj = null;

		public virtual void InitTradeCmd() {
			new CmdObject();
		}
		
		public virtual CmdObject CheckCmd() {
			return cmdObj;
		}
		
		public virtual void ExecuteCommand() {
			switch(TG_AlgoMode) {
				case 0: // 0=liquidate; 
					CloseAllPositions();
					break;
				case 1:	// 1=trading; 
					//CheckPositions();
					CheckTrigger();
					ChangeSLPT();
					CheckEnOrder(-1);
					
					if(NewOrderAllowed() && PatternMatched())
					{
						//indicatorProxy.PrintLog(true, !backTest, "----------------PutTrade, isReversalBar=" + isReversalBar + ",giParabSAR.IsSpvAllowed4PAT(curBarPat)=" + giParabSAR.IsSpvAllowed4PAT(curBarPriceAction.paType));
						//PutTrade(zz_gap, cur_gap, isReversalBar);
					}
					break;
				case 2:	// 2=semi-algo(manual entry, algo exit);
					ChangeSLPT();
					break;
				case -1: // -1=stop trading(no entry/exit, cancel entry orders and keep the exit order as it is if there has position);
					CancelEntryOrders();
					break;
				case -2: // -2=stop trading(no entry/exit, liquidate positions and cancel all entry/exit orders);
					CancelAllOrders();
					break;
				default:
					break;
			}
		}
		
		public FileInfo[] GetCmdFile(string srcDir) {
			Print("GetCmdFile src: " + srcDir);
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
		
		public void MoveCmdFiles(FileInfo[] src, string dest) {
			Print("MoveCmdFile src,dest: " + src.Length + "," + dest);
			foreach (FileInfo item in src)
            {
				string destFile = dest+item.Name;
				if (File.Exists(destFile))
				{
					File.Delete(destFile);
				}
				item.MoveTo(destFile);
				//File.Move(src, dest);
			}
		}
		
		public Dictionary<string,string> ReadParaFile(FileInfo src) {
			Dictionary<string,string> paraMap = new Dictionary<string,string>();
			
			Print("ReadParaFile src: " + src);
			if (!src.Exists)
			{
				return paraMap;
			}
	
			int counter = 0;  
			string line;

			// Read the file and display it line by line.  
			System.IO.StreamReader file =   
				new System.IO.StreamReader(src.FullName);//@"c:\test.txt");
			while((line = file.ReadLine()) != null)  
			{
				string[] pa = line.Split(':');
				paraMap.Add(pa[0], pa[1]);
				Print(line);  
				counter++;
			}

			file.Close();
			Print("There were {0} lines." + counter);
			// Suspend the screen.
			//System.Console.ReadLine();
			return paraMap;
		}
		
        #region Properties
	
        #endregion		
	}
}