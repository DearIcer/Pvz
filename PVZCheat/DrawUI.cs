using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PVZCheat
{
    public static class DrawUI
    {
        public static void TransparentRectangle(Vector2 pos, Vector2 size, Color col)
        {
            int colorValue = (col.A << 24) + (col.R << 16) + (col.G << 8) + col.B;
            ImGui.GetForegroundDrawList().AddRect(pos, size, (uint)colorValue);
            //ImGui.GetForegroundDrawList().AddRectFilled(pos, size, (uint)colorValue);
        }
    }
}
