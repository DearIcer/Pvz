using System.Diagnostics;

int processId = GetProcessId("PlantsVsZombies");
Int64 address = GteModuleAddress(processId, "PlantsVsZombies.exe");
IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, processId);

int bytesRead;

// 读取内存
int offset1 = ReadMemoryInt32(processHandle, (nint)(address + 0x355E0C));
int offset2 = ReadMemoryInt32(processHandle, offset1 + 0x868);

// 写入内存
WriteMemoryInt32(processHandle, offset2 + 0x5578, 2000);
byte[] buffer = [0x29, 0xED, 0x90, 0x90];
WriteMemoryBytes(processHandle, (nint)address + 0x14D0BA, buffer);
buffer = [0x2B, 0x6C, 0x24, 0x20];
WriteMemoryBytes(processHandle, (nint)address + 0x14D0BA, buffer);
// 请确保在操作完成后关闭进程句柄
Win32DllImport.CloseHandle(processHandle);


/// <summary>
/// 获取进程所加载的模块
/// </summary>
/// <param name="processId"></param>
/// <returns></returns>
static void ListProcessModules(int processId)
{
    try
    {
        Process process = Process.GetProcessById(processId);
        Console.WriteLine("\nProcess: {0} ID: {1}", process.ProcessName, process.Id);

        foreach (ProcessModule module in process.Modules)
        {
            Console.WriteLine("\n    MODULE NAME:     {0}", module.ModuleName);
            Console.WriteLine("    Executable     = {0}", module.FileName);
            Console.WriteLine("    Base address   = 0x{0:X}", module.BaseAddress.ToInt64());
            Console.WriteLine("    Base size      = {0}", module.ModuleMemorySize);
        }
    }
    catch (Exception ex)
    {
        // Print the error message
        Console.WriteLine("WARNING: {0}", ex.Message);
    }
}

/// <summary>
/// 获取进程PID
/// </summary>
/// <param name="processName"></param>
/// <returns></returns>
static Int32 GetProcessId(string processName)
{
    Process[] processes = Process.GetProcessesByName(processName);
    if (processes.Length > 0)
    {
        return processes[0].Id;
    }
    return 0;
}

/// <summary>
/// 获取模块地址
/// </summary>
/// <param name="processId"></param>
/// <param name="moduleName"></param>
/// <returns></returns>
static Int64 GteModuleAddress(int processId, string moduleName)
{
    Process process = Process.GetProcessById(processId);
    foreach (ProcessModule module in process.Modules)
    {
        if (module.ModuleName == moduleName)
        {
            return module.BaseAddress.ToInt64();
        }
    }
    return 0;
}

/// <summary>
/// 读内存32位整数
/// </summary>
/// <param name="processHandle">进程句柄</param>
/// <param name="lpBaseAddress">内存地址</param>
/// <returns></returns>
static Int32 ReadMemoryInt32(IntPtr processHandle, IntPtr lpBaseAddress)
{
    int bytesRead;
    byte[] buffer = new byte[1024];
    bool success = Win32DllImport.ReadProcessMemory(processHandle, lpBaseAddress, buffer, buffer.Length, out bytesRead);
    if (success)
    {
        return BitConverter.ToInt32(buffer, 0); 
    }
    return -1;
}

/// <summary>
/// 写内存32位整数
/// </summary>
/// <param name="processHandle">进程句柄</param>
/// <param name="lpBaseAddress">内存地址</param>
/// <param name="value">值</param>
/// <returns></returns>
static Int32 WriteMemoryInt32(IntPtr processHandle, IntPtr lpBaseAddress, Int32 value)
{
    int bytesRead;
    byte[] buffer = BitConverter.GetBytes(value);
    bool success = Win32DllImport.WriteProcessMemory(processHandle, lpBaseAddress, buffer, buffer.Length, out bytesRead);
    if (success)
    {
        return BitConverter.ToInt32(buffer, 0);
    }
    return -1;
}

/// <summary>
/// 读内存64位整数
/// </summary>
/// <param name="processHandle">进程句柄</param>
/// <param name="lpBaseAddress">内存地址</param>
/// <returns></returns>
static Int64 ReadMemoryInt64(IntPtr processHandle, IntPtr lpBaseAddress)
{
    int bytesRead;
    byte[] buffer = new byte[1024]; 
    bool success = Win32DllImport.ReadProcessMemory(processHandle, lpBaseAddress, buffer, buffer.Length, out bytesRead);
    if (success)
    {
        return BitConverter.ToInt64(buffer, 0); 
    }
    return -1;
}

/// <summary>
/// 写内存64位整数
/// </summary>
/// <param name="processHandle">进程句柄</param>
/// <param name="lpBaseAddress">内存地址</param>
/// <param name="value">值</param>
/// <returns></returns>
static Int64 WriteMemoryInt64(IntPtr processHandle, IntPtr lpBaseAddress, Int64 value)
{
    int bytesRead;
    byte[] buffer = BitConverter.GetBytes(value);
    bool success = Win32DllImport.WriteProcessMemory(processHandle, lpBaseAddress, buffer, buffer.Length, out bytesRead);
    if (success)
    {
        return BitConverter.ToInt64(buffer, 0); 
    }
    return -1;
}

/// <summary>
/// 读内存字节
/// </summary>
/// <param name="processHandle">进程句柄</param>
/// <param name="lpBaseAddress">内存地址</param>
/// <returns></returns>
static byte[] ReadMemoryBytes(IntPtr processHandle, IntPtr lpBaseAddress)
{
    int bytesRead;
    byte[] buffer = new byte[1024];
    bool success = Win32DllImport.ReadProcessMemory(processHandle, lpBaseAddress, buffer, buffer.Length, out bytesRead);
    if (success)
    {
        return buffer;
    }
    return null;
}

/// <summary>
/// 写内存字节
/// </summary>
/// <param name="processHandle">进程句柄</param>
/// <param name="lpBaseAddress">内存地址</param>
/// <param name="buffer">字节数据</param>
/// <returns></returns>
static bool WriteMemoryBytes(IntPtr processHandle, IntPtr lpBaseAddress, byte[] buffer)
{
    int bytesRead;
    bool success = Win32DllImport.WriteProcessMemory(processHandle, lpBaseAddress, buffer, buffer.Length, out bytesRead);
    return success;
}
