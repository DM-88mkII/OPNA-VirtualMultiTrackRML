


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace FM_VirtualMultiTrackMML
{
	public class OPNA
	{
		public static readonly byte s_Reg_KeyOn = 0x10;
		public static readonly byte s_Value_Dump = 0x80;
		public static readonly byte[] s_aValue_Track = new byte[]{
			0x01,	// BD
			0x02,	// SD
			0x04,	// TOP
			0x08,	// HH
			0x10,	// TOM
			0x20,	// RIM
		};
		
		public static readonly byte s_Reg_RTL = 0x11;
		
		public static readonly byte[] s_aReg_LR_IL = new byte[]{
			0x18,	// BD
			0x19,	// SD
			0x1a,	// TOP
			0x1b,	// HH
			0x1c,	// TOM
			0x1d,	// RIM
		};
	}
}
