
SOURCES=miniscreen/*.cs						\
		miniscreen/gtk-gui/*.cs				\


	
CSC = mcs
CSC_FLAGS = -r:Mono.Posix.dll -pkg:gtk-sharp-2.0 -r:/usr/lib/cli/notify-sharp-0.4/notify-sharp.dll -r:Mono.Cairo -r:System.Data  -optimize+
# Fix build by M.Angel
#-r:libmono-lliurex-utils -optimize+
OUT = miniscreen.exe
PKG_CONFIG_PATH = "/usr/lib/pkgconfig"
RESOURCE = rsrc/LliureX-MiniScreen.png,MiniScreen.Lliurex-MiniScreen.png 

clean: 
	rm -rf miniscreen/bin/
	rm -f miniscreen/miniscreen.pidb

release: $(SOURCES)
	mkdir -p miniscreen/bin/Release/
	$(CSC) $(CSC_FLAGS) $(SOURCES) -resource:$(RESOURCE) -out:miniscreen/bin/Release/$(OUT) 

	
debug: $(SOURCES)
	mkdir -p miniscreen/bin/Debug/
	$(CSC) $(CSC_FLAGS) $(SOURCES) -resource:$(RESOURCE) -out:miniscreen/bin/Debug/$(OUT)	 -debug
