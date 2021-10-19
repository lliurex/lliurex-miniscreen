using System;
using System.Diagnostics;
using Gtk;
using Gdk;
using System.Collections.Generic;
using Stetic;

namespace LliureXMiniScreen{
	public partial class MainWindow : Gtk.Window
	{
		public int MiniScreenWidth, MiniScreenHeight; 	// Grandària de la minifinestra
		public int MiniScreenPosition;				// 0 left, 1right
		public int ScreenWidth, ScreenHeight;			// Grandària de la pantalla
		public bool BtnPressed;						    // Indica si està polsat el botó del ratolí
	//	int line_x0, line_y0, line_xf, line_yf;		// Dimensiona de la línia
		List <Gdk.Point> Llista_Punts;					// Defineix la línia per on passa
		List <String[]> Input_Devices_List;			// Llista de ratolins

		public Gdk.Pixbuf HiddenMiniScreen; 	// PATCH
		
		bool moving_mouse;								// Indica si s'està movent el ratolí
			
		Gdk.Window root;								// ROOT WINDOW
		//int width, height;
		Pixbuf screenshot;								// Captura de pantalla
		Pixbuf InitScreenshot;								// Captura de pantalla inicial
		Gdk.Color PathColor;
		
		uint RefreshRate;
		uint OldRefreshRate; // per gestionar els temps de refresc
		
		AccelGroup grup = new AccelGroup (); 			// Necessari per als items de menú amb imatges
		
		
		public void Init(){
			/* Creem el fitxer amb la llista dels diospositius d'entrada */
			ExecuteCommandSync("/bin/bash", "getInputMouse");
						
			/* Inicialitza variables i objectes */
			RefreshRate=500;
			OldRefreshRate=500;
			// Timer que es dispara cada X ms per refrescar la finestra
			GLib.Timeout.Add(RefreshRate, DrawPreview); // Amb més sembla que se satura si punxem dins la finestra...
			// Propietats de la pantalla
			MiniScreenWidth=400;
			MiniScreenHeight=320;
			MiniScreenPosition=0;
			MiniScreen.SetSizeRequest(MiniScreenWidth,MiniScreenHeight);
			
			BtnPressed=false;	// Botó premut
			
			// get the root window
			root = Gdk.Global.DefaultRootWindow;
			
			// Dimensions de la pantalla
			root.GetSize(out ScreenWidth, out ScreenHeight);
			//Console.WriteLine("Pantalla: "+ScreenWidth+" "+ScreenHeight);
			
			PathColor=new Color(255,0,0);
				
			Llista_Punts=new List<Point>();
			moving_mouse=false;
			
			// Llegim les propietats del ratoli
			System.IO.StreamReader sr = new System.IO.StreamReader("/tmp/miniscreen_deviceslist");
			
			Input_Devices_List=new List<string[]>();
			string texto;
			string [] split_texto;
			string dev;
			string property;
			
			while (!sr.EndOfStream){
				texto = sr.ReadLine();
				split_texto=texto.Split(new char[] {' '});
				//String[] device=new String();				                           
				//dev=split_texto[0];
				//property=split_texto[1];
				Input_Devices_List.Add(split_texto);
			}
			
			//foreach(String [] device in Input_Devices_List){
			//	Console.WriteLine("DEV: "+device[0]);
			//	Console.WriteLine("PROP: "+device[1]);
			//}
				
			sr.Close();
			// cap posar dev i property com a parametres al xinit, i xinit com a dependencia d'este paquet
			// i invocar el script que crea aço abans de llegir-lo
			// Per si hi ha mes dispositius, ha de ser una llista!!
			
			// PATCH
			// Fem una captura de la pantalla general, per agafar les imatges a substiutir per la minipantalla
			InitScreenshot = Gdk.Pixbuf.FromDrawable(root, root.Colormap, 0,0, 0,0, ScreenWidth, ScreenHeight);
			
			int width, height, px, py;
						
		}
		
	
		
		public MainWindow () : base(Gtk.WindowType.Toplevel)
		{
			Build ();
			
			Init(); 									// Inicialització
			
			MiniScreen.ShowAll();
			
			// Gestió d'events
			eventbox1.ButtonPressEvent += HandleMiniScreenButtonPressEvent;
			eventbox1.ButtonReleaseEvent += HandleEventbox1ButtonReleaseEvent;
			eventbox1.MotionNotifyEvent += HandleEventbox1MotionNotifyEvent;
			
			MiniScreen.Hidden += HandleMiniScreenHidden;
			eventbox1.MotionNotifyEvent += HandleEventbox1MotionNotifyEvent;
			
			DrawPreview();
			
			// // LliureXMiniScreen.MainClass.win.TypeHint=Gdk.WindowTypeHint.Dock;
			// Establim la finestra al cantó inferior
		/*	try{
				LliureXMiniScreen.MainClass.win.Move(0, ScreenHeight);
				LliureXMiniScreen.MainClass.win.TypeHint=Gdk.WindowTypeHint.Dock;
			} catch (Exception ex){
				Console.WriteLine("Init Exception: "+ex);
			}
			 */
			//Gdk.Global.ActiveWindow.Move(0, ScreenHeight);
			//Gdk.Global.ActiveWindow.TypeHint=Gdk.WindowTypeHint.Dock;
			//Gdk.Global.ActiveWindow.TypeHint
	
		}

