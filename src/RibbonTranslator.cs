using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Grasshopper.GUI.Ribbon;
namespace GrasshopperChinese
{
    public sealed class RibbonTranslator
    {
        sealed class Names { internal string Full, Short, Symbol; }
        sealed class PanelName { internal string Value; }
        readonly ConditionalWeakTable<GH_RibbonTab, Names> tabs = new ConditionalWeakTable<GH_RibbonTab, Names>();
        readonly ConditionalWeakTable<GH_RibbonPanel, PanelName> panels = new ConditionalWeakTable<GH_RibbonPanel, PanelName>();
        static readonly FieldInfo full = Field("m_nameFull"), shortName = Field("m_nameShort"), symbol = Field("m_nameSymbol");
        static readonly HashSet<string> categories = new HashSet<string>(StringComparer.Ordinal)
        { "Params", "Maths", "Sets", "Vector", "Curve", "Surface", "Mesh", "Intersect", "Transform", "Display", "Rhino" };
        static readonly Dictionary<string, string> words = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Params", "参数" }, { "Maths", "数学" }, { "Sets", "集合" }, { "Vector", "向量" },
            { "Curve", "曲线" }, { "Surface", "曲面" }, { "Mesh", "网格" }, { "Intersect", "相交" },
            { "Transform", "变换" }, { "Display", "显示" }, { "Rhino", "犀牛" }, { "Kangaroo2", "袋鼠2" },
            { "Geometry", "几何" }, { "Primitive", "基本类型" }, { "Input", "输入" }, { "Util", "工具" },
            { "Domain", "区间" }, { "Matrix", "矩阵" }, { "Operators", "运算符" }, { "Polynomials", "多项式" },
            { "Script", "脚本" }, { "Time", "时间" }, { "Trig", "三角函数" },
            { "List", "列表" }, { "Sequence", "序列" },
            { "Text", "文本" }, { "Tree", "数据树" }, { "Field", "场" }, { "Grid", "网格阵列" },
            { "Plane", "平面" }, { "Point", "点" }, { "Analysis", "分析" }, { "Division", "分割" },
            { "Spline", "样条曲线" }, { "Freeform", "自由形态" }, { "SubD", "细分曲面" },
            { "Triangulation", "三角剖分" }, { "Mathematical", "数学相交" }, { "Physical", "实体相交" },
            { "Region", "区域" }, { "Affine", "仿射" }, { "Array", "阵列" }, { "Euclidean", "欧氏变换" },
            { "Morph", "形态变换" }, { "Colour", "颜色" }, { "Color", "颜色" }, { "Dimension", "标注" },
            { "Preview", "预览" }, { "Vector Display", "向量显示" }, { "Goals", "目标" },
            { "Main", "主要工具" }, { "Solver", "求解器" }, { "Utility", "实用工具" },
            { "Goals-Angle", "目标·角度" }, { "Goals-Co", "目标·共面共线" }, { "Goals-Curve", "目标·曲线" },
            { "Goals-Length", "目标·长度" }, { "Goals-Mesh", "目标·网格" }, { "Goals-On", "目标·约束" },
            { "Goals-Pt", "目标·点" }, { "Goals-6dof", "目标·六自由度" }
        };
        static FieldInfo Field(string name)
        {
            var result = typeof(GH_RibbonTab).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return result != null && result.FieldType == typeof(string) ? result : null;
        }
        public static string Translate(string text, bool bilingual)
        {
            string chinese;
            if (text == null || !words.TryGetValue(text, out chinese)) return text;
            return bilingual ? chinese + " · " + text : chinese;
        }
        public string OriginalCategory(GH_RibbonTab tab)
        { Names n; return tabs.TryGetValue(tab, out n) ? n.Full : tab.NameFull; }
        public void ApplyTab(GH_RibbonTab tab, bool enabled, bool bilingual)
        {
            // Explicitly guarded adapter for Rhino 8: no writes if the expected SDK fields disappear.
            if (full == null || shortName == null || symbol == null) throw new NotSupportedException("This Grasshopper build has an unsupported ribbon layout.");
            Names n;
            if (!tabs.TryGetValue(tab, out n))
            {
                if (!categories.Contains(tab.NameFull)) return;
                n = new Names { Full = tab.NameFull, Short = tab.NameShort, Symbol = tab.NameSymbol };
                tabs.Add(tab, n);
            }
            try
            {
                full.SetValue(tab, enabled ? Translate(n.Full, bilingual) : n.Full);
                shortName.SetValue(tab, enabled ? Translate(n.Full, false) : n.Short);
                symbol.SetValue(tab, enabled ? Translate(n.Full, false).Substring(0, 1) : n.Symbol);
            }
            catch
            {
                full.SetValue(tab, n.Full); shortName.SetValue(tab, n.Short); symbol.SetValue(tab, n.Symbol);
                throw;
            }
        }
        public void ApplyPanel(GH_RibbonPanel panel, string category, bool enabled, bool bilingual)
        {
            if (!categories.Contains(category)) return;
            PanelName n;
            if (!panels.TryGetValue(panel, out n)) { n = new PanelName { Value = panel.Name }; panels.Add(panel, n); }
            panel.Name = enabled ? Translate(n.Value, bilingual) : n.Value;
        }
    }
}
