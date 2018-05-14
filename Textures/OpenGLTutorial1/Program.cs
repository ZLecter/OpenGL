using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1{
    class Program{

        public static string VertexShader = @"
            #version 130
            
            in vec3 vertexPosition;
			in vec2 vertexUV;

			out vec2 uv;

            uniform mat4 projection_matrix;
            uniform mat4 view_matrix;
            uniform mat4 model_matrix;

            void main(void){
				uv = vertexUV;
                gl_Position = projection_matrix 
                * view_matrix * model_matrix *
                vec4(vertexPosition,gl_Position.y);
            }
        ";

        public static string FragmentShader = @"
            #version 130
			uniform sampler2D texture;
            
			in vec2 uv;
			out vec4 fragment;
            
            void main(void){
				fragment = texture2D(texture, uv);
            }
        ";

        private static int width = 1280, height = 720;
        private static ShaderProgram program;
		private static VBO<Vector3> cube, rect, pyramid;
		private static VBO<int> cubeElements, rectElements, pyramidElements;

		private static VBO<Vector2> cubeUV, rectUV, pyUV;
		private static Texture crateTexture, rectTexture, pyTexture;

		private static System.Diagnostics.Stopwatch watch;
		private static float angle;
		private static float speedT1 = 1, speedS = 0.5f;

        static void Main(string[] args){
			//Open GL init
			#region
			Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width,height);
            Glut.glutCreateWindow("Tu mama es maraca");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);
            Glut.glutCloseFunc(OnClose);

			Gl.ClearColor(0,0,0,1);

			//Alpha enabled
			Gl.Enable(EnableCap.DepthTest);
			Gl.Enable(EnableCap.Blend);
			Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			#endregion

			//Load shader
			program = new ShaderProgram(VertexShader,FragmentShader);

            //Create perspective
            program.Use();
            program["projection_matrix"].SetValue(
                Matrix4.CreatePerspectiveFieldOfView(0.45f, 
                (float)width / height, 0.1f, 1000f));
            program["view_matrix"].SetValue(
                Matrix4.LookAt(new Vector3(0, 0, 10), 
                Vector3.Zero, new Vector3(0, 1, 0)));

			//Cube vertices and uv
			#region
			crateTexture = new Texture("crate.jpg");
			cube = new VBO<Vector3>(
				new Vector3[] {
					new Vector3(1,1,-1), new Vector3(-1,1,-1), new Vector3(-1,1,1), new Vector3(1,1,1),
					new Vector3(1,-1,1), new Vector3(-1,-1,1), new Vector3(-1,-1,-1), new Vector3(1,-1,-1),
					new Vector3(1,1,1), new Vector3(-1,1,1), new Vector3(-1,-1,1), new Vector3(1,-1,1),
					new Vector3(1,-1,-1), new Vector3(-1,-1,-1), new Vector3(-1,1,-1), new Vector3(1,1,-1),
					new Vector3(-1,1,1), new Vector3(-1,1,-1), new Vector3(-1,-1,-1), new Vector3(-1,-1,1),
					new Vector3(1,1,-1), new Vector3(1,1,1), new Vector3(1,-1,1), new Vector3(1,-1,-1),
				}
			);
			cubeElements = new VBO<int>(
				new int[] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23 },
				BufferTarget.ElementArrayBuffer
			);
			cubeUV = new VBO<Vector2>(
				new Vector2[] {
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
				}
			);
			#endregion

			//Rect vertices and uv
			#region
			rectTexture = new Texture("4922.jpg");
			rect = new VBO<Vector3>(
				new Vector3[] {
					new Vector3(2,1,-2), new Vector3(-2,1,-2), new Vector3(-2,1,2), new Vector3(2,1,2),
					new Vector3(2,-0,2), new Vector3(-2,-0,2), new Vector3(-2,-1,-2), new Vector3(2,-1,-2),
					new Vector3(2,1,2), new Vector3(-2,1,2), new Vector3(-2,0,2), new Vector3(2,0,2),
					new Vector3(2,-1,-2), new Vector3(-2,-1,-2), new Vector3(-2,1,-2), new Vector3(2,1,-2),
					new Vector3(-2,1,2), new Vector3(-2,1,-2), new Vector3(-2,-1,-2), new Vector3(-2,0,2),
					new Vector3(2,1,-2), new Vector3(2,1,2), new Vector3(2,0,2), new Vector3(2,-1,-2),
				}
			);
			rectElements = new VBO<int>(
				new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 },
				BufferTarget.ElementArrayBuffer
			);
			rectUV = new VBO<Vector2>(
				new Vector2[] {
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)
				}
			);
			#endregion

			//Pyramid vertices and uv
			#region
			pyTexture = new Texture("tile.jpg");
			pyramid = new VBO<Vector3>(
				new Vector3[] {
					new Vector3(1,0,1), new Vector3(-1,0,1), new Vector3(0.5f,1,-0.5f),
					new Vector3(1,0,-1), new Vector3(-1,0,-1), new Vector3(0.5f,1,-0.5f),
					new Vector3(1,0,1), new Vector3(1,0,-1), new Vector3(0.5f,1,-0.5f),
					new Vector3(-1,0,1), new Vector3(-1,0,-1), new Vector3(0.5f,1,-0.5f),
				}
			);
			pyramidElements = new VBO<int>(
				new int[] {0,1,2,3,4,5,6,7,8,9,10,11},
				BufferTarget.ElementArrayBuffer
			);
			pyUV = new VBO<Vector2>(
				new Vector2[] {
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),
					new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),
				}
			);
			#endregion


			watch = System.Diagnostics.Stopwatch.StartNew();

			Glut.glutMainLoop();

        }

        private static void OnClose(){
            //Delete all resources on closing
			cube.Dispose();
            cubeElements.Dispose();
			cubeUV.Dispose();

			rect.Dispose();
			rectElements.Dispose();
			rectUV.Dispose();

			pyramid.Dispose();
			pyramidElements.Dispose();
			pyUV.Dispose();

			program.DisposeChildren = true;
            program.Dispose();
        }

        private static void OnRenderFrame(){
			//Clock tick used for animation
			watch.Stop();
			float deltaTime = (float)watch.ElapsedTicks/System.Diagnostics.Stopwatch.Frequency;
			watch.Restart();
			angle += deltaTime;

            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit 
                | ClearBufferMask.DepthBufferBit);

            //Use shader
            Gl.UseProgram(program);

			//Drawing cube
			#region
			Gl.BindTexture(crateTexture);
			program["model_matrix"].SetValue(
				Matrix4.CreateRotationZ(angle * speedS) *
				Matrix4.CreateRotationY(angle * speedS) *
				Matrix4.CreateRotationX(angle * speedS) *
				Matrix4.CreateScaling(new Vector3(0.5f, 0.5f, 0.5f)) *
				Matrix4.CreateTranslation(new Vector3(0,0,-5)));
			Gl.BindBufferToShaderAttribute(cube, program, "vertexPosition");
			Gl.BindBufferToShaderAttribute(cubeUV, program, "vertexUV");
			Gl.BindBuffer(cubeElements);
			//Dibuja el cube
			Gl.DrawElements(BeginMode.Quads, cubeElements.Count,
				DrawElementsType.UnsignedInt, IntPtr.Zero);
			#endregion

			//Drawing rect
			#region
			Gl.BindTexture(rectTexture);
			program["model_matrix"].SetValue(
				Matrix4.CreateRotationY(angle * speedS) *
				Matrix4.CreateScaling(new Vector3(0.5f,0.5f,0.5f)) * 
				Matrix4.CreateTranslation(new Vector3(-3, 0, -2)));
			Gl.BindBufferToShaderAttribute(rect, program, "vertexPosition");
			Gl.BindBufferToShaderAttribute(rectUV, program, "vertexUV");
			Gl.BindBuffer(rectElements);
			Gl.DrawElements(BeginMode.Quads, rectElements.Count,
				DrawElementsType.UnsignedInt, IntPtr.Zero);
			#endregion

			//Drawing pyramid
			#region
			Gl.BindTexture(pyTexture);
			program["model_matrix"].SetValue(
				Matrix4.CreateRotationZ(angle * speedS) *
				Matrix4.CreateTranslation(new Vector3(2, 0, -2)));
			Gl.BindBufferToShaderAttribute(pyramid, program, "vertexPosition");
			Gl.BindBufferToShaderAttribute(pyUV, program, "vertexUV");
			Gl.BindBuffer(pyramidElements);
			Gl.DrawElements(BeginMode.Triangles, pyramidElements.Count,
				DrawElementsType.UnsignedInt, IntPtr.Zero);
			#endregion

			Glut.glutSwapBuffers();
        }

        private static void OnDisplay(){

        }

    }
}
