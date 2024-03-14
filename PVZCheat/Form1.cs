using ImGuiNET;
using System;
using System.Diagnostics;
using System.Numerics;
using Veldrid.Sdl2;
using Veldrid;
using Veldrid.StartupUtilities;

namespace PVZCheat
{
    public partial class Form1 : Form
    {
        private static int processId;
        private Int64 address;

        #region imgui
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        private static ImGuiController _controller;
        // private static MemoryEditor _memoryEditor;

        // UI state
        private static float _f = 0.0f;
        private static int _counter = 0;
        private static int _dragInt = 0;
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
        private static bool _showImGuiDemoWindow = true;
        private static bool _showAnotherWindow = false;
        private static bool _showMemoryEditor = false;
        private static byte[] _memoryEditorData;
        private static uint s_tab_bar_flags = (uint)ImGuiTabBarFlags.Reorderable;
        static bool[] s_opened = { true, true, true, true }; // Persistent user state

        static void SetThing(out float i, float val) { i = val; }
        #endregion
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                // Create window, GraphicsDevice, and all resources necessary for the demo.
                VeldridStartup.CreateWindowAndGraphicsDevice(
                    new WindowCreateInfo(50, 50, 1280, 720, 0, "ImGui.NET Sample Program"),
                    new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
                    out _window,
                    out _gd);
                _window.Resized += () =>
                {
                    _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                    _controller.WindowResized(_window.Width, _window.Height);
                };
                _cl = _gd.ResourceFactory.CreateCommandList();
                _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
                // _memoryEditor = new MemoryEditor();
                Random random = new Random();
                _memoryEditorData = Enumerable.Range(0, 1024).Select(i => (byte)random.Next(255)).ToArray();

