Mizu
====

Introduction
------------

Mizu is my attempt at creating a compiler for the .NET framework. It compiles a simple esoteric programming language into a executable that runs on the framework. That means the executables are true .NET executes and are compatible with ones that are generated by csc (C# compiler) and vbc (VB.NET compiler).

The name '**Mizu**' means water in Japanese and Mizu (the language) flows like water from left to right.

I took inspiration in making this a math-based language from **Fortran**. 

At the moment, the compiler is written in **C#** but when I get the time, I will port it to **VB.NET** as well.

Tools
-----
I used the following tools in my creation of the compiler.

+   ILSpy (by the SharpDevelop team) for viewing the generated IL and viewing the **C#** equalivent.
+   TinyPG for generating the parser/scanner. Search http://codeproject.com for it.
+   PEVerify (included with Visual Studio) for debugging IL problems. (Especially during Invalid CLR program exceptions).

Syntax
------

Since this is my first true programming language and I didn't expect it to have any actual use, the syntax is weird.

	a`5|b`[1..10]|?c:a+b|.c

I'll explain.

The "a`5" bit declares a variable named "**a**" and assigns it the value of "**5**".

The "b`[1..10]" declares a variable named "**b**" and creates a for loop. B will iterate through **[1..9]** (**10** is exclusive).

"?c:a+b" declares a variable of "**c**" as the output of "**a+b**". Remember, **b** will change so this is loop because this statement occures to the right of the loop.

".c" prints the value of "**c**".

Technical Stuff
---------------

Instead of evaluating mathmatical expressions in IL, I created a external lib for evaling expressions in JScript.NET (jsc). JScript has an internal 'eval' function so I decided to math use of that.


Contact Me
----------

If you have any questions, you can find me on IRC. ##XAMPP @ irc.freenode.net

Remember, theres two #, not one.