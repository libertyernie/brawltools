from BrawlBox.API import bboxapi
from BrawlLib.SSBB.ResourceNodes import *

import os

dir = bboxapi.OpenFolderDialog()
if dir is not None:
	from_ext = ".pac"
	to_ext = ".pcs"

	if not from_ext.startswith("."):
		from_ext = "." + from_ext;
		
	if not to_ext.startswith("."):
		to_ext = "." + to_ext;

	for root, dirs, files in os.walk(dir):
		for file in files:
			if file.endswith(from_ext):
				no_ext, ext = os.path.splitext(file)
				node = NodeFactory.FromFile(None, os.path.join(root, file))
				try:
					node.Export(os.path.join(root, no_ext + to_ext))
					node.Dispose()
				except:
					bboxapi.ShowMessage("Cannot export " + file + " as format " + to_ext, "Error")
		#Uncomment break below to skip subfolders
		#break
