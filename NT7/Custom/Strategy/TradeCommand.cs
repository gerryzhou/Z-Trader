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

    partial class Strategy {
		//private int algo_mode = 1;
		//private string AccName = null;
		
		//private int tradeDirection = 0; // -1=short; 0-both; 1=long;
		//private int tradeStyle = 0; // -1=counter trend; 1=trend following;
		//private bool backTest = true; //if it runs for backtesting;
		
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

/*
		
		[Description("Supervised PR Bits")]
        [GridCategory("Parameters")]
        public int SpvPRBits
        {
            get { return spvPRBits; }
            set { spvPRBits = Math.Max(0, value); }
        }		

		

		[Description("Bars Since PbSAR reversal. Enter the amount of the bars ago maximum for PbSAR entry allowed")]
        [GridCategory("Parameters")]
        public int BarsAgoMaxPbSAREn
        {
            get { return barsAgoMaxPbSAREn; }
            set { barsAgoMaxPbSAREn = Math.Max(1, value); }
        }

		[Description("Bars count for last PbSAR swing. Enter the maximum bars count of last PbSAR allowed for entry")]
        [GridCategory("Parameters")]
        public int BarsMaxLastCross
        {
            get { return barsMaxLastCross; }
            set { barsMaxLastCross = Math.Max(1, value); }
        }		*/

	
        #endregion		
	}
}