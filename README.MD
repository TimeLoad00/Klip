Klip is a simple, compiled programming language that I wrote to help others understand how to write their own programming languages. I have been writing my own programming and scripting languages for a number of years and I realise that there's a lack of example code and tutorials on how to do so and thus I wrote the Klip language as a simple example on how one might start writing their own programming language.

Due to the reason why I created this programming language it is not full-featured but it has enough features to be used as a base for anyone who is looking to create their own programming language. The features are:
    - Functional language
    - Function
    - Variables
    - Printing to the screen
    - Input from the keyboard
    - If, elseif and else statements
    - Repeat loops
While and for loops have not been included because I don't want to spoon-feed whoever is wanting to learn how to create their own programming language. If they want to add while and for loops then they'll have to figure it out for themselves (it really isn't that hard).

Klip comes in two parts: Klip Runtime (KlipRT) and Klip Compiler. The KlipRT is a simple program that is able to run low-level code. I wouldn't go as far to say that it's a virtual machine, it doesn't work at a low level like virtual machines, it just execute the low-level code that is generated from the Klip Compiler.

The Klip Compiler takes high-level code as its input and compiles it down to low-level code that the KlipRT can execute.

Even though I say this is a "simple programming language", if you do not already have an understanding of how programming languages work then this may still be confusing. In the future I plan to create a tutorial series that aims to teach people how to create their own programming language starting from the very basics and leading up to the level of Klip where people will hopefully have enough understanding to continue developing their own language.

The KlipBundleC# is a zip file that you can download which contains all the source code, the compiled programs and example code for you to compile and execute yourself. There's also example code in the examples folder.
