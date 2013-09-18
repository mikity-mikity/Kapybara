using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShoNS.Array;
namespace mikity.ghComponents
{
    public delegate void DrawViewPortWire(Grasshopper.Kernel.IGH_PreviewArgs args);
    public delegate void UpdateGeometry(double x, double y, double z);
    public delegate void BakeGeometry(Rhino.RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, List<Guid> obj_ids);
    public partial class toPlane1 : Grasshopper.Kernel.GH_Component
    {
        public void initialShapeDhm(double[,] rX, double[,] x, int num, bool flag)
        {
            if (flag)
            {
                if (num == 1)
                {
                    projBoundary();
                }
                if (num == 2)
                {
                    circleBoundary();
                }
                if (num == 3)
                {
                    rectangleBoundary();
                }
                updateFixedPoints();
            }
            var origX = DoubleArray.Zeros(nParticles * 2, 1);
            for (int i = 0; i < nParticles; i++)
            {
                origX[i * 2 + 0, 0] = x[i, 0];
                origX[i * 2 + 1, 0] = x[i, 1];
            }
            List<int> shift = new List<int>();
            int T1 = 0;
            int T2 = 0;
            for (int i = 0; i < nParticles; i++)
            {
                shift.Add(i);
            }
            T1 = (nParticles - __boundary.Count()) * 2 - 1;
            T2 = nParticles * 2 - 1;
            int C1 = 0;
            int C2 = nParticles - __boundary.Count();
            for (int i = 0; i < nParticles; i++)
            {
                if (isBoundary(i))
                {
                    shift[i] = C2;
                    C2++;
                }
                else
                {
                    shift[i] = C1;
                    C1++;
                }
            }
            var shiftArray = new SparseDoubleArray(nParticles * 2, nParticles * 2);
            for (int i = 0; i < nParticles; i++)
            {
                shiftArray[i * 2, shift[i] * 2] = 1;
                shiftArray[i * 2 + 1, shift[i] * 2 + 1] = 1;
            }
            var ED = shiftArray.T.Multiply(hessEd) as SparseDoubleArray;
            ED = ED.Multiply(shiftArray) as SparseDoubleArray;
            var slice1 = new SparseDoubleArray(T1 + 1, T2 + 1);
            var slice2 = new SparseDoubleArray(T2 + 1, T2 - T1);
            for (int i = 0; i < T1 + 1; i++)
            {
                slice1[i, i] = 1;
            }
            for (int i = 0; i < T2 - T1; i++)
            {
                slice2[i + T1 + 1, i] = 1;
            }
            var DIB = (slice1.Multiply(ED) as SparseDoubleArray).Multiply(slice2) as SparseDoubleArray;
            var DII = (slice1.Multiply(ED) as SparseDoubleArray).Multiply(slice1.T) as SparseDoubleArray;
            var solver = new SparseLU(DII);
            origX = shiftArray.T * origX;
            var fixX = origX.GetSlice(T1 + 1, T2, 0, 0);
            var B = -DIB * fixX;
            var dx = solver.Solve(B);

            var ret = DoubleArray.Zeros(nParticles * 2, 1);
            for (int i = 0; i < T1 + 1; i++)
            {
                ret[i, 0] = dx[i, 0];
            }
            for (int i = T1 + 1; i <= T2; i++)
            {
                ret[i, 0] = fixX[i - T1 - 1, 0];
            }
            var xx = shiftArray * ret;

            for (int i = 0; i < nParticles; i++)
            {
                x[i, 0] = xx[i * 2, 0];
                x[i, 1] = xx[i * 2 + 1, 0];
            }

        }
        public void initialShapeDcm(double[,] rX, double[,] x)
        {
            var origX = DoubleArray.Zeros(nParticles * 2, 1);
            for (int i = 0; i < nParticles; i++)
            {
                origX[i * 2 + 0, 0] = rX[i, 0];
                origX[i * 2 + 1, 0] = rX[i, 1];
            }
            List<int> shift = new List<int>();
            int T1 = 0;
            int T2 = 0;
            for (int i = 0; i < nParticles; i++)
            {
                shift.Add(i);
            }

            SparseDoubleArray sys = hessEd - hessA;
            int tmp = shift[shift.Count - 1];
            shift.Insert(P1, tmp);
            shift.RemoveAt(shift.Count - 1);
            tmp = shift[shift.Count - 1];
            shift.Insert(P2, tmp);
            shift.RemoveAt(shift.Count - 1);
            T1 = (nParticles - 2) * 2 - 1;
            T2 = nParticles * 2 - 1;

            var shiftArray = SparseDoubleArray.Zeros(nParticles * 2, nParticles * 2);
            for (int i = 0; i < nParticles; i++)
            {
                shiftArray[i * 2, shift[i] * 2] = 1;
                shiftArray[i * 2 + 1, shift[i] * 2 + 1] = 1;
            }
            var ED = (shiftArray.T * sys as SparseDoubleArray) * shiftArray as SparseDoubleArray;

            var slice1 = new SparseDoubleArray(T1 + 1, T2 + 1);
            var slice2 = new SparseDoubleArray(T2 + 1, T2 - T1);
            for (int i = 0; i < T1 + 1; i++)
            {
                slice1[i, i] = 1;
            }
            for (int i = 0; i < T2 - T1; i++)
            {
                slice2[i + T1 + 1, i] = 1;
            }
            var DIB = (slice1.Multiply(ED) as SparseDoubleArray).Multiply(slice2) as SparseDoubleArray;
            var DII = (slice1.Multiply(ED) as SparseDoubleArray).Multiply(slice1.T) as SparseDoubleArray;
            var solver = new SparseLU(DII);
            origX = shiftArray.T * origX;
            var fixX = origX.GetSlice(T1 + 1, T2, 0, 0);
            var B = -DIB * fixX;
            var dx = solver.Solve(B);

            var ret = DoubleArray.Zeros(nParticles * 2, 1);
            for (int i = 0; i < T1 + 1; i++)
            {
                ret[i, 0] = dx[i, 0];
            }
            for (int i = T1 + 1; i <= T2; i++)
            {
                ret[i, 0] = fixX[i - T1 - 1, 0];
            }
            var xx = shiftArray * ret;
            for (int i = 0; i < nParticles; i++)
            {
                x[i, 0] = xx[i * 2, 0];
                x[i, 1] = xx[i * 2 + 1, 0];
            }
            _isFixedBoundary = false;
        }
        public void initialShapeScpl(double[,] rX, double[,] x)
        {
            SparseDoubleArray sys = hessEd - hessA;

            List<int> shift = new List<int>();
            int T1 = 0;
            int T2 = 0;
            for (int i = 0; i < nParticles; i++)
            {
                shift.Add(i);
            }
            T1 = (nParticles - __boundary.Count()) * 2 - 1;
            T2 = nParticles * 2 - 1;
            int C1 = 0;
            int C2 = nParticles - __boundary.Count();
            for (int i = 0; i < nParticles; i++)
            {
                if (isBoundary(i))
                {
                    shift[i] = C2;
                    C2++;
                }
                else
                {
                    shift[i] = C1;
                    C1++;
                }
            }
            var shiftArray = SparseDoubleArray.Zeros(nParticles * 2, nParticles * 2);
            for (int i = 0; i < nParticles; i++)
            {
                shiftArray[i * 2, shift[i] * 2] = 1;
                shiftArray[i * 2 + 1, shift[i] * 2 + 1] = 1;
            }
            var S = (shiftArray.T*hessA as SparseDoubleArray) .Multiply(shiftArray) as SparseDoubleArray;
            var ED = (shiftArray.T.Multiply(hessEd) as SparseDoubleArray).Multiply(shiftArray) as SparseDoubleArray;
            var SYS = (shiftArray.T.Multiply(sys) as SparseDoubleArray).Multiply(shiftArray) as SparseDoubleArray;

            var slice1 = new SparseDoubleArray(T1 + 1, T2 + 1);
            var slice2 = new SparseDoubleArray(T2 + 1, T2 - T1);
            for (int i = 0; i < T1 + 1; i++)
            {
                slice1[i, i] = 1;
            }
            for (int i = 0; i < T2 - T1; i++)
            {
                slice2[i + T1 + 1, i] = 1;
            }


            var QB = (slice2.T.Multiply(S) as SparseDoubleArray).Multiply(slice2) as SparseDoubleArray;
            var LBB = DoubleArray.From((slice2.T.Multiply(SYS) as SparseDoubleArray).Multiply(slice2) as SparseDoubleArray);
            var LIB = DoubleArray.From((slice1.Multiply(SYS) as SparseDoubleArray).Multiply(slice2) as SparseDoubleArray);
            var LII = (slice1.Multiply(SYS) as SparseDoubleArray).Multiply(slice1.T) as SparseDoubleArray;
            //var iLII = LII.Inv();
            var solver = new SparseLU(LII);
            var LB = LBB.Subtract(LIB.T.Multiply(solver.Solve(LIB)));
            var dec = new EigenSym(LB);
            var ss = dec.D;
            int L = 0;
            double val = 10000;
            for (int i = 0; i < T2-T1; i++)
            {
                if (ss[i] > 0)
                {
                    if (ss[i] < val)
                    {
                        L = i;
                        val = ss[i];
                    }

                }
            }
            var eigv = dec.V;
            var ttt = eigv.GetSlice(0, T2 - T1 - 1, L, L);
            double currentArea = (ttt.T * QB * ttt)[0, 0];
            ttt = ttt.Multiply(Math.Sqrt(refArea / currentArea * 2d));
            var DIB = DoubleArray.From((slice1.Multiply(ED) as SparseDoubleArray).Multiply(slice2) as SparseDoubleArray);
            var DII = (slice1.Multiply(ED) as SparseDoubleArray).Multiply(slice1.T) as SparseDoubleArray;
            solver = new SparseLU(DII);

            var dx = solver.Solve(DIB.Multiply(ttt));
            var vert = new SparseDoubleArray(nParticles * 2, 1);
            for (int i = 0; i < nParticles * 2 - __boundary.Count() * 2; i++)
            {
                vert[i, 0] = -dx[i, 0];
            }
            for (int i = 0; i < __boundary.Count() * 2; i++)
            {
                vert[i + (nParticles * 2 - __boundary.Count() * 2), 0] = ttt[i, 0];
            }
            var G = shiftArray.Multiply(vert) as SparseDoubleArray;
            double cx = 0, cy = 0, ex = 0, ey = 0;
            for (int i = 0; i < nParticles; i++)
            {
                x[i, 0] = G[i * 2, 0];
                x[i, 1] = G[i * 2 + 1, 0];
                x[i, 2] = 0;
                cx += x[i, 0];
                cy += x[i, 1];
                ex += rX[i, 0];
                ey += rX[i, 1];
            }
            cx /= nParticles;
            cy /= nParticles;
            ex /= nParticles;
            ey /= nParticles;
            for (int i = 0; i < nParticles; i++)
            {
                x[i, 0] = x[i, 0] - cx + ex;
                x[i, 1] = x[i, 1] - cy + ey;
                x[i, 2] = 0;
            }
            _isFixedBoundary = false;
        }

