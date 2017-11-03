# Nutdeep v1.0.3  ![NutdeepIcon](https://i.imgsafe.org/c7/c7ae82b7c1.png)
[![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

Nutdeep - A light memory manager library made by C# lovers - [.NET Framework 4.7.1](https://www.microsoft.com/en-us/download/details.aspx?id=56119)

Click [here](https://github.com/Adversities/Nutdeep/blob/master/ConsoleExample/Program.cs) to check our Console APP example


### Nuget [URL](https://www.nuget.org/packages/Nutdeep/1.0.3)
- Package Manager
  - Install-Package Nutdeep -Version 1.0.3
- .NET CLI
  - dotnet add package Nutdeep --version 1.0.3



### ProcessHandler

By string
```csharp
using (var handler = new ProcessHandler("ProcessName")) { ... }
```
By process id **(System.Int32)**
```csharp
using (var handler = new ProcessHandler(ProcessId)) { ... }
```
By process **(System.Diagnostics.Process)**
```csharp
using (var handler = new ProcessHandler(Process)) { ... }
```

If you are looking for Chrome Showckwave Flash process
```csharp
using (var handler = new ProcessHandler("chrome&flash")) { ... }
```

You can find easily process that runs **FlashPlayer** this way
```csharp
//using System.Linq
var flashPlayerProcesses = Process.GetProcesses()
  .Where(p => p.RunsFlashPlayer()).ToArray();
```



### MemoryScanner
```csharp
using (var handler = new ProcessHandler(//))
{
  //MemoryScanner needs for ProcessAccess (ProcessHandler : ProcessAccess)
  MemoryScanner scanner = new MemoryScanner(handler);
  scanner.SetSettings(new ScanSettings(writable: ScanType.ONLY));
  
  /.../
}
```
Click [here](https://github.com/Adversities/Nutdeep/blob/master/Nutdeep/Tools/ScanSettings.cs) to check how ScanSettings is setup by default *(that will be the setup if you dont specific it)*

To perfom a scan
```csharp
var addresses = scanner.SearchFor<T>(T obj);
```

Scan by signature
```csharp
var addresses = scanner.SearchFor<Signature>("0I ?? LO ?? VE ?? CO ?? ?? DE");
```

To perform a next scan
```csharp
var nextAddresses = scanner.NextSearchFor<T>(IntPtr[] addresses, T obj);
```
Next scan by signature
```csharp
var nextAddresses = scanner.NextSearchFor<Signature>(addresses, "0I ?? LO ?? VE ?? CO ?? ?? DE");
```


### MemoryDumper
```csharp
using (var handler = new ProcessHandler(//))
{
  //MemoryDumper needs for ProcessAccess (ProcessHandler : ProcessAccess)
  MemoryDumper dumper = new MemoryDumper(handler);
  
  /.../
}
```

Read memory this way
```csharp
var object = dumper.Read<T>(IntPtr address);
```

When **T** is **String** or **Byte[]** you can also define a length as the following
```csharp
var byteArray = dumper.Read<byte[]>(IntPtr address, 16);
```
```csharp
var str = dumper.Read<string>(IntPtr address, 16);
```


### MemoryEditor
```csharp
using (var handler = new ProcessHandler(//))
{
  //MemoryDumper needs for ProcessAccess (ProcessHandler : ProcessAccess)
  MemoryEditor editor = new MemoryEditor(editor);
  
  /.../
}
```

Edit memory this way
```csharp
editor.Write(IntPtr address, T obj);
```


Where **T** might be:

* Char
* Single 
* Byte[]
* String
* Boolean
* Decimal
* Int16/UInt16
* Int32/UInt32
* Int64/UInt64
* Signature : String **(Only for MemoryScan)**
