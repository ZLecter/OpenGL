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

		private int width = 1280, height = 720;
		private ShaderProgram program;

		// private Texture crateTexture;
		private int numCircles = 0;
		private Circle[] circles;
		/*private Block dirt, grass, sand, stone;*/
		private Map map;
		private float xCam = 0, yCam = 0, zCam = 0, mov = 0.1f;
		private float ryCam = 0;

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
				Matrix4.CreatePerspectiveFieldOfView(0.55f,
				(float)width / height, 0.1f, 1000f));
			Vector3 camPos = new Vector3(0,0,0);
			//-5 3 -5
			program["view_matrix"].SetValue(
				Matrix4.LookAt(new Vector3(0, 0, -10),
				camPos,
				new Vector3(0, 1, 0)));

			CreateModels();

			Glut.glutMainLoop();
		}

		private void CreateModels() {
			circles = new Circle[numCircles];
			for(int i = 0; i < numCircles; i++) {
				Vector3 pos = new Vector3(i-numCircles/2, 0, 0);
				Vector3 rot = new Vector3(0, 0, i * 360 / numCircles);
				circles[i] = new Circle(0.5f, 10, pos, rot, Vector3.One);
			}

			map = new Map(50,50);

			/*
			dirt = new Block(0, new Vector3(-2, 0, 0), Vector3.Zero, Vector3.One);
			grass = new Block(1, new Vector3(-1, 0, 0), Vector3.Zero, Vector3.One);
			stone = new Block(2, new Vector3(1, 0, 0), Vector3.Zero, Vector3.One);
			sand = new Block(3, new Vector3(2, 0, 0), Vector3.Zero, Vector3.One);
			*/
		}

		private void OnClose() {
			foreach(Circle c in circles) {
				c.Dispose();
			}
			map.Dispose();
			/*
			dirt.Dispose();
			grass.Dispose();
			stone.Dispose();
			sand.Dispose();
			*/
			program.DisposeChildren = true;
			program.Dispose();
		}

		private void OnKeyboardDown(byte key, int x, int y) {
			if(key == 'w')
				zCam -= mov + 0.2f;
			else if(key == 's')
				zCam += mov + 0.2f;

			if(key == 'd')
				xCam += mov;
			else if(key == 'a')
				xCam -= mov;

			if(key == 'q')
				yCam += mov;
			else if(key == 'e')
				yCam -= mov;

			if(key == 'x')
				ryCam += mov;
			else if(key == 'z')
				ryCam -= mov;

			if(key == 27)
				Glut.glutLeaveMainLoop();
		}

		private void OnRenderFrame() {

			Gl.Viewport(0, 0, width, height);
			Gl.Clear(ClearBufferMask.ColorBufferBit
				| ClearBufferMask.DepthBufferBit);

			Gl.UseProgram(program);

			// Draw shit
			foreach(Circle c in circles) {
				//c.Translate(Vector3.UnitY * 0.001f);
				c.Draw(program);
			}

			map.Draw(program);

			program["view_matrix"].SetValue(
				Matrix4.CreateTranslation(new Vector3(xCam, yCam, zCam)) *
				Matrix4.CreateRotationX(ryCam)*
				Matrix4.LookAt(new Vector3(0,0,-10),
								Vector3.Zero,
								new Vector3(0,1,0)));

			/*
			dirt.Rotate(Vector3.UnitX * 0.001f);
			dirt.Draw(program);

			grass.Rotate(Vector3.UnitX * 0.001f);
			grass.Draw(program);

			stone.Rotate(Vector3.UnitX * 0.001f);
			stone.Draw(program);

			sand.Rotate(Vector3.UnitX * 0.001f);
			sand.Draw(program);
			*/

			Glut.glutSwapBuffers();
		}

		private void OnDisplay() {

		}

	}
}
