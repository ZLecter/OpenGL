using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1{
    class Program{

		#region
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
                vec4(vertexPosition,1);
            }
        ";
		#endregion

		#region
		public static string FragmentShader = @"
            #version 130
			uniform sampler2D texture;
            
			in vec2 uv;
			out vec4 fragment;
            
            void main(void){
				fragment = texture2D(texture, uv);
            }
        ";
		#endregion

		private static int width = 1280, height = 720;
        private static ShaderProgram program;
		private static VBO<Vector3> cube;
		private static VBO<int> cubeElements;

		private static VBO<Vector2> cubeUV;
		private static Texture crateTexture;

		private static int FACES = 100;
		private static int RADIUS = 2;

		private static System.Diagnostics.Stopwatch watch;

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


			float anglePerFace = (float) 360 / FACES;
			Console.WriteLine("ANGLE PER FACE: " + anglePerFace);
			Vector3[] v= new Vector3[FACES*3];
			Vector3 prevVert = new Vector3(RADIUS, 0, 0);
			for(int i = 0, currentFace = 1; i < FACES * 3; i += 3, currentFace++){
				float angle = currentFace * anglePerFace;
				float x = (float) Math.Cos(angle * (Math.PI / 180)) * RADIUS;
				float y = (float) Math.Sin(angle * (Math.PI / 180)) * RADIUS;
				v[i] = new Vector3(0, 0, 0);
				v[i + 1] = prevVert;
				v[i + 2] = new Vector3(x, y, 0);
				prevVert = v[i + 2];

				Console.WriteLine(angle);
				Console.WriteLine("Current Face: " + currentFace);
				Console.WriteLine("VERTEX: " + i + " X: " + v[i].X + " Y: " + v[i].Y);
				Console.WriteLine("VERTEX: " + (i+1) + " X: " + v[i+1].X + " Y: " + v[i+1].Y);
				Console.WriteLine("VERTEX: " + (i+2) + " X: " + v[i+2].X + " Y: " + v[i+2].Y);
				Console.WriteLine("---------------");
				
			}
			
			#region
			crateTexture = new Texture("crate.jpg");
			cube = new VBO<Vector3>(v);

			int[] ce = new int[FACES * 3];
			for(int i = 0; i < FACES*3; i++){
				ce[i] = i;
				Console.Write(i + " ");
			}
			cubeElements = new VBO<int>(
				ce,
				BufferTarget.ElementArrayBuffer
			);

			Vector2[] uv = new Vector2[FACES * 4];
			for(int i = 0; i < FACES * 4; i+=4){
				uv[i] = new Vector2(0, 0);
				uv[i + 1] = new Vector2(1, 0);
				uv[i + 2] = new Vector2(1, 1);
				uv[i + 3] = new Vector2(0, 1);
			}
			cubeUV = new VBO<Vector2>(uv);
			#endregion

			watch = System.Diagnostics.Stopwatch.StartNew();

			Glut.glutMainLoop();

        }

        private static void OnClose(){
            //Delete all resources on closing
			cube.Dispose();
            cubeElements.Dispose();
			cubeUV.Dispose();

			program.DisposeChildren = true;
            program.Dispose();
        }

        private static void OnRenderFrame(){
			//Clock tick used for animation
			watch.Stop();
			float deltaTime = (float)watch.ElapsedTicks/System.Diagnostics.Stopwatch.Frequency;
			watch.Restart();

            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit 
                | ClearBufferMask.DepthBufferBit);

            //Use shader
            Gl.UseProgram(program);

			//Drawing cube
			#region
			Gl.BindTexture(crateTexture);
			program["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(0,0,0)));
			Gl.BindBufferToShaderAttribute(cube, program, "vertexPosition");
			Gl.BindBufferToShaderAttribute(cubeUV, program, "vertexUV");
			Gl.BindBuffer(cubeElements);
			//Dibuja el cube
			Gl.DrawElements(BeginMode.Triangles, cubeElements.Count,
				DrawElementsType.UnsignedInt, IntPtr.Zero);
			#endregion

			
			Glut.glutSwapBuffers();
        }

        private static void OnDisplay(){

        }

    }
}
