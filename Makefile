.PHONY: brawlbox brawllib brawlcmd ikarus clean

brawlbox: packages
	xbuild /target:brawlbox

brawllib: 
	xbuild /target:brawllib

brawlcmd: 
	xbuild /target:brawlcmd

ikarus: 
	xbuild /target:ikarus

clean: 
	xbuild /target:clean
	rm -rf packages

packages: 
	nuget restore
	