        public void initialShapeScps(double[,] rX, double[,] x)
        {
            SparseDoubleArray sys = hessEd - hessA;

            List<int> shift = new List<int>();
            int T1 = 0;
            int T2 = 0;
            for (int i = 0; i < nParticles; i++)
            {
                shift.Add(i);
            }
            T1 = (nParticles - __boundary.Count()) * 2 - 1;
            T2 = nParticles * 2 - 1;
            int C1 = 0;
            int C2 = nParticles - __boundary.Count();
            for (int i = 0; i < nParticles; i++)
            {
                if (isBoundary(i))
                {
                    shift[i] = C2;
                    C2++;
                }
                else
                {
                    shift[i] = C1;
                    C1++;
                }
            }
            var shiftArray = SparseDoubleArray.Zeros(nParticles * 2, nParticles * 2);
            for (int i = 0; i < nParticles; i++)
            {
                shiftArray[i * 2, shift[i] * 2] = 1;
                shiftArray[i * 2 + 1, shift[i] * 2 + 1] = 1;
            }
            var S = (shiftArray.T.Multiply(hessA) as SparseDoubleArray).Multiply(shiftArray) as SparseDoubleArray;
            var ED = (shiftArray.T.Multiply(hessEd) as SparseDoubleArray).Multiply(shiftArray) as SparseDoubleArray;
            var SYS = (shiftArray.T.Multiply(sys) as SparseDoubleArray).Multiply(shiftArray) as SparseDoubleArray;

            var slice1 = new SparseDoubleArray(T1 + 1, T2 + 1);
            var slice2 = new SparseDoubleArray(T2 + 1, T2 - T1);
            for (int i = 0; i < T1 + 1; i++)
            {
                slice1[i, i] = 1;
            }
            for (int i = 0; i < T2 - T1; i++)
            {
                slice2[i + T1 + 1, i] = 1;
            }


            var QB = DoubleArray.From((slice2.T.Multiply(S) as SparseDoubleArray).Multiply(slice2) as SparseDoubleArray);
            var LBB = DoubleArray.From((slice2.T.Multiply(SYS) as SparseDoubleArray).Multiply(slice2) as SparseDoubleArray);
            var LIB = DoubleArray.From((slice1.Multiply(SYS) as SparseDoubleArray).Multiply(slice2) as SparseDoubleArray);
            var LII = (slice1.Multiply(SYS) as ShoNS.Array.SparseDoubleArray).Multiply(slice1.T) as SparseDoubleArray;
            //var iLII = LII.Inv();
            var solver = new SparseLU(LII);
            var LB = LBB.Subtract(LIB.T.Multiply(solver.Solve(LIB)));

             var dec = new EigenAsym(LB, QB);
             var ss = dec.D.Real();
             int L = 0;
             double val = 10000;
             for (int i = 0; i < T2 - T1; i++)
             {
                 if (ss[i] > 0)
                 {
                     if (ss[i] < val)
                     {
                         L = i;
                         val = ss[i];
                     }

                 }
             }
             var eigv = dec.V.Real();
             var ttt = eigv.GetSlice(0, T2 - T1 - 1, L, L);
             double currentArea = (ttt.T * QB * ttt)[0, 0];
             ttt = ttt.Multiply(Math.Sqrt(refArea / currentArea * 2d));

             var DIB = DoubleArray.From((slice1.Multiply(ED) as SparseDoubleArray).Multiply(slice2) as SparseDoubleArray);
             var DII = (slice1.Multiply(ED) as SparseDoubleArray).Multiply(slice1.T) as SparseDoubleArray;
             solver = new SparseLU(DII);

             var dx = solver.Solve(DIB.Multiply(ttt));
             var vert = new SparseDoubleArray(nParticles * 2, 1);
             for (int i = 0; i < nParticles * 2 - __boundary.Count() * 2; i++)
             {
                 vert[i, 0] = -dx[i, 0];
             }
             for (int i = 0; i < __boundary.Count() * 2; i++)
             {
                 vert[i + (nParticles * 2 - __boundary.Count() * 2), 0] = ttt[i, 0];
             }
             var G = shiftArray.Multiply(vert) as SparseDoubleArray;
             double cx = 0, cy = 0, ex = 0, ey = 0;
             for (int i = 0; i < nParticles; i++)
             {
                 x[i, 0] = G[i * 2, 0];
                 x[i, 1] = G[i * 2 + 1, 0];
                 x[i, 2] = 0;
                 cx += x[i, 0];
                 cy += x[i, 1];
                 ex += rX[i, 0];
                 ey += rX[i, 1];
             }
             cx /= nParticles;
             cy /= nParticles;
             ex /= nParticles;
             ey /= nParticles;
             for (int i = 0; i < nParticles; i++)
             {
                 x[i, 0] = x[i, 0] - cx + ex;
                 x[i, 1] = x[i, 1] - cy + ey;
                 x[i, 2] = 0;
             }
             _isFixedBoundary = false;
        }
    }
}