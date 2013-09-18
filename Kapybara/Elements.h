#pragma once
#include<iostream>
#include"Material.h"
#include"IntegratingPoint.h"
#include"Kapybara.h"
using namespace System;
namespace Kapybara3D {

	namespace Elements{
		public class iElement
		{
		public:
		    double Volume;
			double refVolume;
			___integratingPoint* iP;
			virtual void computeMetric()=0;
			virtual double computeVolume()=0;
			virtual void memoryMetric()=0;
			virtual void memoryVolume()=0;
			virtual void setupNodes(double* x)=0;
			virtual void setupIndex(int* f)=0;
			virtual void setupNodesFromList(double* x)=0;
			virtual void setRefVolume(double v)=0;
			virtual void computeHessEd()=0;
			virtual void computeGradient()=0;
			virtual void computeGlobalCoord()=0;
			virtual void copyGradient(double* ptr,int DOF)=0;
			virtual void copyGlobalCoord(double* ptr,int num)=0;
			virtual void copyStress(double* ptr,int n)=0;
			virtual void mergeGradient(double* ptr)=0;
			virtual void mergeHessian(ShoNS::Array::SparseDoubleArray ^hess,int dim)=0;
			virtual void computeBaseVectors()=0;
			virtual void computeEigenVectors()=0;
			virtual void copyBaseVectors(double* ptr,int num)=0;
			virtual double copyEigenVectors(double* ptr,double* ptr2,int num)=0;
			virtual double getNode(int i,int j)=0;
			virtual int getIndex(int i)=0;
			virtual void collectStar(array<System::Collections::Generic::List<array<double,1>^>^,1> ^f)=0;
		};

		template<int _nNode,int _elemDim,int _nIntPoint>
		public class element:public iElement
		{
		public:
			___integratingPoint _alias[_nIntPoint];
			double node[_nNode*__DIM];							//節点座標
			int index[_nNode];
			integratingPoint<_nNode,_elemDim> intP[_nIntPoint];	//積分点
			double gradient[_nNode*__DIM];
			double hess[_nNode*__DIM*_nNode*__DIM];
			double force[_nNode*__DIM];
		
		public:
			element()
			{
				iP=_alias;
				for(int i=0;i<_nNode*__DIM;i++)
				{
					index[i]=i;
				}
			}
			element(int i[_nNode])
			{
				iP=_alias;
				for(int i=0;i<_nNode*__DIM;i++)
				{
					index[i]=i[i];
				}
			}
			virtual double getNode(int i,int j)
			{
				return node[i*__DIM+j];
			}
			virtual int getIndex(int i)
			{
				return index[i];
			}
			virtual void setupNodes(double* x)
			{
				memcpy(node,x,sizeof(node));
			}
			virtual void setupNodesFromList(double* x)
			{
				for(int i=0;i<_nNode;i++)
				{
					for(int j=0;j<__DIM;j++)
					{
						node[i*__DIM+j]=x[index[i]*__DIM+j];
					}
				}
			}
			virtual void setupIndex(int* f)
			{
				memcpy(index,f,sizeof(index));
			}
			virtual double _Volume(){
				return Volume;
			}
			virtual void computeGlobalCoord()
			{
				for(int i=0;i<_nIntPoint;i++)
				{
					intP[i].computeGlobalCoord(node);
				}
			}
			virtual void computeMetric(){
				for(int i=0;i<_nIntPoint;i++)
				{
					intP[i].computeMetric(node);
				}
			}
			virtual void computeBaseVectors()
			{
				for(int i=0;i<_nIntPoint;i++)
				{
					intP[i].computeBaseVectors(node);
				}
			}
			virtual void computeEigenVectors()
			{
				for(int i=0;i<_nIntPoint;i++)
				{
					intP[i].computeEigenVectors();
				}
			}
			virtual double computeVolume(){
				double v=0;
				for(int i=0;i<_nIntPoint;i++)
				{
					v+=intP[i].weight*intP[i].dv;
				}
				this->Volume=v;
				return v;
			}
			virtual void memoryVolume(){
				this->refVolume=this->Volume;
			}
			virtual void setRefVolume(double v){
				this->refVolume=v;
			}
			
			
			virtual void memoryMetric(){
				for(int i=0;i<_nIntPoint;i++)
				{
					intP[i].memoryMetric();
				}
			}
			virtual void computeHessEd()
			{
				int nd=_nNode*__DIM;
				int ee=_elemDim*_elemDim;
				for(int i=0;i<nd;i++)
				{
					for(int j=0;j<nd;j++)
					{
						hess[j+i*nd]=0;
					}
				}
				for(int i=0;i<_nIntPoint;i++)
				{
					double val=intP[i].weight*intP[i].refDv*0.5;
					for(int j=0;j<ee;j++)
					{
						for(int k=0;k<nd;k++)
						{
							for(int l=0;l<nd;l++)
							{
								double D=intP[i].refInvMetric[j];
								double S=intP[i].B[j][l+k*nd];
								hess[l+k*nd]+=val*D*S;
							}
						}
					}
				}
			}

