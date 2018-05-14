
using System;

namespace LliureXMiniScreen
{

	public class Core
	{
		public static Core singleton;
		public LliureXMiniScreen.MainWindow win;
			
		private Core ()
		{
			singleton=this;
			Console.WriteLine("[Core] INIT");
			
			win=new LliureXMiniScreen.MainWindow();
			win.Show();
			
		}
		
		public static Core getCore()
		{
			if(singleton==null)
				return new Core();
			else return singleton;
		}
		
		
	}

}

