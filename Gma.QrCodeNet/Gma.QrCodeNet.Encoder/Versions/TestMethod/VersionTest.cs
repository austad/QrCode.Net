﻿using Gma.QrCodeNet.Encoding.DataEncodation;
using System.Collections.Generic;
using System;

namespace Gma.QrCodeNet.Encoding.Versions
{
	public class VersionTest
	{
		
		private const string DEFAULT_ENCODING = "iso-8859-1";
		
		private static string[] s_EncodingNames = new string[]{"iso-8859-1", "iso-8859-2"};
		
		private static VersionTable versionTable = new VersionTable();
		
		public static VersionCheckStatus VersionCheck(int versionNum, int numDataBits, Mode mode, ErrorCorrectionLevel level, string encodingName)
		{
			int TotalDataBits = numDataBits;
        	if(mode == Mode.EightBitByte)
        	{
        		if(encodingName != DEFAULT_ENCODING)
        		{
        			int eciValue = ECISet.GetECIValueByName(encodingName);
        			TotalDataBits += ECISet.NumOfECIHeaderBits(eciValue);
        		}
        	}
        	int bitCharCountIndicator = CharCountIndicatorTable.GetBitCountInCharCountIndicator(mode, versionNum);
        	TotalDataBits += (4 + bitCharCountIndicator);
        	
        	int expectContainer = DataBits(versionNum, level);
        	int lowerContainer = versionNum == 1 ? 0 : DataBits(versionNum - 1, level);
        	
        	if(expectContainer < TotalDataBits)
        	{
        		return VersionCheckStatus.SmallerThanExpect;	
        	}
        	else if(lowerContainer >= TotalDataBits)
        	{
        		return VersionCheckStatus.LargerThanExpect;	
        	}
        	else
        	{
        		return VersionCheckStatus.Efficient;
        	}
		}
		
		private static int DataBits(int version, ErrorCorrectionLevel level)
        {
        	int totalCodewords = versionTable.GetVersionByNum(version).TotalCodewords;
        	int totalECCodewords = versionTable.GetVersionByNum(version).GetECBlocksByLevel(level).NumErrorCorrectionCodewards;
        	
        	return (totalCodewords - totalECCodewords) * 8;
        }
		
		
		public static IEnumerable<VersionTestProperties> GenerateTestCase
		{
			get
			{
				for(int versionNum = 1; versionNum < 33; versionNum++)
				{
					Version version = versionTable.GetVersionByNum(versionNum);
					
					int totalCodewordsBits = version.TotalCodewords * 8;
					
					foreach(int levelValue in Enum.GetValues(typeof(ErrorCorrectionLevel)))
					{
						int totalECCodewordsBits = version.GetECBlocksByLevel((ErrorCorrectionLevel)levelValue).NumErrorCorrectionCodewards * 8;
						
						int dataCodewordsBits = totalCodewordsBits - totalECCodewordsBits;
						
						foreach(int modeValue in Enum.GetValues(typeof(Mode)))
						{
							int normalHeaderBits = 4 + CharCountIndicatorTable.GetBitCountInCharCountIndicator((Mode)modeValue, versionNum);
							
							foreach(string encodingName in s_EncodingNames)
							{
								int eciHeaderBits = 0;
								if((Mode)modeValue == Mode.EightBitByte && encodingName != DEFAULT_ENCODING)
								{
									int eciValue = ECISet.GetECIValueByName(encodingName);
        							eciHeaderBits += ECISet.NumOfECIHeaderBits(eciValue);
								}
								int numBitsDataContents = dataCodewordsBits - eciHeaderBits - normalHeaderBits;
								
								yield return new VersionTestProperties(numBitsDataContents, (Mode)modeValue, (ErrorCorrectionLevel)levelValue, encodingName, versionNum);
							}
							
						}
					}
				}
			}
		}
		
		
	}
}