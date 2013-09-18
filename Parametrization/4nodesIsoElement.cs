﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
namespace mikity.ghComponents
{
    /// <summary>
    /// Construct a point array using isoparametric shape functions.
    /// </summary>
    public class four_nodes_isoelement : Grasshopper.Kernel.GH_Component
    {
        /*protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //現在のコードを実行しているAssemblyを取得
                System.Reflection.Assembly myAssembly =
                    System.Reflection.Assembly.GetExecutingAssembly();

                System.IO.Stream st = myAssembly.GetManifestResourceStream("mikity.ghComponents.icons.four-nodes-fixed_nodes.bmp");
                //指定されたマニフェストリソースを読み込む
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(st);
                return bmp;
            }
        }
        */
        public four_nodes_isoelement()
            : base("4nodes->isoparametricElement", "4nodes->isoparametricElement", "4nodes->isoparametricElement", "Kapybara3D", "Basic Elements")
        {
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point1", "P1", "First point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point2", "P2", "Second point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point3", "P3", "Third point", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddPointParameter("Point4", "P4", "Fourth point", Grasshopper.Kernel.GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
//            pManager.AddGenericParameter("Particle System", "pS", "Particle System", Grasshopper.Kernel.GH_ParamAccess.item);
        }
        public override void DrawViewportWires(Grasshopper.Kernel.IGH_PreviewArgs args)
        {
            if (this.DVPW != null)
            {
                this.DVPW(args);
            }
            base.DrawViewportWires(args);
        }
        System.Windows.Forms.ToolStripMenuItem __m1;

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);
            __m1 = Menu_AppendItem(menu, "Specify as undeformed state.", Menu_MyCustomItemClicked);
        }

        private void Menu_MyCustomItemClicked(Object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem __m = sender as System.Windows.Forms.ToolStripMenuItem;
            if (__m == __m1)
            {
                if (E2 != null)
                {
                    E2.memoryMetric();
                }
            }
        }
        DrawViewPortWire DVPW = null;
        Kapybara3D.Elements.I4D2 E2 = new Kapybara3D.Elements.I4D2();
        Rhino.Geometry.Point3d[] pointList = new Rhino.Geometry.Point3d[5];
        Kapybara3D.Materials.harmonicMaterial cM = new Kapybara3D.Materials.harmonicMaterial();
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {
                DA.GetData(0, ref pointList[0]);
                DA.GetData(1, ref pointList[1]);
                DA.GetData(2, ref pointList[3]);
                DA.GetData(3, ref pointList[2]);
                pointList[4] = pointList[0];
                double[] x = { pointList[0].X, pointList[0].Y, pointList[0].Z, pointList[1].X, pointList[1].Y, pointList[1].Z, pointList[3].X, pointList[3].Y, pointList[3].Z, pointList[2].X, pointList[2].Y, pointList[2].Z };
                E2.setMaterial(cM.getMaterial());
                E2.setupNodes(x);
                E2.computeGlobalCoord();
                E2.computeMetric();
                E2.computeVolume();
                E2.computeStress();
                E2.computeGradient();
                E2.getGradient(grad);

                this.DVPW = GetDVPW();

        }
        double[] grad=new double[12];
        double[] dot = new double[3];
        public DrawViewPortWire GetDVPW()
        {
            return new DrawViewPortWire((args) =>
            {
                if (Hidden)
                {
                    return;
                }
                if (E2 == null)
                {
                    return;
                }

                for(int i=0;i<9;i++)
                {
                    E2.getGlobalCoord(dot,i);
                    args.Display.DrawPoint(new Rhino.Geometry.Point3d(dot[0], dot[1], dot[2]),Rhino.Display.PointStyle.X,2,System.Drawing.Color.White);
                }
                args.Display.DrawPolyline(pointList, System.Drawing.Color.Red, 1);
                args.Display.DrawArrow(new Rhino.Geometry.Line(pointList[0], new Rhino.Geometry.Vector3d(grad[0], grad[1], grad[2])), System.Drawing.Color.HotPink);
                args.Display.DrawArrow(new Rhino.Geometry.Line(pointList[1], new Rhino.Geometry.Vector3d(grad[3], grad[4], grad[5])), System.Drawing.Color.HotPink);
                args.Display.DrawArrow(new Rhino.Geometry.Line(pointList[3], new Rhino.Geometry.Vector3d(grad[6], grad[7], grad[8])), System.Drawing.Color.HotPink);
                args.Display.DrawArrow(new Rhino.Geometry.Line(pointList[2], new Rhino.Geometry.Vector3d(grad[9], grad[10], grad[11])), System.Drawing.Color.HotPink);
            });
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("5a204879-2032-402f-b48d-4a270f98663e"); }
        }

    }
}