			virtual void computeGradient()
			{
				int nd=_nNode*__DIM;
				int ee=_elemDim*_elemDim;

				for(int i=0;i<nd;i++)
				{
					gradient[i]=0;
				}
				for(int i=0;i<_nIntPoint;i++)
				{
					double val=intP[i].weight*intP[i].dv*0.5;
					for(int j=0;j<ee;j++)
					{
						for(int k=0;k<nd;k++)
						{
							for(int l=0;l<nd;l++)
							{
								double D=intP[i].Cauchy[j];
								double S=intP[i].B[j][l+k*nd];
								double E=node[l];
								gradient[k]+=val*D*S*E;
							}
						}
					}
				}
			}
			virtual void copyGradient(double* ptr,int DOF)
			{
				for(int i=0;i<DOF;i++)
				{
					ptr[i]=gradient[i];
				}
			}
			virtual void copyGlobalCoord(double* ptr,int num)
			{
				memcpy(ptr,intP[num].globalCoord,sizeof(double)*__DIM);
			}
			virtual void copyBaseVectors(double* ptr,int num)
			{
				memcpy(ptr,intP[num].baseVectors,sizeof(double)*__DIM*_elemDim);

			}
			virtual double copyEigenVectors(double* ptr,double* ptr2, int num)
			{
				memcpy(ptr,intP[num].eigenVectors,sizeof(double)*__DIM*_elemDim);
				memcpy(ptr2,intP[num].eigenValues,sizeof(double)*_elemDim);
				return intP[num].weight*intP[num].refDv;
			}
			virtual void copyStress(double* ptr,int n){
				for(int i=0;i<_elemDim*_elemDim;i++)
				{
					ptr[i]=intP[n].Cauchy[i];
				}
			}
			virtual void mergeGradient(double *ptr)
			{
				for(int i=0;i<_nNode;i++)
				{
					for(int j=0;j<__DIM;j++)
					{
						ptr[this->index[i]*__DIM+j]+=this->gradient[i*__DIM+j];
					}
				}
			}
			virtual void mergeHessian(ShoNS::Array::SparseDoubleArray ^hess,int dim)
			{
				int nd=__DIM*_nNode;
				for(int i=0;i<_nNode;i++)
				{
					for(int j=0;j<dim;j++)
					{
						for(int k=0;k<_nNode;k++)
						{
							for(int l=0;l<dim;l++)
							{
								hess[this->index[i]*dim+j,this->index[k]*dim+l]+=this->hess[(i*__DIM+j)*nd+(k*__DIM+l)];
							}
						}
					}
				}
			}
		};

		template<int _nNode,int _elemDim>
		public class simplexElement:public element<_nNode,_elemDim,1>{
		public:
			static double _weight;
			static double _N[_nNode*__DIM*__DIM];
			static double _C[_elemDim][_nNode*__DIM*__DIM];
			static double _B[_elemDim*_elemDim][_nNode*__DIM*_nNode*__DIM];
			static double _localCoord[_elemDim];
		public:
			virtual void collectStar(array<System::Collections::Generic::List<array<double,1>^>^,1> ^f)
			{
				if(_elemDim==2)
				{
					int P1=0;
					int P2=1;
					int P3=2;
					for(double t=0.125;t<1.0;t+=0.125)
					{
						f[this->index[P1]]->Add(gcnew array<double,1>{this->node[P2*3+0]*t+this->node[P3*3+0]*(1-t),this->node[P2*3+1]*t+this->node[P3*3+1]*(1-t),this->node[P2*3+2]*t+this->node[P3*3+2]*(1-t)});
					}
					P1=1;
					P2=2;
					P3=0;
					for(double t=0.125;t<1.0;t+=0.125)
					{
						f[this->index[P1]]->Add(gcnew array<double,1>{this->node[P2*3+0]*t+this->node[P3*3+0]*(1-t),this->node[P2*3+1]*t+this->node[P3*3+1]*(1-t),this->node[P2*3+2]*t+this->node[P3*3+2]*(1-t)});
					}
					P1=2;
					P2=0;
					P3=1;
					for(double t=0.125;t<1.0;t+=0.125)
					{
						f[this->index[P1]]->Add(gcnew array<double,1>{this->node[P2*3+0]*t+this->node[P3*3+0]*(1-t),this->node[P2*3+1]*t+this->node[P3*3+1]*(1-t),this->node[P2*3+2]*t+this->node[P3*3+2]*(1-t)});
					}
				}
			}
			
