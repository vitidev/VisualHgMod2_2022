using System;

namespace VisualHg
{
	/// <summary>
	/// This class is used to expose the list of the IDs of the commands implemented
	/// by the client package. This list of IDs must match the set of IDs defined inside the
	/// VSCT file.
	/// </summary>
	public static class CommandId
	{
		// Define the list a set of public static members.

		// Define the list of menus (these include toolbars)
        public const int imnuToolWindowToolbarMenu      = 0x204;

        public const int icmdHgCommitRoot               = 0x100;
        public const int icmdHgStatus                   = 0x101;
        public const int icmdHgHistoryRoot              = 0x102;
        public const int icmdViewToolWindow             = 0x103;
        public const int icmdToolWindowToolbarCommand   = 0x104;
        public const int icmdHgSynchronize              = 0x105;
        public const int icmdHgUpdateToRevision         = 0x106;
        public const int icmdHgDiff                     = 0x107;
        public const int icmdHgRevert                   = 0x108;
        public const int icmdHgAnnotate                 = 0x109;
        public const int icmdHgCommitSelected           = 0x110;
        public const int icmdHgHistorySelected          = 0x111;
        public const int icmdHgAddSelected              = 0x112;
        
        // Define the list of icons (use decimal numbers here, to match the resource IDs)
        public const int iiconProductIcon               = 400;
	}
}
