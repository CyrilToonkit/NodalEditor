import sys
import clr

_INSTANCE = None

def init(func):
    global _INSTANCE
    if _INSTANCE is None:

        #clr.AddReferenceToFile("TK_NodalEditor.dll")
        clr.AddReferenceToFileAndPath(r"Z:\Toonkit\RnD\Oscar\src\NodalEditor\NodalTester\bin\Debug\TK_NodalEditor.dll")
        from TK.NodalEditor import Sharpy

        _INSTANCE = Sharpy()

    return func

@init
def addNode(a, b, c, d):
    return _INSTANCE.addNode(a, b, c, d)

@init
def deleteNode(a):
    return _INSTANCE.deleteNode(a)
