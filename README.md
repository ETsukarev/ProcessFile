# ProcessFile
An C# application for generating SHA256 hash function for each block of the file.

Start application:

1. Build solution.
2. Run application with 2 arguments: existed file name and size of block in bytes

Example:
ProcessFile d:\Temp\file.txt 10000
or
ProcessFile "d:\Temp\file with data.txt" 10000


File with results named 'SignatureBlocks.txt' will be in the directory of ProcessFile.exe

Warning !
It used classes for multithreading only which exists in .Net Framework 3.5. 
