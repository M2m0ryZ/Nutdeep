# Nutdeep
Nutdeep - A light memory manager library made by C# lovers


# ProcessHandler
```csharp
using (var handler = new ProcessHandler("ProcessName")) { ... }
using (var handler = new ProcessHandler(ProcessId)) { ... }
using (var handler = new ProcessHandler(Process)) { ... }
```

You can find easily process that runs FlashPlayer this way
```csharp
//using System.Linq
var flashPlayerProcess = Process.GetProcesses()
  .Where(p => p.RunsFlashPlayer()).ToArray();
```

If you are looking for Chrome Showckwave Flash process
```csharp
using (var handler = new ProcessHandler("chrome&flash")) { ... }
```


# MemoryScanner
```csharp
using (var handler = new ProcessHandler(//))
{
  //MemoryScanner needs for ProcessAccess (ProcessHandler : ProcessAccess)
  MemoryScanner scanner = handler;
}
```

To perfom a scan
```csharp
var addresses = scanner.SearchFor<T>(T obj);
//where T might be: Signature, bool, char, short, ushort, int, uint, long, ulong, decimal, float, byte[], string
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

# THIS IS INCOMPLETE, GONNA DONE THIS AFTER THE TRAVEL


TODO List:
* Multithreading scan
