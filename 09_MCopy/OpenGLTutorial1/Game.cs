using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial1 {
	class Game {

		// Vertex Shader
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
		#endregion  vertex shader

		// Fragment Shader
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

		private int width = 800, height = 640;
		private ShaderProgram program;
		
		private Map map;
		public static Player player;
		public static float xCam = 0, yCam = 0, zCam = 0, mov = 1.0f;
		public static float ryCam = 0;

		public Game() {
			Init();
		}

		public void Init() {
			//Open GL init
			#region
			Glut.glutInit();
			Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
			Glut.glutInitWindowSize(width, height);
			Glut.glutCreateWindow("Maraca craft");

			Glut.glutIdleFunc(OnRenderFrame);
			Glut.glutDisplayFunc(OnDisplay);
			Glut.glutCloseFunc(OnClose);
			Glut.glutKeyboardFunc(OnKeyboardDown);

			Gl.ClearColor(0, 0, 0, 1);

			//Alpha enabled
			Gl.Enable(EnableCap.DepthTest);
			Gl.Enable(EnableCap.Blend);
			Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			#endregion

			//Load shader
			program = new ShaderProgram(VertexShader, FragmentShader);

			//Create perspective
			program.Use();
			program["projection_matrix"].SetValue(
				Matrix4.CreatePerspectiveFieldOfView(0.95f,
				(float)width / height, 0.1f, 1000f));
			Vector3 camPos = new Vector3(0,0,0);
			//-5 3 -5
			program["view_matrix"].SetValue(
				Matrix4.CreateTranslation(new Vector3(0,0,0)) *
				Matrix4.LookAt(new Vector3(0, 0, -1),
				camPos,
				new Vector3(0, 1, 0)));


			player = new Player(0,0,0);
			CreateModels();

			Glut.glutMainLoop();
		}

		private void CreateModels() {

			map = new Map(50,50);
		}

		private void OnClose() {
			map.Dispose();

			program.DisposeChildren = true;
			program.Dispose();
		}

		private void OnKeyboardDown(byte key, int x, int y) {
			player.KeyInput(key);

			if(key == 'l') {
				map.ResizeMap(false);
				Console.WriteLine("Py Pos: " + (map.mapY  + player.position.Z));
			}

			if(key == 27)
				Glut.glutLeaveMainLoop();
		}

		private void OnRenderFrame() {

			Gl.Viewport(0, 0, width, height);
			Gl.Clear(ClearBufferMask.ColorBufferBit
				| ClearBufferMask.DepthBufferBit);

			Gl.UseProgram(program);

			map.Draw(program);

			program["view_matrix"].SetValue(
				Matrix4.CreateTranslation(new Vector3(xCam, yCam, zCam)) *
				Matrix4.CreateRotationX(ryCam)*
				Matrix4.LookAt(new Vector3(0,0,-1),
								Vector3.Zero,
								new Vector3(0,1,0)));

			player.position = new Vector3(xCam, yCam, zCam);

			if(map.mapY + player.position.Z < player.RenderDistance + 2) {
				Console.WriteLine("Created new line on Z");
				map.ResizeMap(false);
			}else if(map.mapX + player.position.X < player.RenderDistance + 2) {
				Console.WriteLine("Created new line on X");
				map.ResizeMap(true);
			}

			Glut.glutSwapBuffers();
		}

		private void OnDisplay() {

		}

	}
}