		void takeSnapshot(){
			// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			LliureXMiniScreen.Core.getCore().win.Hide();
			InitScreenshot = Gdk.Pixbuf.FromDrawable(root, root.Colormap, 0,0, 0,0, ScreenWidth, ScreenHeight);
			LliureXMiniScreen.Core.getCore().win.Show();
	
			
			// El problema està en que no amaga els menús, però sí la minipantalla!!!

		}
		
		void HandleMiniScreenHidden (object sender, EventArgs e)
		{
			InitScreenshot = Gdk.Pixbuf.FromDrawable(root, root.Colormap, 0,0, 0,0, ScreenWidth, ScreenHeight);
			
	//		Console.WriteLine("New capture from (0,0) to ("+ScreenWidth+","+ScreenHeight+")");
		}
	
		public void setHint(){
			try{
				//LliureXMiniScreen.MainClass.win.Move(0, ScreenHeight);
				//LliureXMiniScreen.Core.getCore().win.Move(0, ScreenHeight);
				LliureXMiniScreen.Core.getCore().win.Move(0, ScreenHeight);
				//LliureXMiniScreen.MainClass.win.TypeHint=Gdk.WindowTypeHint.Dock;
			} catch (Exception ex){
				Console.WriteLine("Init Exception: "+ex);
			}
		}
		
		void HandleEventbox1MotionNotifyEvent (object o, MotionNotifyEventArgs args)
		{
			//Console.WriteLine("Moving");
			//Console.WriteLine(args.Event.X+","+args.Event.Y);
			//line_xf=(int)args.Event.X;
			//line_yf=(int) args.Event.Y;
			try{
			// Afegim el punt a la llista...
				if(BtnPressed){
					//Console.WriteLine("Adding: "+args.Event.X+","+args.Event.Y);
					Llista_Punts.Add(new Gdk.Point((int)args.Event.X, (int)args.Event.Y));
					DrawPreview();
				}
			} catch (Exception ex){
				Console.WriteLine("Exception in moving: "+ex.Message);
			}		
		}
	
		
		void HandleEventbox1ButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
	
			moving_mouse=true; // Indiquem que s'està movent el ratolí (per tant no redibuixarem)
			//Console.WriteLine("RELEASEEE!!!");
			
			int px, py; // Posició de la minipantalla
			int miniscreen_size_x,miniscreen_size_y; // dimensions de la minipantalla
			double pos_mouse_x, pos_mouse_y; // posició del ratolí en el clic
			double pos_mouse_x_global, pos_mouse_y_global; // posició del ratolí en el clic referent al global de pa pantalla
			
			/*line_x0=0; // Amaguem la línia
			line_y0=0;
			line_xf=0;
			line_yf=0;*/
			
			// Agafem les dimensions de la minipantalla
			MiniScreen.GetSizeRequest(out miniscreen_size_x, out miniscreen_size_y);
			
			// Agafem la posició del ratolí en el moment del release
			pos_mouse_x=args.Event.X;
			pos_mouse_y=args.Event.Y;
			
			// Afegim a la llista la última posició
			Llista_Punts.Add(new Gdk.Point((int)pos_mouse_x, (int)pos_mouse_y));
					
			// Agafem la posició de la finestra
			//Gdk.Window root=Gdk.Global.ActiveWindow;
			//root.GetPosition(out px, out py);
			//LliureXMiniScreen.MainClass.win.GetPosition(out px, out py);
 			 LliureXMiniScreen.Core.getCore().win.GetPosition(out px, out py);

			
			
			// Posició global del ratolí en el clic (recalculada)
			pos_mouse_x_global=pos_mouse_x*ScreenWidth/this.MiniScreenWidth;
			pos_mouse_y_global=pos_mouse_y*ScreenHeight/this.MiniScreenHeight;
			
			// Mostrem la informació
			//Console.WriteLine("RELEASE: Screen From: ("+px+","+py+") to ("+(px+miniscreen_size_x)+","+(py+miniscreen_size_y)+")");
			//Console.WriteLine("RELEASE: Pointer at: ("+pos_mouse_x+ ","+pos_mouse_y+")");
			//Console.WriteLine("RELEASE: Pointer at: ("+pos_mouse_x_global+ ","+pos_mouse_y_global+")");
			
