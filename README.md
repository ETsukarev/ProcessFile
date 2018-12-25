# ProcessFile
An C# application for generating SHA256 hash function for each block of the file.

Start application:

1. Build solution.
2. Run application with 3 arguments: existed file name, size of block in bytes, and version (v1 or v2).

Example:
ProcessFile d:\Temp\file.txt 10000 v1
or
ProcessFile "d:\Temp\file with data.txt" 10000 v2


File with results named 'SignatureBlocks.txt' will be in the directory of ProcessFile.exe

Warning !

v1 - It used classes for multithreading only which exists in .Net Framework 3.5;

v2 - It used Task? async/await