			simplexElement()
			{
				static bool isInitialized=false;			//初期化済みか？
				if(_nNode!=_elemDim+1)
					throw gcnew System::ArgumentException("_nNode must be _elemDim+1");
				if(!isInitialized)
				{
					isInitialized=true;
					_weight=1.0;							//1,1/2,1/6...
					for(int j=1;j<=_elemDim;j++)
					{
						_weight/=j;
					}
					for(int j=0;j<_nNode*__DIM*__DIM;j++)
					{
						_N[j]=0;
					}
					for(int k=0;k<__DIM;k++)
					{
						for(int i=k;i<_nNode*__DIM;i+=__DIM)
						{
							_N[i+(k*_nNode*__DIM)]=1./_nNode;		//重心の位置は単純な総和平均
						}
					}
					for(int i=0;i<_elemDim;i++)
					{
						_localCoord[i]=(i+1.)/_nNode;		//重心の座標は例えば{1/3,2/3}
					}
					for(int j=0;j<_elemDim;j++)
					{
						for(int i=0;i<_nNode*__DIM*__DIM;i++)
						{
							_C[j][i]=0;
						}
						for(int k=0;k<__DIM;k++)
						{
							_C[j][k+__DIM*j+(k*_nNode*__DIM)]=1;
							_C[j][k+__DIM*(j+1)+(k*_nNode*__DIM)]=-1;
						}
					}
					
					integratingPoint<_nNode,_elemDim>::___CtoB(_C,_B);
				}
				intP[0].weight=_weight;
				intP[0].N=_N;
				intP[0].C=_C;
				intP[0].B=_B;
				intP[0].localCoord=_localCoord;
				intP[0].makeAlias(&iP[0]);
			}
		};
		
		template<int _nNode,int _elemDim,int _nIntPoint>
		public class isoparametricElement:public element<_nNode,_elemDim,_nIntPoint>{
		private:
			static const double ___cu[3];
			static const double ___pu[3];
			static double _weight[_nIntPoint];
			static double _N[_nIntPoint][_nNode*__DIM*__DIM];
			static double _C[_nIntPoint][_elemDim][_nNode*__DIM*__DIM];
			static double _B[_nIntPoint][_elemDim*_elemDim][_nNode*__DIM*_nNode*__DIM];
			static double _localCoord[_nIntPoint][_elemDim];
		public:
			virtual void collectStar(array<System::Collections::Generic::List<array<double,1>^>^,1> ^f)
			{
				if(_elemDim==2)
				{
					int P1=0;
					int P2=1;
					int P3=3;
					int P4=2;
					for(double t=0.125;t<1.0;t+=0.125)
					{
						f[this->index[P1]]->Add(gcnew array<double,1>{this->node[P2*3+0]*t+this->node[P3*3+0]*(1-t),this->node[P2*3+1]*t+this->node[P3*3+1]*(1-t),this->node[P2*3+2]*t+this->node[P3*3+2]*(1-t)});
						f[this->index[P1]]->Add(gcnew array<double,1>{this->node[P4*3+0]*t+this->node[P4*3+0]*(1-t),this->node[P4*3+1]*t+this->node[P4*3+1]*(1-t),this->node[P4*3+2]*t+this->node[P4*3+2]*(1-t)});
					}
					P1=1;
					P2=3;
					P3=2;
					P4=0;
					for(double t=0.125;t<1.0;t+=0.125)
					{
						f[this->index[P1]]->Add(gcnew array<double,1>{this->node[P2*3+0]*t+this->node[P3*3+0]*(1-t),this->node[P2*3+1]*t+this->node[P3*3+1]*(1-t),this->node[P2*3+2]*t+this->node[P3*3+2]*(1-t)});
						f[this->index[P1]]->Add(gcnew array<double,1>{this->node[P4*3+0]*t+this->node[P4*3+0]*(1-t),this->node[P4*3+1]*t+this->node[P4*3+1]*(1-t),this->node[P4*3+2]*t+this->node[P4*3+2]*(1-t)});
					}
					P1=3;
					P2=2;
					P3=0;
					P4=1;
					for(double t=0.125;t<1.0;t+=0.125)
					{
						f[this->index[P1]]->Add(gcnew array<double,1>{this->node[P2*3+0]*t+this->node[P3*3+0]*(1-t),this->node[P2*3+1]*t+this->node[P3*3+1]*(1-t),this->node[P2*3+2]*t+this->node[P3*3+2]*(1-t)});
						f[this->index[P1]]->Add(gcnew array<double,1>{this->node[P4*3+0]*t+this->node[P4*3+0]*(1-t),this->node[P4*3+1]*t+this->node[P4*3+1]*(1-t),this->node[P4*3+2]*t+this->node[P4*3+2]*(1-t)});
					}
					P1=2;
					P2=0;
					P3=1;
					P4=3;
					for(double t=0.125;t<1.0;t+=0.125)
					{
						f[this->index[P1]]->Add(gcnew array<double,1>{this->node[P2*3+0]*t+this->node[P3*3+0]*(1-t),this->node[P2*3+1]*t+this->node[P3*3+1]*(1-t),this->node[P2*3+2]*t+this->node[P3*3+2]*(1-t)});
						f[this->index[P1]]->Add(gcnew array<double,1>{this->node[P4*3+0]*t+this->node[P4*3+0]*(1-t),this->node[P4*3+1]*t+this->node[P4*3+1]*(1-t),this->node[P4*3+2]*t+this->node[P4*3+2]*(1-t)});
					}
				}
			}