			if(args.Event.Button==1){ // Si el botó era l'1...
				// Movem el ratolí en funció de la llista...
					
				//Console.WriteLine("N PUNTS: "+Llista_Punts.Count);
				if(Llista_Punts.Count>3){
					//Console.WriteLine("N PUNT 1: "+Llista_Punts[0].X+","+Llista_Punts[0].Y);
					//Console.WriteLine("N PUNT 2: "+Llista_Punts[1].X+","+Llista_Punts[1].Y);
					//Console.WriteLine("N PUNT 3: "+Llista_Punts[2].X+","+Llista_Punts[2].Y);
					
					
					Process p = new Process();
					p.StartInfo.FileName = "xdotool";
					
					// Alliberem el ratolí
				//	p.StartInfo.Arguments = " mouseup 1  ";
				//	p.Start();
				//	p.WaitForExit();
					
					
					
					// DESACTIVEM ELS RATOLINS // PATCH 2012
					foreach(String [] device in Input_Devices_List)
								ExecuteCommandSync("xinput","set-prop "+device[0]+" "+device[1]+" 0");
				

					
					
					
					//Console.WriteLine("Posicionant en: "+Llista_Punts[0].X*ScreenWidth/this.MiniScreenWidth+" "+Llista_Punts[0].Y*ScreenHeight/this.MiniScreenHeight);
					p.StartInfo.Arguments = " mousemove  "+Llista_Punts[0].X*ScreenWidth/this.MiniScreenWidth+" "+Llista_Punts[0].Y*ScreenHeight/this.MiniScreenHeight;
					p.Start();
					p.WaitForExit();
					
						
					
					// Click inicial:
					p.StartInfo.Arguments = " mousedown 1 ";
					p.Start();
					p.WaitForExit();
					
					foreach(Gdk.Point pt in Llista_Punts){
						//Console.WriteLine("Movint to: "+pt.X*ScreenWidth/this.MiniScreenWidth+" "+pt.Y*ScreenHeight/this.MiniScreenHeight);
						p.StartInfo.Arguments = " mousemove  "+pt.X*ScreenWidth/this.MiniScreenWidth+" "+pt.Y*ScreenHeight/this.MiniScreenHeight;
						p.Start();
						p.WaitForExit();
					}
					p.StartInfo.Arguments = " mouseup 1  ";
					p.Start();
					p.WaitForExit();
					
					
					// Reposicionament del ratolí en la minipantalla
					double finalx, finaly;
					finalx=px+pos_mouse_x;
					finaly=py+pos_mouse_y;
					
					//Console.WriteLine("px: "+px+" pos_mouse_x: "+pos_mouse_x);
					//Console.WriteLine("px: "+py+" pos_mouse_x: "+pos_mouse_y);
					
				
					p.StartInfo.Arguments = "mousemove "+finalx+" "+finaly;
	
	
					p.Start();
					p.WaitForExit();
					p.Close();
					
					Llista_Punts.Clear();
				
					
					// REACTIVEM EL RATOLI // PATCH 2012
						foreach(String [] device in Input_Devices_List)
								ExecuteCommandSync("xinput","set-prop "+device[0]+" "+device[1]+" 1");
					
					
					
					
				} else {
					// Si només hi ha un clic...
					if(Llista_Punts.Count>0){
						//Console.WriteLine("Click*****************************");
						
						Process p = new Process();
						p.StartInfo.FileName = "xdotool";
					
						// Alliberem el ratolí
						//p.StartInfo.Arguments = " mouseup 1  ";
						//p.Start();
						//p.WaitForExit();
					
						//Console.WriteLine("Movint INIT to: "+Llista_Punts[0].X*ScreenWidth/this.MiniScreenWidth+" "+Llista_Punts[0].Y*ScreenHeight/this.MiniScreenHeight);
						p.StartInfo.Arguments = " mousemove  "+Llista_Punts[0].X*ScreenWidth/this.MiniScreenWidth+" "+Llista_Punts[0].Y*ScreenHeight/this.MiniScreenHeight;
						p.Start();
						p.WaitForExit();
					
						// Click:
						p.StartInfo.Arguments = " click 1 ";
						p.Start();
						p.WaitForExit();
					
						// Reposicionament del ratolí en la minipantalla
						double finalx, finaly;
						finalx=px+pos_mouse_x;
						finaly=py+pos_mouse_y;
					
						p.StartInfo.Arguments = "mousemove "+finalx+" "+finaly;
	
						p.Start();
						p.WaitForExit();
						p.Close();
						Llista_Punts.Clear();
					
					}
					
				}
				
				// Restaurem el botó
				
			}
			BtnPressed=false; // Alliberem el botó es faça on es faça
			moving_mouse=false; // Hem acabat de mouse el ratoló, podem redibuixar			
			
		}
	
		
		void MiniScreenSettings()	{
			//Console.WriteLine("Right Click");
			
			Gtk.Menu menu_settings=new Gtk.Menu();
			Gtk.MenuItem menuitem1=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("MiniScreen Resolution"));
			menu_settings.Append(menuitem1);
			// Submenu 1
			Gtk.Menu menu_dim=new Gtk.Menu();
			Gtk.MenuItem menuitem1_0=new Gtk.MenuItem("300 x 240 (5:4)" );
			menu_dim.Append(menuitem1_0);
			menuitem1_0.Show();
			menuitem1.Submenu=menu_dim;		
			
