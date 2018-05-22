using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1{
    class Program{

        public static string VertexShader = @"
            #version 130
            
            in vec3 vertexPosition;
			in vec2 vertexUV;
			in vec3 vertexNormal;

			out vec2 uv;
			out vec3 normal;

            uniform mat4 projection_matrix;
            uniform mat4 view_matrix;
            uniform mat4 model_matrix;

            void main(void){
				normal = normalize((model_matrix *
									vec4(floor(vertexNormal), 0)).xyz);
				uv = vertexUV;
                gl_Position = projection_matrix 
                * view_matrix * model_matrix *
                vec4(vertexPosition,1);
            }
        ";

        public static string FragmentShader = @"
            #version 130
			uniform sampler2D texture;
			uniform vec3 light_direction;
			uniform bool enable_lighting;

			in vec3 normal;
			in vec2 uv;

			out vec4 fragment;
            
            void main(void){
				float diffuse = max(dot(normal, light_direction), 0);
				float ambient = 0.3;
				float lighting = (enable_lighting ? max(diffuse, ambient) : 1);
				fragment = lighting * texture2D(texture, uv);
            }
        ";

        private static int width = 1280, height = 720;
        private static ShaderProgram program;
		private static VBO<Vector3> cube;
		private static VBO<int> cubeElements;
		private static VBO<Vector2> cubeUV;
		private static Texture crateTexture;

		private static System.Diagnostics.Stopwatch watch;
		private static float angle;
		private static float speedS = 0.5f;

		private static float xangle, yangle;
		private static bool autoRotate, lighting = true, fullscreen;
		private static bool left, right, up, down;

        static void Main(string[] args){
			//Open GL init
			#region
			Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(1280,720);
            Glut.glutCreateWindow("Tu mama es maraca");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);
            Glut.glutCloseFunc(OnClose);
			Glut.glutReshapeFunc(OnReshape);
			//Keyboard functions
			Glut.glutKeyboardFunc(OnKeyboardDown);
			Glut.glutKeyboardUpFunc(OnKeyboardUp);

			Gl.ClearColor(0,0,0,1);

			//Alpha enabled
			
			Gl.Enable(EnableCap.DepthTest);
			Gl.Enable(EnableCap.Blend);
			//Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			
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
			//Create light
			program["light_direction"].SetValue(new Vector3(0,0,1));
			program["enable_lighting"].SetValue(lighting);



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

			watch = System.Diagnostics.Stopwatch.StartNew();

			Glut.glutMainLoop();

        }

		private static void OnReshape(int width, int height){
			Program.width = width;
			Program.height = height;
			program.Use();
			program["projection_matrix"].SetValue(
				Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width/height, 0.1f, 1000f));
		}

		private static void OnKeyboardDown(byte key, int x, int y){
			if(key == 'w')
				up = true;
			else if(key == 's')
				down = true;
			else if(key == 'd')
				right = true;
			else if(key == 'a')
				left = true;
			else if(key == 27)
				Glut.glutLeaveMainLoop();
		}

		private static void OnKeyboardUp(byte key, int x, int y){
			if(key == 'w')
				up = false;
			else if(key == 's')
				down = false;
			else if(key == 'd')
				right = false;
			else if(key == 'a')
				left = false;
			else if(key == 'l')
				lighting = !lighting;
			else if(key == 'f') {
				fullscreen = !fullscreen;
				if(fullscreen)
					Glut.glutFullScreen();
				else{
					Glut.glutPositionWindow(0,0);
					Glut.glutReshapeWindow(1280, 720);
				}
			} else if(key == 32)
				autoRotate = !autoRotate;
			else if(key == 27)
				Glut.glutLeaveMainLoop();
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

			//define rotations based on pressed keys
			if(autoRotate) {
				xangle += deltaTime / 2;
				yangle += deltaTime;
			}

			if(up)
				xangle -= deltaTime;
			if(down)
				xangle += deltaTime;
			if(left)
				yangle -= deltaTime;
			if(right)
				yangle += deltaTime;

            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit 
                | ClearBufferMask.DepthBufferBit);

            //Use shader
            Gl.UseProgram(program);

			//Drawing cube
			#region
			Gl.BindTexture(crateTexture);
			program["model_matrix"].SetValue(
				Matrix4.CreateRotationX(xangle) *
				Matrix4.CreateRotationY(yangle));
			program["enable_lighting"].SetValue(lighting);

			Gl.BindBufferToShaderAttribute(cube, program, "vertexPosition");
			Gl.BindBufferToShaderAttribute(cubeUV, program, "vertexUV");
			Gl.BindBuffer(cubeElements);
			
			Gl.DrawElements(BeginMode.Quads, cubeElements.Count,
				DrawElementsType.UnsignedInt, IntPtr.Zero);
			#endregion

			Glut.glutSwapBuffers();
        }

        private static void OnDisplay(){

        }

    }
}