                var stopwatch = Stopwatch.StartNew();
                float deltaTime = 0f;
                // Main application loop
                while (_window.Exists)
                {
                    deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
                    stopwatch.Restart();
                    InputSnapshot snapshot = _window.PumpEvents();
                    if (!_window.Exists) { break; }
                    _controller.Update(deltaTime, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

                    SubmitUI();

                    _cl.Begin();
                    _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                    _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                    _controller.Render(_gd, _cl);
                    _cl.End();
                    _gd.SubmitCommands(_cl);
                    _gd.SwapBuffers(_gd.MainSwapchain);
                }

                // Clean up Veldrid resources
                _gd.WaitForIdle();
                _controller.Dispose();
                _cl.Dispose();
                _gd.Dispose();
            });
        }


        /// <summary>
        /// 无限阳光周期事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SunshineTimer_Tick(object sender, EventArgs e)
        {
            // 打开进程
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, processId);
            // 读取内存
            int offset1 = MemoryTool.ReadMemoryInt32(processHandle, (nint)(address + 0x355E0C));
            int offset2 = MemoryTool.ReadMemoryInt32(processHandle, offset1 + 0x868);
            // 写入内存
            MemoryTool.WriteMemoryInt32(processHandle, offset2 + 0x5578, 2000);
            Win32DllImport.CloseHandle(processHandle);
        }

        /// <summary>
        /// 无限阳光选择框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfiniteSunshine_CheckedChanged(object sender, EventArgs e)
        {
            if (InfiniteSunshine.Checked)
            {
                SunshineTimer.Enabled = true;
            }
            else
            {
                SunshineTimer.Enabled = false;
            }
        }

        /// <summary>
        /// 秒杀僵尸选择框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KillZombie_CheckedChanged(object sender, EventArgs e)
        {
            // 打开进程
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, processId);

            if (KillZombie.Checked)
            {
                // 这个是僵尸本体秒杀
                byte[] buffer = [0x29, 0xED, 0x90, 0x90];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)address + 0x14D0BA, buffer);

                //// 护甲秒杀
                byte[] buffer2 = [0xEB, 0x02];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)address + 0x14CDCE, buffer2);
            }
            else
            {
                byte[] buffer = [0x2B, 0x6C, 0x24, 0x20];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)address + 0x14D0BA, buffer);
                byte[] buffer2 = [0x7C, 0x02];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)address + 0x14CDCE, buffer2);
            }
        }

        /// <summary>
        /// 获取所有选中的多选框控件
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private List<CheckBox> GetCheckedCheckBoxes(Control container)
        {
            var checkboxes = new List<CheckBox>();

            foreach (Control control in container.Controls)
            {
                if (control is CheckBox checkbox && checkbox.Checked)
                {
                    checkboxes.Add(checkbox);
                }

                checkboxes.AddRange(GetCheckedCheckBoxes(control));
            }

            return checkboxes;
        }

        /// <summary>
        /// 获取所有的多选框控件
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private List<CheckBox> GetCheckBoxes(Control container)
        {
            var checkboxes = new List<CheckBox>();

            foreach (Control control in container.Controls)
            {
                if (control is CheckBox checkbox)
                {
                    checkboxes.Add(checkbox);
                }

                checkboxes.AddRange(GetCheckBoxes(control));
            }

            return checkboxes;
        }

        /// <summary>
        /// 初始化周期事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Init_Tick(object sender, EventArgs e)
        {
            processId = MemoryTool.GetProcessId("PlantsVsZombies");


            // 如果进程ID和地址都不为0，说明找到了进程
            if (processId == 0)
            {
                // 如果没有找到进程，归为休眠状态
                List<CheckBox> checkedCheckboxes = GetCheckedCheckBoxes(this);
                foreach (var checkbox in checkedCheckboxes)
                {
                    checkbox.Checked = false;
                }
                List<CheckBox> checkboxes = GetCheckBoxes(this);
                foreach (var checkbox in checkboxes)
                {
                    checkbox.Enabled = false;
                }
            }
            else
            {
                address = MemoryTool.GteModuleAddress(processId, "PlantsVsZombies.exe");
                List<CheckBox> checkboxes = GetCheckBoxes(this);
                // 如果地址为0，没有获取到模块地址
                if (address == 0)
                {
                    foreach (var checkbox in checkboxes)
                    {
                        checkbox.Enabled = false;
                    }
                }
                // 激活控件
                foreach (var checkbox in checkboxes)
                {
                    checkbox.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 冷却时间周期事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // 打开进程
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, processId);

            var offset1 = MemoryTool.ReadMemoryInt32(processHandle, (nint)address + 0x355E0C);
            var offset2 = MemoryTool.ReadMemoryInt32(processHandle, offset1 + 0x868);
            var offset3 = MemoryTool.ReadMemoryInt32(processHandle, offset2 + 0x15C);
            var count = MemoryTool.ReadMemoryInt32(processHandle, offset3 + 0x24);
            var refresh = 0x4c;
            if (count != -1)
            {
                for (int i = 0; i < count; i++)
                {
                    MemoryTool.WriteMemoryInt32(processHandle, offset3 + refresh, 10000);
                    //Debug.WriteLine(i.ToString() + ":" + MemoryTool.ReadMemoryInt32(processHandle, offset3 + refresh));
                    refresh += 0x50;
                }
            }
        }

        /// <summary>
        /// 冷却时间选择框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_CheckedChanged(object sender, EventArgs e)
        {
            if (Refresh.Checked)
            {
                RefreshTimer.Enabled = true;
            }
            else
            {
                RefreshTimer.Enabled = false;
            }
        }

        /// <summary>
        /// 植物重叠选择框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Overlap_CheckedChanged(object sender, EventArgs e)
        {
            // 打开进程
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, processId);

            if (Overlap.Checked)
            {
                byte[] buffer = [0xE9, 0x47, 0x09, 0x00, 0x00];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)address + 0x1BD2D, buffer);
            }
            else
            {
                byte[] buffer = [0x0F, 0x84, 0x46, 0x09, 0x00, 0x00];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)address + 0x1BD2D, buffer);
            }

        }

        private void AutomaticCollection_CheckedChanged(object sender, EventArgs e)
        {
            // 打开进程
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, processId);

            if (AutomaticCollection.Checked)
            {
                byte[] buffer = [0xEB, 0x09];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)address + 0x3CC72, buffer);
            }
            else
            {
                byte[] buffer = [0x75, 0x09];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)address + 0x3CC72, buffer);
            }
        }

        /// <summary>
        /// 植物无敌选择框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlantsInvincible_CheckedChanged(object sender, EventArgs e)
        {
            // 打开进程
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, processId);

            if (PlantsInvincible.Checked)
            {
                byte[] buffer = [0x83, 0x6E, 0x40, 0xFC];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)address + 0x14BA6A, buffer);
            }
            else
            {
                byte[] buffer = [0x83, 0x46, 0x40, 0xFC];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)address + 0x14BA6A, buffer);
            }
        }

        #region imgui
        private static unsafe void SubmitUI()
        {
            // Demo code adapted from the official Dear ImGui demo program:
            // https://github.com/ocornut/imgui/blob/master/examples/example_win32_directx11/main.cpp#L172

            // 1. Show a simple window.
            // Tip: if we don't call ImGui.Begin(string) / ImGui.End() the widgets automatically appears in a window called "Debug".
            {
                ImGui.Text("");
                ImGui.Text(string.Empty);
                ImGui.Text("Hello, world!");                                        // Display some text (you can use a format string too)
                ImGui.SliderFloat("float", ref _f, 0, 1, _f.ToString("0.000"));  // Edit 1 float using a slider from 0.0f to 1.0f    
                //ImGui.ColorEdit3("clear color", ref _clearColor);                   // Edit 3 floats representing a color

                ImGui.Text($"Mouse position: {ImGui.GetMousePos()}");

                ImGui.Checkbox("ImGui Demo Window", ref _showImGuiDemoWindow);                 // Edit bools storing our windows open/close state
                ImGui.Checkbox("Another Window", ref _showAnotherWindow);
                ImGui.Checkbox("Memory Editor", ref _showMemoryEditor);
                if (ImGui.Button("Button"))                                         // Buttons return true when clicked (NB: most widgets return true when edited/activated)
                    _counter++;
                ImGui.SameLine(0, -1);
                ImGui.Text($"counter = {_counter}");

                ImGui.DragInt("Draggable Int", ref _dragInt);

                float framerate = ImGui.GetIO().Framerate;
                ImGui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");
            }

            // 2. Show another simple window. In most cases you will use an explicit Begin/End pair to name your windows.
            if (_showAnotherWindow)
            {
                ImGui.Begin("Another Window", ref _showAnotherWindow);
                ImGui.Text("Hello from another window!");
                if (ImGui.Button("Close Me"))
                    _showAnotherWindow = false;
                ImGui.End();
            }

            // 3. Show the ImGui demo window. Most of the sample code is in ImGui.ShowDemoWindow(). Read its code to learn more about Dear ImGui!
            if (_showImGuiDemoWindow)
            {
                // Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
                // Here we just want to make the demo initial state a bit more friendly!
                ImGui.SetNextWindowPos(new Vector2(650, 20), ImGuiCond.FirstUseEver);
                ImGui.ShowDemoWindow(ref _showImGuiDemoWindow);
            }

            if (ImGui.TreeNode("Tabs"))
            {
                if (ImGui.TreeNode("Basic"))
                {
                    ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;
                    if (ImGui.BeginTabBar("MyTabBar", tab_bar_flags))
                    {
                        if (ImGui.BeginTabItem("Avocado"))
                        {
                            ImGui.Text("This is the Avocado tab!\nblah blah blah blah blah");
                            ImGui.EndTabItem();
                        }
                        if (ImGui.BeginTabItem("Broccoli"))
                        {
                            ImGui.Text("This is the Broccoli tab!\nblah blah blah blah blah");
                            ImGui.EndTabItem();
                        }
                        if (ImGui.BeginTabItem("Cucumber"))
                        {
                            ImGui.Text("This is the Cucumber tab!\nblah blah blah blah blah");
                            ImGui.EndTabItem();
                        }
                        ImGui.EndTabBar();
                    }
                    ImGui.Separator();
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Advanced & Close Button"))
                {
                    // Expose a couple of the available flags. In most cases you may just call BeginTabBar() with no flags (0).
                    ImGui.CheckboxFlags("ImGuiTabBarFlags_Reorderable", ref s_tab_bar_flags, (uint)ImGuiTabBarFlags.Reorderable);
                    ImGui.CheckboxFlags("ImGuiTabBarFlags_AutoSelectNewTabs", ref s_tab_bar_flags, (uint)ImGuiTabBarFlags.AutoSelectNewTabs);
                    ImGui.CheckboxFlags("ImGuiTabBarFlags_NoCloseWithMiddleMouseButton", ref s_tab_bar_flags, (uint)ImGuiTabBarFlags.NoCloseWithMiddleMouseButton);
                    if ((s_tab_bar_flags & (uint)ImGuiTabBarFlags.FittingPolicyMask) == 0)
                        s_tab_bar_flags |= (uint)ImGuiTabBarFlags.FittingPolicyDefault;
                    if (ImGui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyResizeDown", ref s_tab_bar_flags, (uint)ImGuiTabBarFlags.FittingPolicyResizeDown))
                        s_tab_bar_flags &= ~((uint)ImGuiTabBarFlags.FittingPolicyMask ^ (uint)ImGuiTabBarFlags.FittingPolicyResizeDown);
                    if (ImGui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyScroll", ref s_tab_bar_flags, (uint)ImGuiTabBarFlags.FittingPolicyScroll))
                        s_tab_bar_flags &= ~((uint)ImGuiTabBarFlags.FittingPolicyMask ^ (uint)ImGuiTabBarFlags.FittingPolicyScroll);

                    // Tab Bar
                    string[] names = { "Artichoke", "Beetroot", "Celery", "Daikon" };

                    for (int n = 0; n < s_opened.Length; n++)
                    {
                        if (n > 0) { ImGui.SameLine(); }
                        ImGui.Checkbox(names[n], ref s_opened[n]);
                    }

                    // Passing a bool* to BeginTabItem() is similar to passing one to Begin(): the underlying bool will be set to false when the tab is closed.
                    if (ImGui.BeginTabBar("MyTabBar", (ImGuiTabBarFlags)s_tab_bar_flags))
                    {
                        for (int n = 0; n < s_opened.Length; n++)
                            if (s_opened[n] && ImGui.BeginTabItem(names[n], ref s_opened[n]))
                            {
                                ImGui.Text($"This is the {names[n]} tab!");
                                if ((n & 1) != 0)
                                    ImGui.Text("I am an odd tab.");
                                ImGui.EndTabItem();
                            }
                        ImGui.EndTabBar();
                    }
                    ImGui.Separator();
                    ImGui.TreePop();
                }
                ImGui.TreePop();
            }

            ImGuiIOPtr io = ImGui.GetIO();
            SetThing(out io.DeltaTime, 2f);

            if (_showMemoryEditor)
            {
                ImGui.Text("Memory editor currently supported.");
                // _memoryEditor.Draw("Memory Editor", _memoryEditorData, _memoryEditorData.Length);
            }

            // On .NET Standard 2.1 or greater, you can use ReadOnlySpan<char> instead of string to prevent allocations.
            long allocBytesStringStart = GC.GetAllocatedBytesForCurrentThread();
            ImGui.Text($"Hello, world {Random.Shared.Next(100)}!");
            long allocBytesStringEnd = GC.GetAllocatedBytesForCurrentThread() - allocBytesStringStart;
            Console.WriteLine("GC (string): " + allocBytesStringEnd);

            long allocBytesSpanStart = GC.GetAllocatedBytesForCurrentThread();
            ImGui.Text($"Hello, world {Random.Shared.Next(100)}!".AsSpan()); // Note that this call will STILL allocate memory due to string interpolation, but you can prevent that from happening by using an InterpolatedStringHandler.
            long allocBytesSpanEnd = GC.GetAllocatedBytesForCurrentThread() - allocBytesSpanStart;
            Console.WriteLine("GC (span): " + allocBytesSpanEnd);

            ImGui.Text("Empty span:");
            ImGui.SameLine();
            ImGui.GetWindowDrawList().AddText(ImGui.GetCursorScreenPos(), uint.MaxValue, ReadOnlySpan<char>.Empty);
            ImGui.NewLine();
            ImGui.GetWindowDrawList().AddText(ImGui.GetCursorScreenPos(), uint.MaxValue, $"{ImGui.CalcTextSize("h")}");
            ImGui.NewLine();
            ImGui.TextUnformatted("TextUnformatted now passes end ptr but isn't cut off");
        }
        #endregion
    }
}
