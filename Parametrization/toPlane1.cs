using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShoNS.Array;
using Rhino.Geometry;
namespace mikity.ghComponents
{
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public partial class toPlane1 : Grasshopper.Kernel.GH_Component
    {
        Func<double, double> Drift0 = (v) => { return 0.98; };
        Func<double, double> Drift1 = (v) => { /*if (v > 0)*/ return v / 20d + 0.95; /*else return 0.95;*/ };
        Func<double, double> Drift2 = (v) => { if (v >= 0)return 1.0; else return 0.0; };
        static float Shifting = 500;
        mikity.visualize.FigureUI controlPanel;
        /*        protected override System.Drawing.Bitmap Icon
                {
                    get
                    {
                        //現在のコードを実行しているAssemblyを取得
                        System.Reflection.Assembly myAssembly =
                            System.Reflection.Assembly.GetExecutingAssembly();

                        System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.icon46.bmp");
                        //指定されたマニフェストリソースを読み込む
                        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                        return bmp;
                    }
                }
        */
        public toPlane1()
            : base("pushSurface2Plane", "toPlanar", "Push Surface to x-y plane", "Kapybara3D", "Computation")
        {
        }
        ~toPlane1()
        {
            keyboardHook.Uninstall();
        }
        RamGecTools.MouseHook mouseHook = new RamGecTools.MouseHook();
        RamGecTools.KeyboardHook keyboardHook = new RamGecTools.KeyboardHook();
        bool reset = false;
        void activate()
        {
            full.activate();
        }
        void deactivate()
        {
            full.deactivate();
        }
        bool keyboardHook_KeyUp(RamGecTools.KeyboardHook.VKeys key)
        {
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_M)
            {
                full.dropMaterial();
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_R)
            {
                if (_RF)
                {
                    _RF = false;
                    full.offRF();
                }
                else
                {
                    _RF = true;
                    full.onRF();
                }
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_F)
            {
                if (_fixFlip)
                {
                    _fixFlip = false;
                    full.offFix();
                }
                else
                {
                    _fixFlip = true;
                    full.onFix();
                }
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_A)
            {
                if (_drift1)
                {
                    _drift1 = false;
                    _drift2 = true;
                    full.drift2();
                    full.renewPlot(Drift2);
                }
                else if (_drift2)
                {
                    _drift1 = false;
                    _drift2 = false;
                    full.drift0();
                    full.renewPlot(Drift0);
                }
                else
                {
                    _drift1 = true;
                    _drift2 = false;
                    full.drift1();
                    full.renewPlot(Drift1);
                }
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.ESCAPE)
            {
                full.resetGo();
                _go = false;
                Clock = -1;
                timer.Enabled = false;
                isInitialized = false;
                reset = true;
                vel.FillValue(0);
                vel2.FillValue(0);
                ExpireSolution(true);
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_G)
            {
                if (_go)
                {
                    full.pauseGo();
                    _go = false;
                    timer.Enabled = false;
                }
                else
                {
                    full.onGo();
                    _go = true;
                    timer.Enabled = true;
                    setupMaterial(mat);
                }
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_I)
            {
                if (_intPoint)
                {
                    _intPoint = false;
                }
                else
                {
                    _intPoint = true;
                }
                reset = true;
                this.ExpireSolution(true);
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_B)
            {
                if (_base)
                {
                    _base = false;
                }
                else
                {
                    _base = true;
                }
                reset = true;
                this.ExpireSolution(true);
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_D)
            {
                if (_eigen)
                {
                    _eigen = false;
                    _conformal = true;
                }
                else if (_conformal)
                {
                    _eigen = false;
                    _conformal = false;
                }
                else
                {
                    _eigen = true;
                    _conformal = false;
                }
                reset = true;
                this.ExpireSolution(true);
                return true;
            }
            return false;
        }

