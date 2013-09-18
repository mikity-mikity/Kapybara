#include "stdafx.h"
#include"Elements.h"
#include"NurbsSurface.h"
#include"ManagedElements.h"
#include"IntegratingPoint.h"
namespace Kapybara3D {
	namespace Elements{
		template<int _nNode,int _elemDim> double simplexElement<_nNode,_elemDim>::_weight;
		template<int _nNode,int _elemDim> double simplexElement<_nNode,_elemDim>::_N[_nNode*__DIM*__DIM];
		template<int _nNode,int _elemDim> double simplexElement<_nNode,_elemDim>::_C[_elemDim][_nNode*__DIM*__DIM];
		template<int _nNode,int _elemDim> double simplexElement<_nNode,_elemDim>::_B[_elemDim*_elemDim][_nNode*__DIM*_nNode*__DIM];
		template<int _nNode,int _elemDim> double simplexElement<_nNode,_elemDim>::_localCoord[_elemDim];

		template<int _nNode,int _elemDim,int _nIntPoint> const double isoparametricElement<_nNode,_elemDim,_nIntPoint>::___cu[3]={(1-std::sqrt(0.6))/2.,0.5,(1+std::sqrt(0.6))/2.};
		template<int _nNode,int _elemDim,int _nIntPoint> const double isoparametricElement<_nNode,_elemDim,_nIntPoint>::___pu[3]={5./18.,8./18.,5./18.};
		template<int _nNode,int _elemDim,int _nIntPoint> double isoparametricElement<_nNode,_elemDim,_nIntPoint>::_weight[_nIntPoint];
		template<int _nNode,int _elemDim,int _nIntPoint> double isoparametricElement<_nNode,_elemDim,_nIntPoint>::_N[_nIntPoint][_nNode*__DIM*__DIM];
		template<int _nNode,int _elemDim,int _nIntPoint> double isoparametricElement<_nNode,_elemDim,_nIntPoint>::_C[_nIntPoint][_elemDim][_nNode*__DIM*__DIM];
		template<int _nNode,int _elemDim,int _nIntPoint> double isoparametricElement<_nNode,_elemDim,_nIntPoint>::_B[_nIntPoint][_elemDim*_elemDim][_nNode*__DIM*_nNode*__DIM];
		template<int _nNode,int _elemDim,int _nIntPoint> double isoparametricElement<_nNode,_elemDim,_nIntPoint>::_localCoord[_nIntPoint][_elemDim];	
#define __nIntPoint (_uDim+1)*(_vDim+1)
#define __nNode (_uDim)*(_vDim)
#define __elemDim 2

		template<int _uDim,int _vDim> double nurbsSurfaceElement<_uDim,_vDim>::_weight[__nIntPoint];
		template<int _uDim,int _vDim> double nurbsSurfaceElement<_uDim,_vDim>::_localCoord[__nIntPoint][__elemDim];
		template<int _uDim,int _vDim> double nurbsSurfaceElement<_uDim,_vDim>::_cu[__elemDim][_uDim+1];
		template<int _uDim,int _vDim> double nurbsSurfaceElement<_uDim,_vDim>::_pu[__elemDim][_uDim+1];
#undef __nIntPoint
#undef __nNode
#undef __elemDim
	}
}
