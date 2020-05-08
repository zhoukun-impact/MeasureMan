﻿using CSharpGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureMan
{
    /// <summary>
    /// 线模型结点
    /// </summary>
    public class LegacyLineNode : SceneNodeBase, IRenderable
    {
        /// <summary>
        /// 线段结点1
        /// </summary>
        public vec3 Vertex0 { get; set; }

        /// <summary>
        /// 线段结点2
        /// </summary>
        public vec3 Vertex1 { get; set; }

        /// <summary>
        /// 结点1颜色
        /// </summary>
        public vec3 Color0 { get; set; }

        /// <summary>
        /// 结点2颜色
        /// </summary>
        public vec3 Color1 { get; set; }

        /// <summary>
        /// 多边形模式
        /// </summary>
        public PolygonMode PolygonMode
        {
            get { return this.polygonModeState.Mode; }
            set { this.polygonModeState.Mode = value; }
        }

        /// <summary>
        /// 线段宽度
        /// </summary>
        public float LineWidth
        {
            get { return this.lineWidthState.LineWidth; }
            set { this.lineWidthState.LineWidth = value; }
        }

        /// <summary>
        /// 结点大小
        /// </summary>
        public float PointSize
        {
            get { return this.pointSizeState.PointSize; }
            set { this.pointSizeState.PointSize = value; }
        }

        /// <summary>
        /// 线模型初始化方法
        /// </summary>
        public LegacyLineNode()
        {
            this.Color0 = new vec3(1, 0, 0);
            this.Color1 = new vec3(1, 0, 0);
        }

        #region IRenderable 成员

        private ThreeFlags enableRendering = ThreeFlags.BeforeChildren | ThreeFlags.Children;
        public ThreeFlags EnableRendering
        {
            get { return this.enableRendering; }
            set { this.enableRendering = value; }
        }

        private GLSwitch polygonOffsetState = new PolygonOffsetFillSwitch();
        private PolygonModeSwitch polygonModeState = new PolygonModeSwitch(PolygonMode.Line);
        private LineWidthSwitch lineWidthState = new LineWidthSwitch(1.0f);
        private PointSizeSwitch pointSizeState = new PointSizeSwitch(1.0f);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        public void RenderBeforeChildren(RenderEventArgs arg)
        {
            this.PushProjectionViewMatrix(arg);
            this.PushModelMatrix();

            this.polygonOffsetState.On();
            this.polygonModeState.On();
            this.lineWidthState.On();
            this.pointSizeState.On();

            GL.Instance.Begin((uint)DrawMode.Lines);
            GL.Instance.Color3f(this.Color0.x, this.Color0.y, this.Color0.z);
            GL.Instance.Vertex3f(this.Vertex0.x, this.Vertex0.y, this.Vertex0.z);
            GL.Instance.Color3f(this.Color1.x, this.Color1.y, this.Color1.z);
            GL.Instance.Vertex3f(this.Vertex1.x, this.Vertex1.y, this.Vertex1.z);
            GL.Instance.End();

            this.pointSizeState.Off();
            this.lineWidthState.Off();
            this.polygonModeState.Off();
            this.polygonOffsetState.Off();

            this.PopModelMatrix();
            this.PopProjectionViewMatrix();
        }

        public void RenderAfterChildren(RenderEventArgs arg)
        {
        }

        #endregion
    }
}