			Gtk.MenuItem menuitem1_1=new Gtk.MenuItem("400 x 320 (5:4)");
			menu_dim.Append(menuitem1_1);
			menuitem1_1.Show();
			
			Gtk.MenuItem menuitem1_6=new Gtk.MenuItem("400 x 235 (16:9)");
			menu_dim.Append(menuitem1_6);
			menuitem1_6.Show();
			
			Gtk.MenuItem menuitem1_7=new Gtk.MenuItem("300 x 176 (16:9)");
			menu_dim.Append(menuitem1_7);
			menuitem1_7.Show();
			
			Gtk.MenuItem menuitem1_2=new Gtk.MenuItem("300 x 225 (4:3)" );
			menu_dim.Append(menuitem1_2);
			menuitem1_2.Show();
					
			Gtk.MenuItem menuitem1_3=new Gtk.MenuItem("200 x 150 (4:3)");
			menu_dim.Append(menuitem1_3);
			menuitem1_3.Show();
			
			Gtk.MenuItem menuitem1_4=new Gtk.MenuItem("300 x 166 (9:5)");
			menu_dim.Append(menuitem1_4);
			menuitem1_4.Show();
			
			Gtk.MenuItem menuitem1_5=new Gtk.MenuItem("200 x 111 (9:5)");
			menu_dim.Append(menuitem1_5);
			menuitem1_5.Show();		
			
			
			// Opcio refreshRate
			
