using ImGuiNET;
using System.Diagnostics;
using System.Numerics;
using Veldrid.Sdl2;
using Veldrid;
using Veldrid.StartupUtilities;
using static Win32DllImport;

namespace PVZCheat
{
    public partial class Form1 : Form
    {
        private static int _processId;
        private Int64 _address;
        private Int32 _gameWindowHandle;
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
        private static byte[] _memoryEditorData;
        private static uint s_tab_bar_flags = (uint)ImGuiTabBarFlags.Reorderable;
        static bool[] s_opened = { true, true, true, true }; // Persistent user state
        private static bool _enableESP = false;
        static void SetThing(out float i, float val) { i = val; }
        #endregion
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ��ʼ������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                _gameWindowHandle = (int)FindWindow("MainWindow", "Plants vs. Zombies");
                if (_gameWindowHandle != -1)
                {
                    RECT rect;
                    GetWindowRect(_gameWindowHandle, out rect);

                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;
                    // Create window, GraphicsDevice, and all resources necessary for the demo.
                    VeldridStartup.CreateWindowAndGraphicsDevice(
                        new WindowCreateInfo(rect.Left, rect.Top, width + 5, height + 5, Veldrid.WindowState.Normal, "ImGui.NET Sample Program"),
                        new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
                        out _window,
                        out _gd);
                    _window.Resized += () =>
                    {
                        _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                        _controller.WindowResized(_window.Width, _window.Height);
                    };

                    // ��ȡ��ǰ������ʽ
                    int style = (int)GetWindowLong32(_window.Handle, GWL_STYLE);

                    // �Ƴ����������߿����ʽ
                    style &= ~(WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_BORDER);

                    // �����µĴ�����ʽ
                    SetWindowLong(_window.Handle, GWL_STYLE, style);

                    // ʹ���ڴ�С��λ�ò�����Ч
                    SetWindowPos(_window.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);

                    // ���ô���Ϊ͸����ʽ
                    int exStyle = (int)GetWindowLong32(_window.Handle, GWL_EXSTYLE);
                    exStyle |= WS_EX_LAYERED;
                    SetWindowLong(_window.Handle, GWL_EXSTYLE, exStyle);

                    // ���ô���͸���ȣ�0-255��
                    byte opacity = 255; // ����Ϊ��͸�����ɸ�����Ҫ����ֵ

                    SetLayeredWindowAttributes(_window.Handle, 0, opacity, LWA_ALPHA);

                    //MSGloop();
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

                        uint uFlags = 0x0001 | 0x0002;  // SWP_NOSIZE | SWP_NOMOVE������ԭʼ��С��λ��
                        //SubmitUI();


                        //MoveWindow(_window.Handle, rect.Left - 5, rect.Top, width + 10, height + 5, false);
                        //SetWindowPos(_window.Handle, -1, rect.Left - 5, rect.Top, width + 10, height + 5, uFlags);
                        // Render
                        FirstBox();
                        if (_enableESP)
                        {
                            ImGui.Begin("aa");
                            DrawESP();
                            ImGui.End();
                   
                        }
                        RefreshDrawWindow();
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
        /// �������������¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SunshineTimer_Tick(object sender, EventArgs e)
        {
            // �򿪽���
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, _processId);
            // ��ȡ�ڴ�
            int offset1 = MemoryTool.ReadMemoryInt32(processHandle, (nint)(_address + 0x355E0C));
            int offset2 = MemoryTool.ReadMemoryInt32(processHandle, offset1 + 0x868);
            // д���ڴ�
            MemoryTool.WriteMemoryInt32(processHandle, offset2 + 0x5578, 2000);
            Win32DllImport.CloseHandle(processHandle);
        }

        /// <summary>
        /// ��������ѡ���
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
        /// ��ɱ��ʬѡ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KillZombie_CheckedChanged(object sender, EventArgs e)
        {
            // �򿪽���
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, _processId);