			isoparametricElement()
			{
				if(_nNode!=std::pow(2.0,_elemDim))
				{
					throw gcnew System::ArgumentException("_nNode must be 2^_elemDim");
				}
				if(_nIntPoint!=std::pow(3.0,_elemDim))
				{
					throw gcnew System::ArgumentException("_nNode must be 3^_elemDim");
				}
				static int ss[_nIntPoint][_elemDim];		//積分点インデクス
				static int dd[_nNode][_elemDim];			//節点インデクス
				static bool isInitialized=false;			//初期化済みか？
				if(!isInitialized)
				{
					isInitialized=true;
					//積分点インデクス作成
					for(int i=0;i<_elemDim;i++)
					{
						ss[0][i]=0;
					}
					for(int i=1;i<_nIntPoint;i++)
					{
						for(int j=0;j<_elemDim;j++)
						{
							ss[i][j]=ss[i-1][j];
						}
						for(int j=0;j<_elemDim;j++)
						{
							if(ss[i][j]<2)
							{
								ss[i][j]++;
								for(int k=0;k<j;k++)
								{
									ss[i][k]=0;
								}
								break;
							}
						}
					}
					//節点インデクス作成
					for(int i=0;i<_elemDim;i++)
					{
						dd[0][i]=0;
					}
					for(int i=1;i<_nNode;i++)
					{
						for(int j=0;j<_elemDim;j++)
						{
							dd[i][j]=dd[i-1][j];
						}
						for(int j=0;j<_elemDim;j++)
						{
							if(dd[i][j]<1)
							{
								dd[i][j]++;
								for(int k=0;k<j;k++)
								{
									dd[i][k]=0;
								}
								break;
							}
						}
					}

					//積分点係数計算
					for(int i=0;i<_nIntPoint;i++)
					{
						//重み
						_weight[i]=1.0;
						for(int j=0;j<_elemDim;j++)
						{
							_localCoord[i][j]=___cu[ss[i][j]];
							_weight[i]*=___pu[ss[i][j]];
						}
						//形状関数計算用ベクトル
						double hh[_elemDim][2];
						for(int j=0;j<_elemDim;j++)
						{
							hh[j][0]=_localCoord[i][j];
							hh[j][1]=1.-_localCoord[i][j];
						}
						for(int j=0;j<_nNode*__DIM*__DIM;j++)
						{
							_N[i][j]=0;
						}
						for(int k=0;k<_nNode;k++)
						{
							//形状関数
							double N=1.0;
							for(int j=0;j<_elemDim;j++)
							{
								N*=hh[j][dd[k][j]];
							}
							for(int j=0;j<__DIM;j++)
							{
								_N[i][k*__DIM+j+j*_nNode*__DIM]=N;
							}
						}
						//基底用係数
						for(int j=0;j<_elemDim;j++)
						{
							for(int k=0;k<_elemDim;k++)
							{
								if(k==j)
								{
									hh[k][0]=1;
									hh[k][1]=-1;
								}else
								{
									hh[k][0]=_localCoord[i][k];
									hh[k][1]=1.-_localCoord[i][k];
								}
							}
							for(int f=0;f<_nNode*__DIM*__DIM;f++)
							{
								_C[i][j][f]=0;
							}
							for(int l=0;l<_nNode;l++)
							{
								//基底用係数
								double C=1.0;
								for(int k=0;k<_elemDim;k++)
								{
									C*=hh[k][dd[l][k]];
								}
								for(int k=0;k<__DIM;k++)
								{
									_C[i][j][l*__DIM+k+k*_nNode*__DIM]=C;
								}
							}
						}
						//計量用インデクス更新
						integratingPoint<_nNode,_elemDim>::___CtoB(_C[i],_B[i]);
					}
				}
				//コピー
				for(int i=0;i<_nIntPoint;i++)
				{
					intP[i].weight=_weight[i];
					intP[i].N=_N[i];
					intP[i].C=_C[i];
					intP[i].B=_B[i];
					intP[i].localCoord=_localCoord[i];
					intP[i].makeAlias(&iP[i]);
				}
			}
		};

	}
}