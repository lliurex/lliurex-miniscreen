using System;
using Gtk;
using LliureXMiniScreen;
using Notifications;

namespace LliureXMiniScreen
{
	public class MainClass
	{

		public int MiniScreenWidth {get;set;}
		public int MiniScreenHeight {get;set;} 	// Grandària de la minifinestra

		public MainWindow win {get;set;}
		
		public static void Main (string[] args)
		{
			/*Application.Init ();
			//MainWindow win = new MainWindow ();
			win = new MainWindow ();

			
			win.setHint();
		
			
			
			// Gdk.WindowType.Utility
			win.Show ();
			Application.Run ();
			*/
			
			Mono.Unix.Catalog.Init("lliurex-miniscreen","/usr/share/locale");
			
			Application.Init ();
			LliureXMiniScreen.Core.getCore();
			LliureXMiniScreen.Core.getCore().win.setHint();
			
			string sumary="LliureX MiniScreen";
			string body=Mono.Unix.Catalog.GetString("To Quit, press right button on MiniScreen and select Quit.");
			/*string icon="/usr/share/icons/LliureX-Accessibility/Lliurex-MiniScreen.png";*/
			string icon="lliurex-miniscreen";
			Notifications.Notification note=new Notifications.Notification(sumary, body, icon);
			
			note.Show();
			Application.Run ();
		}
	}
}

/*         HACK PER AL LliureX-MiniScreen.About.cs, per modificar el que elimina automàticament en crear de nou la interfície
 * 
			Cal canviar la línia:
			this.image1.Pixbuf = global::Gdk.Pixbuf.LoadFromResource ("MiniScreen.Lliurex-MiniScreen.png");
			
			Per...

			// HACK: 
			Gtk.Image img=new Gtk.Image("/usr/share/icons/LliureX-Accessibility/Lliurex-MiniScreen.png");
			this.image1.Pixbuf = img.Pixbuf;
			
			// END HACK
			
 * 
 * 
 * */

