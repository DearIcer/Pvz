using ImGuiNET;
using System;
using System.Diagnostics;
using System.Numerics;
using Veldrid.Sdl2;
using Veldrid;
using Veldrid.StartupUtilities;
using System.Text;
using System.Windows.Forms;
using static Win32DllImport;
using Vulkan.Win32;

namespace PVZCheat
{
    public partial class Form1 : Form
    {
        private static int _processId;
        private Int64 _address;
        private Int32 _windowHandle;
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
        private static Vector3 _clearColor = new Vector3(0, 0, 0);
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
                _windowHandle = (int)FindWindow("MainWindow", "Plants vs. Zombies");
                if (_windowHandle != -1)
                {
                    RECT rect;
                    GetWindowRect(_windowHandle, out rect);

                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;
                    // Create window, GraphicsDevice, and all resources necessary for the demo.
                    VeldridStartup.CreateWindowAndGraphicsDevice(
                        new WindowCreateInfo(rect.Left , rect.Top, width + 5, height + 5, Veldrid.WindowState.BorderlessFullScreen, "ImGui.NET Sample Program"),
                        new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
                        out _window,
                        out _gd);
                    _window.Resized += () =>
                    {
                        _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                        _controller.WindowResized(_window.Width, _window.Height);
                    };
                    
                    ShowWindow(_window.Handle, SW_SHOWDEFAULT);
                    UpdateWindow(_window.Handle);
                    // 设置窗口扩展样式
                    int exStyle = (int)GetWindowLong32(_window.Handle, GWL_EXSTYLE);
                    SetWindowLong(_window.Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
                    // 设置窗口透明度
                    byte alpha = 255; // 透明度值，取值范围为 0（完全透明）到 255（完全不透明）
                    SetLayeredWindowAttributes(_window.Handle, 0, alpha, LWA_ALPHA);

                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        var margins = new MARGINS
                        {
                            cxLeftWidth = -1,
                            cxRightWidth = 0,
                            cyTopHeight = 0,
                            cyBottomHeight = 0
                        };
                        DwmExtendFrameIntoClientArea(_window.Handle, ref margins);
                    }
                   
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

                        uint uFlags = 0x0001 | 0x0002;  // SWP_NOSIZE | SWP_NOMOVE，保持原始大小和位置
                        SubmitUI();
                        MoveWindow(_window.Handle, rect.Left - 5, rect.Top, width + 10, height + 5, true);
                        SetWindowPos(_window.Handle, -1, 0, 0, 0, 0, uFlags);
                        // Render
                        FirstBox();
                        _cl.Begin();
                        _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                        _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 0f));
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
                }
               
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
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, _processId);
            // 读取内存
            int offset1 = MemoryTool.ReadMemoryInt32(processHandle, (nint)(_address + 0x355E0C));
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
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, _processId);

            if (KillZombie.Checked)
            {
                // 这个是僵尸本体秒杀
                byte[] buffer = [0x29, 0xED, 0x90, 0x90];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)_address + 0x14D0BA, buffer);

                //// 护甲秒杀
                byte[] buffer2 = [0xEB, 0x02];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)_address + 0x14CDCE, buffer2);
            }
            else
            {
                byte[] buffer = [0x2B, 0x6C, 0x24, 0x20];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)_address + 0x14D0BA, buffer);
                byte[] buffer2 = [0x7C, 0x02];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)_address + 0x14CDCE, buffer2);
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
            _processId = MemoryTool.GetProcessId("PlantsVsZombies");


            // 如果进程ID和地址都不为0，说明找到了进程
            if (_processId == 0)
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
                _address = MemoryTool.GteModuleAddress(_processId, "PlantsVsZombies.exe");
                List<CheckBox> checkboxes = GetCheckBoxes(this);
                // 如果地址为0，没有获取到模块地址
                if (_address == 0)
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
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, _processId);

            var offset1 = MemoryTool.ReadMemoryInt32(processHandle, (nint)_address + 0x355E0C);
            var offset2 = MemoryTool.ReadMemoryInt32(processHandle, offset1 + 0x868);
            var offset3 = MemoryTool.ReadMemoryInt32(processHandle, offset2 + 0x15C);
            var count = MemoryTool.ReadMemoryInt32(processHandle, offset3 + 0x24);
            var refresh = 0x4c;
            if (count != -1)
            {
                for (int i = 0; i < count; i++)
                {
                    MemoryTool.WriteMemoryInt32(processHandle, offset3 + refresh, 10000);
                    //Debug.WriteLine(i.ToString() + ":" + MemoryTool.ReadMemoryInt32(processHandle, zombie + refresh));
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
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, _processId);

            if (Overlap.Checked)
            {
                byte[] buffer = [0xE9, 0x47, 0x09, 0x00, 0x00];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)_address + 0x1BD2D, buffer);
            }
            else
            {
                byte[] buffer = [0x0F, 0x84, 0x46, 0x09, 0x00, 0x00];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)_address + 0x1BD2D, buffer);
            }

        }

        private void AutomaticCollection_CheckedChanged(object sender, EventArgs e)
        {
            // 打开进程
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, _processId);

            if (AutomaticCollection.Checked)
            {
                byte[] buffer = [0xEB, 0x09];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)_address + 0x3CC72, buffer);
            }
            else
            {
                byte[] buffer = [0x75, 0x09];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)_address + 0x3CC72, buffer);
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
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, _processId);

            if (PlantsInvincible.Checked)
            {
                byte[] buffer = [0x83, 0x6E, 0x40, 0xFC];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)_address + 0x14BA6A, buffer);
            }
            else
            {
                byte[] buffer = [0x83, 0x46, 0x40, 0xFC];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)_address + 0x14BA6A, buffer);
            }
        }

        #region imgui
        private static unsafe void SubmitUI()
        {
            ImGui.Begin("Another Window", ref _showAnotherWindow);

            ImGui.Text("Hello from another window!");
            ImGui.End();

            {
                ImGui.Text("");
                ImGui.Text(string.Empty);
                ImGui.Text("hi !");                                        // Display some text (you can use a format string too)
                ImGui.SliderFloat("float", ref _f, 0, 1, _f.ToString("0.000"));  // Edit 1 float using a slider from 0.0f to 1.0f    
                //ImGui.ColorEdit3("clear color", ref _clearColor);                   // Edit 3 floats representing a color

                ImGui.Text($"Mouse position: {ImGui.GetMousePos()}");

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

        /// <summary>
        /// 第一个框
        /// </summary>
        private void FirstBox()
        {
            Color color = Color.FromArgb(50, 255, 255, 255); // 红色

            int colorValue = (color.A << 24) + (color.R << 16) + (color.G << 8) + color.B;

            ImGui.GetForegroundDrawList().AddLine(new Vector2(500, 100), new Vector2(500, 200), (uint)colorValue);
            ImGui.GetForegroundDrawList().AddLine(new Vector2(500, 100), new Vector2(700, 100), (uint)colorValue);

            ImGui.GetForegroundDrawList().AddLine(new Vector2(700, 100), new Vector2(700, 200), (uint)colorValue);
            ImGui.GetForegroundDrawList().AddLine(new Vector2(500, 200), new Vector2(700, 200), (uint)colorValue);

            DrawUI.TransparentRectangle(new Vector2(500, 100), new Vector2(200, 200), color);
        }
        #endregion

        /// <summary>
        /// 方框绘制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ESP_CheckedChanged(object sender, EventArgs e)
        {
            if (ESP.Checked)
            {
                ESPTimer.Enabled = true;
            }
            else
            {
                ESPTimer.Enabled = false;
            }
        }
        
        /// <summary>
        /// 方框绘制周期事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ESPTimer_Tick(object sender, EventArgs e)
        {
            // 打开进程
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, _processId);

            var offset1 = MemoryTool.ReadMemoryInt32(processHandle, (nint)_address + 0x355E0C);
            var offset2 = MemoryTool.ReadMemoryInt32(processHandle, offset1 + 0x868);
            var offset3 = MemoryTool.ReadMemoryInt32(processHandle, offset2 + 0xA8);
            var count = MemoryTool.ReadMemoryInt32(processHandle, offset2 + 0xb8);
            var zombieIndex = MemoryTool.ReadMemoryInt32(processHandle, offset2 + 0xb4);
            List<int> zombieList = new List<int>();
            for (int i = zombieIndex -1; i >= 0; i--)
            {
                var zombie = MemoryTool.ReadMemoryInt32(processHandle, offset3 + i * 0x168);
                if(41004408 == zombie)
                {
                    int health = MemoryTool.ReadMemoryInt32(processHandle, offset3 + i * 0x168 + 0xC8);
                    if (health == 0)
                        continue;
                    var x = MemoryTool.ReadMemoryFloat(processHandle, offset3 + i * 0x168 + 0x2C);
                    var y = MemoryTool.ReadMemoryFloat(processHandle, offset3 + i * 0x168 + 0x30);
                    Debug.Print("=========");
                    Debug.Print("Zmobie:" + (i + 1));
                    Debug.Print("X:" + x.ToString("r"));
                    Debug.Print("Y:" + y.ToString("r"));
                    Debug.Print("Health:" + health.ToString("r"));
                }
            }

        }
    }
}
