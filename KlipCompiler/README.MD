The KlipCompiler, as the name suggest, is a compiler that takes high-level code and compiles it down to a lower-level code which can be run on the Klip Runtime (KlipRT). The compiler takes .txt documents as an input, compiles the code and then writes the low-level code to a file with the same name but changes the file typ to a .krt file. It can also import libraries writting in the low-level code and add that on to the compiled code for more functionality. The basic "system" library that is released with the KlipCompiler has support to print text to the screen, pause the program and input text from the keyboard.

Hello World Example:

import system

function Main()
{
    Print("Hello World")
    ReadLine()
}