            if (KillZombie.Checked)
            {
                // ����ǽ�ʬ������ɱ
                byte[] buffer = [0x29, 0xED, 0x90, 0x90];
                MemoryTool.WriteMemoryBytes(processHandle, (nint)_address + 0x14D0BA, buffer);

                //// ������ɱ
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
        /// ��ȡ����ѡ�еĶ�ѡ��ؼ�
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
        /// ��ȡ���еĶ�ѡ��ؼ�
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
        /// ��ʼ�������¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Init_Tick(object sender, EventArgs e)
        {
            _processId = MemoryTool.GetProcessId("PlantsVsZombies");


            // �������ID�͵�ַ����Ϊ0��˵���ҵ��˽���
            if (_processId == 0)
            {
                // ���û���ҵ����̣���Ϊ����״̬
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
                // �����ַΪ0��û�л�ȡ��ģ���ַ
                if (_address == 0)
                {
                    foreach (var checkbox in checkboxes)
                    {
                        checkbox.Enabled = false;
                    }
                }
                // ����ؼ�
                foreach (var checkbox in checkboxes)
                {
                    checkbox.Enabled = true;
                }
            }
        }

        /// <summary>
        /// ��ȴʱ�������¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // �򿪽���
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
        /// ��ȴʱ��ѡ���
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
        /// ֲ���ص�ѡ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Overlap_CheckedChanged(object sender, EventArgs e)
        {
            // �򿪽���
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
            // �򿪽���
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
        /// ֲ���޵�ѡ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlantsInvincible_CheckedChanged(object sender, EventArgs e)
        {
            // �򿪽���
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
        }

        /// <summary>
        /// ��һ����
        /// </summary>
        private void FirstBox()
        {
            Color color = Color.FromArgb(50, 255, 255, 255); // ��ɫ

            int colorValue = (color.A << 24) + (color.R << 16) + (color.G << 8) + color.B;

            //ImGui.GetForegroundDrawList().AddLine(new Vector2(500, 100), new Vector2(500, 200), (uint)colorValue);
            //ImGui.GetForegroundDrawList().AddLine(new Vector2(500, 100), new Vector2(700, 100), (uint)colorValue);

            //ImGui.GetForegroundDrawList().AddLine(new Vector2(700, 100), new Vector2(700, 200), (uint)colorValue);
            //ImGui.GetForegroundDrawList().AddLine(new Vector2(500, 200), new Vector2(700, 200), (uint)colorValue);

            DrawUI.TransparentRectangle(new Vector2(100, 100), new Vector2(1, 1), color);
        }
        #endregion

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ESP_CheckedChanged(object sender, EventArgs e)
        {
            if (ESP.Checked)
            {
                _enableESP = true;
            }
            else
            {
                _enableESP = false;
            }
           
        }

        /// <summary>
        /// ������������¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ESPTimer_Tick(object sender, EventArgs e)
        {
            // �򿪽���
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, _processId);

            var offset1 = MemoryTool.ReadMemoryInt32(processHandle, (nint)_address + 0x355E0C);
            var offset2 = MemoryTool.ReadMemoryInt32(processHandle, offset1 + 0x868);
            var offset3 = MemoryTool.ReadMemoryInt32(processHandle, offset2 + 0xA8);
            var count = MemoryTool.ReadMemoryInt32(processHandle, offset2 + 0xb8);
            var zombieIndex = MemoryTool.ReadMemoryInt32(processHandle, offset2 + 0xb4);
            List<int> zombieList = new List<int>();
            for (int i = zombieIndex - 1; i >= 0; i--)
            {
                var zombie = MemoryTool.ReadMemoryInt32(processHandle, offset3 + i * 0x168);
                if (41004408 == zombie)
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
        private void DrawESP()
        {
            // �򿪽���
            IntPtr processHandle = Win32DllImport.OpenProcess(Win32DllImport.PROCESS_ALL_ACCESS | Win32DllImport.PROCESS_VM_READ | Win32DllImport.PROCESS_VM_WRITE | Win32DllImport.PROCESS_VM_OPERATION, false, _processId);

            var offset1 = MemoryTool.ReadMemoryInt32(processHandle, (nint)_address + 0x355E0C);
            var offset2 = MemoryTool.ReadMemoryInt32(processHandle, offset1 + 0x868);
            var offset3 = MemoryTool.ReadMemoryInt32(processHandle, offset2 + 0xA8);
            var count = MemoryTool.ReadMemoryInt32(processHandle, offset2 + 0xb8);
            var zombieIndex = MemoryTool.ReadMemoryInt32(processHandle, offset2 + 0xb4);
            for (int i = zombieIndex - 1; i >= 0; i--)
            {
                var zombie = MemoryTool.ReadMemoryInt32(processHandle, offset3 + i * 0x168);
                if (41004408 == zombie)
                {
                    int health = MemoryTool.ReadMemoryInt32(processHandle, offset3 + i * 0x168 + 0xC8);
                    if (health == 0)
                        continue;
                    var x = MemoryTool.ReadMemoryFloat(processHandle, offset3 + i * 0x168 + 0x2C);
                    var y = MemoryTool.ReadMemoryFloat(processHandle, offset3 + i * 0x168 + 0x30);

                    Color color = Color.FromArgb(255, 0, 0, 255);

                    int colorValue = (color.A << 24) + (color.R << 16) + (color.G << 8) + color.B;
                    ImGui.GetForegroundDrawList().AddText(new Vector2(x, y), (uint)colorValue, "Zombie");
                    Debug.Print("=========");
                    Debug.Print("Zmobie:" + (i + 1));
                    Debug.Print("X:" + x.ToString("r"));
                    Debug.Print("Y:" + y.ToString("r"));
                    Debug.Print("Health:" + health.ToString("r"));
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Color color = Color.FromArgb(50, 255, 255, 255);
            DrawUI.TransparentRectangle(new Vector2(Convert.ToSingle(textBox1.Text), Convert.ToSingle(textBox2.Text)),
                new Vector2(Convert.ToSingle(textBox3.Text), Convert.ToSingle(textBox4.Text)),
                color);
        }

        /// <summary>
        /// ˢ�»��ƴ���
        /// </summary>
        private void RefreshDrawWindow()
        {
            RECT clientRect;
            GetClientRect(_gameWindowHandle, out clientRect);
            POINT upperLeft = new POINT() { X = clientRect.Left, Y = clientRect.Top };
            ClientToScreen(_gameWindowHandle, ref upperLeft);
            int gameWidth = clientRect.Right;
            int gameHeight = clientRect.Bottom;
            MoveWindow(_window.Handle, upperLeft.X, upperLeft.Y, gameWidth, gameHeight, true);
            //UpdateWindow(_window.Handle);
            uint uFlags = 0x0001 | 0x0002;  // SWP_NOSIZE | SWP_NOMOVE������ԭʼ��С��λ��
            SetWindowPos(_window.Handle, -1, 0, 0, 0, 0, uFlags);
            BringWindowToTop(_window.Handle);
            UpdateWindow(_window.Handle);
        }
        private void MSGloop()
        {
            MSG msg;
            while (GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);

                // �����Ҫ�˳�ѭ������������Ϣ��������з��� WM_QUIT ��Ϣ
                if (msg.message == WM_QUIT)
                {
                    break;
                }
            }
        }
    }
}
