/*
Hello1.c : Reads library version and flash memory size.

Compiler: BC++ 3.1, 
          Turbo C++ 1.01(3.01) (free from http://cc.codegear.com/free/cpp) 
          MSC 6.0,
          MSVC 1.52.
          
Compile mode: Large

Project: Hello1.c
         ..\..\Lib\7186el.Lib

Detail description:
    User can use MiniOs utility to load hello.exe to 7188E.

Hardware: uPAC-7186

[Oct 30, 2008] by Liam
*/

#include "..\..\lib\7186e.h"

class Test
{
    int number;
    
    public:
        Test() { number=0;  printno(); }
        ~Test() {}
   
    void printno(void)
    {
        Print("number=%d\n\r",number);
        number++;
    }
};

void main(int argc, char *argv[])
{
    int type;

    InitLib();
    
    type=Is7186e();
    if(type)
    {
        Print("Hello uPAC-7186EX! (Flash memory is %d K)\n\r", type);
        
        Test test1;
        test1.printno();
    }
    else
    {
        Print("Hello PC!, this program is not run under uPAC-7186EX.");
    }
}