        bool keyboardHook_KeyDown(RamGecTools.KeyboardHook.VKeys key)
        {
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_M)
            {
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_R)
            {
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_F)
            {
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_A)
            {
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.ESCAPE)
            {
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_G)
            {
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_I)
            {
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_B)
            {
                return true;
            }
            if (key == RamGecTools.KeyboardHook.VKeys.KEY_D)
            {
                return true;
            }
            return false; 
        }
        void timer_Tick(object sender, EventArgs e)
        {
            this.ExpireSolution(true);
        }
        mikity.visualize.fullScreen full;
        System.Windows.Forms.Timer timer;
        System.Windows.Forms.ToolStripMenuItem __gm1, __vf1, __vf2, __vf3, __vf4;
        System.Windows.Forms.ToolStripMenuItem __bm1, __bm2, __bm3, __gm2;
        System.Windows.Forms.ToolStripMenuItem __ig1, __ig21, __ig22, __ig23, __ig3, __ig5;
        /*private void test()
        {
            var m = new Mesh();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m.Vertices.Add(new Point3d(j, i, 0));
                }
            }
            m.Faces.AddFace(0, 1, 4);  //p
            m.Faces.AddFace(1, 4, 5);  //n
            m.Faces.AddFace(1, 5, 2);  //n
            m.Faces.AddFace(2, 6, 5);  //p
            m.Faces.AddFace(2, 3, 7, 6);//p
            m.Faces.AddFace(4, 9, 8);  //p
            m.Faces.AddFace(4, 5, 9);  //p
            m.Faces.AddFace(5, 9, 10); //n
            m.Faces.AddFace(5, 10, 6);  //n
            m.Faces.AddFace(6, 7, 11, 10); //p
            m.Faces.AddFace(8, 9, 13, 12); //p
            m.Faces.AddFace(9, 10, 13);  //p
            m.Faces.AddFace(10, 13, 14); //n
            m.Faces.AddFace(10, 11, 15, 14); //p
            mikity.GeometryProcessing.MeshStructure.CreateFrom(m);
        }*/
        public override void AddedToDocument(Grasshopper.Kernel.GH_Document document)
        {
            base.AddedToDocument(document);
            //test();
            Rhino.RhinoDoc.ReplaceRhinoObject += RhinoDoc_ReplaceRhinoObject;
            timer = new System.Windows.Forms.Timer();
            timer.Tick += timer_Tick;
            timer.Enabled = false;
            timer.Interval = 1;

            // register evens
            keyboardHook.KeyDown = new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyDown);
            keyboardHook.KeyUp = new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyUp);
            keyboardHook._activate = new RamGecTools.KeyboardHook.activate(activate);
            keyboardHook._deactivate = new RamGecTools.KeyboardHook.deactivate(deactivate);
            keyboardHook.Install();

            mat = materialChoice.DHM;
            _material = new Kapybara3D.Materials.harmonicMaterial();
            type = modelType.membrane;
            _isFixedBoundary = true;
            controlPanel = new mikity.visualize.FigureUI();
            full = new mikity.visualize.fullScreen();
            _param.wA = 50;
            _param.wL = 50;
            _param.wC = 50;
            _param.wT = 0;
            _param.wT2 = 0;
            _param.Neo = 100;
            _param.Alpha = 50;
            _param.hmAlpha = 50;
            _param.dcmDist = 50;
            __rebuildControlPanel();
            controlPanel.Show();
            full.deactivate();
            full.Show();
            full.resetGo();
            full.drift1();
            full.onFix();
            full.onRF();
            full.renewPlot(Drift1);
            full._selectMaterial = (s) => selectMaterial(s);
            full.initMaterial();

        }
        public override void RemovedFromDocument(Grasshopper.Kernel.GH_Document document)
        {
            base.RemovedFromDocument(document);
            keyboardHook.Uninstall();
            keyboardHook.KeyDown -= new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyDown);
            keyboardHook.KeyUp -= new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyUp);
            if (full != null)
            {
                full.Close();
                full = null;
            }
        }
        public override void DocumentContextChanged(Grasshopper.Kernel.GH_Document document, Grasshopper.Kernel.GH_DocumentContext context)
        {
            base.DocumentContextChanged(document, context);
            if (context == Grasshopper.Kernel.GH_DocumentContext.Unloaded)
            {

                keyboardHook.Uninstall();
                keyboardHook.KeyDown -= new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyDown);
                keyboardHook.KeyUp -= new RamGecTools.KeyboardHook.KeyboardHookCallback(keyboardHook_KeyUp);
                if (full != null)
                {
                    full.Close();
                    full = null;
                }
            }
        }
        public struct parameter
        {
            public int wL, wA, wC, Alpha, Neo, hmAlpha, dcmDist, wT, wT2;
        }
        delegate void updateParam();
        updateParam __UPDATE = () => { };
        private parameter _param;
        private double refArea = 0, area = 0;
        enum modelType
        {
            wire, membrane
        }
        enum materialChoice
        {
            DCM, L2G, CLM, SCP, DHM, SV, NH, Cotan, MKM
        }
        private materialChoice mat;
        private modelType type;

        private void __rebuildControlPanel()
        {
            controlPanel.clearSliders();
            switch (mat)
            {
                case materialChoice.DCM:
                    controlPanel.addSlider(0, 2, 100, _param.dcmDist, "dist");
                    controlPanel.listSlider[0].Converter = (v) => v * 0.02;
                    __UPDATE = () =>
                    {
                        if (mat == materialChoice.DCM)
                        {
                            Kapybara3D.Materials.leastSquaresMaterial ls = _material as Kapybara3D.Materials.leastSquaresMaterial;
                        }
                    };
                    break;
                case materialChoice.SCP:
                    controlPanel.addSlider(0, 2, 99, _param.hmAlpha, "Alpha");
                    controlPanel.listSlider[0].Converter = (v) => v * 0.02;
                    __UPDATE = () =>
                    {
                        if (mat == materialChoice.SCP)
                        {
                            Kapybara3D.Materials.conformalMaterial cm = _material as Kapybara3D.Materials.conformalMaterial;
                            cm.refArea = refArea * controlPanel.listSlider[0].value;
                            _param.hmAlpha = controlPanel.listSlider[0].originalValue;
                        }
                    }; break;
                case materialChoice.DHM:
                    __UPDATE = () => { };

                    break;
                case materialChoice.CLM:
                    controlPanel.addSlider(0, 2, 100, _param.wL, "wL");
                    controlPanel.addSlider(0, 2, 100, _param.wA, "wA");
                    controlPanel.addSlider(0, 2, 100, _param.wC, "wC");
                    controlPanel.addSlider(0, 2, 100, _param.wT, "t");
                    controlPanel.listSlider[0].Converter = (v) => v * 0.01;
                    controlPanel.listSlider[1].Converter = (v) => v * 0.01;
                    controlPanel.listSlider[2].Converter = (v) => v * 0.01;
                    controlPanel.listSlider[3].Converter = (v) => v / 50d + 1.0;
                    __UPDATE = () =>
                    {
                        if (mat == materialChoice.CLM)
                        {
                            Kapybara3D.Materials.clarenzMaterial cl = _material as Kapybara3D.Materials.clarenzMaterial;
                            cl.WL = controlPanel.listSlider[0].value;
                            cl.WA = controlPanel.listSlider[1].value;
                            cl.WC = controlPanel.listSlider[2].value;
                            cl.T = controlPanel.listSlider[3].value;
                            _param.wL = controlPanel.listSlider[0].originalValue;
                            _param.wA = controlPanel.listSlider[1].originalValue;
                            _param.wC = controlPanel.listSlider[2].originalValue;
                            _param.wT = controlPanel.listSlider[3].originalValue;
                        }
                    };
                    break;
                case materialChoice.MKM:
                    controlPanel.addSlider(0, 2, 100, _param.wT2, "t");
                    controlPanel.listSlider[0].Converter = (v) => v / 50d + 1.0;
                    __UPDATE = () =>
                    {
                        if (mat == materialChoice.MKM)
                        {
                            Kapybara3D.Materials.mikityMaterial cl = _material as Kapybara3D.Materials.mikityMaterial;
                            cl.T = controlPanel.listSlider[0].value;
                            _param.wT2 = controlPanel.listSlider[0].originalValue;
                        }
                    };
                    break;
                case materialChoice.NH:
                    controlPanel.addSlider(0, 2, 500, _param.Neo, "K");
                    controlPanel.listSlider[0].Converter = (v) => v * 0.1;
                    __UPDATE = () =>
                    {
                        if (mat == materialChoice.NH)
                        {
                            Kapybara3D.Materials.neoHookeanMaterial nH = _material as Kapybara3D.Materials.neoHookeanMaterial;
                            nH.mu1 = 0.1;
                            nH.K = controlPanel.listSlider[0].value;
                            _param.Neo = controlPanel.listSlider[0].originalValue;
                        }
                    };
                    break;
                case materialChoice.L2G:
                    __UPDATE = () => { };
                    break;
                case materialChoice.SV:
                    controlPanel.addSlider(1, 2, 99, _param.Alpha, "Alpha");
                    controlPanel.listSlider[0].Converter = (v) => v * 0.01;
                    __UPDATE = () =>
                    {
                        if (mat == materialChoice.SV)
                        {
                            Kapybara3D.Materials.stVenantMaterial sv = _material as Kapybara3D.Materials.stVenantMaterial;
                            sv.Alpha = controlPanel.listSlider[0].value;
                            _param.Alpha = controlPanel.listSlider[0].originalValue;
                        }
                    }; break;
                case materialChoice.Cotan:
                    __UPDATE = () => { };
                    break;

            }
        }
        enum initialGuess { prj, dhm1, dhm2, dhm3, dcm, scpl, scps };
        initialGuess InitialGuess = initialGuess.prj;
        private bool assignChecked(bool flag, System.Windows.Forms.ToolStripMenuItem m)
        {
            if (flag)
            {
                m.CheckState = System.Windows.Forms.CheckState.Checked;
            }
            else
            {
                m.CheckState = System.Windows.Forms.CheckState.Unchecked;
            }
            return flag;
        }
        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            __gm1 = Menu_AppendItem(menu, "Go?", Menu_GoClicked);
            __gm2 = Menu_AppendItem(menu, "Show Parameters", Menu_ShowParam);
            Menu_AppendSeparator(menu);
            System.Windows.Forms.ToolStripMenuItem tt = Menu_AppendItem(menu, "Initial guess");
            __ig1 = Menu_AppendItem(tt.DropDown, "Projection", Menu_InitialClicked);
            System.Windows.Forms.ToolStripMenuItem ttt = Menu_AppendItem(tt.DropDown, "DHM");
            __ig21 = Menu_AppendItem(ttt.DropDown, "DHM (Projection)", Menu_InitialClicked);
            __ig22 = Menu_AppendItem(ttt.DropDown, "DHM (Circle)", Menu_InitialClicked);
            __ig23 = Menu_AppendItem(ttt.DropDown, "DHM (Rectangle)", Menu_InitialClicked);
            __ig3 = Menu_AppendItem(tt.DropDown, "DCM", Menu_InitialClicked);
            //__ig4 = Menu_AppendItem(tt.DropDown, "SCPL", Menu_InitialClicked);
            __ig5 = Menu_AppendItem(tt.DropDown, "SCPS", Menu_InitialClicked);
            tt = Menu_AppendItem(menu, "Optional features");
            __vf1 = Menu_AppendItem(tt.DropDown, "Show base vectors?", Menu_VisualClicked);
            __vf2 = Menu_AppendItem(tt.DropDown, "Show integrating points?", Menu_VisualClicked);
            __vf3 = Menu_AppendItem(tt.DropDown, "Show eigen vectors?", Menu_VisualClicked);
            __vf4 = Menu_AppendItem(tt.DropDown, "Show conformality?", Menu_VisualClicked);
            tt.DropDownItems.Add(__vf1);
            tt.DropDownItems.Add(__vf2);
            tt.DropDownItems.Add(__vf3);
            tt.DropDownItems.Add(__vf4);
            tt = Menu_AppendItem(menu, "Boundary Shapes");
            __bm1 = Menu_AppendItem(menu, "Boundary:Projection", Menu_BoundaryClicked);
            __bm2 = Menu_AppendItem(menu, "Boundary:Circle", Menu_BoundaryClicked);
            __bm3 = Menu_AppendItem(menu, "Boundary:Rectangle", Menu_BoundaryClicked);
            tt.DropDownItems.Add(__bm1);
            tt.DropDownItems.Add(__bm2);
            tt.DropDownItems.Add(__bm3);
            Menu_AppendSeparator(menu);
            assignChecked(InitialGuess == initialGuess.prj, __ig1);
            assignChecked(InitialGuess == initialGuess.dhm1, __ig21);
            assignChecked(InitialGuess == initialGuess.dhm2, __ig22);
            assignChecked(InitialGuess == initialGuess.dhm3, __ig23);
            assignChecked(InitialGuess == initialGuess.dcm, __ig3);
            assignChecked(InitialGuess == initialGuess.scps, __ig5);
            if (!assignChecked(_go, __gm1)) Clock = -1;

            assignChecked(controlPanel.Visibility == System.Windows.Visibility.Visible, __gm2);
            assignChecked(_base, __vf1);
            assignChecked(_eigen, __vf2);
            assignChecked(_intPoint, __vf3);
            assignChecked(_conformal, __vf4);

        }
        private string switches()
        {
            string output="";
            if (_drift1)
            {
                output += "Drift:1\n";
            }else if(_drift2)
            {
                output += "Drift:2\n";
            }else{
                output+="Drift:None\n";
            }

            return output;
        }
        private bool _go = false;
        private bool _base = false;
        private bool _conformal = false;
        private bool _eigen = false;
        private bool _intPoint = true;
        private bool _isFixedBoundary;
        private bool _drift1 = true, _drift2 = false;
        private bool _fixFlip = true, _RF = true;
        private int shift = 0;
        private Kapybara3D.Materials.iMaterial _material = new Kapybara3D.Materials.stVenantMaterial();
        private void Menu_ShowParam(Object sender, EventArgs e)
        {
            if (sender == __gm2)
            {
                if (controlPanel.Visibility == System.Windows.Visibility.Visible)
                {
                    controlPanel.Hide();
                }
                else
                {
                    controlPanel.Show();
                }
            }
        }

        private void Menu_BoundaryClicked(Object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem _m = sender as System.Windows.Forms.ToolStripMenuItem;

            if (_m == __bm1)
            {
                projBoundary();
            }
            if (_m == __bm2)
            {
                circleBoundary();
            }
            if (_m == __bm3)
            {
                rectangleBoundary();
            }
        }
        private bool switchState(System.Windows.Forms.ToolStripMenuItem _m, ref bool flag)
        {
            if (_m.CheckState == System.Windows.Forms.CheckState.Checked)
            {
                _m.CheckState = System.Windows.Forms.CheckState.Unchecked;
                flag = false;
            }
            else if (_m.CheckState == System.Windows.Forms.CheckState.Unchecked)
            {
                _m.CheckState = System.Windows.Forms.CheckState.Checked;
                flag = true;
            }
            return flag;
        }
        private void Menu_VisualClicked(Object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem _m = sender as System.Windows.Forms.ToolStripMenuItem;
            if (_m == __vf1) switchState(_m, ref _base);
            if (_m == __vf2) switchState(_m, ref _intPoint);
            if (_m == __vf3)
            {
                if (switchState(_m, ref _eigen)) { _conformal = false; }
            }
            if (_m == __vf4)
            {
                if (switchState(_m, ref _conformal)) { _eigen = false; }
            }
            reset = true;
            this.ExpireSolution(true);

        }

        private void Menu_GoClicked(Object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem _m = sender as System.Windows.Forms.ToolStripMenuItem;
            if (_m == __gm1)
            {
                if (switchState(_m, ref _go))
                {
                    timer.Enabled = true;
                    setupMaterial(mat);
                }
                else
                {
                    timer.Enabled = false;
                    Clock = -1;
                }
            }
        }
       /* private void selectMaterial(System.Windows.Forms.ToolStripMenuItem _m)
        {
            foreach (var i in menuitems)
            {
                if (i == _m)
                {
                    i.CheckState = System.Windows.Forms.CheckState.Checked;
                }
                else
                {
                    i.CheckState = System.Windows.Forms.CheckState.Unchecked;
                }
            }
        }*/
        private void setupMaterial(materialChoice mC)
        {
            switch (mC)
            {
                case materialChoice.DHM:
                    _material = new Kapybara3D.Materials.harmonicMaterial();
                    mat = materialChoice.DHM;
                    gS.setMaterial(_material.getMaterial());
                    _isFixedBoundary = true;
                    type = modelType.membrane;
                    break;
                case materialChoice.L2G:
                    _material = new Kapybara3D.Materials.sanderMaterial();
                    mat = materialChoice.L2G;
                    gS.setMaterial(_material.getMaterial());
                    _isFixedBoundary = true;
                    type = modelType.membrane;
                    break;
                case materialChoice.DCM:
                    mat = materialChoice.DCM;
                    _material = new Kapybara3D.Materials.leastSquaresMaterial();
                    gS.setMaterial(_material.getMaterial());
                    _isFixedBoundary = false;
                    type = modelType.membrane;
                    break;
                case materialChoice.SCP:
                    _material = new Kapybara3D.Materials.conformalMaterial();
                    mat = materialChoice.SCP;
                    gS.setMaterial(_material.getMaterial());
                    _isFixedBoundary = false;
                    type = modelType.membrane;
                    break;
                case materialChoice.CLM:
                    _material = new Kapybara3D.Materials.clarenzMaterial();
                    mat = materialChoice.CLM;
                    gS.setMaterial(_material.getMaterial());
                    _isFixedBoundary = false;
                    type = modelType.membrane;
                    break;
                case materialChoice.MKM:
                    _material = new Kapybara3D.Materials.mikityMaterial();
                    mat = materialChoice.MKM;
                    gS.setMaterial(_material.getMaterial());
                    _isFixedBoundary = false;
                    type = modelType.membrane;
                    break;
                case materialChoice.Cotan:
                    mat = materialChoice.Cotan;
                    _isFixedBoundary = true;
                    type = modelType.wire;
                    _wireMaterial = wL2;
                    break;
                case materialChoice.SV:
                    _material = new Kapybara3D.Materials.stVenantMaterial();
                    mat = materialChoice.SV;
                    gS.setMaterial(_material.getMaterial());
                    _isFixedBoundary = false;
                    type = modelType.membrane;
                    break;
                case materialChoice.NH:
                    _material = new Kapybara3D.Materials.neoHookeanMaterial();
                    mat = materialChoice.NH;
                    gS.setMaterial(_material.getMaterial());
                    _isFixedBoundary = false;
                    type = modelType.membrane;

                    break;
            }
            __rebuildControlPanel();

        }
        private void selectMaterial(string name)
        {
            switch (name)
            {
                case "DHM":
                    setupMaterial(materialChoice.DHM);
                    break;
                case "L2G":
                    setupMaterial(materialChoice.L2G);
                    break;
                case "DCM":
                    setupMaterial(materialChoice.DCM);
                    break;
                case "SCP":
                    setupMaterial(materialChoice.SCP);
                    break;
                case "CLM":
                    setupMaterial(materialChoice.CLM);
                    break;
                case "Cotan":
                    setupMaterial(materialChoice.Cotan);
                    break;
                case "NH":
                    setupMaterial(materialChoice.NH);
                    break;
                default:
                    return;
            }
            //reset = true;
            //this.ExpireSolution(true);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            _go = false;//default value;
            reader.TryGetBoolean("Go?", ref _go);
            _base = false;
            reader.TryGetBoolean("Show Base Vectors?", ref _base);
            _eigen = true;
            reader.TryGetBoolean("Show Eigen Vectors?", ref _eigen);
            _intPoint = true;
            reader.TryGetBoolean("Show Integrating Points?", ref _intPoint);
            return base.Read(reader);
        }
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetBoolean("Go?", false);
            writer.SetBoolean("Show Base Vectors?", _base);
            writer.SetBoolean("Show Eigen Vectors?", _eigen);
            writer.SetBoolean("Show Integrating Points?", _intPoint);
            return base.Write(writer);
        }

        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh/Surface", "m", "Mesh/Surface", Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
