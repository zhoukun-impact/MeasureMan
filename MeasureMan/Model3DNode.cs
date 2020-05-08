using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpGL;

namespace MeasureMan
{
    /// <summary>
    /// CSharpGL认可的三维模型类
    /// </summary>
    class Model3DNode : PickableNode, IRenderable
    {
        private const string inPosition = "inPosition";
        private const string inColor = "inColor";
        private const string projectionMat = "projectionMat";
        private const string viewMat = "viewMat";
        private const string modelMat = "modelMat";
        private const string passColor = "passColor";
        private const string vertexCode =//for vertex shader
            @"#version 330 core

in vec3 " + inPosition + @";
in vec3 " + inColor + @";

uniform mat4 " + projectionMat + @";
uniform mat4 " + viewMat + @";
uniform mat4 " + modelMat + @";

out vec3 passColor;

void main(void) {
	gl_Position = projectionMat * viewMat * modelMat * vec4(inPosition, 1.0);
    passColor = inColor;
}
";
        private const string fragmentCode =//for fragment shader
            @"#version 330 core

in vec3 passColor;
uniform bool renderWireframe = false;

out vec4 outColor;

void main(void) {
    if (renderWireframe)
    {
	    outColor = vec4(1.0, 1.0, 1.0, 1.0);
    }
    else
    {
	    outColor = vec4(passColor, 1.0);
    }
}
";
        /// <summary>
        /// 点数量
        /// </summary>
        private int vertexCount;
        /// <summary>
        /// 面数量
        /// </summary>
        private int faceCount;
        /// <summary>
        /// 第一个点
        /// </summary>
        private Point3D firstPt;

        /// <summary>
        /// 创建CSharpGL认可的模型
        /// </summary>
        /// <param name="flag">0=sp,1=dp,2=model</param>
        /// <param name="path">点云路径</param>
        /// <returns>模型</returns>
        public static Model3DNode Create(int flag,string path)
        {
            IBufferSource model; vec3 size;
            Model3D m = new Model3D();
            size = m.GetModelSize();
            if (flag == 0)
                m.LoadSparsePC(path);
            else if (flag == 1)
                m.LoadDensePC(path);
            else if (flag == 2)
                m.LoadModel(path);
            model = m;

            string position = Model3D.strPosition;
            string color = Model3D.strColor;

            var vs = new VertexShader(vertexCode);
            var fs = new FragmentShader(fragmentCode);
            var provider = new ShaderArray(vs, fs);
            var map = new AttributeMap();
            map.Add(inPosition, position);
            map.Add(inColor, color);
            var builder = new RenderMethodBuilder(provider, map);
            var node = new Model3DNode(model, position, builder);
            node.Initialize();
            node.ModelSize = size;

            return node;
        }

        private Model3DNode(IBufferSource model, string positionNameInIBufferable, params RenderMethodBuilder[] builders)
            : base(model, positionNameInIBufferable, builders)
        {
            vertexCount=(model as Model3D).VertexCount();
            faceCount=(model as Model3D).FaceCount();
            firstPt = (model as Model3D).GetFirstPoint();
            this.RenderWireframe = false;//为false显示更加真实
            this.RenderBody = true;
        }

        /// <summary>
        /// 获取模型点数量
        /// </summary>
        /// <returns>点数量</returns>
        public int GetVertCount()
        {
            return vertexCount;
        }

        /// <summary>
        /// 获取模型面数量
        /// </summary>
        /// <returns>面数量</returns>
        public int GetFaceCount()
        {
            return faceCount;
        }

        /// <summary>
        /// 获得第一个点
        /// </summary>
        /// <returns>第一个点</returns>
        public Point3D GetFirstPoint()
        {
            return firstPt;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool RenderWireframe { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool RenderBody { get; set; }

        private PolygonModeSwitch polygonMode = new PolygonModeSwitch(PolygonMode.Line);
        private GLSwitch polygonOffsetState = new PolygonOffsetFillSwitch();

        /// <summary>
        /// for debugging.
        /// </summary>
        public float RotateSpeed { get; set; }

        private ThreeFlags enableRendering = ThreeFlags.BeforeChildren | ThreeFlags.Children | ThreeFlags.AfterChildren;
        /// <summary>
        /// Render before/after children? Render children? 
        /// RenderAction cares about this property. Other actions, maybe, maybe not, your choice.
        /// </summary>
        public ThreeFlags EnableRendering
        {
            get { return this.enableRendering; }
            set { this.enableRendering = value; }
        }

        public void RenderBeforeChildren(RenderEventArgs arg)
        {
            if (!this.IsInitialized) { this.Initialize(); }

            this.RotationAngle += this.RotateSpeed;

            ICamera camera = arg.Camera;
            mat4 projection = camera.GetProjectionMatrix();
            mat4 view = camera.GetViewMatrix();
            mat4 model = this.GetModelMatrix();

            var method = this.RenderUnit.Methods[0]; // the only render unit in this node.
            ShaderProgram program = method.Program;
            program.SetUniform(projectionMat, projection);
            program.SetUniform(viewMat, view);
            program.SetUniform(modelMat, model);

            if (this.RenderWireframe)
            {
                // render wireframe.
                program.SetUniform("renderWireframe", true);
                polygonMode.On();
                polygonOffsetState.On();
                method.Render();
                polygonOffsetState.Off();
                polygonMode.Off();
            }

            if (this.RenderBody)
            {
                // render solid body.
                program.SetUniform("renderWireframe", false);
                method.Render();
            }
        }

        public void RenderAfterChildren(RenderEventArgs arg)
        {
        }


        public override void RenderForPicking(PickingEventArgs arg)
        {
            if (this.RenderWireframe || this.RenderBody)
            {
                base.RenderForPicking(arg);
            }
        }

    }
}
