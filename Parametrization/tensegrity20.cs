using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mikity.ghComponents
{
    public class tensegrity20 : Grasshopper.Kernel.GH_Component
    {
        static int nConstraints = 20;
        static int nParticles = nConstraints * 2;
        static int S1 = 40;
        static int S2 = 40;
        int Shift = 6;
        int Shift2 = 6;
        bool _go = false;
        double[,] pos= new double[nParticles, 3], vel= new double[nParticles, 3], acc=new double[nParticles, 3];
        double[] grad= new double[nParticles * 3];
        double[,] jacobian= new double[nConstraints, nParticles * 3];
        double[] lambda= new double[nConstraints];
        double[] residual = new double[nConstraints];
        double[] dx = new double[nParticles * 3];
        double[] weight1 = new double[S1];
        double[] weight2 = new double[S2];
        Rhino.Geometry.Point3d center = new Rhino.Geometry.Point3d(0, 0, 0);
        Kapybara3D.Materials.formFindingMaterial[] material1 = new Kapybara3D.Materials.formFindingMaterial[S1];
        Kapybara3D.Materials.formFindingMaterial[] material2 = new Kapybara3D.Materials.formFindingMaterial[S2];
        Kapybara3D.Elements.I2D1[] bar = new Kapybara3D.Elements.I2D1[nConstraints];
        Kapybara3D.Elements.I2D1[] cbl1 = new Kapybara3D.Elements.I2D1[S1];
        Kapybara3D.Elements.I2D1[] cbl2 = new Kapybara3D.Elements.I2D1[S2];
        DrawViewPortWire DVPW = null;
        BakeGeometry BKGT = null;
        List<Rhino.Geometry.Point3d> iP = new List<Rhino.Geometry.Point3d>();
        List<Rhino.Geometry.Line> lineBar = new List<Rhino.Geometry.Line>();
        List<Rhino.Geometry.Line> lineCbl1 = new List<Rhino.Geometry.Line>();
        List<Rhino.Geometry.Line> lineCbl2 = new List<Rhino.Geometry.Line>();
        Kapybara3D.Objects.generalSpring gS = new Kapybara3D.Objects.generalSpring();
        Kapybara3D.Objects.constraineVolume[] cV = new Kapybara3D.Objects.constraineVolume[nConstraints];
        System.Random rand = new System.Random(1);
        int t = -1;
        System.Windows.Forms.ToolStripMenuItem __m1, __m2,__m3,__m4;
        double damping = 0.99, dt = 0.12;
        int __repeat = 20;
        int number = 0;
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

        private void initialize()
        {
            for (int i = 0; i < nConstraints; i++)
            {
                pos[i * 2, 0] = i * 3d - (nConstraints / 2d) * 3d;
                pos[i * 2, 1] = 5;
                pos[i * 2, 2] = 0;
                pos[i * 2 + 1, 0] = i * 3d - (nConstraints / 2d) * 3d;
                pos[i * 2 + 1, 1] = -5;
                pos[i * 2 + 1, 2] = 0;
            }
            for (int i = 0; i < nParticles; i++)
            {
                vel[i, 0] = 0;
                vel[i, 1] = 0;
                vel[i, 2] = 0;
            }
        }
        private void initialize2()
        {
            for (int i = 0; i < nConstraints; i++)
            {
                pos[i * 2, 0] = (rand.NextDouble()-0.5)*5;
                pos[i * 2, 1] = (rand.NextDouble() - 0.5) * 5;
                pos[i * 2, 2] = (rand.NextDouble() - 0.5) * 5;
                pos[i * 2 + 1, 0] = (rand.NextDouble() - 0.5) * 5;
                pos[i * 2 + 1, 1] = (rand.NextDouble() - 0.5) * 5;
                pos[i * 2 + 1, 2] = (rand.NextDouble() - 0.5) * 5;
            }
            for (int i = 0; i < nParticles; i++)
            {
                vel[i, 0] = 0;
                vel[i, 1] = 0;
                vel[i, 2] = 0;
            }
        }
        public tensegrity20()
            : base("Tensegrity20", "Tensegrity20", "Tensegrity20", "Kapybara3D", "Computation")
        {
            www.Clear();
            for (int i = 0; i < 80; i++)
            {
                www.Add(1d);
            }
            for (int i = 0; i < nConstraints; i++)
            {
                bar[i] = new Kapybara3D.Elements.I2D1(new int[2] { i * 2, i * 2 + 1 });
                bar[i].setRefVolume(30d);
            }
            for (int i = 0; i < S1; i++)
            {
                cbl1[i] = new Kapybara3D.Elements.I2D1();
                material1[i] = new Kapybara3D.Materials.formFindingMaterial();
                material1[i].Power = 4.0;
                material1[i].Weight = 2.0;
                cbl1[i].setMaterial(material1[i].getMaterial());
            }
            for (int i = 0; i < S2 ; i++)
            {
                cbl2[i] = new Kapybara3D.Elements.I2D1();
                material2[i] = new Kapybara3D.Materials.formFindingMaterial();
                material2[i].Power = 4.0;
                material2[i].Weight = 1.0;
                cbl2[i].setMaterial(material2[i].getMaterial());
            }
            setup();
            __randomize();
            gS.Clear();
            gS.AddRange(cbl1);
            gS.AddRange(cbl2);
            gS.initialize(nParticles);
            for (int i = 0; i < nConstraints; i++)
            {
                cV[i] = new Kapybara3D.Objects.constraineVolume();
                cV[i].Clear();
                cV[i].Add(bar[i]);
                cV[i].setRefVolume(30d);
                cV[i].initialize(nParticles);
            }
            this.DVPW = this.GetDVPW();
            this.BKGT = this.GetBKGT();
        }
        private void setup()
        {
            for (int i = 0; i < S1; i++)
            {
                int t = i + Shift * 2;
                t = t % (nConstraints*2);
                cbl1[i].setupIndex(new int[2] { i, t });
            }
            for (int i = 0; i < S2; i++)
            {
                int t = i + Shift2 * 2+1;
                t = t % (nConstraints*2);   
                cbl2[i].setupIndex(new int[2] { i, t });
            }
        }
        double[] ws11 = new double[S1];
        double[] ws12 = new double[S1];
        double[] ws13 = new double[S1];
        double[] ws14 = new double[S1];
        double[] ws21 = new double[S2];
        double[] ws22 = new double[S2];
        double[] ws23 = new double[S2];
        double[] ws24 = new double[S2];
        double W1 = 2.0, W2 = 2.0, W3 = 1.0, W4 = 1.0;
        double sw1 = 0.0, sw2 = 0.0, sw3 = 0.0, sw4 = 0.0;
        public void __randomize()
        {
            for (int i = 0; i < S1; i++)
            {
                ws11[i] = rand.NextDouble();
                ws12[i] = rand.NextDouble();
                ws13[i] = rand.NextDouble();
                ws14[i] = rand.NextDouble();
            }
            for (int i = 0; i < S2; i++)
            {
                ws21[i] = rand.NextDouble();
                ws22[i] = rand.NextDouble();
                ws23[i] = rand.NextDouble();
                ws24[i] = rand.NextDouble();
            }
            asignWeight();
        }
        public void asignWeight()
        {
            for (int i = 0; i < S1; i++)
            {
                if (i % 2 == 0)
                {
                    material1[i].Weight = (W1 + ws11[i] * sw1 + ws12[i] * sw2 + ws13[i] * sw3 + ws14[i] * sw4) * www[i];
                }
                else
                {
                    material1[i].Weight = (W2 + ws11[i] * sw1 + ws12[i] * sw2 + ws13[i] * sw3 + ws14[i] * sw4) * www[i];
                }
            }
            for (int i = 0; i < S2; i++)
            {
                if (i % 2 == 0)
                {
                    material2[i].Weight = (W3 + ws21[i] * sw1 + ws22[i] * sw2 + ws23[i] * sw3 + ws24[i] * sw4) * www[i];
                }
                else
                {
                    material2[i].Weight = (W4 + ws21[i] * sw1 + ws22[i] * sw2 + ws23[i] * sw3 + ws24[i] * sw4) * www[i];
                }
            }
        }

        public void __default()
        {
            for (int i = 0; i < S1; i++)
            {
                weight1[i] = 2.0;
                material1[i].Weight = weight1[i];
            }
            for (int i = 0; i < S2; i++)
            {
                weight2[i] = 1.0;
                material2[i].Weight = weight2[i];
            }
        }

        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Shift", "Shift", "Shift", Grasshopper.Kernel.GH_ParamAccess.item, 6);
            pManager.AddNumberParameter("W11", "W11", "W11", Grasshopper.Kernel.GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("W12", "W12", "W12", Grasshopper.Kernel.GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("W21", "W21", "W21", Grasshopper.Kernel.GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("W22", "W22", "W22", Grasshopper.Kernel.GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("sw1", "sw1", "sw1", Grasshopper.Kernel.GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("sw2", "sw2", "sw2", Grasshopper.Kernel.GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("sw3", "sw3", "sw3", Grasshopper.Kernel.GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("sw4", "sw4", "sw4", Grasshopper.Kernel.GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("www1", "www1", "www1", Grasshopper.Kernel.GH_ParamAccess.list, 1.0);
        }
        private List<double> www=new List<double>();
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("P", "P", "P", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddTextParameter("output", "out", "out", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddTextParameter("verify", "verify", "verify", Grasshopper.Kernel.GH_ParamAccess.list);
        }
        public override void DrawViewportWires(Grasshopper.Kernel.IGH_PreviewArgs args)
        {
            if (this.DVPW != null)
            {
                this.DVPW(args);
            }
            base.DrawViewportWires(args);
        }
        public override void BakeGeometry(Rhino.RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, List<Guid> obj_ids)
        {
            if (this.BKGT != null)
            {
                this.BKGT(doc, att, obj_ids);
            }
        }

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            __m1 = Menu_AppendItem(menu, "Go?", Menu_MyCustomItemClicked);
            Menu_AppendSeparator(menu);
            __m2 = Menu_AppendItem(menu, "Random Weights!", Menu_MyCustomItemClicked);
            __m3 = Menu_AppendItem(menu, "Random Nodes!", Menu_MyCustomItemClicked);
            __m4 = Menu_AppendItem(menu, "Default Weights!", Menu_MyCustomItemClicked);

            if (_go == true)
            {
                __m1.CheckState = System.Windows.Forms.CheckState.Checked;
            }
            else
            {
                __m1.CheckState = System.Windows.Forms.CheckState.Unchecked;
                t = -1;
            }
        }
        private void Menu_MyCustomItemClicked(Object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem _m = sender as System.Windows.Forms.ToolStripMenuItem;
            if (_m == __m1)
            {
                if (_m.CheckState == System.Windows.Forms.CheckState.Checked)
                {
                    _m.CheckState = System.Windows.Forms.CheckState.Unchecked;
                    _go = false;
                    t = -1;
                }
                else if (_m.CheckState == System.Windows.Forms.CheckState.Unchecked)
                {
                    _m.CheckState = System.Windows.Forms.CheckState.Checked;
                    t = -1;
                    _go = true;
                }
            }
            if (_m == __m2)
            {
                _m.CheckState = System.Windows.Forms.CheckState.Checked;
                __randomize();
            }
            if (_m == __m3)
            {
                _m.CheckState = System.Windows.Forms.CheckState.Checked;
                initialize2();
            }
            if (_m == __m4)
            {
                _m.CheckState = System.Windows.Forms.CheckState.Checked;
                __default();
            }
        }
    
        unsafe protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
            int __Shift = Shift;
            if (!DA.GetData(0, ref Shift)) { return; }
            if (!DA.GetData(1, ref W1)) { return; }
            if (!DA.GetData(2, ref W2)) { return; }
            if (!DA.GetData(3, ref W3)) { return; }
            if (!DA.GetData(4, ref W4)) { return; }
            if (!DA.GetData(5, ref sw1)) { return; }
            if (!DA.GetData(6, ref sw2)) { return; }
            if (!DA.GetData(7, ref sw3)) { return; }
            if (!DA.GetData(8, ref sw4)) { return; }
            www.Clear();
            if (!DA.GetDataList(9,www)) { return; }
            Shift2 = Shift;
            if (__Shift != Shift ) setup();
            asignWeight();
            if (_go == false)
            {
                initialize();
                __update();
                t = -1;
            }
            else
            {
                t++;
                if (t == 0)
                {
                    initialize2();
                }
                fixed (double* _ptr1 = &acc[0,0],_ptr2=&pos[0,0],_ptr3=&dx[0])
                {
                    for (int tt = 0; tt < __repeat; tt++)
                    {

                        for (int i = 0; i < nConstraints; i++)
                        {
                            cV[i].computeAll(pos);
                            cV[i].getGrad(jacobian, i);
                            cV[i].getResidual(out residual[i]);
                        }
                        var f=ShoNS.Array.DoubleArray.From(jacobian);
                        var g = ShoNS.Array.DoubleArray.From(residual);
                        g=g.T;
                        var solver = new ShoNS.Array.Solver(f);
                        var h = solver.Solve(g);
                        
                        double* ptr3 = _ptr3, ptr2 = _ptr2;
                        for (int i = 0; i < nParticles * 3; i++)
                        {
                            *ptr2 += - h[i]*0.8;
                            ptr3++;
                            ptr2++;
                        }
                        
                        gS.computeAll(pos);
                        gS.getGrad(grad);
                        for (int i = 0; i < nConstraints; i++)
                        {
                            cV[i].computeAll(pos);
                            cV[i].getGrad(jacobian, i);
                        }
                        var x = ShoNS.Array.DoubleArray.From(jacobian);
                        var y= ShoNS.Array.DoubleArray.From(grad);
                        y = y.T;
                        solver = new ShoNS.Array.Solver(x.T);
                        var z = solver.Solve(y);
                        double* ptr1 = _ptr1;
                        for (int c = 0; c < grad.Length; c++)
                        {
                            double v = 0;
                            for (int k = 0; k < nConstraints; k++)
                            {
                                v += z[k] * jacobian[k, c];
                            }
                            *ptr1 = grad[c] - v;
                            ptr1++;
                        }
                        threeTerm(pos, vel, acc);
                    }
                }
                __update();
            }
            DA.SetDataList(0, iP);
            List<String> output = new List<String>();
            for (int i = 0; i < S1; i++)
            {
                output.Add("D"+i.ToString("00") + "  " + cbl1[i][0] + "-" + cbl1[i][1] + "  L:" + cbl1[i].getVolume().ToString("000.00"));
            }
            for (int i = 0; i < S1; i++)
            {
                output.Add("E"+i.ToString("00") + "  " + cbl2[i][0] + "-" + cbl2[i][1] + "  L:" + cbl2[i].getVolume().ToString("000.00"));
            }
            DA.SetDataList(1, output);
            DA.SetData(2, verify().ToString("0.000"));
        }
        private double verify()
        {
            double dm = 1000;
            foreach (Rhino.Geometry.Line l in lineBar)
            {
                foreach (Rhino.Geometry.Line l2 in(from f in lineBar where f!=l select f))
                {
                    Rhino.Geometry.Vector3d a = l.To - l.From;
                    Rhino.Geometry.Vector3d b = l2.To - l2.From;
                    Rhino.Geometry.Vector3d c = l2.From - l.From;
                    double[] B = new double[2] { c * b, c * a };
                    double[,] A = new double[2, 2] { { a * b, -b * b }, { a * a, -a * b } };
                    Rhino.Geometry.Matrix m = new Rhino.Geometry.Matrix(2, 2);
                    m[0, 0] = A[0, 0];
                    m[1, 1] = A[1, 1];
                    m[0, 1] = A[0, 1];
                    m[1, 0] = A[1, 0];
                    m.Invert(0.0000001);
                    double s = m[0, 0] * B[0] + m[0, 1] * B[1];
                    double t = m[1, 0] * B[0] + m[1, 1] * B[1];
                    if (s > 1 || s < 0 || t > 1 || t < 0)
                    {
                        continue;
                    }
                    else
                    {
                        Rhino.Geometry.Point3d L1 = l.From + a * s;
                        Rhino.Geometry.Point3d L2 = l2.From + b * t;
                        double tol = Math.Sqrt((L1 - L2) * (L1 - L2));
                        if (tol < dm)
                        {
                            dm=tol;
                        }
                    }
                }
            }
            return dm;
        }
        private void threeTerm(double[,] _pos, double[,] _vel, double[,] _acc)
        {
            double norm = 0;
            for (int i = 0; i < nParticles; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    norm += acc[i, j] * acc[i, j];
                }
            }
            norm = Math.Sqrt(norm);
            if (norm < 1.0) norm = 1.0;
            for (int i = 0; i < nParticles; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _vel[i, j] = _vel[i, j] * damping - _acc[i, j] * dt / norm;
                }
            }
            for (int i = 0; i < nParticles; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _pos[i, j] = _pos[i, j] + _vel[i, j] * dt;
                }
            }
        }

        private void __update()
        {
            iP.Clear();
            lineBar.Clear();
            lineCbl1.Clear();
            lineCbl2.Clear();
            for (int i = 0; i < nParticles; i++)
            {
                iP.Add(new Rhino.Geometry.Point3d(pos[i, 0]+center.X, pos[i, 1]+center.Y, pos[i, 2]+center.Z));
            }
            foreach (Kapybara3D.Elements.I2D1 e in bar)
            {
                lineBar.Add(new Rhino.Geometry.Line(pos[e[0], 0] + center.X, pos[e[0], 1] + center.Y, pos[e[0], 2] + center.Z, pos[e[1], 0] + center.X, pos[e[1], 1] + center.Y, pos[e[1], 2] + center.Z));
            }
            foreach (Kapybara3D.Elements.I2D1 e in cbl1)
            {
                lineCbl1.Add(new Rhino.Geometry.Line(pos[e[0], 0] + center.X, pos[e[0], 1] + center.Y, pos[e[0], 2] + center.Z, pos[e[1], 0] + center.X, pos[e[1], 1] + center.Y, pos[e[1], 2] + center.Z));
            }
            foreach (Kapybara3D.Elements.I2D1 e in cbl2)
            {
                lineCbl2.Add(new Rhino.Geometry.Line(pos[e[0], 0] + center.X, pos[e[0], 1] + center.Y, pos[e[0], 2] + center.Z, pos[e[1], 0] + center.X, pos[e[1], 1] + center.Y, pos[e[1], 2] + center.Z));
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("10c8247b-771f-4312-bd95-f409a37503dd"); }
        }
        public BakeGeometry GetBKGT()
        {
            return new BakeGeometry((d, a, o) =>
            {
                Rhino.DocObjects.ObjectAttributes a2 = a.Duplicate();
                a2.LayerIndex = 2;
                List<Guid> objects = new List<Guid>();
                foreach (Rhino.Geometry.Point3d p in iP)
                {
                    a2.Name = "P"+(iP.IndexOf(p)+1).ToString("00");
                   objects.Add(d.Objects.AddPoint(p,a2));
                }
                a2.LayerIndex = 3;
                foreach (Rhino.Geometry.Line l in lineBar)
                {
                    a2.Name = "S" + (bar[lineBar.IndexOf(l)][0]+1).ToString("00") + "-" +(bar[lineBar.IndexOf(l)][1]+1).ToString("00")+" "+"L:"+l.Length.ToString("00.00");
                    objects.Add(d.Objects.AddLine(l, a2));
                }
                a2.LayerIndex = 4;
                foreach (Rhino.Geometry.Line l in lineCbl1)
                {
                    a2.Name = "TS" + (cbl1[lineCbl1.IndexOf(l)][0]+1).ToString("00") + "-" + (cbl1[lineCbl1.IndexOf(l)][1]+1).ToString("00")+" "+"L:"+l.Length.ToString("00.00");
                    objects.Add(d.Objects.AddLine(l, a2));
                }
                a2.LayerIndex = 5;
                foreach (Rhino.Geometry.Line l in lineCbl2)
                {
                    a2.Name = "TD" + (cbl2[lineCbl2.IndexOf(l)][0]+1).ToString("00") + "-" + (cbl2[lineCbl2.IndexOf(l)][1]+1).ToString("00")+" "+"L:"+l.Length.ToString("00.00");
                    objects.Add(d.Objects.AddLine(l, a2));
                }
                a2.LayerIndex = 1;
//                for (int i = 0; i < nParticles; i++)
//                {
//                    Rhino.Geometry.TextDot num = new Rhino.Geometry.TextDot(i.ToString(), iP[i]);
//                    objects.Add(d.Objects.AddTextDot(num,a2));
//                }
                o.AddRange(objects);
                number++;
                d.Groups.Add("Tensegrity"+number.ToString("000"),objects);
                center.Transform(Rhino.Geometry.Transform.Translation(new Rhino.Geometry.Vector3d(50, 0, 0)));
            });
        }

        public DrawViewPortWire GetDVPW()
        {
            return new DrawViewPortWire((args) =>
            {
                if (Hidden)
                {
                    return;
                }
                args.Display.DrawPoints(iP, Rhino.Display.PointStyle.ControlPoint, 1, System.Drawing.Color.White);
                args.Display.DrawLines(lineBar, System.Drawing.Color.Brown, 3);
                args.Display.DrawLines(lineCbl1, System.Drawing.Color.Blue);
                args.Display.DrawLines(lineCbl2, System.Drawing.Color.Magenta);
            });
        }
    }

}
