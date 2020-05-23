using ESRI.ArcGIS.Analyst3DTools;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 提供Arcgis Engine操作的方法
    /// </summary>
    public static class AEOperation
    {
        /// <summary>
        /// 创建矢量图层
        /// </summary>
        /// <param name="path">图层路径</param>
        /// <param name="sr">参考系</param>
        /// <param name="attributeNames">属性名称</param>
        /// <param name="attributeTypes">属性类型</param>
        /// <param name="shapeType">图层形状</param>
        ///<returns>要素图层</returns>
        public static IFeatureLayer CreateFeatureLayer(string path, ISpatialReference sr, string[] attributeNames, string[] attributeTypes, string shapeType)
        {
            IWorkspaceFactory xjWsF = new ShapefileWorkspaceFactoryClass();
            IWorkspaceFactoryLockControl ipWsFactoryLock = (IWorkspaceFactoryLockControl)xjWsF;
            if (ipWsFactoryLock.SchemaLockingEnabled)
            {
                ipWsFactoryLock.DisableSchemaLocking();
            }
            IFeatureWorkspace xjFWs = (IFeatureWorkspace)xjWsF.OpenFromFile(System.IO.Path.GetDirectoryName(path), 0);
            //新建字段集
            IFields xjFields = new FieldsClass();
            IFieldsEdit xjFieldsEdit = (IFieldsEdit)xjFields;

            //新建Shape字段
            IField xjField = new FieldClass();
            IFieldEdit xjFieldEdit = (IFieldEdit)xjField;
            xjFieldEdit.Name_2 = "Shape";//字段名
            xjFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;//字段类型
            IGeometryDef xjGeometryDef = new GeometryDefClass();
            IGeometryDefEdit xjGDefEdit = (IGeometryDefEdit)xjGeometryDef;
            if (shapeType.Equals("P"))
                xjGDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;//几何类型
            else if (shapeType.Equals("L"))
                xjGDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;//几何类型
            else
                xjGDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;//几何类型
            xjGDefEdit.SpatialReference_2 = sr;//参考系
            xjFieldEdit.GeometryDef_2 = xjGeometryDef;
            xjFieldsEdit.AddField(xjField);

            IFeatureClass xjFeatureClass;
            IFeatureLayer fl = new FeatureLayerClass();
            //新建其他字段
            if (attributeNames == null)
            {
                xjFeatureClass = xjFWs.CreateFeatureClass(System.IO.Path.GetFileName(path), xjFields, null, null, esriFeatureType.esriFTSimple, "Shape", "");
                fl.FeatureClass = xjFeatureClass;
                return fl;
            }
            for (int i = 0; i < attributeNames.Length; i++)
            {
                IField xjField2 = new FieldClass();
                IFieldEdit xjFieldEdit2 = xjField2 as IFieldEdit;
                xjFieldEdit2.Name_2 = attributeNames[i];
                xjFieldEdit2.Type_2 = GetType(attributeTypes[i]);
                xjFieldsEdit.AddField(xjField2);
            }
            xjFeatureClass = xjFWs.CreateFeatureClass(System.IO.Path.GetFileName(path), xjFields, null, null, esriFeatureType.esriFTSimple, "Shape", "");
            fl.FeatureClass = xjFeatureClass;
            return fl;
        }

        /// <summary>
        /// 获得字段类型
        /// </summary>
        /// <param name="type">字符串表示</param>
        /// <returns>枚举表示</returns>
        private static esriFieldType GetType(string type)
        {
            switch (type)
            {
                case "D":
                    return esriFieldType.esriFieldTypeDouble;
                case "F":
                    return esriFieldType.esriFieldTypeSingle;
                case "I":
                    return esriFieldType.esriFieldTypeInteger;
                case "S":
                    return esriFieldType.esriFieldTypeSmallInteger;
                default:
                    return esriFieldType.esriFieldTypeString;
            }
        }

        /// <summary>
        /// 为除了OID和Shape字段外的所有属性添加值的方法
        /// </summary>
        /// <param name="fc">(点和线)要素图层</param>
        /// <param name="points">不多于2个的点集</param>
        /// <param name="objs">添加值</param>
        public static void AddFeature(IFeatureClass fc,Point3D[] points,object[] objs)
        {
            if(fc.ShapeType==esriGeometryType.esriGeometryPoint)
            {
                IPoint pt = new PointClass();
                pt.PutCoords(points[0].x, points[0].y);
                IFeature feature = fc.CreateFeature();
                feature.Shape = pt;
                if (objs != null)
                {
                    for (int i = 2; i < 2 + objs.Length; i++)
                        feature.set_Value(i, objs[i - 2]);
                }
                feature.Store();
            }
            else if (fc.ShapeType == esriGeometryType.esriGeometryPolyline)
            {
                IPolyline line = new PolylineClass();
                IPoint from = new PointClass();
                from.PutCoords(points[0].x, points[0].y);
                IPoint to = new PointClass();
                to.PutCoords(points[1].x, points[1].y);
                line.FromPoint = from;
                line.ToPoint = to;
                IFeature feature = fc.CreateFeature();
                feature.Shape =line;
                if (objs != null)
                {
                    for (int i = 2; i < 2 + objs.Length; i++)
                        feature.set_Value(i, objs[i - 2]);
                }
                feature.Store();
            }
        }

        /// <summary>
        /// 获得指定文件夹的工作区间要素编辑接口
        /// </summary>
        /// <param name="folder">文件夹路径</param>
        /// <returns>要素编辑接口</returns>
        public static IWorkspaceEdit GetEdit(string folder)
        {
            IWorkspaceFactory wsf = new ShapefileWorkspaceFactoryClass();
            IWorkspaceFactoryLockControl ipWsFactoryLock = (IWorkspaceFactoryLockControl)wsf;
            if (ipWsFactoryLock.SchemaLockingEnabled)
            {
                ipWsFactoryLock.DisableSchemaLocking();
            }
            IFeatureWorkspace fws = (IFeatureWorkspace)wsf.OpenFromFile(folder, 0);
            IWorkspaceEdit wsEdit = fws as IWorkspaceEdit;
            return wsEdit;
        }

        /// <summary>
        /// 为要素图层设置符号
        /// </summary>
        /// <param name="fl">要素图层</param>
        /// <param name="paras">4参数（颜色和宽度）表示线图层，5参数（颜色、形状、大小）表示点图层</param>
        public static void SetSymbol(IFeatureLayer fl,object[] paras)
        {
            //设置线图层的颜色、线宽
            if (paras.Length == 4)
            {
                ISimpleLineSymbol ls = new SimpleLineSymbolClass();
                IRgbColor color = new RgbColorClass();
                color.Red = (int)paras[0]; color.Green = (int)paras[1]; color.Blue = (int)paras[2];
                ls.Color = color;
                ls.Width = (double)paras[3];
                ISimpleRenderer render = new SimpleRendererClass();
                render.Symbol = ls as ISymbol;
                (fl as IGeoFeatureLayer).Renderer = render as IFeatureRenderer;
            }
            else if (paras.Length == 5)//设置点图层的颜色、大小、形状
            {
                ISimpleMarkerSymbol ms = new SimpleMarkerSymbolClass();
                IRgbColor color = new RgbColorClass();
                color.Red = (int)paras[0]; color.Green = (int)paras[1]; color.Blue = (int)paras[2];
                ms.Color = color;
                ms.Style = (esriSimpleMarkerStyle)paras[3];
                ms.Size =(double)paras[4];
                ISimpleRenderer render = new SimpleRendererClass();
                render.Symbol = ms as ISymbol;
                (fl as IGeoFeatureLayer).Renderer = render as IFeatureRenderer;
            }
        }

        /// <summary>
        /// 点云转图层
        /// </summary>
        /// <param name="path">保存路径</param>
        /// <param name="sr">参考系</param>
        /// <param name="pc">三维点云</param>
        /// <returns>图层要素</returns>
        public static IFeatureClass CreatePointCloudFeature(string path, ISpatialReference sr, List<Point3D> pc)
        {
            string folder = System.IO.Path.GetDirectoryName(path);
            //先将点云转换成点图层
            IFeatureClass fc = (CreateFeatureLayer(folder + "\\PointCloud.shp", sr, new string[] { "x", "y", "z" }, new string[] { "D", "D", "D" }, "P")).FeatureClass;
            IWorkspaceEdit wsEdit = GetEdit(folder);
            wsEdit.StartEditing(false);
            wsEdit.StartEditOperation();
            foreach (Point3D point in pc)
                AddFeature(fc, new Point3D[] { point }, new object[] { point.x, point.y, point.z });
            wsEdit.StopEditing(true);
            wsEdit.StopEditOperation();
            return fc;
        }

        /// <summary>
        /// 高程点生成TIN
        /// </summary>
        /// <param name="featureClass">点要素</param>
        /// <param name="path">保存路径</param>
        /// <returns>TIN数据</returns>
        public static ITin CreateTIN(IFeatureClass featureClass, string path)
        {
            string folder = System.IO.Path.GetDirectoryName(path);
            IGeoDataset geoDataset = featureClass as IGeoDataset;
            IField zField = featureClass.Fields.get_Field(4);//高程字段
            ITinEdit tinEdit = new TinClass();
            tinEdit.InitNew(geoDataset.Extent);
            tinEdit.AddFromFeatureClass(featureClass, null, zField, null, esriTinSurfaceType.esriTinMassPoint);
            if (System.IO.Directory.Exists(folder + "\\tin"))
                (new System.IO.DirectoryInfo(folder + "\\tin")).Delete(true);
            tinEdit.SaveAs(folder + "\\tin");
            tinEdit.Refresh();
            tinEdit.StopEditing(true);
            return tinEdit as ITin;
        }

        /// <summary>
        /// TIN转DEM
        /// </summary>
        /// <param name="tin">TIN数据</param>
        /// <param name="path">DEM数据</param>
        /// <param name="pixelSize">像元大小</param>
        public static void TIN2DEM(ITin tin, string path, double pixelSize)
        {
            Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;
            TinRaster tinRaster = new TinRaster();
            tinRaster.in_tin = tin;
            tinRaster.out_raster = path;
            tinRaster.data_type = "FLOAT";
            tinRaster.method = "NATURAL_NEIGHBORS";
            tinRaster.sample_distance = "CELLSIZE " + pixelSize;
            tinRaster.z_factor = 1;
            gp.Execute(tinRaster, null);
        }
    }
}