/*            pManager.AddIntegerParameter("iteration", "out", "out", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("a/A", "out", "out", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("maxC", "maxC", "maxC", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("minC", "minC", "minC", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("integral", "out4", "out4", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("area", "out5", "out5", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddTextParameter("switches", "switches", "switches", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddNumberParameter("damping", "damping", "damping", Grasshopper.Kernel.GH_ParamAccess.item);*/
        }
        public override void BakeGeometry(Rhino.RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, List<Guid> obj_ids)
        {
            if (this.BKGT != null)
            {
                this.BKGT(doc, att, obj_ids);
            }
        }
        public override void DrawViewportWires(Grasshopper.Kernel.IGH_PreviewArgs args)
        {
            if (this.DVPW != null)
            {
                this.DVPW(args);
            }
            base.DrawViewportWires(args);
        }
        List<Kapybara3D.Elements.managedElement> elemList = new List<Kapybara3D.Elements.managedElement>();
        List<Kapybara3D.Elements.managedElement> elemList2 = new List<Kapybara3D.Elements.managedElement>();
        int Clock = -1;
        Rhino.Geometry.Mesh m;
        Rhino.Geometry.Mesh triMesh = new Rhino.Geometry.Mesh();
        Rhino.Geometry.NurbsSurface ns;
        List<Rhino.Geometry.Point3d> iP = new List<Rhino.Geometry.Point3d>();
        List<Rhino.Geometry.Line> bV = new List<Rhino.Geometry.Line>();
        List<Rhino.Geometry.Line> eVT = new List<Rhino.Geometry.Line>();
        List<Rhino.Geometry.Line> reF = new List<Rhino.Geometry.Line>();
        List<Rhino.Geometry.Line> eVC = new List<Rhino.Geometry.Line>();
        List<Rhino.Geometry.Circle> cfm = new List<Rhino.Geometry.Circle>();
        int nParticles = 0;
        Kapybara3D.Objects.generalSpring gS = new Kapybara3D.Objects.generalSpring();
        Kapybara3D.Objects.generalSpring cN = new Kapybara3D.Objects.generalSpring();
        //double _wA = 1.0, _wL = 1.0, _wC = 1.0;
        int nElements = 0;
        double[,] pos, refPos, Re;
        DoubleArray vel, vel2,force, acc;
        List<double[]>[] star;
        SparseDoubleArray hessEd, hessA;
        SparseDoubleArray metric, invMetric;
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;
        List<face> faces = new List<face>();
        private void initialize(Rhino.Geometry.Mesh _m, Rhino.Geometry.Mesh _m2)
        {
            if (internalState == state.mesh)
            {
                elemList.Clear();
            }
            elemList2.Clear();
            faces.Clear();
            triMesh.Vertices.Clear();
            triMesh.Faces.Clear();
            for (int i = 0; i < _m.Vertices.Count; i++)
            {
                triMesh.Vertices.Add(_m.Vertices[i]);
            }
            foreach (Rhino.Geometry.MeshFace F in _m.Faces)
            {
                if (F.IsQuad)
                {
                    if (internalState == state.mesh)
                    {
                        elemList.Add(new Kapybara3D.Elements.I4D2(new int[4] { F.A, F.B, F.D, F.C }));
                    }
                    faces.Add(new face(F.A, F.B, F.C));
                    faces.Add(new face(F.A, F.C, F.D));
                    triMesh.Faces.AddFace(new Rhino.Geometry.MeshFace(F.A, F.B, F.C));
                    triMesh.Faces.AddFace(new Rhino.Geometry.MeshFace(F.A, F.C, F.D));
                    elemList2.AddRange(faces[faces.Count - 1].edges);
                    elemList2.AddRange(faces[faces.Count - 2].edges);
                }
                else if (F.IsTriangle)
                {
                    if (internalState == state.mesh)
                    {
                        elemList.Add(new Kapybara3D.Elements.S3D2(new int[3] { F.A, F.B, F.C }));
                    }
                    faces.Add(new face(F.A, F.B, F.C));
                    triMesh.Faces.AddFace(new Rhino.Geometry.MeshFace(F.A, F.B, F.C));
                    elemList2.AddRange(faces[faces.Count - 1].edges);
                }
            }
            nElements = elemList.Count();
            nParticles = _m.Vertices.Count();
            gS.Clear();
            gS.AddRange(elemList);
            gS.initialize(nParticles);
            cN.Clear();
            foreach (face f in faces)
            {
                cN.AddRange(f.edges);
            }
            cN.initialize(nParticles);
            force = DoubleArray.Zeros(nParticles * 3);
            acc = DoubleArray.Zeros(nParticles * 3);
            Re = new double[nParticles, 3];
            hessEd = new SparseDoubleArray(nParticles * 2, nParticles * 2);
            hessA = new SparseDoubleArray(nParticles * 2, nParticles * 2);

            vel = DoubleArray.Zeros(nParticles * 3);
            vel2 = DoubleArray.Zeros(nParticles * 3);
            invMetric = SparseDoubleArray.Zeros(nParticles * 3, nParticles * 3);
            metric = SparseDoubleArray.Zeros(nParticles * 3, nParticles * 3);
            pos = new double[nParticles, 3];
            refPos = new double[nParticles, 3];
            star = new List<double[]>[nParticles];
            for (int i = 0; i < nParticles; i++)
            {
                star[i] = new List<double[]>();
            }
            for (int i = 0; i < nParticles; i++)
            {
                pos[i, 0] = _m2.Vertices[i].X;
                pos[i, 1] = _m2.Vertices[i].Y;
                pos[i, 2] = _m2.Vertices[i].Z;
                refPos[i, 0] = _m.Vertices[i].X;
                refPos[i, 1] = _m.Vertices[i].Y;
                refPos[i, 2] = _m.Vertices[i].Z;
            }
            getBoundary(_m2);
        }
        struct boundaryVertex
        {
            public int index;
            public double param;
        }

        struct face
        {
            public int _A, _B, _C;
            public Kapybara3D.Elements.S2D1[] edges;
            public Kapybara3D.Materials.formFindingMaterial[] ffM;
            public List<int>[] oneRings;
            public face(int A, int B, int C)
            {
                oneRings = new List<int>[3];
                for (int i = 0; i < 3; i++)
                {
                    oneRings[i] = new List<int>();
                }
                _A = A; _B = B; _C = C;
                edges = new Kapybara3D.Elements.S2D1[3];
                ffM = new Kapybara3D.Materials.formFindingMaterial[3];
                int[,] ee = new int[3, 2] { { A, B }, { B, C }, { C, A } };
                for (int i = 0; i < 3; i++)
                {
                    edges[i] = new Kapybara3D.Elements.S2D1(new int[2] { ee[i, 0], ee[i, 1] });
                    ffM[i] = new Kapybara3D.Materials.formFindingMaterial();
                    edges[i].setMaterial(ffM[i].getMaterial());
                    ffM[i].Power = 2;
                    ffM[i].Weight = 1.0;
                }
            }
        }
        delegate void wireMaterial(face f, double[,] _pos, double[,] Pos);
        static wireMaterial HookSpring = (f, _pos, Pos) =>
        {
            double px = _pos[f._A, 0];
            double py = _pos[f._A, 1];
            double pz = _pos[f._A, 2];
            double qx = _pos[f._B, 0];
            double qy = _pos[f._B, 1];
            double qz = _pos[f._B, 2];
            double rx = _pos[f._C, 0];
            double ry = _pos[f._C, 1];
            double rz = _pos[f._C, 2];
            double g1x = px - qx;
            double g1y = py - qy;
            double g1z = pz - qz;
            double g2x = qx - rx;
            double g2y = qy - ry;
            double g2z = qz - rz;
            double g3x = rx - px;
            double g3y = ry - py;
            double g3z = rz - pz;
            double L1 = Math.Sqrt(g1x * g1x + g1y * g1y + g1z * g1z);
            double L2 = Math.Sqrt(g2x * g2x + g2y * g2y + g2z * g2z);
            double L3 = Math.Sqrt(g3x * g3x + g3y * g3y + g3z * g3z);
            double Px = Pos[f._A, 0];
            double Py = Pos[f._A, 1];
            double Qx = Pos[f._B, 0];
            double Qy = Pos[f._B, 1];
            double Rx = Pos[f._C, 0];
            double Ry = Pos[f._C, 1];
            double G1x = Px - Qx;
            double G1y = Py - Qy;
            double G2x = Qx - Rx;
            double G2y = Qy - Ry;
            double G3x = Rx - Px;
            double G3y = Ry - Py;
            double l1 = Math.Sqrt(G1x * G1x + G1y * G1y);
            double l2 = Math.Sqrt(G2x * G2x + G2y * G2y);
            double l3 = Math.Sqrt(G3x * G3x + G3y * G3y);
            f.ffM[0].Weight = (l1 - L1) / l1;
            f.ffM[1].Weight = (l2 - L2) / l2;
            f.ffM[2].Weight = (l3 - L3) / l3;
        };
        static wireMaterial FloaterwL2 = (f, _pos, Pos) =>
        {
            double px = _pos[f._A, 0];
            double py = _pos[f._A, 1];
            double pz = _pos[f._A, 2];
            double qx = _pos[f._B, 0];
            double qy = _pos[f._B, 1];
            double qz = _pos[f._B, 2];
            double rx = _pos[f._C, 0];
            double ry = _pos[f._C, 1];
            double rz = _pos[f._C, 2];
            double g1x = px - qx;
            double g1y = py - qy;
            double g1z = pz - qz;
            double g2x = qx - rx;
            double g2y = qy - ry;
            double g2z = qz - rz;
            double g3x = rx - px;
            double g3y = ry - py;
            double g3z = rz - pz;
            double L1 = Math.Sqrt(g1x * g1x + g1y * g1y + g1z * g1z);
            double L2 = Math.Sqrt(g2x * g2x + g2y * g2y + g2z * g2z);
            double L3 = Math.Sqrt(g3x * g3x + g3y * g3y + g3z * g3z);
            f.ffM[0].Weight = 1.0 / (Math.Pow(L1, FloaterPower));
            f.ffM[1].Weight = 1.0 / (Math.Pow(L2, FloaterPower));
            f.ffM[2].Weight = 1.0 / (Math.Pow(L3, FloaterPower));
        };
        static double FloaterPower = 1.0;
        static wireMaterial dbe = (f, _pos, Pos) =>
        {
            double px = _pos[f._A, 0];
            double py = _pos[f._A, 1];
            double pz = _pos[f._A, 2];
            double qx = _pos[f._B, 0];
            double qy = _pos[f._B, 1];
            double qz = _pos[f._B, 2];
            double rx = _pos[f._C, 0];
            double ry = _pos[f._C, 1];
            double rz = _pos[f._C, 2];
            double g1x = px - qx;
            double g1y = py - qy;
            double g1z = pz - qz;
            double g2x = qx - rx;
            double g2y = qy - ry;
            double g2z = qz - rz;
            double g3x = rx - px;
            double g3y = ry - py;
            double g3z = rz - pz;
            double L1 = Math.Sqrt(g1x * g1x + g1y * g1y + g1z * g1z);
            double L2 = Math.Sqrt(g2x * g2x + g2y * g2y + g2z * g2z);
            double L3 = Math.Sqrt(g3x * g3x + g3y * g3y + g3z * g3z);
            double Px = Pos[f._A, 0];
            double Py = Pos[f._A, 1];
            double Qx = Pos[f._B, 0];
            double Qy = Pos[f._B, 1];
            double Rx = Pos[f._C, 0];
            double Ry = Pos[f._C, 1];
            double G1x = Px - Qx;
            double G1y = Py - Qy;
            double G2x = Qx - Rx;
            double G2y = Qy - Ry;
            double G3x = Rx - Px;
            double G3y = Ry - Py;
            double l1 = Math.Sqrt(G1x * G1x + G1y * G1y);
            double l2 = Math.Sqrt(G2x * G2x + G2y * G2y);
            double l3 = Math.Sqrt(G3x * G3x + G3y * G3y);
            f.ffM[0].Weight = (l1 * l1 - L1 * L1) / (L1 * L1) / 2 * paramDBE;
            f.ffM[1].Weight = (l2 * l2 - L2 * L2) / (L2 * L2) / 2 * paramDBE;
            f.ffM[2].Weight = (l3 * l3 - L3 * L3) / (L3 * L3) / 2 * paramDBE;
        };
        static double paramDBE=0;
        static wireMaterial L2 = (f, _pos, Pos) =>
        {
            f.ffM[0].Weight = 1.0;
            f.ffM[1].Weight = 1.0;
            f.ffM[2].Weight = 1.0;
        };
        static wireMaterial wL2 = (f, _pos, Pos) =>
        {
            double px = _pos[f._A, 0];
            double py = _pos[f._A, 1];
            double pz = _pos[f._A, 2];
            double qx = _pos[f._B, 0];
            double qy = _pos[f._B, 1];
            double qz = _pos[f._B, 2];
            double rx = _pos[f._C, 0];
            double ry = _pos[f._C, 1];
            double rz = _pos[f._C, 2];
            double g1x = px - rx;
            double g1y = py - ry;
            double g1z = pz - rz;
            double g2x = qx - rx;
            double g2y = qy - ry;
            double g2z = qz - rz;
            f.ffM[0].Weight = cot(g1x, g1y, g1z, g2x, g2y, g2z);
            g1x = qx - px;
            g1y = qy - py;
            g1z = qz - pz;
            g2x = rx - px;
            g2y = ry - py;
            g2z = rz - pz;
            f.ffM[1].Weight = cot(g1x, g1y, g1z, g2x, g2y, g2z);
            g1x = rx - qx;
            g1y = ry - qy;
            g1z = rz - qz;
            g2x = px - qx;
            g2y = py - qy;
            g2z = pz - qz;
            f.ffM[2].Weight = cot(g1x, g1y, g1z, g2x, g2y, g2z);

        };
        static double intrinsicAlpha = 0.5;
        static wireMaterial intrinsic = (f, _pos, Pos) =>
        {
            double px = _pos[f._A, 0];
            double py = _pos[f._A, 1];
            double pz = _pos[f._A, 2];
            double qx = _pos[f._B, 0];
            double qy = _pos[f._B, 1];
            double qz = _pos[f._B, 2];
            double rx = _pos[f._C, 0];
            double ry = _pos[f._C, 1];
            double rz = _pos[f._C, 2];
            double u = 1.0 - intrinsicAlpha, v = intrinsicAlpha;
            double g1x = px - rx;
            double g1y = py - ry;
            double g1z = pz - rz;
            double g2x = qx - rx;
            double g2y = qy - ry;
            double g2z = qz - rz;
            f.ffM[0].Weight = 2 * u * cot(g1x, g1y, g1z, g2x, g2y, g2z);
            g1x = qx - px;
            g1y = qy - py;
            g1z = qz - pz;
            g2x = rx - px;
            g2y = ry - py;
            g2z = rz - pz;
            f.ffM[1].Weight = 2 * u * cot(g1x, g1y, g1z, g2x, g2y, g2z);
            g1x = rx - qx;
            g1y = ry - qy;
            g1z = rz - qz;
            g2x = px - qx;
            g2y = py - qy;
            g2z = pz - qz;
            f.ffM[2].Weight = 2 * u * cot(g1x, g1y, g1z, g2x, g2y, g2z);

            g1x = qx - px;
            g1y = qy - py;
            g1z = qz - pz;
            g2x = rx - px;
            g2y = ry - py;
            g2z = rz - pz;
            double L = g1x * g1x + g1y * g1y + g1z * g1z;
            f.ffM[0].Weight += v * cot(g1x, g1y, g1z, g2x, g2y, g2z) / L;
            g1x = px - qx;
            g1y = py - qy;
            g1z = pz - qz;
            g2x = rx - qx;
            g2y = ry - qy;
            g2z = rz - qz;
            f.ffM[0].Weight += v * cot(g1x, g1y, g1z, g2x, g2y, g2z) / L;
            g1x = px - qx;
            g1y = py - qy;
            g1z = pz - qz;
            g2x = rx - qx;
            g2y = ry - qy;
            g2z = rz - qz;
            L = g2x * g2x + g2y * g2y + g2z * g2z;
            f.ffM[1].Weight += v * cot(g1x, g1y, g1z, g2x, g2y, g2z) / L;
            g1x = px - rx;
            g1y = py - ry;
            g1z = pz - rz;
            g2x = qx - rx;
            g2y = qy - ry;
            g2z = qz - rz;
            f.ffM[1].Weight += v * cot(g1x, g1y, g1z, g2x, g2y, g2z) / L;
            g1x = px - rx;
            g1y = py - ry;
            g1z = pz - rz;
            g2x = qx - rx;
            g2y = qy - ry;
            g2z = qz - rz;
            L = g1x * g1x + g1y * g1y + g1z * g1z;
            f.ffM[2].Weight += v * cot(g1x, g1y, g1z, g2x, g2y, g2z) / L;
            g1x = qx - px;
            g1y = qy - py;
            g1z = qz - pz;
            g2x = rx - px;
            g2y = ry - py;
            g2z = rz - pz;
            f.ffM[2].Weight += v * cot(g1x, g1y, g1z, g2x, g2y, g2z) / L;
        };

        static double cot(double g1x, double g1y, double g1z, double g2x, double g2y, double g2z)
        {
            double innerproduct = g1x * g2x + g1y * g2y + g1z * g2z;
            double ox = g1y * g2z - g1z * g2y;
            double oy = g1z * g2x - g1x * g2z;
            double oz = g1x * g2y - g1y * g2x;
            double outerproduct = Math.Sqrt(ox * ox + oy * oy + oz * oz);
            double val = innerproduct / outerproduct;
            return val;
        }
        wireMaterial _wireMaterial = L2;
        private boundaryVertex[] __boundary;
        public void getBoundary(Rhino.Geometry.Mesh _m)
        {
            Rhino.Geometry.Polyline boundary = _m.GetNakedEdges()[0];

            double[] boundaryParam = new double[boundary.Count];
            double totalLength = 0;
            for (int i = 0; i < boundary.Count - 1; i++)
            {
                totalLength += (boundary[i + 1] - boundary[i]).Length;
            }
            double currentLength = 0;
            boundaryParam[0] = 0;
            for (int i = 1; i < boundary.Count; i++)
            {
                currentLength += (boundary[i] - boundary[i - 1]).Length;
                boundaryParam[i] = currentLength;
            }
            __boundary = new boundaryVertex[boundary.Count - 1];
            for (int i = 0; i < boundary.Count - 1; i++)
            {
                for (int j = 0; j < _m.Vertices.Count; j++)
                {
                    if (boundary[i] == _m.Vertices[j])
                    {
                        __boundary[i].index = j;
                        __boundary[i].param = boundaryParam[i];
                    }
                }
            }
        }
        bool isInitialized = false;
        private void computeInitialGuess(double[,] rX, double[,] x, bool flag)
        {
            if (isInitialized)
            {
                if (InitialGuess == initialGuess.prj)
                {
                    initialShapePrj(rX, x);
                }
                if (InitialGuess == initialGuess.dhm1)
                {
                    initialShapeDhm(rX, x, 1, flag);
                }
                if (InitialGuess == initialGuess.dhm2)
                {
                    initialShapeDhm(rX, x, 2, flag);
                }
                if (InitialGuess == initialGuess.dhm3)
                {
                    initialShapeDhm(rX, x, 3, flag);
                }
                if (InitialGuess == initialGuess.dcm)
                {
                    initialShapeDcm(rX, x);
                }
                if (InitialGuess == initialGuess.scpl)
                {
                    initialShapeScpl(rX, x);
                }
                if (InitialGuess == initialGuess.scps)
                {
                    initialShapeScps(rX, x);
                }
            }
        }
        private void Menu_InitialClicked(Object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem _m = sender as System.Windows.Forms.ToolStripMenuItem;
            if (Clock == -1)
            {
                if (_m == __ig1)
                {
                    InitialGuess = initialGuess.prj;
                }
                if (_m == __ig21)
                {
                    InitialGuess = initialGuess.dhm1;
                }
                if (_m == __ig22)
                {
                    InitialGuess = initialGuess.dhm2;
                }
                if (_m == __ig23)
                {
                    InitialGuess = initialGuess.dhm3;
                }
                if (_m == __ig3)
                {
                    InitialGuess = initialGuess.dcm;
                }
                /*if (_m == __ig4)
                {
                    InitialGuess = initialGuess.scpl;
                }*/
                if (_m == __ig5)
                {
                    InitialGuess = initialGuess.scps;
                }
            }

            reset = true;
            ExpireSolution(true);
            //            computeInitialGuess(refPos, pos, true);
        }
        public void initialShapePrj(double[,] rX, double[,] x)
        {
            for (int i = 0; i < x.GetLength(0); i++)
            {
                x[i, 0] = rX[i, 0];
                x[i, 1] = rX[i, 1];
                x[i, 2] = 0;
            }
            _isFixedBoundary = false;
        }
        bool isBoundary(int n)
        {
            for (int i = 0; i < __boundary.Count(); i++)
            {
                if (__boundary[i].index == n)
                {
                    return true;
                }
            }
            return false;
        }
        double[] node = new double[3];
        double[,] node2 = new double[2, 3];
        double[] vals = new double[2];
        double  dt;
        List<Guid> fixedPointGuids = null;
        void deleteFixedPoints()
        {
            if (fixedPointGuids != null)
            {
                foreach (Guid g in fixedPointGuids)
                {
                    Rhino.RhinoDoc.ActiveDoc.Objects.Delete(g, true);
                }
            }
            fixedPointGuids = null;

        }
        void createFixedPoints()
        {
            if (fixedPointGuids != null)
            {
                deleteFixedPoints();
            }
            fixedPointGuids = new List<Guid>();
            for (int i = 0; i < __boundary.Count(); i++)
            {
                boundaryVertex bV = __boundary[i];
                fixedPointGuids.Add(Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pos[bV.index, 0] + Shifting, pos[bV.index, 1], 0));
            }
        }
        void projBoundary()
        {
            Rhino.RhinoDoc.ReplaceRhinoObject -= RhinoDoc_ReplaceRhinoObject;
            //find the varycentric
            if (fixedPointGuids == null || fixedPointGuids.Count() != __boundary.Count())
            {
                createFixedPoints();
            }
            double x, y, z;
            for (int i = 0; i < __boundary.Count(); i++)
            {
                x = refPos[__boundary[i].index, 0] + Shifting;
                y = refPos[__boundary[i].index, 1];
                z = 0;
                Rhino.Geometry.Point3d P = new Rhino.Geometry.Point3d(x, y, z);
                Rhino.RhinoDoc.ActiveDoc.Objects.Replace(fixedPointGuids[i], P);
            }
            Rhino.RhinoDoc.ReplaceRhinoObject += RhinoDoc_ReplaceRhinoObject;
        }

        void circleBoundary()
        {
            Rhino.RhinoDoc.ReplaceRhinoObject -= RhinoDoc_ReplaceRhinoObject;
            //find the varycentric
            double cx = 0, cy = 0;
            for (int i = 0; i < nParticles; i++)
            {
                cx += refPos[i, 0];
                cy += refPos[i, 1];
            }
            cx /= nParticles;
            cy /= nParticles;
            cx += Shifting;
            //Calculate radius
            double totalLength = __boundary[__boundary.Count() - 1].param;
            double R = __boundary[__boundary.Count() - 1].param / (2 * Math.PI);
            if (fixedPointGuids == null || fixedPointGuids.Count() != __boundary.Count())
            {
                createFixedPoints();
            }
            for (int i = 0; i < __boundary.Count(); i++)
            {
                double x, y, z;
                double theta = 2 * Math.PI * ((double)i / __boundary.Count());
                x = cx + R * Math.Cos(theta);
                y = cy + R * Math.Sin(theta);
                z = 0;
                Rhino.Geometry.Point3d P = new Rhino.Geometry.Point3d(x, y, z);
                Rhino.RhinoDoc.ActiveDoc.Objects.Replace(fixedPointGuids[i], P);
            }
            Rhino.RhinoDoc.ReplaceRhinoObject += RhinoDoc_ReplaceRhinoObject;
        }

        void RhinoDoc_ReplaceRhinoObject(object sender, Rhino.DocObjects.RhinoReplaceObjectEventArgs e)
        {
            if (Clock == -1)
            {
                if (isInitialized && fixedPointGuids != null)
                {
                    for (int i = 0; i < __boundary.Count(); i++)
                    {
                        Guid gi = fixedPointGuids[i];
                        if (e.ObjectId.CompareTo(gi) == 0)
                        {
                            var PO = e.NewRhinoObject as Rhino.DocObjects.PointObject;
                            var P = PO.PointGeometry;
                            pos[__boundary[i].index, 0] = P.Location.X - Shifting;
                            pos[__boundary[i].index, 1] = P.Location.Y;
                            computeInitialGuess(refPos, pos, false);
                            __update();
                            this.ExpirePreview(true);
                            return;
                        }
                    }
                }
            }
        }
        void rectangleBoundary()
        {
            Rhino.RhinoDoc.ReplaceRhinoObject -= RhinoDoc_ReplaceRhinoObject;
            int _S = shift % (__boundary.Count());

            //find the varycentric
            double cx = 0, cy = 0;
            for (int i = 0; i < nParticles; i++)
            {
                cx += refPos[i, 0];
                cy += refPos[i, 1];
            }
            cx /= nParticles;
            cy /= nParticles;
            cx += Shifting;
            //Calculate radius
            double totalLength = __boundary[__boundary.Count() - 1].param;
            double R = __boundary[__boundary.Count() - 1].param / 4;
            if (fixedPointGuids == null || fixedPointGuids.Count() != __boundary.Count())
            {
                createFixedPoints();
            }
            for (int i = 0; i < (int)__boundary.Count() / 4; i++)
            {
                double x, y, z;
                int range = (int)__boundary.Count() / 4 - 0;
                x = cx - R / 2;
                y = cy - R / 2 + (double)i * R / range;
                z = 0;
                Rhino.Geometry.Point3d P = new Rhino.Geometry.Point3d(x, y, z);
                Rhino.RhinoDoc.ActiveDoc.Objects.Replace(fixedPointGuids[(i + shift) % (__boundary.Count())], P);
            }
            for (int i = (int)__boundary.Count() / 4; i < (int)__boundary.Count() / 2; i++)
            {
                double x, y, z;
                int _i = i - (int)__boundary.Count() / 4;
                int range = ((int)__boundary.Count() / 2) - ((int)__boundary.Count() / 4);
                x = cx - R / 2 + (double)_i * R / range;
                y = cy + R / 2;
                z = 0;
                Rhino.Geometry.Point3d P = new Rhino.Geometry.Point3d(x, y, z);
                Rhino.RhinoDoc.ActiveDoc.Objects.Replace(fixedPointGuids[(i + shift) % (__boundary.Count())], P);
            }
            for (int i = (int)__boundary.Count() / 2; i < (int)__boundary.Count() * 3 / 4; i++)
            {
                double x, y, z;
                int _i = i - (int)__boundary.Count() / 2;
                int range = ((int)__boundary.Count() * 3 / 4) - ((int)__boundary.Count() / 2);
                x = cx + R / 2;
                y = cy + R / 2 - (double)_i * R / range;
                z = 0;
                Rhino.Geometry.Point3d P = new Rhino.Geometry.Point3d(x, y, z);
                Rhino.RhinoDoc.ActiveDoc.Objects.Replace(fixedPointGuids[(i + shift) % (__boundary.Count())], P);
            }
            for (int i = (int)__boundary.Count() * 3 / 4; i < (int)__boundary.Count(); i++)
            {
                int _i = i - (int)__boundary.Count() * 3 / 4;
                double x, y, z;
                int range = (int)__boundary.Count() - ((int)__boundary.Count() * 3 / 4);
                x = cx + R / 2 - (double)_i * R / range;
                y = cy - R / 2;
                z = 0;
                Rhino.Geometry.Point3d P = new Rhino.Geometry.Point3d(x, y, z);
                Rhino.RhinoDoc.ActiveDoc.Objects.Replace(fixedPointGuids[(i + shift) % (__boundary.Count())], P);
            }
            Rhino.RhinoDoc.ReplaceRhinoObject += RhinoDoc_ReplaceRhinoObject;

        }
        void updateFixedPoints()
        {
            Rhino.RhinoDoc.ReplaceRhinoObject -= RhinoDoc_ReplaceRhinoObject;
            if (__boundary.Count() == fixedPointGuids.Count())
            {
                for (int i = 0; i < __boundary.Count(); i++)
                {
                    boundaryVertex bV = __boundary[i];
                    Rhino.DocObjects.PointObject obj = (Rhino.DocObjects.PointObject)Rhino.RhinoDoc.ActiveDoc.Objects.Find(fixedPointGuids[i]);
                    pos[bV.index, 0] = obj.PointGeometry.Location.X - Shifting;
                    pos[bV.index, 1] = obj.PointGeometry.Location.Y;
                    pos[bV.index, 2] = 0;
                    if (obj.PointGeometry.Location.Z != 0)
                    {
                        Rhino.RhinoDoc.ReplaceRhinoObject -= RhinoDoc_ReplaceRhinoObject;
                        Rhino.RhinoDoc.ActiveDoc.Objects.Transform(fixedPointGuids[i], Rhino.Geometry.Transform.Translation(0, 0, -obj.PointGeometry.Location.Z), true);
                        Rhino.RhinoDoc.ReplaceRhinoObject += RhinoDoc_ReplaceRhinoObject;
                    }
                }
            }
            Rhino.RhinoDoc.ReplaceRhinoObject += RhinoDoc_ReplaceRhinoObject;
        }
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        public int P1 = 10, P2 = 20;
        public int uDim, vDim;
        public int nU, nV;
        Rhino.Geometry.Mesh initializeNurbs(Rhino.Geometry.NurbsSurface S)
        {
            double[] uKnot;
            double[] vKnot;

            nU = S.Points.CountU;
            nV = S.Points.CountV;
            int N = nU * nV;
            int uDim = S.OrderU;
            int vDim = S.OrderV;
            int uDdim = S.OrderU - 1;
            int vDdim = S.OrderV - 1;
            Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
            for (int j = 0; j < nV; j++)
            {
                for (int i = 0; i < nU; i++)
                {
                    Rhino.Geometry.ControlPoint cPoint = S.Points.GetControlPoint(i, j);
                    mesh.Vertices.Add(cPoint.Location);
                }
            }
            for (int j = 0; j < nV - 1; j++)
            {
                for (int i = 0; i < nU - 1; i++)
                {
                    int D1 = j * nU + i;
                    int D2 = j * nU + i + 1;
                    int D3 = (j + 1) * nU + i + 1;
                    int D4 = (j + 1) * nU + i;
                    mesh.Faces.AddFace(new Rhino.Geometry.MeshFace(D1, D2, D3, D4));
                }
            }

            uKnot = new double[nU - uDdim + 1 + uDdim * 2];
            vKnot = new double[nV - vDdim + 1 + vDdim * 2];
            for (int i = 0; i < uDdim; i++)
            {
                uKnot[i] = 0;
            }
            for (int i = 0; i < vDdim; i++)
            {
                vKnot[i] = 0;
            }
            for (int i = 0; i < nU - uDdim + 1; i++)
            {
                uKnot[i + uDdim] = i;
            }
            for (int i = 0; i < nV - vDdim + 1; i++)
            {
                vKnot[i + vDdim] = i;
            }
            for (int i = 0; i < uDdim; i++)
            {
                uKnot[i + nU + 1] = nU - uDdim;
            }
            for (int i = 0; i < vDdim; i++)
            {
                vKnot[i + nV + 1] = nV - vDdim;
            }
            elemList.Clear();
            for (int i = 1; i < nV - vDdim + 1; i++)
            {
                for (int j = 1; j < nU - uDdim + 1; j++)
                {
                    int[] index = new int[uDim * vDim];
                    for (int k = 0; k < vDim; k++)
                    {
                        for (int l = 0; l < uDim; l++)
                        {
                            index[k * uDim + l] = (i - 1 + k) * nU + j - 1 + l;
                        }
                    }
                    if (uDim == 2 & vDim == 2)
                    {
                        elemList.Add(new Kapybara3D.Elements.N22D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 2 & vDim == 3)
                    {
                        elemList.Add(new Kapybara3D.Elements.N23D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 3 & vDim == 2)
                    {
                        elemList.Add(new Kapybara3D.Elements.N32D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 2 & vDim == 4)
                    {
                        elemList.Add(new Kapybara3D.Elements.N24D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 4 & vDim == 2)
                    {
                        elemList.Add(new Kapybara3D.Elements.N42D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 2 & vDim == 5)
                    {
                        elemList.Add(new Kapybara3D.Elements.N25D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 5 & vDim == 2)
                    {
                        elemList.Add(new Kapybara3D.Elements.N52D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 3 & vDim == 3)
                    {
                        elemList.Add(new Kapybara3D.Elements.N33D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 3 & vDim == 4)
                    {
                        elemList.Add(new Kapybara3D.Elements.N34D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 4 & vDim == 3)
                    {
                        elemList.Add(new Kapybara3D.Elements.N43D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 3 & vDim == 5)
                    {
                        elemList.Add(new Kapybara3D.Elements.N35D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 5 & vDim == 3)
                    {
                        elemList.Add(new Kapybara3D.Elements.N53D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 4 & vDim == 4)
                    {
                        elemList.Add(new Kapybara3D.Elements.N44D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 4 & vDim == 5)
                    {
                        elemList.Add(new Kapybara3D.Elements.N45D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 5 & vDim == 4)
                    {
                        elemList.Add(new Kapybara3D.Elements.N54D2(index, j, i, uKnot, vKnot));
                    }
                    if (uDim == 5 & vDim == 5)
                    {
                        elemList.Add(new Kapybara3D.Elements.N55D2(index, j, i, uKnot, vKnot));
                    }

                }
            }

            return mesh;
        }
        void computeHessArea(SparseDoubleArray hess)
        {
            //hess.Clear();
            //hess.RemoveZeros();
            for (int i = 0; i < nParticles * 2; i++)
            {
                for (int j = 0; j < nParticles * 2; j++)
                {
                    hess[i, j] = 0;
                }
            }
            foreach (Rhino.Geometry.MeshFace t in triMesh.Faces)
            {
                double g = 0.5;
                if (t.IsTriangle)
                {
                    int A = t.A;
                    int B = t.B;
                    int C = t.C;
                    Rhino.Geometry.Vector3d AB = triMesh.Vertices[A] - triMesh.Vertices[B];
                    Rhino.Geometry.Vector3d BC = triMesh.Vertices[B] - triMesh.Vertices[C];
                    Rhino.Geometry.Vector3d N = Rhino.Geometry.Vector3d.CrossProduct(AB, BC);
                    if (N.Z > 0)
                    {
                        hess[A * 2, B * 2 + 1] += g;
                        hess[A * 2 + 1, B * 2] += -g;
                        hess[A * 2, C * 2 + 1] += -g;
                        hess[A * 2 + 1, C * 2] += g;

                        hess[B * 2, C * 2 + 1] += g;
                        hess[B * 2 + 1, C * 2] += -g;
                        hess[B * 2, A * 2 + 1] += -g;
                        hess[B * 2 + 1, A * 2] += g;

                        hess[C * 2, A * 2 + 1] += g;
                        hess[C * 2 + 1, A * 2] += -g;
                        hess[C * 2, B * 2 + 1] += -g;
                        hess[C * 2 + 1, B * 2] += g;
                    }
                    else
                    {
                        hess[A * 2, B * 2 + 1] += -g;
                        hess[A * 2 + 1, B * 2] += g;
                        hess[A * 2, C * 2 + 1] += g;
                        hess[A * 2 + 1, C * 2] += -g;

                        hess[B * 2, C * 2 + 1] += -g;
                        hess[B * 2 + 1, C * 2] += g;
                        hess[B * 2, A * 2 + 1] += g;
                        hess[B * 2 + 1, A * 2] += -g;

                        hess[C * 2, A * 2 + 1] += -g;
                        hess[C * 2 + 1, A * 2] += g;
                        hess[C * 2, B * 2 + 1] += g;
                        hess[C * 2 + 1, B * 2] += -g;
                    }
                }
            }
        }
        enum state { mesh, nurbs };
        state internalState;
        Rhino.Geometry.Mesh inputMesh = null;
        mikity.GeometryProcessing.MeshStructure meshStructure;
        private void firstActions()
        {
            P1 = nParticles - 1;
            P2 = 0;

            Kapybara3D.Materials.harmonicMaterial hm = new Kapybara3D.Materials.harmonicMaterial();
            gS.setMaterial(hm.getMaterial());
            gS.computeAll(refPos);
            gS.memoryMetric();
            gS.memoryVolume();
            gS.computeAll(refPos);
            area = gS.getTotalVolume();
            refArea = area;
            gS.setMaterial(_material.getMaterial());
            gS.computeAll(refPos);
            initialShapePrj(refPos, pos);
            gS.setMaterial(_material.getMaterial());
            gS.computeAll(pos);
            cN.computeAll(refPos);
            cN.memoryMetric();
            cN.memoryVolume();
            if (_eigen || _conformal) gS.computeEigenVectors();
            gS.computeHessEd(refPos);
            gS.getHessian(hessEd, 2);
            computeHessArea(hessA);

            isInitialized = true;
        }
        string dbg = "";
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            Rhino.Geometry.NurbsSurface inputNurbs = null;
            Object inputGeometry = null;
            dbg = "";
            double area = 0;
/*            if (!DA.GetData(1, ref dt))
            {
                isInitialized = false;
                return;
            }*/
            if (full != null) dt = full.getDt(); else dt = 0.1;
            if (_go == false)
            {
                isInitialized = false;
                if (reset == false)
                {
                    if (!DA.GetData(0, ref inputGeometry)) { isInitialized = false; return; }
                    if (inputGeometry is Grasshopper.Kernel.Types.GH_Mesh)
                    {
                        inputMesh = (inputGeometry as Grasshopper.Kernel.Types.GH_Mesh).Value;
                        internalState = state.mesh;
                    }
                    else if (inputGeometry is Grasshopper.Kernel.Types.GH_Surface)
                    {
                        inputNurbs = (inputGeometry as Grasshopper.Kernel.Types.GH_Surface).Value.Surfaces[0].ToNurbsSurface();
                        Rhino.Geometry.ControlPoint cp = inputNurbs.Points.GetControlPoint(0, 0);
                        inputMesh = initializeNurbs(inputNurbs);
                        internalState = state.nurbs;
                    }
                    else
                    {
                        isInitialized = false;
                        return;
                    }
                    m = inputMesh.DuplicateMesh();
                    if (internalState == state.nurbs)
                    {
                        ns = inputNurbs.Duplicate() as Rhino.Geometry.NurbsSurface;
                    }
                    initialize(m, m);
                    meshStructure=mikity.GeometryProcessing.MeshStructure.CreateFrom(m);
                    if (internalState == state.mesh)
                    {
                        this.BKGT = GetBKGT(m);
                        this.DVPW = GetDVPW(m);
                    }
                    else if (internalState == state.nurbs)
                    {
                        this.BKGT = GetBKGT(ns);
                        this.DVPW = GetDVPW(ns);
                    }
                    firstActions();
                }
                else
                {
                    reset = false;
                }
                isInitialized = true;
                if (_isFixedBoundary == true)
                {
                    if (fixedPointGuids == null)
                    {
                        createFixedPoints();
                    }
                    if (fixedPointGuids.Count() != __boundary.Count())
                    {
                        createFixedPoints();
                    }
                    updateFixedPoints();
                }
                else
                {
                    deleteFixedPoints();
                }
                computeInitialGuess(refPos, pos, true);


                if (_eigen || _conformal)
                {
                    gS.computeAll(pos);
                    gS.computeEigenVectors();
                }
                gS.computeVolume(pos);
                area = gS.getTotalVolume();
                __update();

                Clock = -1;
                dbg += "AreaRatio=" + (area / refArea).ToString()+"\n";
                dbg += "Area=" + area.ToString()+"\n";
                dbg += "refArea=" + refArea.ToString()+"\n";
                if (_conformal)
                {
                    dbg += "max(Mc)=" + maxC.ToString() + "\n";
                    dbg += "min(Mc)=" + minC.ToString() + "\n";
                }
                full.setDbgText(dbg);
            }
            else
            {
                reset = false;
                sw.Reset();
                int S = 0;
                do
                {
                    S++;
                    sw.Start();
                    Clock++;
                    __UPDATE();
                    gS.computeVolume(pos);
                    area = gS.getTotalVolume();
                    //area = gS.getTotalVolume();
                    if (_isFixedBoundary == true)
                    {
                        if (fixedPointGuids == null)
                        {
                            createFixedPoints();
                        }
                        if (fixedPointGuids.Count() != __boundary.Count())
                        {
                            createFixedPoints();
                        }
                        updateFixedPoints();
                    }
                    else
                    {
                        deleteFixedPoints();
                    }
                    if (type == modelType.wire)
                    {
                        foreach (face f in faces)
                        {
                            _wireMaterial(f, refPos, pos);
                        }
                        cN.computeAll(pos);
                        cN.getGrad(force);
                        gS.computeAll(pos);
                    }
                    else if (type == modelType.membrane)
                    {
                        if (mat == materialChoice.SCP)
                        {
                            Kapybara3D.Materials.conformalMaterial cm = _material as Kapybara3D.Materials.conformalMaterial;
                            cm.area = area;
                        }
                        gS.computeAll(pos);
                        gS.getGrad(force);
                        cN.computeAll(pos);
                        meshStructure.Update(pos);
                        if(_fixFlip)fixFlip(meshStructure);
                    }
                    threeTerm();
                    sw.Stop();
                    full.addNorm(normW);
                } while (sw.ElapsedMilliseconds < 25);

                if (_eigen || _conformal) gS.computeEigenVectors();
                __update();
                dbg += "repeatCycle=" + S.ToString() + "\n";
                dbg += "AreaRatio=" + (area / refArea).ToString() + "\n";
                dbg += "Area=" + area.ToString() + "\n";
                dbg += "refArea=" + refArea.ToString() + "\n";
                if (_conformal)
                {
                    dbg += "max(Mc)=" + maxC.ToString() + "\n";
                    dbg += "min(Mc)=" + minC.ToString() + "\n";
                }
                dbg += "|F|=" + normW.ToString() + "\n";
                full.setDbgText(dbg);
                
            }
        }
        double maxC = 0;
        double minC = 0;
        double intC = 0;
        double normW = 0;
        double[] V1 = new double[3];
        double[] V2 = new double[3];
        double[] g1 = new double[3];
        double[] g2 = new double[3];
        double[] V3 = new double[3];
        private void fixFlip(mikity.GeometryProcessing.MeshStructure model)
        {
            foreach (var v in model.innerVertices)
            {
                double val = 0;
                bool flag = false;
                foreach (var he in v.star)
                {
                    var ut = he.prev;
                    int P1 = he.next.P.N;
                    int P2 = ut.P.N;
                    int P3 = v.N;
                    V1[0] = he.next.P.x - v.x;
                    V1[1] = he.next.P.y - v.y;
                    V1[2] = 0;
                    V2[0] = ut.P.x - v.x;
                    V2[1] = ut.P.y - v.y;
                    V2[2] = 0;
                    V3[0] = 0;
                    V3[1] = 0;
                    V3[2] = V1[0] * V2[1] - V1[1] * V2[0];
                    if (val == 0) val = V3[2];
                    if (val * V3[2] < 0) { flag = true; break; }
                }
                if (flag)
                {
                    double x = 0;
                    double y = 0;
                    foreach (var he in v.star)
                    {
                        x += he.next.P.x;
                        y += he.next.P.y;
                    }
                    x /= v.star.Count;
                    y /= v.star.Count;
                    int P1 = v.N;
                    //vel[P1 * 3 + 0] = (x - pos[P1, 0]) / dt / 10d;
                    //vel[P1 * 3 + 1] = (y - pos[P1, 1]) / dt / 10d;
                    //pos[P1, 0] = vel[P1 * 3 + 0] * dt;
                    //pos[P1, 1] = vel[P1 * 3 + 1] * dt;
                    pos[P1, 0] = (x - pos[P1, 0]) / 10d + pos[P1, 0];
                    pos[P1, 1] = (y - pos[P1, 1]) / 10d + pos[P1, 1];
                    //v.x = pos[P1, 0];
                    //v.y = pos[P1, 1];
                }
            }
            if (!_isFixedBoundary)
            {
                foreach (var v in model.outerVertices)
                {
                    double val = 0;
                    bool flag = false;
                    foreach (var he in v.star)
                    {
                        var ut = he.prev;
                        int P1 = he.next.P.N;
                        int P2 = ut.P.N;
                        int P3 = v.N;
                        V1[0] = he.next.P.x - v.x;
                        V1[1] = he.next.P.y - v.y;
                        V1[2] = 0;
                        V2[0] = ut.P.x - v.x;
                        V2[1] = ut.P.y - v.y;
                        V2[2] = 0;
                        V3[0] = 0;
                        V3[1] = 0;
                        V3[2] = V1[0] * V2[1] - V1[1] * V2[0];
                        if (val == 0) val = V3[2];
                        if (val * V3[2] < 0) { flag = true; break; }
                    }
                    if (flag)
                    {
                        int P1 = v.N;
                        double x = (v.hf_begin.next.P.x + v.hf_end.P.x) / 2d;
                        double y = (v.hf_begin.next.P.y + v.hf_end.P.y) / 2d;
                        //vel[P1 * 3 + 0] = (x - pos[P1, 0]) / dt / 1d;
                        //vel[P1 * 3 + 1] = (y - pos[P1, 1]) / dt / 1d;
                        pos[P1, 0] = x;// (x - pos[P1, 0]) + pos[P1, 0];
                        pos[P1, 1] = y;// (y - pos[P1, 1]) + pos[P1, 1];
                        //pos[P1, 0] += vel[P1 * 3 + 0] * dt;
                        //pos[P1, 1] += vel[P1 * 3 + 1] * dt;
                        //v.x = pos[P1, 0];
                        //v.y = pos[P1, 1];
                    }
                }
            }
        }
        private void __update()
        {
            double S2 = 20;
            if (internalState == state.mesh)
            {
                for (int i = 0; i < nParticles; i++)
                {
                    m.Vertices[i] = new Rhino.Geometry.Point3f((float)pos[i, 0] + Shifting, (float)pos[i, 1], (float)pos[i, 2]);
                }
            }
            for (int i = 0; i < nParticles; i++)
            {
                triMesh.Vertices[i] = new Rhino.Geometry.Point3f((float)pos[i, 0] + Shifting, (float)pos[i, 1], (float)pos[i, 2]);
            }
            if (internalState == state.nurbs)
            {
                for (int i = 0; i < nV; i++)
                {
                    for (int j = 0; j < nU; j++)
                    {
                        int k = j + i * nU;
                        ns.Points.SetControlPoint(j, i, new Rhino.Geometry.ControlPoint(pos[k, 0] + Shifting, pos[k, 1], pos[k, 2]));
                    }
                }
            }
            iP.Clear();
            bV.Clear();
            eVT.Clear();
            eVC.Clear();
            reF.Clear();
            cfm.Clear();
            if (_isFixedBoundary || mat == materialChoice.DCM)
            {
                for (int i = 0; i < nParticles; i++)
                {
                    reF.Add(new Rhino.Geometry.Line(pos[i, 0] + Shifting, pos[i, 1], 0, pos[i, 0] + Re[i, 0] * S2 + Shifting, pos[i, 1] + Re[i, 1] * S2, 0));
                }
            }
            if (_base)
            {
                foreach (Kapybara3D.Elements.managedElement e in elemList)
                {
                    for (int i = 0; i < e.nIntPoint; i++)
                    {
                        e.getGlobalCoord(node, i);
                        e.getBaseVectors(node2, i);
                        double S = 0.15;
                        bV.Add(new Rhino.Geometry.Line(node[0] + Shifting, node[1], node[2], node[0] + node2[0, 0] * S + Shifting, node[1] + node2[0, 1] * S, node[2] + node2[0, 2] * S));
                        bV.Add(new Rhino.Geometry.Line(node[0] + Shifting, node[1], node[2], node[0] + node2[1, 0] * S + Shifting, node[1] + node2[1, 1] * S, node[2] + node2[1, 2] * S));
                    }
                }

            }
            if (_intPoint)
            {
                if (type == modelType.membrane)
                {
                    foreach (Kapybara3D.Elements.managedElement e in elemList)
                    {
                        for (int i = 0; i < e.nIntPoint; i++)
                        {
                            e.getGlobalCoord(node, i);
                            iP.Add(new Rhino.Geometry.Point3d(node[0] + Shifting, node[1], node[2]));
                        }
                    }
                }
            }
            if (_eigen)
            {
                foreach (Kapybara3D.Elements.managedElement e in elemList)
                {
                    for (int i = 0; i < e.nIntPoint; i++)
                    {
                        e.getGlobalCoord(node, i);
                        e.getEigenVectors(node2, vals, i);
                        vals[0] = Math.Log(vals[0]);
                        vals[1] = Math.Log(vals[1]);
                        vals[0] *= 5;
                        vals[1] *= 5;
                        if (vals[0] > 0)
                        {
                            eVT.Add(new Rhino.Geometry.Line(node[0] + Shifting, node[1], node[2], node[0] + node2[0, 0] * vals[0] + Shifting, node[1] + node2[0, 1] * vals[0], node[2] + node2[0, 2] * vals[0]));
                            eVT.Add(new Rhino.Geometry.Line(node[0] + Shifting, node[1], node[2], node[0] - node2[0, 0] * vals[0] + Shifting, node[1] - node2[0, 1] * vals[0], node[2] - node2[0, 2] * vals[0]));
                        }
                        else
                        {
                            eVC.Add(new Rhino.Geometry.Line(node[0] + node2[0, 0] * vals[0] + Shifting, node[1] + node2[0, 1] * vals[0], node[2] + node2[0, 2] * vals[0], node[0] + Shifting, node[1], node[2]));
                            eVC.Add(new Rhino.Geometry.Line(node[0] - node2[0, 0] * vals[0] + Shifting, node[1] - node2[0, 1] * vals[0], node[2] - node2[0, 2] * vals[0], node[0] + Shifting, node[1], node[2]));
                        }
                        if (vals[1] > 0)
                        {
                            eVT.Add(new Rhino.Geometry.Line(node[0] + Shifting, node[1], node[2], node[0] + node2[1, 0] * vals[1] + Shifting, node[1] + node2[1, 1] * vals[1], node[2] + node2[1, 2] * vals[1]));
                            eVT.Add(new Rhino.Geometry.Line(node[0] + Shifting, node[1], node[2], node[0] - node2[1, 0] * vals[1] + Shifting, node[1] - node2[1, 1] * vals[1], node[2] - node2[1, 2] * vals[1]));
                        }
                        else
                        {
                            eVC.Add(new Rhino.Geometry.Line(node[0] + node2[1, 0] * vals[1] + Shifting, node[1] + node2[1, 1] * vals[1], node[2] + node2[1, 2] * vals[1], node[0] + Shifting, node[1], node[2]));
                            eVC.Add(new Rhino.Geometry.Line(node[0] - node2[1, 0] * vals[1] + Shifting, node[1] - node2[1, 1] * vals[1], node[2] - node2[1, 2] * vals[1], node[0] + Shifting, node[1], node[2]));
                        }
                    }
                }
            }
            if (_conformal)
            {
                maxC = 0;
                minC = Double.MaxValue;
                intC = 0;
                double weight = 0;
                foreach (Kapybara3D.Elements.managedElement e in elemList)
                {
                    for (int i = 0; i < e.nIntPoint; i++)
                    {
                        e.getGlobalCoord(node, i);
                        weight = e.getEigenVectors(node2, vals, i);
                        double criteria = 0;
                        if (_conformal)
                        {
                            criteria = ((vals[1] - vals[0]) * (vals[1] - vals[0])) / (vals[1] * vals[0]);
                        }
                        if (criteria > maxC) maxC = criteria;
                        if (criteria < minC) minC = criteria;
                        intC += weight * criteria;
                        criteria = criteria * 20;
                        cfm.Add(new Rhino.Geometry.Circle(new Rhino.Geometry.Point3d(node[0] + Shifting, node[1], node[2]), criteria));
                    }
                }
            }
        }

        private void threeTerm()
        {
            
            for (int i = 0; i < nParticles; i++)
            {
                Re[i, 0] = 0;
                Re[i, 1] = 0;
                Re[i, 2] = 0;
            }
            if (_isFixedBoundary == true)
            {
                foreach (boundaryVertex b in __boundary)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Re[b.index, j] = force[b.index * 3 + j];
                        force[b.index * 3 + j] = 0;
                    }
                }
            }
            if (mat == materialChoice.DCM)
            {
                Re[P1, 0] = force[P1 * 3 + 0];
                Re[P1, 1] = force[P1 * 3 + 1];
                Re[P2, 0] = force[P2 * 3 + 0];
                Re[P2, 1] = force[P2 * 3 + 1];
                force[P1 * 3 + 0] = 0;
                force[P1 * 3 + 1] = 0;
                force[P2 * 3 + 0] = 0;
                force[P2 * 3 + 1] = 0;
            }
            
            acc = force;
            for (int i = 0; i < nParticles; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Double.IsNaN(acc[i*3+ j])) acc[i*3+ j] = 0;
                }
            }
            
            double norm1 = (vel * vel.T)[0,0];
            double norm2 = (vel * acc.T)[0,0];
            double norm3 = (acc * acc.T)[0,0];
            double norm = Math.Sqrt((acc * force.T)[0, 0]);
            normW = norm;
            double f = 0;
            if (norm1 * norm3 != 0)
            {
                f = -norm2 / Math.Sqrt(norm1 * norm3);
            }
            else
            {
                f = 1;
            }
            full.move(f);
            double damping1 = 0;
            double damping2 = 0;
            if (_drift1)
            {
                damping1 = Drift1(f);
                damping2 = 0;// damping1 * 0.9;
            }
            else if (_drift2)
            {
                damping1 = Drift2(f);
                damping2 = 0;
            }
            else
            {
                damping1 = Drift0(f);
                damping2 = 0;
            }
            if (norm < 1.0) norm = 1.0;
            vel = vel * damping1 - acc * (dt/norm);
            vel2 = vel2 * damping2 - acc * (dt/norm);
            
            for (int i = 0; i < nParticles; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    pos[i, j] = pos[i, j] + vel[i * 3 + j] * dt;
                }
            }

        }
        public BakeGeometry GetBKGT(Rhino.Geometry.Mesh _m)
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 2;
                if (this.type == modelType.membrane)
                {
                    o.Add(d.Objects.AddMesh(_m, a2));
                }
                else if (this.type == modelType.wire)
                {
                    o.Add(d.Objects.AddMesh(triMesh, a2));
                }
            });
        }
        public BakeGeometry GetBKGT(Rhino.Geometry.NurbsSurface _m)
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 2;
                if (this.type == modelType.membrane)
                {
                    o.Add(d.Objects.AddSurface(_m, a2));
                }
                else if (this.type == modelType.wire)
                {
                    o.Add(d.Objects.AddMesh(triMesh, a2));
                }
            });
        }

        public DrawViewPortWire GetDVPW(Rhino.Geometry.Mesh _m)
        {
            return new DrawViewPortWire((args) =>
            {
                if (Hidden)
                {
                    return;
                }
                if (type != modelType.wire)
                {
                    args.Display.DrawMeshWires(_m, System.Drawing.Color.Black);
                }

                if (type != modelType.membrane)
                {
                    args.Display.DrawMeshWires(triMesh, System.Drawing.Color.Brown, 3);
                    args.Display.DrawPoints(triMesh.Vertices.ToPoint3dArray(), Rhino.Display.PointStyle.Simple, 2, System.Drawing.Color.White);
                }
                args.Display.DrawPoints(iP, Rhino.Display.PointStyle.ControlPoint, 1, System.Drawing.Color.Black);
                args.Display.DrawLines(bV, System.Drawing.Color.Red, 1);
                args.Display.DrawLines(eVT, System.Drawing.Color.Cyan);
                args.Display.DrawLines(eVC, System.Drawing.Color.Magenta);
                if (mat == materialChoice.DCM)
                {
                    args.Display.DrawPoint(new Rhino.Geometry.Point3d(pos[P1, 0] + Shifting, pos[P1, 1], 0), Rhino.Display.PointStyle.X, 3, System.Drawing.Color.Red);
                    args.Display.DrawPoint(new Rhino.Geometry.Point3d(pos[P2, 0] + Shifting, pos[P2, 1], 0), Rhino.Display.PointStyle.X, 3, System.Drawing.Color.Red);
                }
                if ((mat == materialChoice.DCM || _isFixedBoundary)&&_RF)
                {
                    args.Display.DrawLines(reF, System.Drawing.Color.Red);
                    foreach (Rhino.Geometry.Line l in reF)
                    {
                        args.Display.DrawArrowHead(l.To, l.Direction, System.Drawing.Color.Red, 0.0, l.Length / 5d);
                    }
                }
                foreach (Rhino.Geometry.Line l in eVT)
                {
                    args.Display.DrawArrowHead(l.To, l.Direction, System.Drawing.Color.Cyan, 0.0, l.Length / 5d);
                }
                foreach (Rhino.Geometry.Line l in eVC)
                {
                    args.Display.DrawArrowHead(l.To, l.Direction, System.Drawing.Color.Magenta, 0.0, l.Length / 5d);
                }
                if (_conformal)
                {
                    foreach (Rhino.Geometry.Circle c in cfm)
                    {
                        args.Display.DrawCircle(c, System.Drawing.Color.Purple);
                    }
                }
                if (_isFixedBoundary == true)
                {
                    foreach (boundaryVertex b in __boundary)
                    {
                        args.Display.DrawPoint(_m.Vertices[b.index], Rhino.Display.PointStyle.X, 3, System.Drawing.Color.Red);
                    }
                }
                args.Display.DrawPoint(_m.Vertices[0], Rhino.Display.PointStyle.ActivePoint, 5, System.Drawing.Color.OrangeRed);
                args.Display.DrawPoint(inputMesh.Vertices[0], Rhino.Display.PointStyle.ActivePoint, 5, System.Drawing.Color.OrangeRed);
            });
        }
        public DrawViewPortWire GetDVPW(Rhino.Geometry.NurbsSurface _m)
        {
            return new DrawViewPortWire((args) =>
            {
                if (Hidden)
                {
                    return;
                }
                if (type != modelType.wire)
                {
                    args.Display.DrawSurface(_m, System.Drawing.Color.Red, 1);
                }

                if (type != modelType.membrane)
                {
                    args.Display.DrawMeshWires(triMesh, System.Drawing.Color.Brown, 3);
                    args.Display.DrawPoints(triMesh.Vertices.ToPoint3dArray(), Rhino.Display.PointStyle.Simple, 2, System.Drawing.Color.White);
                }
                args.Display.DrawPoints(iP, Rhino.Display.PointStyle.ControlPoint, 1, System.Drawing.Color.Black);
                args.Display.DrawLines(bV, System.Drawing.Color.Pink, 2);
                args.Display.DrawLines(eVT, System.Drawing.Color.Cyan);
                args.Display.DrawLines(eVC, System.Drawing.Color.Magenta);
                if (mat == materialChoice.DCM)
                {
                    args.Display.DrawPoint(new Rhino.Geometry.Point3d(pos[P1, 0] + Shifting, pos[P1, 1], 0), Rhino.Display.PointStyle.X, 3, System.Drawing.Color.Red);
                    args.Display.DrawPoint(new Rhino.Geometry.Point3d(pos[P2, 0] + Shifting, pos[P2, 1], 0), Rhino.Display.PointStyle.X, 3, System.Drawing.Color.Red);
                }
                if ((mat == materialChoice.DCM || _isFixedBoundary)&&_RF)
                {
                    args.Display.DrawLines(reF, System.Drawing.Color.Red);
                    foreach (Rhino.Geometry.Line l in reF)
                    {
                        args.Display.DrawArrowHead(l.To, l.Direction, System.Drawing.Color.Red, 0.0, l.Length / 5d);
                    }
                }
                foreach (Rhino.Geometry.Line l in eVT)
                {
                    args.Display.DrawArrowHead(l.To, l.Direction, System.Drawing.Color.Cyan, 0.0, l.Length / 5d);
                }
                foreach (Rhino.Geometry.Line l in eVC)
                {
                    args.Display.DrawArrowHead(l.To, l.Direction, System.Drawing.Color.Magenta, 0.0, l.Length / 5d);
                }
                if (_conformal)
                {
                    foreach (Rhino.Geometry.Circle c in cfm)
                    {
                        args.Display.DrawCircle(c, System.Drawing.Color.Purple);
                    }
                }

                if (_isFixedBoundary)
                {
                    foreach (boundaryVertex b in __boundary)
                    {
                        args.Display.DrawPoint(triMesh.Vertices[b.index], Rhino.Display.PointStyle.X, 3, System.Drawing.Color.Red);
                    }
                }
            });
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("b3c112b7-59a1-40a5-9fd4-c63b8356f02c"); }
        }

    }
}