			Gtk.MenuItem menuitemRefreshRate=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("Set Refresh Rate"));
			menu_settings.Append(menuitemRefreshRate);
			
			Gtk.Menu menu_ref_rate=new Gtk.Menu();
			Gtk.MenuItem menuitem_ref_500=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("500 ms (high refresh rate)" ));
			menu_ref_rate.Append(menuitem_ref_500);
			menuitem_ref_500.Show();
			menuitemRefreshRate.Submenu=menu_ref_rate;		
			
			Gtk.MenuItem menuitem_ref_750=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("750 ms"));
			menu_ref_rate.Append(menuitem_ref_750);
			menuitem_ref_750.Show();
			
			Gtk.MenuItem menuitem_ref_1000=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("1000 ms"));
			menu_ref_rate.Append(menuitem_ref_1000);
			menuitem_ref_1000.Show();
			
			Gtk.MenuItem menuitem_ref_1500=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("1500 ms"));
			menu_ref_rate.Append(menuitem_ref_1500);
			menuitem_ref_1500.Show();
			
			Gtk.MenuItem menuitem_ref_2000=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("2000 ms (low refresh rate)" ));
			menu_ref_rate.Append(menuitem_ref_2000);
			menuitem_ref_2000.Show();
			
			
			
			
			
			// Opció 2
			
			Gtk.MenuItem menuitem2=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("Set path color"));
			menu_settings.Append(menuitem2);
			
			
			// Opció 3 - Submenú posició
			
			
			Gtk.Menu menu_position=new Gtk.Menu();
			Gtk.MenuItem menuitem3=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("Window position"));
			menu_settings.Append(menuitem3);
			
			// Submenu 3
			Gtk.MenuItem menuitem3_0=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("Left"));
			menu_position.Append(menuitem3_0);
			menuitem3_0.Show();
			menuitem3.Submenu=menu_position;
	
			Gtk.MenuItem menuitem3_1=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("Right"));
			menu_position.Append(menuitem3_1);
			menuitem3_1.Show();
			
			// Opció 4 - Eixir
			Gtk.ImageMenuItem menuitem4=new Gtk.ImageMenuItem(Stock.Quit, grup);
			menuitem4.RenderIcon(Stock.Quit, IconSize.Menu, Mono.Unix.Catalog.GetString("Exit from MiniScreen"));
			menu_settings.Append(menuitem4);
			
			// Opció 5 - About
		
			
			//Gtk.MenuItem menuitem5=new Gtk.MenuItem(Mono.Unix.Catalog.GetString("About"));
			//Gtk.ImageMenuItem menuitem5=new Gtk.MenuItem((Mono.Unix.Catalog.GetString("About"));
			
			
			Gtk.ImageMenuItem menuitem5=new Gtk.ImageMenuItem(Stock.About, grup);
			menuitem5.RenderIcon(Stock.About, IconSize.Menu, Mono.Unix.Catalog.GetString("About LliureX MiniScreen"));
			menu_settings.Append(menuitem5);
			
			menuitem1.Show();
			menuitemRefreshRate.Show();
			menuitem2.Show();
			menuitem3.Show();
			menuitem4.Show();
			menuitem5.ShowAll();
			menuitem5.Show();
				
			menu_settings.Popup();
			
			menuitem1_0.ButtonPressEvent+= HandleMenuitem1ButtonPressEvent;
			menuitem1_1.ButtonPressEvent+= HandleMenuitem1ButtonPressEvent;
			menuitem1_2.ButtonPressEvent+= HandleMenuitem1ButtonPressEvent;
			menuitem1_3.ButtonPressEvent+= HandleMenuitem1ButtonPressEvent;
			menuitem1_4.ButtonPressEvent+= HandleMenuitem1ButtonPressEvent;
			menuitem1_5.ButtonPressEvent+= HandleMenuitem1ButtonPressEvent;
			menuitem1_6.ButtonPressEvent+= HandleMenuitem1ButtonPressEvent;
			menuitem1_7.ButtonPressEvent+= HandleMenuitem1ButtonPressEvent;
	

			menuitem_ref_500.ButtonPressEvent += HandleMenuitem_ref_500ButtonPressEvent;
			menuitem_ref_750.ButtonPressEvent += HandleMenuitem_ref_750ButtonPressEvent;
			menuitem_ref_1000.ButtonPressEvent += HandleMenuitem_ref_1000ButtonPressEvent;
			menuitem_ref_1500.ButtonPressEvent += HandleMenuitem_ref_1500ButtonPressEvent;
			menuitem_ref_2000.ButtonPressEvent += HandleMenuitem_ref_2000ButtonPressEvent;
			
			
			
			menuitem2.ButtonPressEvent+= HandleMenuitem2ButtonPressEvent;
			
			menuitem3_0.ButtonPressEvent+= HandleMenuitem3_0ButtonPressEvent;
			menuitem3_1.ButtonPressEvent+= HandleMenuitem3_1ButtonPressEvent;
			menuitem4.ButtonPressEvent += HandleMenuitem4ButtonPressEvent;
			menuitem5.ButtonPressEvent += HandleMenuitem5ButtonPressEvent;
		
		}

			void HandleMenuitem_ref_2000ButtonPressEvent (object o, ButtonPressEventArgs args)
			{		
				RefreshRate=2000;
				GLib.Timeout.Add(RefreshRate, DrawPreview); 			
			}

			void HandleMenuitem_ref_1500ButtonPressEvent (object o, ButtonPressEventArgs args)
			{ 		RefreshRate=1500; 	
					GLib.Timeout.Add(RefreshRate, DrawPreview); 
			}

			void HandleMenuitem_ref_1000ButtonPressEvent (object o, ButtonPressEventArgs args)
			{		RefreshRate=1000;	
					GLib.Timeout.Add(RefreshRate, DrawPreview);
			}

			void HandleMenuitem_ref_750ButtonPressEvent (object o, ButtonPressEventArgs args)
			{ 		RefreshRate=750; 	
					GLib.Timeout.Add(RefreshRate, DrawPreview); 
			}

			void HandleMenuitem_ref_500ButtonPressEvent (object o, ButtonPressEventArgs args)
			{		RefreshRate=500;	
					GLib.Timeout.Add(RefreshRate, DrawPreview); 
			}
	
			
		void HandleMenuitem5ButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			About dlgAbout=new About();
			dlgAbout.Run();
			dlgAbout.Destroy();
		}
	
			void HandleMenuitem4ButtonPressEvent (object o, ButtonPressEventArgs args)
			{
				Application.Quit();
			}
	
			
		
		
		void MoveMiniScreen(int position){
				// Amaguem la finestra per tornar a fer la captura inicial
				

				// Establim la finestra al cantó inferior dret
				//LliureXMiniScreen.MainClass.win.Move(ScreenWidth-MiniScreenWidth, ScreenHeight-MiniScreenHeight);
				LliureXMiniScreen.Core.getCore().win.Move(position*(ScreenWidth-MiniScreenWidth), ScreenHeight-MiniScreenHeight);
				//Console.WriteLine("Movent a: "+position*(ScreenWidth-MiniScreenWidth)+","+(ScreenHeight-MiniScreenHeight));
				
				//LliureXMiniScreen.MainClass.win.Activate();
				LliureXMiniScreen.Core.getCore().win.Activate();
						
		}
		
		void HandleMenuitem3_1ButtonPressEvent (object o, ButtonPressEventArgs args)
			{
					//LliureXMiniScreen.Core.getCore().win.Hide();
					takeSnapshot();
					//Console.WriteLine("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
					MiniScreenPosition=1;
					MoveMiniScreen(MiniScreenPosition);
					LliureXMiniScreen.Core.getCore().win.Show();
			
			}
	
			void HandleMenuitem3_0ButtonPressEvent (object o, ButtonPressEventArgs args)
			{
				//Console.WriteLine("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
				MiniScreenPosition=0;
				MoveMiniScreen(MiniScreenPosition);
			}
	
		void HandleMenuitem2ButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			//Gdk.Color color=new Gdk.Color();
			Gtk.ColorSelection selection;
			Console.Write("Menu Item 2");
			Gtk.ColorSelectionDialog dlg=new Gtk.ColorSelectionDialog("Select path color");
			
			Gtk.ResponseType result=(Gtk.ResponseType)dlg.Run();
			
			if(result==Gtk.ResponseType.Ok)	{

				selection=dlg.ColorSelection;
				PathColor=selection.CurrentColor;
			} //else Console.WriteLine("Cancelled");
			
			dlg.Destroy();
			
		}
	
		void HandleMenuitem1ButtonPressEvent (object obj, ButtonPressEventArgs args) {
			//Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
			try{
				
				// Amaguem la finestra per tornar a fer la captura inicial
				//MiniScreen.Hide();
				takeSnapshot();
				
				if(((Gtk.AccelLabel)((Gtk.MenuItem)obj).Children[0]).Text=="300 x 240 (5:4)"){
					//Console.WriteLine("300 x 240 (5:4)");
					MiniScreenWidth=300;
					MiniScreenHeight=240;
					
				//	MiniScreen.SetSizeRequest(MiniScreenWidth,MiniScreenHeight);
				} else if(((Gtk.AccelLabel)((Gtk.MenuItem)obj).Children[0]).Text=="400 x 320 (5:4)"){
					//Console.WriteLine("400 x 320 (5:4)");
					MiniScreenWidth=400;
					MiniScreenHeight=320;
					
				//	MiniScreen.SetSizeRequest(MiniScreenWidth,MiniScreenHeight);
				} else if(((Gtk.AccelLabel)((Gtk.MenuItem)obj).Children[0]).Text=="400 x 235 (16:9)"){
					//Console.WriteLine("400 x 235 (16:9)");
					MiniScreenWidth=400;
					MiniScreenHeight=235;
					
				//	MiniScreen.SetSizeRequest(MiniScreenWidth,MiniScreenHeight);
				} else if(((Gtk.AccelLabel)((Gtk.MenuItem)obj).Children[0]).Text=="300 x 176 (16:9)"){
					//Console.WriteLine("300 x 176 (16:9)");
					MiniScreenWidth=300;
					MiniScreenHeight=176;
					
				//	MiniScreen.SetSizeRequest(MiniScreenWidth,MiniScreenHeight);
				} else if(((Gtk.AccelLabel)((Gtk.MenuItem)obj).Children[0]).Text=="300 x 225 (4:3)"){
					//Console.WriteLine("300 x 225 (4:3)" );
					MiniScreenWidth=300;
					MiniScreenHeight=225;
					
				//	MiniScreen.SetSizeRequest(MiniScreenWidth,MiniScreenHeight);
				} else if(((Gtk.AccelLabel)((Gtk.MenuItem)obj).Children[0]).Text=="200 x 150 (4:3)"){
					//Console.WriteLine("200 x 150 (4:3)");
					MiniScreenWidth=200;
					MiniScreenHeight=150;
					
				//	MiniScreen.SetSizeRequest(MiniScreenWidth,MiniScreenHeight);
				} else if(((Gtk.AccelLabel)((Gtk.MenuItem)obj).Children[0]).Text=="300 x 166 (9:5)"){
					//Console.WriteLine("300 x 166 (9:5)");
					MiniScreenWidth=300;
					MiniScreenHeight=166;
					
				//	MiniScreen.SetSizeRequest(MiniScreenWidth,MiniScreenHeight);
				} else if(((Gtk.AccelLabel)((Gtk.MenuItem)obj).Children[0]).Text=="200 x 111 (9:5)"){
					//Console.WriteLine("200 x 111 (9:5)");	
					MiniScreenWidth=200;
					MiniScreenHeight=111;
					
				//	MiniScreen.SetSizeRequest(MiniScreenWidth,MiniScreenHeight);
				}
				
				MiniScreen.Show();
				MiniScreen.SetSizeRequest(MiniScreenWidth,MiniScreenHeight);
				 //LliureXMiniScreen.Core.getCore().win.Move(MiniScreenPosition*(ScreenWidth-MiniScreenWidth), ScreenHeight);
			//Console.WriteLine("<><><><><><>"+ScreenHeight);
			//	Console.WriteLine("<><><><>-"+MiniScreenHeight);
			//	Console.WriteLine("________-_"+(ScreenHeight-MiniScreenHeight));
				//LliureXMiniScreen.Core.getCore().win.Move(0, 784);
				
			
				//Console.WriteLine("Nou tam: "+MiniScreenWidth+","+MiniScreenHeight);
				MoveMiniScreen(MiniScreenPosition);
				

			} catch(Exception ex){
				Console.WriteLine("Exception: "+ex.Message);		
			}
			
		}
	
	
		void HandleMiniScreenButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			
			if(args.Event.Button==3) MiniScreenSettings();
			else{
				//Console.WriteLine("Press");
				Llista_Punts.Clear();
				int px, py; 							 		// Posició de la minipantalla
				int miniscreen_size_x,miniscreen_size_y; 		// dimensions de la minipantalla
				double pos_mouse_x, pos_mouse_y; 				// posició del ratolí en el clic
				double pos_mouse_x_global, pos_mouse_y_global; 	// posició del ratolí en el clic referent al global de pa pantalla
				
				// Agafem les dimensions de la minipantalla
				MiniScreen.GetSizeRequest(out miniscreen_size_x, out miniscreen_size_y);
				
				// Agafem la posició del ratolí en el moment del clic
				pos_mouse_x=args.Event.X;
				pos_mouse_y=args.Event.Y;
				
				// Agafem la posició de la finestra
				//Gdk.Window root=Gdk.Global.ActiveWindow;
				//root.GetPosition(out px, out py);
				//LliureXMiniScreen.MainClass.win.GetPosition(out px, out py);
				LliureXMiniScreen.Core.getCore().win.GetPosition(out px, out py);
				
				// Posició global del ratolí en el clic (recalculada)
				pos_mouse_x_global=pos_mouse_x*ScreenWidth/this.MiniScreenWidth;
				pos_mouse_y_global=pos_mouse_y*ScreenHeight/this.MiniScreenHeight;
				
				// Mostrem la informació
				//Console.WriteLine("Screen From: ("+px+","+py+") to ("+(px+miniscreen_size_x)+","+(py+miniscreen_size_y)+")");
				//Console.WriteLine("Pointer at: ("+pos_mouse_x+ ","+pos_mouse_y+")");
				//Console.WriteLine("Pointer at: ("+pos_mouse_x_global+ ","+pos_mouse_y_global+")");
				
				// Controlem si el clic és dins de la seua finestra de miniscreen
				
				if(pos_mouse_x_global>px && pos_mouse_x_global<px+miniscreen_size_x &&
			   	pos_mouse_y_global>py && pos_mouse_y_global<py+miniscreen_size_y)
				{
				//	Console.WriteLine("ESTA DINS!!!");
					Init();
			
				} else {
					if(args.Event.Button==1){ // Si el botó era l'1...
						BtnPressed=true; // Indiquem que el botó està polsat
						Llista_Punts.Add(new Gdk.Point((int)pos_mouse_x, (int)pos_mouse_y));				
					}
				}
			}
		
		}
			
		
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}
		
		private bool DrawPreview(){
			
			//Console.WriteLine(OldRefreshRate+"--------"+RefreshRate);
			
			if(OldRefreshRate==RefreshRate){
				if(!moving_mouse){
					
					//Console.WriteLine("Drawing with ref rate..."+RefreshRate);
					
					int width = 0;
					int height = 0;
					
					if(!BtnPressed){
						try{
						root = Gdk.Global.DefaultRootWindow;
							
						// get its width and height
						root.GetSize(out width, out height);
						// create a new pixbuf from the root window
						//try{
						//LliureXMiniScreen.Core.getCore().win.Hide();
		  				screenshot = Gdk.Pixbuf.FromDrawable(root, root.Colormap, 0,0, 0,0, width, height);
						
						int miniscreen_size_x,miniscreen_size_y; 		// dimensions de la minipantalla
						int px, py, win_px, win_py;
						if (LliureXMiniScreen.Core.getCore().win!=null){
							LliureXMiniScreen.Core.getCore().win.GetPosition(out win_px, out win_py);
							MiniScreen.GetSizeRequest(out miniscreen_size_x, out miniscreen_size_y);
							px=MiniScreenPosition*(width-miniscreen_size_x);
							py=height-miniscreen_size_y;
															
							//Console.WriteLine("Dibuixe en: ("+px+","+py+") una finestra de "+miniscreen_size_x+"x"+miniscreen_size_y);
							if (px!=win_px || py!=win_py) LliureXMiniScreen.Core.getCore().win.Move(px, py);
								
							// Amaguem la minipantallla
							
							// PATCH
							//Gdk.Pixbuf HiddenMiniScreen=new Pixbuf("/usr/share/icons/LliureX-Accessibility/llx-miniscreen-hide.png", miniscreen_size_x, miniscreen_size_y);	
							HiddenMiniScreen=new Pixbuf(InitScreenshot, px,py, MiniScreenWidth, MiniScreenHeight);
								
							// END PATCH
								
							//Console.WriteLine("->"+HiddenMiniScreen.Width+" " + HiddenMiniScreen.Height);
				 			HiddenMiniScreen=HiddenMiniScreen.ScaleSimple(miniscreen_size_x, miniscreen_size_y, InterpType.Bilinear);
							HiddenMiniScreen.CopyArea(0,0, miniscreen_size_x, miniscreen_size_y,
							                          	screenshot, px, py);
														
							}
							
							//LliureXMiniScreen.Core.getCore().win.Activate();
						//	LliureXMiniScreen.Core.getCore().win.Show();
						
						/*} catch (Exception exc){
							Console.WriteLine("Excepció...: "+exc);
						}*/
						
						//Gdk.Pixbuf screenshot = Gdk.Pixbuf.FromDrawable(root, root.Colormap, 0,0, 0,0, width, height);
							// 
							// Cal inserir una imatge al screenshot per poder ocultar la minifinestra a la captura!!!
							//
							screenshot=screenshot.ScaleSimple(this.MiniScreenWidth,this.MiniScreenHeight,InterpType.Bilinear);
						}catch (Exception exc){
							Console.WriteLine("Excepció...: "+exc);
						}
					} 
					
					// Creem la imatge per emmagatzemar el pixbuf
					
					Gtk.Image MyImage=new Gtk.Image();
					MyImage.Pixbuf=screenshot;
					
					// Agafem les dimensions de la finestra
					Gdk.GC gc=((Gtk.DrawingArea)MiniScreen).Style.TextGC(Gtk.StateType.Normal);
					
					MiniScreen.GdkWindow.DrawPixbuf(gc, MyImage.Pixbuf, 0, 0, 0, 0, this.MiniScreenWidth, this.MiniScreenHeight, Gdk.RgbDither.Max , 0, 0);
					
					Cairo.Context context = Gdk.CairoHelper.Create(MiniScreen.GdkWindow);
			
					if(BtnPressed){
						double r=double.Parse((PathColor.Red).ToString())/65535;
						double g=double.Parse((PathColor.Green).ToString())/65535;
						double b=double.Parse((PathColor.Blue).ToString())/65535;
						
						context.Color=new Cairo.Color(r,g,b);
							
						try{
							if(Llista_Punts.Count>1){
								context.LineWidth=3;
								context.MoveTo (Llista_Punts[0].X, Llista_Punts[0].Y);
								foreach(Gdk.Point p in Llista_Punts){
									context.LineTo (p.X, p.Y);
									context.MoveTo (p.X, p.Y);
								}
							}
							context.Stroke ();	
							context.FillPreserve();
						}
						
						catch(Exception ex){
							Console.WriteLine("Exception: "+ex.Message);
						}
					}
				
					((IDisposable)context.Target).Dispose();
					((IDisposable)context).Dispose();
					
					
					return true;
				}
			}else{
				//if(RefreshRate!=OldRefreshRate){
					//Console.WriteLine("Change refresg rate!!!");
					OldRefreshRate=RefreshRate;
					return false;
				//}
			}
			return false;
		
			
		}
		
		
		/*[GLib.ConnectBefore]
		protected virtual void OnSizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			Console.WriteLine("ALLOCATION: "+args.Allocation.Width+" "+args.Allocation.Height);
			MiniScreenWidth=args.Allocation.Width;
			MiniScreenHeight=args.Allocation.Height;
		}*/
		
	
		public void ExecuteCommandSync(String firstCommand, object command)
		{
     	try
     	{
         	// create the ProcessStartInfo using "cmd" as the program to be run,
         	// and "/c " as the parameters.
         	// Incidentally, /c tells cmd that we want it to execute the command that follows,
         	// and then exit.
    			System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo(firstCommand, command.ToString());
	
    		// The following commands are needed to redirect the standard output.
    		// This means that it will be redirected to the Process.StandardOutput StreamReader.
    		procStartInfo.RedirectStandardOutput = true;
    		procStartInfo.UseShellExecute = false;
    		// Do not create the black window.
    		procStartInfo.CreateNoWindow = true;
    		// Now we create a process, assign its ProcessStartInfo and start it
    		System.Diagnostics.Process proc = new System.Diagnostics.Process();
    		proc.StartInfo = procStartInfo;
    		proc.Start();
    		// Get the output into a string
    		string result = proc.StandardOutput.ReadToEnd();
    		// Display the command output.
    		//Console.WriteLine(result);
      		}
      		catch (Exception objException)
      		{
				Console.WriteLine("*******************************************************"+objException.ToString());
      		// Log the exception
      		}
		}	
		
		
		
		
	}
}
